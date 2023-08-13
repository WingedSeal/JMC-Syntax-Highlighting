/**
 * remember to use "Fold All" for better reading :P
 * it is too long :(
 */
import * as vscode from "vscode-languageserver/node";
import { BaseServer, ServerData } from "./serverData";
import * as get_files from "get-all-files";
import * as url from "url";
import * as fs from "fs/promises";
import { HeaderParser, HeaderType } from "../parseHeader";
import { Lexer, TokenData, TokenType } from "../lexer";
import { ExtractedTokensHelper, HirarchyHelper } from "../helpers/serverHelper";
import {
	JMCFile,
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
	removeDuplicate,
	splitTokenString,
} from "../helpers/general";
import { TextDocument } from "vscode-languageserver-textdocument";
import { HEADERS } from "../data/headers";
import { BuiltInFunctions, methodInfoToDoc } from "../data/builtinFuncs";
import {
	COMMAND_ENTITY_SELECTORS,
	START_COMMAND,
	getNode,
} from "../data/commands";
import { URI } from "vscode-uri";
export class JMCServer extends ServerData implements BaseServer {
	connection: vscode.Connection;
	documents: vscode.TextDocuments<TextDocument>;

	constructor(connection: vscode.Connection) {
		super();
		this.connection = connection;
		this.documents = new vscode.TextDocuments(TextDocument);
		this.validateJMC = this.validateJMC.bind(this);
	}

	/**
	 * register features
	 * @reminder remember use {@link start} to start the server
	 */
	register() {
		//documents
		this.documents.onDidChangeContent((change) => {
			this.validateTextDocument(change.document);
		});

		//features
		this.connection.onInitialize(this.onInitialize.bind(this));
		this.connection.onInitialized(this.onInitialized.bind(this));
		this.connection.onDidChangeWatchedFiles(
			this.onDidChangeWatchedFiles.bind(this)
		);
		this.connection.onDidChangeConfiguration(
			this.onDidChangeConfiguration.bind(this)
		);
		this.connection.onCompletion(this.onCompletion.bind(this));
		this.connection.onCompletionResolve(
			this.onCompletionResolve.bind(this)
		);
		this.connection.onRequest(
			"textDocument/semanticTokens/full",
			this.onSemanticHighlightFull.bind(this)
		);
		this.connection.onSignatureHelp(this.onSignatureHelp.bind(this));
		this.connection.onDefinition(this.onDefinition.bind(this));

		this.logger.info("register completed");
	}

	/**
	 * start the server
	 */
	start() {
		this.documents.listen(this.connection);
		this.connection.listen();
		this.logger.info("Server Started");
	}

	//#region register
	async onInitialize(params: vscode.InitializeParams) {
		//initialze the JMCFiles & HJMC Files
		if (params.workspaceFolders) {
			for (const folder of params.workspaceFolders) {
				const files = get_files
					.getAllFilesSync(url.fileURLToPath(folder.uri))
					.toArray();
				this.jmcConfigPaths = this.jmcConfigPaths.concat(
					files.filter((v) => v.endsWith("jmc_config.json"))
				);
				const jfiles = files.filter(
					(v) => v.endsWith(".jmc") || v.endsWith(".hjmc")
				);
				for (const f of jfiles.filter((v) => v.endsWith(".hjmc"))) {
					const text = await fs.readFile(f, "utf-8");
					const parser = new HeaderParser(text);
					this.hjmcFiles.push({
						path: f,
						parser: parser,
					});
					for (const header of parser.data) {
						if (header.type == HeaderType.DEFINE) {
							this.macros.push({
								path: f,
								target: header.values[0],
								values: header.values.slice(1),
							});
						}
					}
				}
				for (const f of jfiles.filter((v) => v.endsWith(".jmc"))) {
					const text = await fs.readFile(f, "utf-8");
					this.jmcFiles.push({
						path: f,
						lexer: new Lexer(text, this.macros),
						text: text,
					});
				}

				this.extractedTokens = await ExtractedTokensHelper.getTokens(
					this.jmcFiles
				);
			}
		}
		return this.initResult;
	}

	async onInitialized(params: vscode.InitializedParams) {
		this.connection.client.register(
			vscode.DidChangeConfigurationNotification.type,
			undefined
		);
	}

