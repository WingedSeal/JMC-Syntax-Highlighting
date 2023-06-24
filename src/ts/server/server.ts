import {
	createConnection,
	TextDocuments,
	ProposedFeatures,
	InitializeParams,
	DidChangeConfigurationNotification,
	CompletionItem,
	TextDocumentSyncKind,
	InitializeResult,
	CompletionItemKind,
	SemanticTokens,
	Position,
	Range,
} from "vscode-languageserver/node";
import { TextDocument } from "vscode-languageserver-textdocument";
import * as get_files from "get-all-files";
import * as url from "url";
import { Lexer, TokenData, TokenType } from "../lexer";
import * as fs from "fs/promises";
import {
	ExtractedTokens,
	HJMCFile,
	JMCFile,
	MacrosData,
	Settings,
	findStringDifference,
	getAllFunctionsCall,
	getClassRange,
	getCurrentStatement,
	getFunctions,
	getIndexByOffset,
	getLiteralWithDot,
	getVariablesDeclare,
	offsetToPosition,
	splitTokenString,
} from "../helpers/general";
import {
	concatFuncsTokens,
	concatVariableTokens,
	getAllVariables,
	getFirstHirarchy,
	getHirarchy,
	getTokens,
} from "./serverHelper";
import { HeaderParser, HeaderType } from "../parseHeader";
import * as vscode from "vscode-languageserver";
import {
	SemanticTokenModifiers,
	SemanticTokenTypes,
} from "../data/semanticDatas";
import { URI } from "vscode-uri";
import { BuiltInFunctions, methodInfoToDoc } from "../data/builtinFuncs";
import { HEADERS } from "../data/headers";
import { START_COMMAND } from "../data/commands";

export let jmcConfigs: string[] = [];
export const jmcFiles: JMCFile[] = [];
export let hjmcFiles: HJMCFile[] = [];
export let extracted: ExtractedTokens = {
	variables: [],
	funcs: [],
};
export let macros: MacrosData[] = [];
export let currentFile: string | undefined;

//#region default
const connection = createConnection(ProposedFeatures.all);
const documents: TextDocuments<TextDocument> = new TextDocuments(TextDocument);

let hasConfigurationCapability = false;
let hasWorkspaceFolderCapability = false;
let hasDiagnosticRelatedInformationCapability = false;
//#endregion
connection.onInitialize(async (params: InitializeParams) => {
	try {
		//initialze the JMCFiles & HJMC Files
		if (params.workspaceFolders) {
			for (const folder of params.workspaceFolders) {
				const files = get_files
					.getAllFilesSync(url.fileURLToPath(folder.uri))
					.toArray();
				jmcConfigs = jmcConfigs.concat(
					files.filter((v) => v.endsWith("jmc_config.json"))
				);
				const jfiles = files.filter(
					(v) => v.endsWith(".jmc") || v.endsWith(".hjmc")
				);
				for (const f of jfiles.filter((v) => v.endsWith(".hjmc"))) {
					const text = await fs.readFile(f, "utf-8");
					const parser = new HeaderParser(text);
					hjmcFiles.push({
						path: f,
						parser: parser,
					});
					for (const header of parser.data) {
						if (header.type == HeaderType.DEFINE) {
							macros.push({
								path: f,
								target: header.values[0],
								values: header.values.slice(1),
							});
						}
					}
				}
				for (const f of jfiles.filter((v) => v.endsWith(".jmc"))) {
					const text = await fs.readFile(f, "utf-8");
					jmcFiles.push({
						path: f,
						lexer: new Lexer(text, macros),
						text: text,
					});
				}

				extracted = await getTokens(jmcFiles);
			}
		}
		//#region default
		const capabilities = params.capabilities;

		hasConfigurationCapability = !!(
			capabilities.workspace && !!capabilities.workspace.configuration
		);
		hasWorkspaceFolderCapability = !!(
			capabilities.workspace && !!capabilities.workspace.workspaceFolders
		);
		hasDiagnosticRelatedInformationCapability = !!(
			capabilities.textDocument &&
			capabilities.textDocument.publishDiagnostics &&
			capabilities.textDocument.publishDiagnostics.relatedInformation
		);

		const SemanticTokensOptions: vscode.SemanticTokensOptions = {
			legend: {
				tokenTypes: SemanticTokenTypes,
				tokenModifiers: SemanticTokenModifiers,
			},
			full: true,
		};

		const result: InitializeResult = {
			capabilities: {
				textDocumentSync: TextDocumentSyncKind.Incremental,
				// Tell the client that this server supports code completion.
				completionProvider: {
					resolveProvider: true,
					triggerCharacters: [".", "#", " ", "/"],
				},
				signatureHelpProvider: {
					triggerCharacters: ["(", ",", " "],
					retriggerCharacters: [",", " "],
				},
				semanticTokensProvider: SemanticTokensOptions,
				definitionProvider: true,
			},
		};
		if (hasWorkspaceFolderCapability) {
			result.capabilities.workspace = {
				workspaceFolders: {
					supported: true,
				},
			};
		}
		return result;
		//#endregion
	} catch (e: any) {
		console.log(e);
		throw new e();
	}
});

