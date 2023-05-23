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
} from "vscode-languageserver/node";
import { TextDocument } from "vscode-languageserver-textdocument";
import * as get_files from "get-all-files";
import * as url from "url";
import { Lexer, TokenType } from "../lexer";
import * as fs from "fs/promises";
import {
	ExtractedTokens,
	HJMCFile,
	JMCFile,
	MacrosData,
	getAllFunctionsCall,
	getClassRange,
	getCurrentStatement,
	getFunctions,
	getFunctionsCall,
	getIndexByOffset,
	getLiteralWithDot,
	getVariablesDeclare,
	offsetToPosition,
} from "../helpers/general";
import {
	concatFuncsTokens,
	concatVariableTokens,
	getAllVariables,
	getTokens,
} from "./serverHelper";
import { HeaderParser, HeaderType } from "../parseHeader";
import * as vscode from "vscode-languageserver";
import {
	SemanticTokenModifiers,
	SemanticTokenTypes,
} from "../data/semanticDatas";
import { URI } from "vscode-uri";

let jmcConfigs: string[] = [];
let jmcFiles: JMCFile[] = [];
let hjmcFiles: HJMCFile[] = [];
let extracted: ExtractedTokens = {
	variables: [],
	funcs: [],
};
let macros: MacrosData[] = [];
export let currentFile: string | undefined;

//#region default
const connection = createConnection(ProposedFeatures.all);
const documents: TextDocuments<TextDocument> = new TextDocuments(TextDocument);

