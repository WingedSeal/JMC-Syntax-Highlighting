import {
	workspace,
	ExtensionContext,
	languages,
	DocumentSelector,
} from "vscode";
import {
	LanguageClient,
	LanguageClientOptions,
	ServerOptions,
	TransportKind,
} from "vscode-languageclient/node";
import * as path from "path";
import { BuiltInFunctions, methodInfoToDoc } from "./data/builtinFunctions";
import * as vscode from "vscode";
import { HEADERS, HeaderData, JSON_FILE_TYPES } from "./data/common";
import { getAllFiles } from "get-all-files";
import { semanticLegend } from "./semanticHighlight";
import { getCurrentCommand } from "./helpers/documentHelper";
import { COMMANDS, Command, ValueType } from "./data/vanillaCommands";
import { Language, TokenType } from "./helpers/lexer";
import { getLogger, initLogger } from "./helpers/logger";
import {
	ATTRIBUTE_LIST,
	BLOCKS_ID,
	ITEMS_ID,
	SELECTORS,
} from "./data/staticData";

interface ClassesMethods {
	name: string;
	methods: string[];
}

const selector: DocumentSelector = {
	language: "jmc",
	scheme: "file",
};

const headerSelector: DocumentSelector = {
	language: "hjmc",
	scheme: "file",
};

let client: LanguageClient;
let classesMethods: ClassesMethods[] | undefined;
let mainHeader: HeaderData[] = [];