connection.onInitialized(() => {
	if (hasConfigurationCapability) {
		// Register for all configuration changes.
		connection.client.register(
			DidChangeConfigurationNotification.type,
			undefined
		);
	}
	if (hasWorkspaceFolderCapability) {
		connection.workspace.onDidChangeWorkspaceFolders((_event) => {
			connection.console.log("Workspace folder change event received.");
		});
	}
});

interface ServerSettings {
	maxNumberOfProblems: number;
}

const defaultSettings: ServerSettings = { maxNumberOfProblems: 1000 };
let globalSettings: ServerSettings = defaultSettings;

const documentSettings: Map<string, Thenable<ServerSettings>> = new Map();

connection.onDidChangeConfiguration((change) => {
	if (hasConfigurationCapability) {
		documentSettings.clear();
	} else {
		globalSettings = <ServerSettings>(
			(change.settings.languageServerExample || defaultSettings)
		);
	}

	documents.all().forEach(validateTextDocument);
});

function getDocumentSettings(resource: string): Thenable<ServerSettings> {
	if (!hasConfigurationCapability) {
		return Promise.resolve(globalSettings);
	}
	let result = documentSettings.get(resource);
	if (!result) {
		result = connection.workspace.getConfiguration({
			scopeUri: resource,
			section: "jmc",
		});
		documentSettings.set(resource, result);
	}
	return result;
}

documents.onDidClose((e) => {
	documentSettings.delete(e.document.uri);
});

documents.onDidChangeContent((change) => {
	validateTextDocument(change.document);
});

/**
 * validate .jmc
 * @param fileText the file text
 * @param path fsPath of the file
 * @returns the changed lexer - {@link Lexer}
 */
async function validateJMC(
	fileText: string,
	path: string,
	doc: TextDocument
): Promise<Lexer | undefined> {
	const file = jmcFiles.find((v) => v.path == path);
	if (file) {
		const changedIndex = await findStringDifference(file.text, fileText);
		const differenceLength = Math.abs(file.text.length - fileText.length);
		if (changedIndex) {
			const lexerTokens = file.lexer.tokens;

			//get ranged text
			const start = doc.positionAt(changedIndex);
			const startPos = Position.create(start.line, 0);
			const startOffset = doc.offsetAt(startPos);
			const startIndex = getIndexByOffset(file.lexer.tokens, startOffset);

			const end = doc.positionAt(changedIndex + differenceLength);
			const endPos = Position.create(end.line + 1, 0);
			const endIndex =
				getIndexByOffset(file.lexer.tokens, doc.offsetAt(endPos)) - 1;

			const range = vscode.Range.create(startPos, endPos);
			const text = doc.getText(range);

			let currentIndex = startOffset;

			//tokenize the text
			const tokens: TokenData[] = [];
			const splited = await splitTokenString(text);
			for (let i = 0; i < splited.length; i++) {
				const s = splited[i].trim();
				const t = splited[i];
				const token = file.lexer.tokenize(
					s,
					currentIndex,
					file.lexer.tokens
				);
				if (token) tokens.push(token);
				currentIndex += t.length;
			}

			//modify the tokens
			if (file.text.length > fileText.length) {
				file.lexer.tokens = lexerTokens
					.slice(0, startIndex)
					.concat(tokens)
					.concat(
						lexerTokens.slice(endIndex).map((v) => {
							v.pos -= differenceLength;
							return v;
						})
					);
				file.lexer.parseCommand(changedIndex, fileText);
			} else {
				file.lexer.tokens = lexerTokens
					.slice(0, startIndex)
					.concat(tokens)
					.concat(
						lexerTokens.slice(endIndex).map((v) => {
							v.pos += differenceLength;
							return v;
						})
					);
				file.lexer.parseCommand(changedIndex, fileText);
			}
		}

		file.text = fileText;
		return file.lexer;
	}
}

