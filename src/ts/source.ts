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
import { Headers } from "./data/common";

const selector: DocumentSelector = {
	language: "jmc",
	scheme: "file",
};

const headerSelector: DocumentSelector = {
	language: "hjmc",
	scheme: "file",	
}

let client: LanguageClient;

export async function activate(context: ExtensionContext) {
	//setup client
	let clientOptions: LanguageClientOptions = {
		documentSelector: [{ scheme: "file", language: "jmc" },{ scheme: "file", language: "hjmc" }],
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
				for (let i of Headers) {
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

	//TODO: add file import completion for files
	// const fileImportCompletion = languages.registerCompletionItemProvider(
	// 	selector,
	// 	{
	// 		provideCompletionItems(document, position, token, c) {

	// 		}
	// 	},
	// 	"\""
	// )

	client.start();
}

export function deactivate(): Thenable<void> | undefined {
	if (!client) {
		return undefined;
	}
	return client.stop();
}