	async onDidChangeWatchedFiles(
		params: vscode.DidChangeWatchedFilesParams
	): Promise<void> {
		for (const change of params.changes) {
			switch (change.type) {
				case vscode.FileChangeType.Created: {
					const path = url.fileURLToPath(change.uri);
					const text = await fs.readFile(path, "utf-8");
					const lexer = new Lexer(text, this.macros);
					const file: JMCFile = {
						path: path,
						text: text,
						lexer: lexer,
					};
					this.jmcFiles.push(file);
					break;
				}
				case vscode.FileChangeType.Deleted: {
					const path = url.fileURLToPath(change.uri);
					const q = this.jmcFiles.find((v) => v && v.path === path);
					if (q) {
						const index = this.jmcFiles.indexOf(q);
						delete this.jmcFiles[index];
						this.jmcFiles = this.jmcFiles.filter((v) => v);
					}
					break;
				}
				case vscode.FileChangeType.Changed:
				default:
					break;
			}
		}
	}

	async onDidChangeConfiguration(
		change: vscode.DidChangeConfigurationParams
	): Promise<void> {
		this.documents.all().forEach(this.validateTextDocument);
	}

	async onSemanticHighlightFull(
		params: vscode.SemanticTokensParams
	): Promise<vscode.SemanticTokens> {
		const doc = this.documents.get(params.textDocument.uri);
		const builder = new vscode.SemanticTokensBuilder();
		if (doc) {
			const settings = (await this.connection.workspace.getConfiguration({
				section: "jmc",
				scopeUri: params.textDocument.uri,
			})) as Settings;
			const file = this.jmcFiles.find(
				(v) => v.path == url.fileURLToPath(doc.uri)
			);
			if (file) {
				const tokens = file.lexer.tokens;
				for (let i = 0; i < tokens.length; i++) {
					const token = tokens[i];
					switch (token.type) {
						case TokenType.CLASS: {
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
									settings.capitalizedClass &&
										/^[A-Z]/.test(current.value)
										? 0b10000
										: 0
								);
							}
							break;
						}
						case TokenType.NEW: {
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
									settings.capitalizedClass &&
										/^[A-Z]/.test(current.value)
										? 0b10000
										: 0
								);
							}
							if (
								tokens[i + 2] &&
								tokens[i + 2].type === TokenType.DOT &&
								tokens[i + 3] &&
								tokens[i + 3].type === TokenType.LITERAL
							) {
								const current = tokens[i + 3];
								const pos = doc.positionAt(current.pos);
								builder.push(
									pos.line,
									pos.character,
									current.value.length,
									0,
									settings.capitalizedClass &&
										/^[A-Z]/.test(current.value)
										? 0b10000
										: 0
								);
							}
							break;
						}
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
							if (
								tokens[i + 1] &&
								tokens[i + 1].type == TokenType.LPAREN
							) {
								const pos = doc.positionAt(token.pos);
								builder.push(
									pos.line,
									pos.character,
									token.value.length,
									3,
									0
								);
							} else if (
								tokens[i + 1] &&
								tokens[i + 1].type == TokenType.DOT &&
								!settings.rawFuncHighlight
							) {
								const pos = doc.positionAt(token.pos);
								builder.push(
									pos.line,
									pos.character,
									token.value.length,
									0,
									settings.capitalizedClass &&
										/^[A-Z]/.test(token.value)
										? 0b10000
										: 0
								);
							} else if (
								tokens[i + 1] &&
								tokens[i + 1].type == TokenType.DOT &&
								settings.rawFuncHighlight
							) {
								const pos = doc.positionAt(token.pos);
								builder.push(
									pos.line,
									pos.character,
									token.value.length,
									3,
									settings.capitalizedClass &&
										/^[A-Z]/.test(token.value)
										? 0b10000
										: 0
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

	async onSignatureHelp(
		params: vscode.SignatureHelpParams
	): Promise<vscode.SignatureHelp | null | undefined> {
		const context = params.context;
		const doc = this.documents.get(params.textDocument.uri);
		const file = this.jmcFiles.find(
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
			const statement = await getCurrentStatement(
				file.lexer,
				currentToken
			);

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
							const result = methods.find(
								(v) => v.name == method
							);
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
														parameters:
															result.args.map(
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
														documentation:
															result.doc,
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
	}

	async onDefinition(
		params: vscode.DefinitionParams
	): Promise<vscode.Definition | vscode.LocationLink[] | null | undefined> {
		const currFile = this.jmcFiles.find(
			(v) => v.path == url.fileURLToPath(params.textDocument.uri)
		);
		const rDoc = this.documents.get(params.textDocument.uri);

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
				for (const ev of await ExtractedTokensHelper.getAllVariables(
					this.jmcFiles
				)) {
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
				let literal = await getLiteralWithDot(
					currentStatement.slice(1)
				);
				const startToken = currentStatement[0];
				for (const range of classRanges) {
					if (
						range.range[0] < startToken.pos &&
						range.range[1] > startToken.pos
					) {
						literal = range.name + "." + literal;
					}
				}
				const funcCalls = await getAllFunctionsCall(this.jmcFiles);
				for (const funcCall of funcCalls) {
					for (const t of funcCall.tokens) {
						if (t.value == literal) {
							const docText = await fs.readFile(
								funcCall.path,
								"utf-8"
							);
							const startPos = await offsetToPosition(
								t.pos,
								docText
							);
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
				for (const file of this.jmcFiles) {
					const funcs = await getFunctions(file.lexer);
					for (const func of funcs) {
						if (literal == func.value.split("\0")[0]) {
							const docText = await fs.readFile(
								file.path,
								"utf-8"
							);

							const start = await offsetToPosition(
								func.pos,
								docText
							);
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
	}
	//#endregion

	//#region validate docs
	/**
	 * validate a document
	 * @description use {@link validateJMC} or {@link validateHJMC} if document changed
	 * @param textDocument
	 */
	private async validateTextDocument(
		textDocument: TextDocument
	): Promise<void> {
		const path = url.fileURLToPath(textDocument.uri);
		if (path.endsWith(".jmc")) {
			const lexer = await this.validateJMC(
				textDocument.getText(),
				path,
				textDocument
			);
			if (lexer) {
				const funcs = await getFunctions(lexer);
				const vars = await getVariablesDeclare(lexer);

				this.extractedTokens.variables =
					this.extractedTokens.variables.map((v) => {
						if (v.path == path) v.tokens = vars;
						return v;
					});
				this.extractedTokens.funcs = this.extractedTokens.funcs.map(
					(v) => {
						if (v.path == path) v.tokens = funcs;
						return v;
					}
				);
			}
		} else if (path.endsWith(".hjmc")) {
			const parser = await this.validateHJMC(
				textDocument.getText(),
				path
			);
		}
	}

	/**
	 * validate .jmc
	 * @param fileText the file text
	 * @param path fsPath of the file
	 * @returns the changed lexer - {@link Lexer}
	 */
	private async validateJMC(
		fileText: string,
		path: string,
		doc: TextDocument
	): Promise<Lexer | undefined> {
		const file = this.jmcFiles.find((v) => v.path == path);
		if (file) {
			const changedIndex = await findStringDifference(
				file.text,
				fileText
			);
			const differenceLength = Math.abs(
				file.text.length - fileText.length
			);
			if (changedIndex) {
				const lexerTokens = file.lexer.tokens;

				//get ranged text
				const start = doc.positionAt(changedIndex);
				const startPos = vscode.Position.create(start.line, 0);
				const startOffset = doc.offsetAt(startPos);
				const startIndex = getIndexByOffset(
					file.lexer.tokens,
					startOffset
				);

				const end = doc.positionAt(changedIndex + differenceLength);
				const endPos = vscode.Position.create(end.line + 1, 0);
				const endIndex =
					getIndexByOffset(file.lexer.tokens, doc.offsetAt(endPos)) -
					1;

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
	private async validateHJMC(
		fileText: string,
		path: string
	): Promise<HeaderParser> {
		const parser = new HeaderParser(fileText);
		this.macros = this.macros.filter((macro) => macro.path !== path);
		parser.data
			.filter((header) => header.type === HeaderType.DEFINE)
			.forEach((header) => {
				this.macros.push({
					path,
					target: header.values[0],
					values: header.values.slice(1),
				});
			});
		this.hjmcFiles = this.hjmcFiles.map((v) => {
			if (v.path == path) v.parser == parser;
			return v;
		});
		return parser;
	}
	//#endregion

	//#region completion
	async onCompletion(
		arg: vscode.CompletionParams
	): Promise<vscode.CompletionItem[] | undefined> {
		if (arg.textDocument.uri.endsWith(".hjmc"))
			return await this.hjmcCompletion(arg);
		else if (arg.textDocument.uri.endsWith(".jmc"))
			return await this.jmcCompletion(arg);
	}

	async onCompletionResolve(
		item: vscode.CompletionItem
	): Promise<vscode.CompletionItem> {
		return item;
	}

	private async hjmcCompletion(
		arg: vscode.CompletionParams
	): Promise<vscode.CompletionItem[] | undefined> {
		//return headers
		if (arg.context?.triggerCharacter == "#")
			return HEADERS.map((v) => {
				return {
					label: v,
					kind: vscode.CompletionItemKind.Keyword,
				};
			});
		else {
			const doc = this.documents.get(arg.textDocument.uri);
			if (doc) {
				const start = vscode.Position.create(arg.position.line, 0);
				const end = doc.positionAt(
					doc.offsetAt(
						vscode.Position.create(arg.position.line + 1, 0)
					)
				);
				const range = vscode.Range.create(start, end);
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
									kind: vscode.CompletionItemKind.Value,
								},
								{
									label: "__UUID__",
									kind: vscode.CompletionItemKind.Value,
								},
							];
						break;
					case HeaderType.STATIC:
					default:
						return undefined;
				}
			}
		}
	}

	private async jmcCompletion(arg: vscode.CompletionParams) {
		const oFuncs = ExtractedTokensHelper.concatFuncsTokens(
			this.extractedTokens
		).map((v) => v.value.split("\0")[0]);
		const cfDatas = await HirarchyHelper.getFirstHirarchy(oFuncs);
		const doc = this.documents.get(arg.textDocument.uri);
		const file = this.jmcFiles.find(
			(v) => v.path === url.fileURLToPath(arg.textDocument.uri)
		);

		if (arg.context?.triggerCharacter == " " && doc && file) {
			const offset = doc.offsetAt(arg.position);
			const index = getIndexByOffset(file.lexer.tokens, offset - 1);
			const token = file.lexer.tokens[index - 1];
			if (token.type === TokenType.MULTILINE_STRING) return [];
		}

		//check if `$VARIABLE.get()` or `CLASS.METHOD`
		if (arg.context?.triggerCharacter == "." && doc && file) {
			const offset = doc.offsetAt(arg.position);
			const index = getIndexByOffset(file.lexer.tokens, offset - 1);
			const token = file.lexer.tokens[index - 1];
			if (token && token.type == TokenType.VARIABLE) {
				return [
					{
						label: "get",
						kind: vscode.CompletionItemKind.Function,
						insertText: "get()",
					},
				];
			} else if (token && token.type == TokenType.LITERAL) {
				const classResult = BuiltInFunctions.find(
					(v) => v.class == token.value
				);
				if (classResult) {
					return classResult.methods.map((v) => {
						return {
							label: v.name,
							kind: vscode.CompletionItemKind.Function,
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
							const query = await HirarchyHelper.getHirarchy(
								oFuncs,
								splited
							);
							if (query) {
								const cls = query.classes.map(
									(v): vscode.CompletionItem => {
										return {
											label: v,
											kind: vscode.CompletionItemKind
												.Class,
										};
									}
								);
								const funcs = query.funcs.map(
									(v): vscode.CompletionItem => {
										return {
											label: v,
											kind: vscode.CompletionItemKind
												.Function,
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

		//keywords
		const keywords: vscode.CompletionItem[] = [
			{
				label: "import",
				kind: vscode.CompletionItemKind.Keyword,
			},
			{
				label: "class",
				kind: vscode.CompletionItemKind.Keyword,
			},
			{
				label: "function",
				kind: vscode.CompletionItemKind.Keyword,
			},
			{
				label: "new",
				kind: vscode.CompletionItemKind.Keyword,
			},
		];

		//start commands eg. execute, xp, weather
		const commands: vscode.CompletionItem[] = START_COMMAND.map((v) => ({
			label: v,
			kind: vscode.CompletionItemKind.Keyword,
		}));

		//variables
		const vars: vscode.CompletionItem[] =
			ExtractedTokensHelper.concatVariableTokens(
				this.extractedTokens
			).map((v) => {
				return {
					label: v.value.slice(1),
					insertText: v.value,
					kind: vscode.CompletionItemKind.Variable,
				};
			});

		//funcs & classes
		const funcs: vscode.CompletionItem[] = cfDatas.funcs.map(
			(v): vscode.CompletionItem => {
				return {
					label: v,
					kind: vscode.CompletionItemKind.Function,
					insertText: `${v}()`,
				};
			}
		);
		const classes: vscode.CompletionItem[] = cfDatas.classes.map(
			(v): vscode.CompletionItem => {
				return {
					label: v,
					kind: vscode.CompletionItemKind.Class,
				};
			}
		);

		//macros
		const mos: vscode.CompletionItem[] = this.macros.map((v) => {
			return {
				label: v.target,
				kind: vscode.CompletionItemKind.Snippet,
				detail: v.values.join(" "),
			};
		});

		//builtin classes
		const builtInClasses: vscode.CompletionItem[] = BuiltInFunctions.map(
			(v) => {
				return {
					label: v.class,
					kind: vscode.CompletionItemKind.Class,
					detail: "builtin functions provided by JMC",
				};
			}
		);

		return vars
			.concat(keywords)
			.concat(commands)
			.concat(funcs)
			.concat(classes)
			.concat(mos)
			.concat(builtInClasses);
	}
	//#endregion
}