/**
 * validate .hjmc
 * @param fileText the file text
 * @param path fsPath of the file
 * @returns the changed parser - {@link HeaderParser}
 */
async function validateHJMC(
	fileText: string,
	path: string
): Promise<HeaderParser> {
	const parser = new HeaderParser(fileText);
	currentFile = path;
	macros = macros.filter((macro) => macro.path !== path);
	parser.data
		.filter((header) => header.type === HeaderType.DEFINE)
		.forEach((header) => {
			macros.push({
				path,
				target: header.values[0],
				values: header.values.slice(1),
			});
		});
	hjmcFiles = hjmcFiles.map((v) => {
		if (v.path == path) v.parser == parser;
		return v;
	});
	return parser;
}

async function validateTextDocument(textDocument: TextDocument): Promise<void> {
	const settings = await getDocumentSettings(textDocument.uri);

	const path = url.fileURLToPath(textDocument.uri);
	if (path.endsWith(".jmc")) {
		const lexer = await validateJMC(
			textDocument.getText(),
			path,
			textDocument
		);
		if (lexer) {
			const funcs = await getFunctions(lexer);
			const vars = await getVariablesDeclare(lexer);

			extracted.variables = extracted.variables.map((v) => {
				if (v.path == path) v.tokens = vars;
				return v;
			});
			extracted.funcs = extracted.funcs.map((v) => {
				if (v.path == path) v.tokens = funcs;
				return v;
			});
		}
	} else if (path.endsWith(".hjmc")) {
		const parser = await validateHJMC(textDocument.getText(), path);
	}
}

