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
import { getAllFiles } from "get-all-files";

const selector: DocumentSelector = {
	language: "jmc",
	scheme: "file",
};

const headerSelector: DocumentSelector = {
	language: "hjmc",
	scheme: "file",
};

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

	client.start();
}

export function deactivate(): Thenable<void> | undefined {
	if (!client) {
		return undefined;
	}
	return client.stop();
}