let hasConfigurationCapability = false;
let hasWorkspaceFolderCapability = false;
let hasDiagnosticRelatedInformationCapability = false;
//#endregion
connection.onInitialize(async (params: InitializeParams) => {
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
				triggerCharacters: ["."],
			},
			signatureHelpProvider: {
				triggerCharacters: ["("],
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
async function validateJMC(fileText: string, path: string): Promise<Lexer> {
	const lexer = new Lexer(fileText, macros);
	currentFile = path;
	jmcFiles = jmcFiles.map((v) => {
		if (v.path == path) v.lexer = lexer;
		return v;
	});
	return lexer;
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
		const lexer = await validateJMC(textDocument.getText(), path);

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
	} else if (path.endsWith(".hjmc")) {
		const parser = await validateHJMC(textDocument.getText(), path);
	}
}

connection.onCompletion(
	async (arg, token, progress, result): Promise<CompletionItem[]> => {
		//check if `$VARIABLE.get()`
		if (arg.context?.triggerCharacter == ".") {
			const doc = documents.get(arg.textDocument.uri);
			const path = url.fileURLToPath(arg.textDocument.uri);
			const file = jmcFiles.find((v) => v.path == path);
			if (doc && file) {
				const offset = doc?.offsetAt(arg.position);
				const index = getIndexByOffset(file.lexer, offset - 1);
				const token = file.lexer.tokens[index - 2];
				if (token.type == TokenType.VARIABLE) {
					return [
						{
							label: "get",
							kind: CompletionItemKind.Function,
							insertText: "get()",
						},
					];
				}
			}
		}

		const vars: CompletionItem[] = concatVariableTokens(extracted).map(
			(v) => {
				return {
					label: v.value.slice(1),
					insertText: v.value,
					kind: CompletionItemKind.Variable,
				};
			}
		);
		const funcs: CompletionItem[] = concatFuncsTokens(extracted).map(
			(v) => {
				return {
					label: v.value,
					insertText: v.value + "()",
					kind: CompletionItemKind.Function,
				};
			}
		);
		const mos: CompletionItem[] = macros.map((v) => {
			return {
				label: v.target,
				kind: CompletionItemKind.Snippet,
				detail: v.values.join(" "),
			};
		});
		return vars.concat(funcs).concat(mos);
	}
);

connection.onCompletionResolve((item: CompletionItem): CompletionItem => {
	return item;
});

connection.onSignatureHelp((handler) => {
	return handler.context?.activeSignatureHelp;
});

connection.onDefinition(async (params) => {
	const currFile = jmcFiles.find(
		(v) => v.path == url.fileURLToPath(params.textDocument.uri)
	);
	const rDoc = documents.get(params.textDocument.uri);

	if (currFile && rDoc) {
		const index = getIndexByOffset(
			currFile.lexer,
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

//semantic highlight
connection.onRequest(
	"textDocument/semanticTokens/full",
	(params: vscode.SemanticTokensParams): SemanticTokens => {
		const doc = documents.get(params.textDocument.uri);
		const builder = new vscode.SemanticTokensBuilder();
		if (doc) {
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
							} else if (tokens[i + 1].type == TokenType.DOT) {
								const pos = doc.positionAt(token.pos);
								builder.push(
									pos.line,
									pos.character,
									token.value.length,
									5,
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
						default:
							break;
					}
				}
			}
		}
		return builder.build();
	}
);

// 	const document = documents.get(v.textDocument.uri);
// 	const builder = new SemanticTokensBuilder();
// 	if (document) {
// 		const tokens = jmcFiles.find(
// 			(v) => v.path == url.fileURLToPath(document?.uri)
// 		)?.lexer.tokens;
// 		if (tokens) {
// 			for (let i = 0; i < tokens.length; i++) {
// 				const token = tokens[i];
// 				switch (token.type) {
// 					case TokenType.CLASS:
// 						if (
// 							tokens[i + 1] !== undefined &&
// 							tokens[i + 1].type === TokenType.LITERAL
// 						) {
// 							const current = tokens[i + 1];
// 							const startPos = document.positionAt(current.pos);
// 							builder.push(
// 								startPos.line,
// 								startPos.character,
// 								current.value.length,
// 								0,
// 								0
// 							);
// 						}
// 						break;
// 					case TokenType.VARIABLE: {
// 						const startPos = document.positionAt(token.pos);
// 						builder.push(
// 							startPos.line,
// 							startPos.character,
// 							token.value.length,
// 							4,
// 							0
// 						);
// 						break;
// 					}
// 					case TokenType.LITERAL: {
// 						if (tokens[i + 1].type == TokenType.LPAREN) {
// 							const startPos = document.positionAt(token.pos);
// 							builder.push(
// 								startPos.line,
// 								startPos.character,
// 								token.value.length,
// 								3,
// 								0
// 							);
// 						} else if (tokens[i + 1].type == TokenType.DOT) {
// 							const startPos = document.positionAt(token.pos);
// 							builder.push(
// 								startPos.line,
// 								startPos.character,
// 								token.value.length,
// 								5,
// 								0
// 							);
// 						}

// 						break;
// 					}
// 				}
// 			}
// 		} else return builder.build();
// 	}
// 	connection.languages.semanticTokens.refresh();
// 	return builder.build();
// });

//data methods
connection.onRequest("data/getFile", (path: string): JMCFile | undefined => {
	return jmcFiles.find((v) => v.path == path);
});
connection.onRequest("data/getFiles", (path: string): JMCFile[] => {
	return jmcFiles;
});
connection.onRequest("data/getExtracted", (): ExtractedTokens => {
	return extracted;
});
//file operation
connection.onRequest(
	"file/update",
	async (path: string): Promise<Lexer | HeaderParser | undefined> => {
		if (path.endsWith(".jmc"))
			return await validateJMC(await fs.readFile(path, "utf-8"), path);
		else if (path.endsWith(".hjmc"))
			return await validateHJMC(await fs.readFile(path, "utf-8"), path);
		else return undefined;
	}
);

connection.onRequest("file/add", (file: JMCFile): number => {
	return jmcFiles.push(file);
});

connection.onRequest("file/add", (file: HJMCFile): number => {
	return hjmcFiles.push(file);
});

connection.onRequest("file/remove", (path: string): void => {
	for (let i = 0; i < jmcFiles.length; i++) {
		if (jmcFiles[i].path == path) {
			jmcFiles.splice(i, 1);
			return;
		}
	}
	for (let i = 0; i < hjmcFiles.length; i++) {
		if (hjmcFiles[i].path == path) {
			hjmcFiles.splice(i, 1);
			return;
		}
	}
});

documents.listen(connection);
connection.listen();