connection.onCompletion(async (arg) => {
	if (arg.textDocument.uri.endsWith(".hjmc")) {
		//return headers
		if (arg.context?.triggerCharacter == "#")
			return HEADERS.map((v) => {
				return {
					label: v,
					kind: CompletionItemKind.Keyword,
				};
			});
		else {
			const doc = documents.get(arg.textDocument.uri);
			if (doc) {
				const start = Position.create(arg.position.line, 0);
				const end = doc.positionAt(
					doc.offsetAt(Position.create(arg.position.line + 1, 0))
				);
				const range = Range.create(start, end);
				const text = doc.getText(range);
				const current = HeaderParser.parseText(text);
				switch (current.type) {
					case HeaderType.BIND:
						if (
							current.values[0] == "" ||
							current.values[0] == undefined
						)
							return [
								{
									label: "__namespace__",
									kind: CompletionItemKind.Value,
								},
								{
									label: "__UUID__",
									kind: CompletionItemKind.Value,
								},
							];
						break;
					case HeaderType.STATIC:
					default:
						return undefined;
				}
			}
		}
	} else if (arg.textDocument.uri.endsWith(".jmc")) {
		const oFuncs = concatFuncsTokens(extracted).map(
			(v) => v.value.split("\0")[0]
		);
		const cfDatas = await getFirstHirarchy(oFuncs);
		const doc = documents.get(arg.textDocument.uri);
		const file = jmcFiles.find(
			(v) => v.path === url.fileURLToPath(arg.textDocument.uri)
		);

		//check if `$VARIABLE.get()` or `CLASS.METHOD`
		if (arg.context?.triggerCharacter == ".") {
			const doc = documents.get(arg.textDocument.uri);
			const path = url.fileURLToPath(arg.textDocument.uri);
			const file = jmcFiles.find((v) => v.path == path);
			if (doc && file) {
				const offset = doc.offsetAt(arg.position);
				let index = getIndexByOffset(file.lexer.tokens, offset - 1);
				if (
					file.lexer.tokens[index].type == TokenType.LCP ||
					file.lexer.tokens[index].type == TokenType.RCP
				)
					index--;
				else if (file.lexer.tokens[index].type == TokenType.SEMI)
					index++;
				const token = file.lexer.tokens[index - 3];
				if (token.type == TokenType.VARIABLE) {
					return [
						{
							label: "get",
							kind: CompletionItemKind.Function,
							insertText: "get()",
						},
					];
				} else if (token.type == TokenType.LITERAL) {
					const classResult = BuiltInFunctions.find(
						(v) => v.class == token.value
					);
					if (classResult) {
						return classResult.methods.map((v) => {
							return {
								label: v.name,
								kind: CompletionItemKind.Function,
							};
						});
					} else {
						const statement = await getCurrentStatement(
							file.lexer,
							token
						);
						if (statement) {
							const literal = await getLiteralWithDot(
								statement,
								token
							);
							if (literal) {
								const splited = literal.split(".");
								const query = await getHirarchy(
									oFuncs,
									splited
								);
								if (query) {
									const cls = query.classes.map(
										(v): CompletionItem => {
											return {
												label: v,
												kind: CompletionItemKind.Class,
											};
										}
									);
									const funcs = query.funcs.map(
										(v): CompletionItem => {
											return {
												label: v,
												kind: CompletionItemKind.Function,
												insertText: `${v}()`,
											};
										}
									);
									return cls.concat(funcs);
								}
							}
						}
					}
				}
			}
		}

		//keywords
		const keywords: CompletionItem[] = [
			{
				label: "import",
				kind: CompletionItemKind.Keyword,
			},
			{
				label: "class",
				kind: CompletionItemKind.Keyword,
			},
			{
				label: "function",
				kind: CompletionItemKind.Keyword,
			},
			{
				label: "new",
				kind: CompletionItemKind.Keyword,
			},
		];

		const commands: CompletionItem[] = START_COMMAND.map((v) => ({
			label: v,
			kind: CompletionItemKind.Keyword,
		}));

		//variables
		const vars: CompletionItem[] = concatVariableTokens(extracted).map(
			(v) => {
				return {
					label: v.value.slice(1),
					insertText: v.value,
					kind: CompletionItemKind.Variable,
				};
			}
		);

		//funcs & classes
		const funcs: CompletionItem[] = cfDatas.funcs.map(
			(v): CompletionItem => {
				return {
					label: v,
					kind: CompletionItemKind.Function,
					insertText: `${v}()`,
				};
			}
		);
		const classes: CompletionItem[] = cfDatas.classes.map(
			(v): CompletionItem => {
				return {
					label: v,
					kind: CompletionItemKind.Class,
				};
			}
		);

		//macros
		const mos: CompletionItem[] = macros.map((v) => {
			return {
				label: v.target,
				kind: CompletionItemKind.Snippet,
				detail: v.values.join(" "),
			};
		});

		//builtin classes
		const builtInClasses: CompletionItem[] = BuiltInFunctions.map((v) => {
			return {
				label: v.class,
				kind: CompletionItemKind.Class,
				detail: "builtin functions provided by JMC",
			};
		});

		//return vars.concat(funcs).concat(mos).concat(builtInClasses);
		return vars
			.concat(keywords)
			.concat(commands)
			.concat(funcs)
			.concat(classes)
			.concat(mos)
			.concat(builtInClasses);
	}
});

connection.onCompletionResolve(
	async (item: CompletionItem): Promise<CompletionItem> => {
		return item;
	}
);

