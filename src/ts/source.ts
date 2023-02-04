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
import { BuiltInFunctions } from "./data/builtinFunctions";
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
		let path =
			vscode.window.activeTextEditor.document.uri.fsPath.split("\\");
		path.pop();
		return path.join("/");
	}
}

let client: LanguageClient;

export async function activate(context: ExtensionContext) {
	//setup client
	let clientOptions: LanguageClientOptions = {
		documentSelector: [
			{ scheme: "file", language: "jmc" },
			{ scheme: "file", language: "hjmc" },
		],
		synchronize: {
			fileEvents: workspace.createFileSystemWatcher("**/.clientsrc"),
		},
	};

	//setup server
	let serverModule = context.asAbsolutePath(
		path.join("src", "js", "server.js")
	);
	let debugOptions = { execArgv: ["--nolazy", "--inspect=6009"] };

	let serverOptions: ServerOptions = {
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
				for (let i of BuiltInFunctions) {
					if (linePrefix.endsWith(`${i.class}.`)) {
						var methods: vscode.CompletionItem[] = [];
						for (let method of i.methods) {
							methods.push(
								new vscode.CompletionItem(
									method,
									vscode.CompletionItemKind.Method
								)
							);
						}
						return methods;
					}
				}
			},
		},
		"."
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
				let headers: vscode.CompletionItem[] = [];
				for (let i of HEADERS) {
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
					let path = document.uri.fsPath.split("\\");
					path.pop();
					let folder = path.join("/");

					let files: string[] = await getAllFiles(folder).toArray();
					let items: vscode.CompletionItem[] = [];
					for (let i of files) {
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
					let items: vscode.CompletionItem[] = [];
					for (let i of JSON_FILE_TYPES) {
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
				var text = document.getText();

				// let scoreboardPattern = /(.+):(@[parse])/g;
				let m: RegExpExecArray | null;
				let variables = getVariablesClient(text);
				for (let variable of variables) {
					let pattern = RegExp(`\\\$${variable}\\b`, "g");
					while ((m = pattern.exec(text)) !== null) {
						var pos = getLineByIndex(m.index, getLinePos(text));
						var lineText = document.lineAt(pos.line).text.trim();

						if (!lineText.startsWith("//")) {
							builder.push(
								new vscode.Range(
									new vscode.Position(pos.line, pos.pos),
									new vscode.Position(
										pos.line,
										pos.pos + m[0].length
									)
								),
								"variable",
								["declaration"]
							);
						}
					}
				}

				let functions = getFunctionsClient(text);
				for (let func of functions) {
					let pattern = RegExp(`\\b${func}\\b`, "g");
					while ((m = pattern.exec(text)) !== null) {
						var pos = getLineByIndex(m.index, getLinePos(text));
						var lineText = document.lineAt(pos.line).text.trim();

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

				let classes = getClassesClient(text);
				for (let clas of classes) {
					let pattern = RegExp(`\\b(${clas})\\b`, "g");
					while ((m = pattern.exec(text)) !== null) {
						var pos = getLineByIndex(m.index, getLinePos(text));
						var lineText = document.lineAt(pos.line).text.trim();

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
