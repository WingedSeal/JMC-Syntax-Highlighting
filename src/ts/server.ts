import {
	createConnection,
	TextDocuments,
	Diagnostic,
	ProposedFeatures,
	InitializeParams,
	DidChangeConfigurationNotification,
	CompletionItem,
	CompletionItemKind,
	TextDocumentPositionParams,
	TextDocumentSyncKind,
	InitializeResult,
	HandlerResult,
	SignatureHelp,
	SignatureHelpParams,
} from "vscode-languageserver/node";
import { TextDocument } from "vscode-languageserver-textdocument";
import { BuiltInFunctions, methodInfoToDoc } from "./data/builtinFunctions";
import {
	KEYWORDS as Keywords,
	VANILLA_COMMANDS,
	getCurrentFolder,
} from "./data/common";
import { getDiagnostics } from "./diagnostics";
import * as url from "url";
import {
	getCurrentCommand,
	getVariables,
} from "./helpers/documentAnalyze";

const connection = createConnection(ProposedFeatures.all);
let text: string;

export let userVariables: CompletionItem[] = [];
export let userFunctions: CompletionItem[] = [];

const documents: TextDocuments<TextDocument> = new TextDocuments(TextDocument);

let hasConfigurationCapability = false;
let hasWorkspaceFolderCapability = false;
let hasDiagnosticRelatedInformationCapability = false;

connection.onInitialize((params: InitializeParams) => {
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

interface ValidateData {
	variables: CompletionItem[];
	functions: CompletionItem[];
}

async function validateText(text: string, path: string): Promise<ValidateData> {
	let m: RegExpExecArray | null;

	const variables: CompletionItem[] = [];
	for (const variable of getVariables(text, getCurrentFolder(path))) {
		const filter = variables.filter((v) => v.label == variable);
		if (!(filter.length > 0)) {
			variables.push({
				label: variable,
				kind: CompletionItemKind.Variable,
			});
		}
	}

	const functions: CompletionItem[] = [];
	const functionPattern = /function\s*([\w\.]+)/g;
	while ((m = functionPattern.exec(text))) {
		const filter = functions.filter((v) => v.label == m![1]);
		if (!(filter.length > 0)) {
			functions.push({ label: m[1], kind: CompletionItemKind.Function });
		}
	}

	return {
		variables: variables,
		functions: functions,
	};
}

async function validateTextDocument(textDocument: TextDocument): Promise<void> {
	const settings = await getDocumentSettings(textDocument.uri);

	text = textDocument.getText();

	const data = await validateText(text, url.fileURLToPath(textDocument.uri));
	userVariables = data.variables;
	userFunctions = data.functions;

	const diagnostics: Diagnostic[] = getDiagnostics(
		text,
		url.fileURLToPath(textDocument.uri)
	);
	connection.sendDiagnostics({ uri: textDocument.uri, diagnostics });
}

connection.onDidChangeWatchedFiles((_change) => {
	connection.console.log("We received a file change event");
});

connection.onCompletion(
	async (pos: TextDocumentPositionParams): Promise<CompletionItem[]> => {
		const builtinFunctionsName = BuiltInFunctions.map((v) => ({
			name: v.class,
		}));
		const items: CompletionItem[] = [];
		let num = 0;

		for (const i of builtinFunctionsName) {
			items.push({
				label: i.name,
				kind: CompletionItemKind.Class,
				data: num,
			});
			num++;
		}
		for (const i of userVariables) {
			i.data = num;
			items.push(i);
			num++;
		}
		for (const i of userFunctions) {
			i.data = num;
			items.push(i);
			num++;
		}
		for (const i of Keywords) {
			items.push({
				label: i,
				kind: CompletionItemKind.Keyword,
				data: num,
			});
			num++;
		}
		for (const i of VANILLA_COMMANDS) {
			items.push({
				label: i,
				kind: CompletionItemKind.Keyword,
				data: num,
			});
			num++;
		}
		return items;
	}
);

connection.onCompletionResolve((item: CompletionItem): CompletionItem => {
	return item;
});

connection.onSignatureHelp(
	(
		v: SignatureHelpParams
	): HandlerResult<SignatureHelp | null | undefined, void> => {
		const document = documents.get(v.textDocument.uri);
		if (document !== undefined) {
			const index = document.offsetAt(v.position);
			const text = document.getText();

			const command = getCurrentCommand(document.getText(), index);
			const commaCount = command.match(/,/g || [])?.length;
			// while ((index -= 1) !== -1) {
			// 	let char = text[index];
			// 	if (char === "(") {
			// 		break;
			// 	}
			// 	if (char === ",") {
			// 		commaCount += 1;
			// 	}
			// }

			if (v.context?.triggerCharacter === ",") {
				if (
					v.context.activeSignatureHelp !== undefined &&
					v.context.activeSignatureHelp.activeParameter !== undefined
				) {
					v.context.activeSignatureHelp.activeParameter = commaCount;
				} else {
					const pattern = /(\w+)\.(\w+)\(([\w\s,()$]*)\)/g;
					let m: RegExpExecArray | null;
					console.log(command);
					while ((m = pattern.exec(command)) !== null) {
						const func = m[1];
						const method = m[2];

						const methods = BuiltInFunctions.flatMap((v) => {
							const target = v.methods.filter((value) => {
								return (
									func === v.class && method === value.name
								);
							});
							return target;
						});
						const target = methods[0];

						v.context.activeSignatureHelp = {
							signatures: [
								{
									label: methodInfoToDoc(target),
									parameters: target.args.flatMap((v) => {
										const def =
											v.default !== undefined
												? ` = ${v.default}`
												: "";
										const arg = `${v.name}: ${v.type}${def}`;
										return {
											label: arg,
										};
									}),
								},
							],
							activeSignature: 0,
							activeParameter: commaCount,
						};
					}
				}
			}
		}
		return v.context?.activeSignatureHelp;
	}
);

documents.listen(connection);
connection.listen();
