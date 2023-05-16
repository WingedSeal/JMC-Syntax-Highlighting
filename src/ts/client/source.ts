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

	languages.registerDocumentSemanticTokensProvider(
		JMC_SELECTOR,
		semanticProvider,
		semanticLegend
	);

	languages.registerDefinitionProvider(
		JMC_SELECTOR,
		new definationProvider()
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
