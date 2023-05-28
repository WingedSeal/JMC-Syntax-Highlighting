import { workspace, ExtensionContext } from "vscode";
import {
	ConfigurationItem,
	ConfigurationParams,
	LanguageClient,
	LanguageClientOptions,
	ServerOptions,
	TransportKind,
} from "vscode-languageclient/node";
import * as path from "path";
import * as vscode from "vscode";
import { Settings } from "../helpers/general";

export let client: LanguageClient;

export async function activate(context: ExtensionContext) {
	//setup config
	const config = vscode.workspace.getConfiguration("jmc");

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
		path.join("src", "js", "server", "server.js")
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

	const compileCommand = vscode.commands.registerCommand(
		"jmc.compileCode",
		() => {
			const editor = vscode.window.activeTextEditor;
			if (editor) {
				const doc = editor.document;
				const exePath: string = vscode.workspace
					.getConfiguration("jmc", doc.uri)
					.get("executable") as string;
				if (exePath == "") {
					vscode.window.showErrorMessage(
						"Setting up the executable before compile it!"
					);
					return;
				}
				const terminal = vscode.window.createTerminal(
					"JMC Compile",
					undefined
				);
				terminal.show();
				terminal.sendText(`&\"${exePath}\" compile`);
				terminal.hide();
			}
		}
	);

	context.subscriptions.push(compileCommand);

	//define client
	client = new LanguageClient("jmc", "JMC", serverOptions, clientOptions);

	client.start().then(async () => {
		console.log("Client Started");
	});
}

export function deactivate(): Thenable<void> | undefined {
	if (!client) {
		return undefined;
	}
	return client.stop();
}
