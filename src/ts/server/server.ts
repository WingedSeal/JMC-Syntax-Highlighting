import {
	createConnection,
	TextDocuments,
	ProposedFeatures,
	InitializeParams,
	DidChangeConfigurationNotification,
	CompletionItem, TextDocumentSyncKind,
	InitializeResult
} from "vscode-languageserver/node";
import { TextDocument } from "vscode-languageserver-textdocument";
import * as get_files from "get-all-files";
import * as url from "url";
import { Lexer } from "../lexer";

let jmcConfigs: string[] = [];

//#region default
const connection = createConnection(ProposedFeatures.all);
const documents: TextDocuments<TextDocument> = new TextDocuments(TextDocument);

let hasConfigurationCapability = false;
let hasWorkspaceFolderCapability = false;
let hasDiagnosticRelatedInformationCapability = false;
//#endregion
connection.onInitialize((params: InitializeParams) => {
	if (params.workspaceFolders) {
		for (const folder of params.workspaceFolders) {
			const files = get_files
				.getAllFilesSync(url.fileURLToPath(folder.uri))
				.toArray();
			jmcConfigs = jmcConfigs.concat(
				files.filter((v) => v.endsWith("jmc_config.json"))
			);
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

	const result: InitializeResult = {
		capabilities: {
			textDocumentSync: TextDocumentSyncKind.Incremental,
			// Tell the client that this server supports code completion.
			completionProvider: {
				resolveProvider: true,
			},
			signatureHelpProvider: {
				triggerCharacters: ["("],
			},
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
async function validateText(fileText: string) {
	const lexer = new Lexer(fileText);

	connection.sendNotification("data/currentFile", lexer);
}

async function validateTextDocument(textDocument: TextDocument): Promise<void> {
	const settings = await getDocumentSettings(textDocument.uri);
	await validateText(textDocument.getText());
}

connection.onDefinition((v) => {
	return [];
});

connection.onDidChangeWatchedFiles((_change) => {
	connection.console.log("We received a file change event");
});

connection.onCompletion(
	async (arg, token, progress, result): Promise<CompletionItem[]> => {
		return [];
	}
);

connection.onCompletionResolve((item: CompletionItem): CompletionItem => {
	return item;
});

connection.onSignatureHelp((handler) => {
	return handler.context?.activeSignatureHelp;
});

documents.listen(connection);
connection.listen();