export async function activate(context: ExtensionContext) {
	await initLogger(context);
	getLogger().info("setup done");
	//setup client
	const clientOptions: LanguageClientOptions = {
		documentSelector: [
			{ scheme: "file", language: "jmc" },
			{ scheme: "file", language: "hjmc" },
		],
		synchronize: {
			fileEvents: workspace.createFileSystemWatcher("**/.clientsrc"),
		},
	};

	//setup server
	const serverModule = context.asAbsolutePath(
		path.join("src", "js", "server.js")
	);
	const debugOptions = { execArgv: ["--nolazy", "--inspect=6009"] };

	const serverOptions: ServerOptions = {
		run: { module: serverModule, transport: TransportKind.ipc },
		debug: {
			module: serverModule,
			transport: TransportKind.ipc,
			options: debugOptions,
		},
	};

	//define client
	client = new LanguageClient("jmc", "JMC", serverOptions, clientOptions);

	//register compltion item
	const builtinFunctionsCompletion = languages.registerCompletionItemProvider(
		selector,
		{
			async provideCompletionItems(document, position, token, context) {
				const linePrefix = await getCurrentCommand(
					document.getText(),
					document.offsetAt(position)
				);
				for (const i of BuiltInFunctions) {
					if (linePrefix.endsWith(`${i.class}.`)) {
						const methods: vscode.CompletionItem[] = [];
						for (const method of i.methods) {
							const item = new vscode.CompletionItem(
								method.name,
								vscode.CompletionItemKind.Method
							);
							methods.push(item);
						}
						return methods;
					}
				}
			},
		},
		"."
	);

	const classMethodsCompletion = languages.registerCompletionItemProvider(
		selector,
		{
			async provideCompletionItems(document, position, token, context) {
				if (classesMethods === undefined) return;
				const linePrefix = await getCurrentCommand(
					document.getText(),
					document.offsetAt(position)
				);
				for (const item of classesMethods) {
					const cItems: vscode.CompletionItem[] = [];
					if (linePrefix.endsWith(`${item.name}.`)) {
						item.methods.forEach((v) => {
							cItems.push({
								label: v,
								kind: vscode.CompletionItemKind.Function,
							});
						});
						return cItems;
					}
				}
				return undefined;
			},
		},
		"."
	);

	const signatureHelper = languages.registerSignatureHelpProvider(
		selector,
		{
			async provideSignatureHelp(document, position, token, ctx) {
				const linePrefix = await getCurrentCommand(
					document.getText(),
					document.offsetAt(position)
				);
				if (ctx.triggerCharacter === "(") {
					const methods = BuiltInFunctions.flatMap((v) => {
						const target = v.methods.filter((value) => {
							return linePrefix.endsWith(
								`${v.class}.${value.name}(`
							);
						});
						return target;
					});
					const method = methods[0];

					return {
						signatures: [
							{
								label: methodInfoToDoc(method),
								parameters: method.args.flatMap((v) => {
									const def =
										v.default !== undefined
											? ` = ${v.default}`
											: "";
									const arg = `${v.name}: ${v.type}${def}`;
									return {
										label: arg,
										documentation: v.doc,
									};
								}),
								documentation: method.doc,
							},
						],
						activeSignature: 0,
						activeParameter: 0,
					};
				}
				return undefined;
			},
		},
		"(",
		","
	);

	const variablesFunctionsCompletion =
		languages.registerCompletionItemProvider(
			selector,
			{
				async provideCompletionItems(
					document,
					position,
					token,
					context
				) {
					const linePrefix = await getCurrentCommand(
						document.getText(),
						document.offsetAt(position)
					);
					if (/\$(\w+)\./g.test(linePrefix)) {
						return [
							{
								label: "get",
								kind: vscode.CompletionItemKind.Method,
							},
						];
					}
					return undefined;
				},
			},
			"."
		);

	const headerCompletion = languages.registerCompletionItemProvider(
		headerSelector,
		{
			provideCompletionItems(document, position, token, c) {
				const headers: vscode.CompletionItem[] = [];
				for (const i of HEADERS) {
					headers.push({
						label: i,
						kind: vscode.CompletionItemKind.Keyword,
					});
				}
				return headers;
			},
		},
		"#"
	);

	const importCompletion = languages.registerCompletionItemProvider(
		selector,
		{
			provideCompletionItems(document, position, token, c) {
				return [
					{
						label: "import",
						kind: vscode.CompletionItemKind.Keyword,
					},
				];
			},
		},
		"@"
	);

	const fileImportCompletion = languages.registerCompletionItemProvider(
		selector,
		{
			async provideCompletionItems(document, position, token, c) {
				const linePrefix = await getCurrentCommand(
					document.getText(),
					document.offsetAt(position)
				);
				if (linePrefix.endsWith("@import ")) {
					const path = document.uri.fsPath.split("\\");
					path.pop();
					const folder = path.join("/");

					const files: string[] = await getAllFiles(folder).toArray();
					const items: vscode.CompletionItem[] = [];
					for (const i of files) {
						if (i.endsWith(".jmc")) {
							items.push({
								label: `"${i
									.slice(folder.length + 1)
									.slice(0, -4)}"`,
								kind: vscode.CompletionItemKind.Module,
							});
						}
					}
					return items;
				}
				return undefined;
			},
		},
		" "
	);

	const newKeywordCompletion = languages.registerCompletionItemProvider(
		selector,
		{
			async provideCompletionItems(document, position, token, context) {
				const linePrefix = await getCurrentCommand(
					document.getText(),
					document.offsetAt(position)
				);
				if (linePrefix.endsWith("new ")) {
					const items: vscode.CompletionItem[] = [];
					for (const i of JSON_FILE_TYPES) {
						items.push({
							label: i,
							kind: vscode.CompletionItemKind.Value,
						});
					}
					return items;
				}
				return undefined;
			},
		},
		" "
	);

	// const classMethodCompletion = languages.registerCompletionItemProvider(
	// 	selector,
	// 	{
	// 		async provideCompletionItems(document, position, token, context) {
	// 			const text = document.getText();
	// 			let offset = document.offsetAt(position);
	// 			let t = "";
	// 			while ((offset -= 1) !== -1) {
	// 				const current = text[offset].trim();
	// 				if (current === "") continue;
	// 				else if (SEMI_CHECKCHAR.includes(current)) break;
	// 				else t += current;
	// 			}
	// 			const dotNum = t.split(".").length;
	// 			const func = getFunctionsClient(text);

	// 			const target = func.filter((v) => {
	// 				return v.startsWith(t) && v.split(".").length > dotNum;
	// 			});
	// 			const items: vscode.CompletionItem[] = [];
	// 			for (const i of target) {
	// 				items.push({
	// 					label: i.split(".")[dotNum],
	// 					kind: vscode.CompletionItemKind.Method,
	// 				});
	// 			}
	// 			console.log(items);
	// 			return items;
	// 		},
	// 	},
	// 	"."
	// );

	const vanillaCommandsCompletion = languages.registerCompletionItemProvider(
		selector,
		{
			async provideCompletionItems(document, position, token, context) {
				const text = document.getText();
				const linePrefix = await getCurrentCommand(
					text,
					document.offsetAt(position)
				);
				const data = linePrefix
					.split(/\s/g)
					.map((v) => v.trim())
					.filter((v) => v !== "");
				const current = data.length - 1;
				const commands = COMMANDS.filter((v) => v.command === data[0]);
				const match: Command[] = [];
				for (const command of commands) {
					let num = current;
					(function () {
						if (current === 0) {
							match.push(command);
							return;
						}
						while (num-- !== 0) {
							const arg = command.args[num];
							const currentData = data[num + 1];
							if (arg === undefined) return;
							if (arg.optional) continue;
							switch (arg.type) {
								case ValueType.VECTOR:
									if (
										!/(?:(?:~|\^)(?:\d*(?:\.\d+)?)?)|\d+(?:\.\d+)?/g.test(
											currentData
										)
									) {
										return;
									}
									break;
								case ValueType.KEYWORD:
									if (
										arg.value !== undefined &&
										!arg.value.includes(currentData)
									)
										return;
									break;
								case ValueType.ENUM:
									if (
										arg.value !== undefined &&
										!arg.value.includes(currentData)
									)
										return;

									break;
								case ValueType.SELECTOR:
									if (
										!SELECTORS.includes(
											currentData.slice(0, 1)
										)
									)
										return;

									break;
								case ValueType.NUMBER:
									if (
										!/\d+(?:\.\d+)?|\.\d+/g.test(
											currentData
										)
									)
										return;
									break;
								case ValueType.ADVANCEMENT:
								case ValueType.CRITERION:
								case ValueType.ATTRIBUTE:
								case ValueType.VALUE:
								case ValueType.UUID:
								case ValueType.NAME:
								case ValueType.ID:
								case ValueType.BOOLEANS:
								case ValueType.ITEM:
								case ValueType.BLOCK:
								case ValueType.DIMENSION:
									break;
							}
							if (num === 0) match.push(command);
						}
					})();
				}

				const result = match.map((v) => v.args[current]);
				const items: vscode.CompletionItem[] = [];

				let vector = false;
				let item = false;
				let block = false;
				let booleans = false;
				let attr = false;
				let selector = false;

				for (const arg of result) {
					switch (arg.type) {
						case ValueType.KEYWORD:
							if (arg.value !== undefined) {
								arg.value.forEach((v) => {
									items.push({
										label: v,
										kind: vscode.CompletionItemKind.Keyword,
									});
								});
							}
							break;
						case ValueType.ENUM:
							if (arg.value !== undefined) {
								arg.value.forEach((v) => {
									items.push({
										label: v,
										kind: vscode.CompletionItemKind
											.EnumMember,
									});
								});
							}
							break;
						case ValueType.VECTOR:
							vector = true;
							break;
						case ValueType.ITEM:
							item = true;
							break;
						case ValueType.BLOCK:
							block = true;
							break;
						case ValueType.BOOLEANS:
							booleans = true;
							break;
						case ValueType.ATTRIBUTE:
							attr = true;
							break;
						case ValueType.SELECTOR:
							selector = true;
							break;
						case ValueType.NUMBER:
						case ValueType.ADVANCEMENT:
						case ValueType.CRITERION:
						case ValueType.VALUE:
						case ValueType.UUID:
						case ValueType.NAME:
						case ValueType.ID:
						case ValueType.DIMENSION:
							break;
					}
				}

				if (booleans) {
					items.push({
						label: "true",
						kind: vscode.CompletionItemKind.Keyword,
					});
					items.push({
						label: "false",
						kind: vscode.CompletionItemKind.Keyword,
					});
				}

				if (item) {
					for (const i of ITEMS_ID) {
						items.push({
							label: i,
							kind: vscode.CompletionItemKind.Value,
						});
					}
				}

				if (attr) {
					for (const i of ATTRIBUTE_LIST) {
						items.push({
							label: i,
							kind: vscode.CompletionItemKind.Value,
						});
					}
				}

				if (block) {
					for (const i of BLOCKS_ID) {
						items.push({
							label: i,
							kind: vscode.CompletionItemKind.Value,
						});
					}
				}

				if (vector) {
					items.push({
						label: "^",
						kind: vscode.CompletionItemKind.Struct,
						insertText: new vscode.SnippetString("^${1}"),
					});
					items.push({
						label: "~",
						kind: vscode.CompletionItemKind.Struct,
						insertText: new vscode.SnippetString("~${1}"),
					});
					items.push({
						label: "^ ^ ^",
						kind: vscode.CompletionItemKind.Snippet,
						insertText: new vscode.SnippetString(
							"^${1} ^${2} ^${3}"
						),
					});
					items.push({
						label: "~ ~ ~",
						kind: vscode.CompletionItemKind.Snippet,
						insertText: new vscode.SnippetString(
							"~${1} ~${2} ~${3}"
						),
					});
				}

				if (selector) {
					for (const i of SELECTORS) {
						items.push({
							label: i,
							kind: vscode.CompletionItemKind.Value,
						});
					}
				}

				return items;
			},
		},
		" "
	);

	const semanticHighlight = languages.registerDocumentSemanticTokensProvider(
		selector,
		{
			async provideDocumentSemanticTokens(document, token) {
				const builder = new vscode.SemanticTokensBuilder(
					semanticLegend
				);
				const text = document.getText();
				const language = new Language(text, mainHeader);
				console.log(
					language.tokens.filter((v) => v.type === TokenType.MACRO)
				);
				for (const _var of language.tokens) {
					// const startPos = document.positionAt(_var.offset);
					// const endPos = document.positionAt(_var.offset + _var.length);
					// const range = new vscode.Range(startPos, endPos);
					// builder.push(
					// 	range, "variable", ["declaration"]
					// );
					if (_var.type === TokenType.USE_VARIABLE) {
						const startPos = document.positionAt(_var.offset);
						const endPos = document.positionAt(
							_var.offset + _var.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "variable", ["declaration"]);
					} else if (_var.type === TokenType.MACRO) {
						const startPos = document.positionAt(_var.offset);
						const endPos = document.positionAt(
							_var.offset + _var.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "macro", ["declaration"]);
					} else if (_var.type === TokenType.VARIABLE) {
						const startPos = document.positionAt(_var.offset);
						const endPos = document.positionAt(
							_var.offset + _var.value![0].length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "variable", ["declaration"]);
					} else if (_var.type === TokenType.FUNCTION) {
						const startPos = document.positionAt(_var.offset + 9);
						const endPos = document.positionAt(
							_var.offset + _var.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "function", ["declaration"]);
					} else if (_var.type === TokenType.CLASS) {
						const startPos = document.positionAt(_var.offset + 6);
						const endPos = document.positionAt(
							_var.offset + _var.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "class", ["declaration"]);
					} else if (_var.type === TokenType.CALL_FUNCTION) {
						if (
							_var.value !== undefined &&
							_var.value.length === 1
						) {
							const startPos = document.positionAt(_var.offset);
							const endPos = document.positionAt(
								_var.offset + _var.value[0].length
							);
							const range = new vscode.Range(startPos, endPos);
							builder.push(range, "function", ["declaration"]);
						} else if (
							_var.value !== undefined &&
							_var.value.length === 2
						) {
							const startPos = document.positionAt(_var.offset);
							const endPos = document.positionAt(
								_var.offset + _var.value[0].length
							);
							const range = new vscode.Range(startPos, endPos);
							builder.push(range, "class", ["declaration"]);

							const startPos2 = document.positionAt(
								_var.offset + _var.value[0].length + 1
							);
							const endPos2 = document.positionAt(
								_var.offset +
									_var.value[0].length +
									1 +
									_var.value[1].length
							);
							const range2 = new vscode.Range(startPos2, endPos2);
							builder.push(range2, "function", ["declaration"]);
						} else if (
							_var.value !== undefined &&
							_var.value.length > 2
						) {
							const startPos = document.positionAt(_var.offset);
							const endPos = document.positionAt(
								_var.offset + _var.value[0].length
							);
							const range = new vscode.Range(startPos, endPos);
							builder.push(range, "class", ["declaration"]);

							let pos = _var.offset + _var.value[0].length + 1;
							for (const v of _var.value.slice(
								1,
								_var.value.length - 2
							)) {
								const startPos = document.positionAt(pos);
								const endPos = document.positionAt(
									pos + v.length
								);
								const range = new vscode.Range(
									startPos,
									endPos
								);
								builder.push(range, "class", ["declaration"]);
								pos += v.length + 1;
							}

							const startPos2 = document.positionAt(pos);
							const endPos2 = document.positionAt(
								pos + _var.value[_var.value.length - 1].length
							);
							const range2 = new vscode.Range(startPos2, endPos2);
							builder.push(range2, "function", ["declaration"]);
						}
					}
				}
				return builder.build();
			},
		},
		semanticLegend
	);

	client.start();

	client.onNotification("data/classesData", (datas: ClassesMethods[]) => {
		classesMethods = datas;
	});

	client.onNotification("data/mainHeader", (data: HeaderData[]) => {
		mainHeader = data;
	});
}

export function deactivate(): Thenable<void> | undefined {
	if (!client) {
		return undefined;
	}
	return client.stop();
}