connection.onSignatureHelp(async (params) => {
	const context = params.context;
	const doc = documents.get(params.textDocument.uri);
	const file = jmcFiles.find(
		(v) => v.path == url.fileURLToPath(params.textDocument.uri)
	);
	if (context && doc && file) {
		const triggerChar = context.triggerCharacter;

		const index = getIndexByOffset(
			file.lexer.tokens,
			doc.offsetAt(params.position) - 2
		);
		const tokens = file.lexer.tokens;
		const currentToken = tokens[index];
		const statement = await getCurrentStatement(file.lexer, currentToken);

		if (triggerChar == "(" && statement) {
			const funcName = await getLiteralWithDot(
				statement,
				tokens[index - 3]
			);
			if (funcName) {
				const splited = funcName.split(".");
				if (splited.length == 2) {
					const _class = splited[0];
					const method = splited[1];
					const methods = BuiltInFunctions.find(
						(v) => v.class == _class
					)?.methods;
					if (methods) {
						const result = methods.find((v) => v.name == method);
						if (result) {
							return {
								signatures: [
									{
										label: methodInfoToDoc(result),
										parameters: result.args.map((v) => {
											const def =
												v.default !== undefined
													? ` = ${v.default}`
													: "";
											const arg = `${v.name}: ${v.type}${def}`;
											return {
												label: arg,
											};
										}),
										documentation: result.doc,
									},
								],
								activeParameter: 0,
								activeSignature: 0,
							};
						}
					}
				}
			}
		} else if (triggerChar == "," && statement) {
			let commaCount = 0;
			let pCount = 1;
			for (let i = index; i != -1; i--) {
				const current = tokens[i];
				if (current.type == TokenType.RCP) {
					let count = 0;
					for (; i != 0; i--) {
						const t = tokens[i];
						if (t.type == TokenType.RCP) count++;
						else if (t.type == TokenType.LCP) count--;
						else if (count == 0) break;
					}
				} else if (current.type == TokenType.COMMA) commaCount++;
				else if (current.type == TokenType.RPAREN) pCount++;
				else if (current.type == TokenType.LPAREN) pCount--;
				else if (pCount == 0) {
					const funcStatement = await getCurrentStatement(
						file.lexer,
						current
					);
					if (funcStatement) {
						const funcName = await getLiteralWithDot(
							funcStatement,
							current
						);
						if (funcName) {
							const splited = funcName.split(".");
							if (splited.length == 2) {
								const _class = splited[0];
								const method = splited[1];
								const methods = BuiltInFunctions.find(
									(v) => v.class == _class
								)?.methods;
								if (methods) {
									const result = methods.find(
										(v) => v.name == method
									);
									if (result) {
										return {
											signatures: [
												{
													label: methodInfoToDoc(
														result
													),
													parameters: result.args.map(
														(v) => {
															const def =
																v.default !==
																undefined
																	? ` = ${v.default}`
																	: "";
															const arg = `${v.name}: ${v.type}${def}`;
															return {
																label: arg,
															};
														}
													),
													documentation: result.doc,
												},
											],
											activeParameter: commaCount,
											activeSignature: 0,
										};
									}
								}
							}
						}
					}
					break;
				}
			}
		}
	}
	return undefined;
});

connection.onDefinition(async (params) => {
	const currFile = jmcFiles.find(
		(v) => v.path == url.fileURLToPath(params.textDocument.uri)
	);
	const rDoc = documents.get(params.textDocument.uri);

	if (currFile && rDoc) {
		const index = getIndexByOffset(
			currFile.lexer.tokens,
			rDoc.offsetAt(params.position)
		);

		const tokens = currFile.lexer.tokens;
		const currentToken = tokens[index - 1];
		const datas: vscode.Location[] = [];
		const currentStatement = await getCurrentStatement(
			currFile.lexer,
			currentToken
		);

		if (currentToken.type == TokenType.VARIABLE) {
			for (const ev of await getAllVariables(jmcFiles)) {
				const vTokens = ev.tokens.filter(
					(v) => v.value == currentToken.value
				);
				if (vTokens.length == 0) {
					continue;
				}
				const docText = await fs.readFile(ev.path, "utf-8");
				for (const v of vTokens) {
					const start = await offsetToPosition(v.pos, docText);
					const startPos = vscode.Position.create(
						start.line,
						start.character
					);

					const end = await offsetToPosition(
						v.pos + v.value.length,
						docText
					);
					const endPos = vscode.Position.create(
						end.line,
						end.character
					);

					const range = vscode.Range.create(startPos, endPos);
					datas.push({
						uri: URI.file(ev.path).toString(),
						range: range,
					});
				}
			}
		} else if (
			currentStatement &&
			currentStatement[0].type == TokenType.FUNCTION
		) {
			const classRanges = await getClassRange(currFile.lexer);
			let literal = await getLiteralWithDot(currentStatement.slice(1));
			const startToken = currentStatement[0];
			for (const range of classRanges) {
				if (
					range.range[0] < startToken.pos &&
					range.range[1] > startToken.pos
				) {
					literal = range.name + "." + literal;
				}
			}
			const funcCalls = await getAllFunctionsCall(jmcFiles);
			for (const funcCall of funcCalls) {
				for (const t of funcCall.tokens) {
					if (t.value == literal) {
						const docText = await fs.readFile(
							funcCall.path,
							"utf-8"
						);
						const startPos = await offsetToPosition(t.pos, docText);
						const start = vscode.Position.create(
							startPos.line,
							startPos.character
						);

						const endPos = await offsetToPosition(
							t.pos + t.value.length,
							docText
						);
						const end = vscode.Position.create(
							endPos.line,
							endPos.character
						);

						const range = vscode.Range.create(start, end);
						datas.push({
							uri: URI.file(funcCall.path).toString(),
							range: range,
						});
					}
				}
			}
		} else if (currentStatement) {
			const literal = await getLiteralWithDot(
				currentStatement,
				currentToken
			);
			for (const file of jmcFiles) {
				const funcs = await getFunctions(file.lexer);
				for (const func of funcs) {
					if (literal == func.value.split("\0")[0]) {
						const docText = await fs.readFile(file.path, "utf-8");

						const start = await offsetToPosition(func.pos, docText);
						const startPos = vscode.Position.create(
							start.line,
							start.character
						);

						const end = await offsetToPosition(
							func.pos + func.value.split("\0")[1].length,
							docText
						);
						const endPos = vscode.Position.create(
							end.line,
							end.character
						);

						const range = vscode.Range.create(startPos, endPos);

						return {
							uri: URI.file(file.path).toString(),
							range: range,
						};
					}
				}
			}
		}

		return datas;
	}
	return null;
});

