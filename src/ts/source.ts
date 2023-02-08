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
import { HEADERS, JSON_FILE_TYPES } from "./data/common";
import { getAllFiles } from "get-all-files";
import {
	getClassClient as getClassesClient,
	getFunctionsClient,
	getVariablesClient,
	semanticLegend,
} from "./semanticHighlight";
import { getLineByIndex, getLinePos } from "./helpers/documentHelper";

const selector: DocumentSelector = {
	language: "jmc",
	scheme: "file",
};

const headerSelector: DocumentSelector = {
	language: "hjmc",
	scheme: "file",
};

export function getCurrentFile(): string | undefined {
	if (vscode.window.activeTextEditor !== undefined) {
		const path =
			vscode.window.activeTextEditor.document.uri.fsPath.split("\\");
		path.pop();
		return path.join("/");
	}
}

let client: LanguageClient;

export async function activate(context: ExtensionContext) {
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
			provideCompletionItems(document, position, token, context) {
				const linePrefix = document
					.lineAt(position)
					.text.substring(0, position.character);
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

	//TODO:
	const signatureHelper = languages.registerSignatureHelpProvider(
		selector,
		{
			async provideSignatureHelp(document, position, token, ctx) {
				const linePrefix = document
					.lineAt(position)
					.text.substring(0, position.character);
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
										documentation: v.doc
									};
								}),
								documentation: method.doc
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
				provideCompletionItems(document, position, token, context) {
					const linePrefix = document
						.lineAt(position)
						.text.substring(0, position.character);
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
				const linePrefix = document
					.lineAt(position)
					.text.substring(0, position.character);
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
				const linePrefix = document
					.lineAt(position)
					.text.substring(0, position.character);
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

	// const vanillaCommandsCompletion = languages.registerCompletionItemProvider(
	// 	selector,
	// 	{
	// 		async provideCompletionItems(document, position, token, context) {
	// 			const linePrefix = document
	// 				.lineAt(position)
	// 				.text.substring(0, position.character);
	// 			let items: vscode.CompletionItem[] = [];

	// 			for (let command of CommandArguments) {
	// 				let current: number =
	// 					linePrefix.split(" ").filter((v) => v !== "").length -
	// 					1;
	// 				let matchString: string = "";
	// 				let args = command.args[current];
	// 				for (let arg of args) {
	// 					items.push({
	// 						label: arg,
	// 						kind: vscode.CompletionItemKind.Property,
	// 					});
	// 				}
	// 			}

	// 			return items;
	// 		},
	// 	},
	// 	" "
	// );

	//TODO: add it for vanilla commands
	const semanticHighlight = languages.registerDocumentSemanticTokensProvider(
		selector,
		{
			provideDocumentSemanticTokens(document, token) {
				const builder = new vscode.SemanticTokensBuilder(
					semanticLegend
				);
				const text = document.getText();

				// let scoreboardPattern = /(.+):(@[parse])/g;
				let m: RegExpExecArray | null;
				const variables = getVariablesClient(text);
				for (const variable of variables) {
					const pattern = RegExp(`(\\\$${variable})(\.get)?\\b`, "g");
					while ((m = pattern.exec(text)) !== null) {
						const pos = getLineByIndex(m.index, getLinePos(text));
						const lineText = document.lineAt(pos.line).text.trim();

						if (!lineText.startsWith("//")) {
							builder.push(
								new vscode.Range(
									new vscode.Position(pos.line, pos.pos),
									new vscode.Position(
										pos.line,
										pos.pos + m[1].length
									)
								),
								"variable",
								["declaration"]
							);

							if (m[2] !== undefined) {
								builder.push(
									new vscode.Range(
										new vscode.Position(pos.line, pos.pos + m[1].length),
										new vscode.Position(
											pos.line,
											pos.pos + m[1].length + m[0].length
										)
									),
									"function",
									["declaration"]
								);								
							}
						}
					}				
				}

				const functions = getFunctionsClient(text);
				for (const func of functions) {
					const pattern = RegExp(`\\b${func}\\b`, "g");
					while ((m = pattern.exec(text)) !== null) {
						const pos = getLineByIndex(m.index, getLinePos(text));
						const lineText = document.lineAt(pos.line).text.trim();

						if (!lineText.startsWith("//")) {
							builder.push(
								new vscode.Range(
									new vscode.Position(pos.line, pos.pos),
									new vscode.Position(
										pos.line,
										pos.pos + m[0].length
									)
								),
								"function",
								["declaration"]
							);
						}
					}
				}

				const classes = getClassesClient(text);
				for (const clas of classes) {
					const pattern = RegExp(`\\b(${clas})\\b`, "g");
					while ((m = pattern.exec(text)) !== null) {
						const pos = getLineByIndex(m.index, getLinePos(text));
						const lineText = document.lineAt(pos.line).text.trim();

						if (!lineText.startsWith("//")) {
							builder.push(
								new vscode.Range(
									new vscode.Position(pos.line, pos.pos),
									new vscode.Position(
										pos.line,
										pos.pos + m[1].length
									)
								),
								"class",
								["declaration"]
							);
						}
					}
				}

				// let pattern = RegExp(`\\\$${variable}\\b`,'g');
				// while ((m = variablePattern.exec(text)) !== null) {
				// 	let variables = getUnusedVariables(text, getCurrentFile());

				// 	if (variables.includes(m[1])) {
				// 		var pos = getLineByIndex(m.index, getLinePos(text));
				// 		builder.push(
				// 			new vscode.Range(
				// 				new vscode.Position(pos.line, pos.pos),
				// 				new vscode.Position(pos.line, pos.pos + m[0].length)
				// 			),
				// 			'variable',
				// 			['declaration']
				// 		)
				// 	}
				// }

				// while ((m = scoreboardPattern.exec(text)) !== null) {
				// 	var pos = getLineByIndex(m.index, getLinePos(text));
				// 	builder.push(
				// 		new vscode.Range(
				// 			new vscode.Position(pos.line, pos.pos),
				// 			new vscode.Position(pos.line, pos.pos + m[0].length)
				// 		),
				// 		'variable',
				// 		['declaration']
				// 	)
				// }
				return builder.build();
			},
		},
		semanticLegend
	);

	client.start();
}

export function deactivate(): Thenable<void> | undefined {
	if (!client) {
		return undefined;
	}
	return client.stop();
}
