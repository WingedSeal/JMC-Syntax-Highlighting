import { workspace, ExtensionContext, languages } from "vscode";
import {
	LanguageClient,
	LanguageClientOptions,
	ServerOptions,
	TransportKind,
} from "vscode-languageclient/node";
import * as path from "path";
import { JMC_SELECTOR } from "../data/selectors";
import { semanticProvider, semanticLegend } from "./semanticHighlight";
import { definationProvider } from "./defination";
import * as vscode from "vscode";
import { HJMCFile, JMCFile } from "../helpers/general";
import { Lexer } from "../lexer";
import { HeaderParser } from "../parseHeader";
import { variableCompletion } from "./completion";

export let client: LanguageClient;

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

	//file system provider
	const jmcfsProvider = vscode.workspace.createFileSystemWatcher(
		"**/*.{jmc,hjmc}",
		false,
		false,
		false
	);
	jmcfsProvider.onDidCreate(async (v) => {
		if (v.fsPath.endsWith(".jmc")) {
			const file: JMCFile = {
				lexer: new Lexer(""),
				path: v.fsPath,
			};
			await client.sendRequest("file/add", file);
		} else if (v.fsPath.endsWith(".hjmc")) {
			const file: HJMCFile = {
				parser: new HeaderParser(""),
				path: v.fsPath,
			};
			await client.sendRequest("file/add", file);
		}
	});
	jmcfsProvider.onDidChange(async (v) => {
		await client.sendRequest("file/update", v.fsPath);
	});
	jmcfsProvider.onDidDelete(async (v) => {
		await client.sendRequest("file/remove", v.fsPath);
	});

	//languages settings
	languages.registerDocumentSemanticTokensProvider(
		JMC_SELECTOR,
		semanticProvider,
		semanticLegend
	);
	languages.registerDefinitionProvider(
		JMC_SELECTOR,
		new definationProvider()
	);
	languages.registerCompletionItemProvider(
		JMC_SELECTOR,
		new variableCompletion(),
		"."
	);

	//define client
	client = new LanguageClient("jmc", "JMC", serverOptions, clientOptions);

	client.start().then(() => {
		console.log("Client Started");
	});
}

export function deactivate(): Thenable<void> | undefined {
	if (!client) {
		return undefined;
	}
	return client.stop();
}