//*semantic highlight
connection.onRequest(
	"textDocument/semanticTokens/full",
	async (params: vscode.SemanticTokensParams): Promise<SemanticTokens> => {
		const doc = documents.get(params.textDocument.uri);
		const builder = new vscode.SemanticTokensBuilder();
		if (doc) {
			const settings = (await connection.workspace.getConfiguration({
				section: "jmc",
				scopeUri: params.textDocument.uri,
			})) as Settings;
			const file = jmcFiles.find(
				(v) => v.path == url.fileURLToPath(doc.uri)
			);
			if (file) {
				const tokens = file.lexer.tokens;
				for (let i = 0; i < tokens.length; i++) {
					const token = tokens[i];
					switch (token.type) {
						case TokenType.CLASS:
							if (
								tokens[i + 1] &&
								tokens[i + 1].type === TokenType.LITERAL
							) {
								const current = tokens[i + 1];
								const pos = doc.positionAt(current.pos);
								builder.push(
									pos.line,
									pos.character,
									current.value.length,
									0,
									0
								);
							}
							break;
						case TokenType.VARIABLE: {
							const pos = doc.positionAt(token.pos);
							builder.push(
								pos.line,
								pos.character,
								token.value.length,
								4,
								0
							);
							break;
						}
						case TokenType.LITERAL: {
							if (tokens[i + 1].type == TokenType.LPAREN) {
								const pos = doc.positionAt(token.pos);
								builder.push(
									pos.line,
									pos.character,
									token.value.length,
									3,
									0
								);
							} else if (
								tokens[i + 1].type == TokenType.DOT &&
								!settings.rawFuncHighlight
							) {
								const pos = doc.positionAt(token.pos);
								builder.push(
									pos.line,
									pos.character,
									token.value.length,
									0,
									0
								);
							} else if (
								tokens[i + 1].type == TokenType.DOT &&
								settings.rawFuncHighlight
							) {
								const pos = doc.positionAt(token.pos);
								builder.push(
									pos.line,
									pos.character,
									token.value.length,
									3,
									0
								);
							}
							break;
						}
						case TokenType.MACROS: {
							const pos = doc.positionAt(token.pos);
							builder.push(
								pos.line,
								pos.character,
								token.value.length,
								5,
								0
							);
							break;
						}
						case TokenType.COMMAND_LITERAL: {
							const pos = doc.positionAt(token.pos);
							builder.push(
								pos.line,
								pos.character,
								token.value.length,
								6,
								0
							);
							break;
						}
						case TokenType.OLD_IMPORT: {
							const pos = doc.positionAt(token.pos);
							builder.push(
								pos.line,
								pos.character,
								token.value.length,
								6,
								0b0100
							);
							break;
						}
						default: {
							const pos = doc.positionAt(token.pos);
							builder.push(
								pos.line,
								pos.character,
								token.value.length,
								-1,
								0
							);
							break;
						}
					}
				}
			}
		}
		return builder.build();
	}
);

documents.listen(connection);
connection.listen();
