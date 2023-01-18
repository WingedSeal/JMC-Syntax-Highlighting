import {
    createConnection,
    TextDocuments,
    Diagnostic,
    DiagnosticSeverity,
    ProposedFeatures,
    InitializeParams,
    DidChangeConfigurationNotification,
    CompletionItem,
    CompletionItemKind,
    TextDocumentPositionParams,
    TextDocumentSyncKind,
    InitializeResult,
} from "vscode-languageserver/node";
import { TextDocument } from "vscode-languageserver-textdocument";
import { BuiltInFunctions } from "./data/builtinFunctions";
import { keywords as Keywords } from "./data/common";
import { SnippetString } from "vscode";

let connection = createConnection(ProposedFeatures.all);
let text: string;

export var userVariables: CompletionItem[] = [];
export var userFunctions: CompletionItem[] = [];

let documents: TextDocuments<TextDocument> = new TextDocuments(TextDocument);

let hasConfigurationCapability: boolean = false;
let hasWorkspaceFolderCapability: boolean = false;
let hasDiagnosticRelatedInformationCapability: boolean = false;

connection.onInitialize((params: InitializeParams) => {
    let capabilities = params.capabilities;

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

let documentSettings: Map<string, Thenable<ServerSettings>> = new Map();

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

async function validateTextDocument(textDocument: TextDocument): Promise<void> {
    let settings = await getDocumentSettings(textDocument.uri);

    text = textDocument.getText();
    let m: RegExpExecArray | null;

    let variables: CompletionItem[] = [];
    let variablePattern = /(\$[\w\.]+)/g;
    while ((m = variablePattern.exec(text))) {
        if (m![0].slice(-4) === ".get") {
            continue;
        }
        let filter = variables.filter(v => v.label == m![0].slice(1));
        if (!(filter.length > 0)) {
            variables.push({label: m[0].slice(1), kind: CompletionItemKind.Variable});
        }
    }    

    let functions: CompletionItem[] = [];
    let functionPattern = /function\s*([\w\.]+)/g;
    while ((m = functionPattern.exec(text))) { 
        let filter = functions.filter(v => v.label == m![1]);
        if (!(filter.length > 0)) {
            variables.push({label: m[1], kind: CompletionItemKind.Function});
        }
    }

    userVariables = variables;
    userFunctions = functions;


    //TODO: problems finder 
    let pattern = /\b[A-Z]{2,}\b/g;
    
    let problems = 0;
    let diagnostics: Diagnostic[] = [];
    while ((m = pattern.exec(text)) && problems < 100) {
        problems++;
    }

    connection.sendDiagnostics({ uri: textDocument.uri, diagnostics });
}

connection.onDidChangeWatchedFiles((_change) => {
    connection.console.log("We received a file change event");
});

connection.onCompletion(
    async (_textDocumentPosition: TextDocumentPositionParams): Promise<CompletionItem[]> => {
        var builtinFunctionsName = BuiltInFunctions.map((v) => ({name: v.class}));
        var items: CompletionItem[] = [];
        var num = 0;
        for (let i of builtinFunctionsName) {
            items.push({label: i.name, kind: CompletionItemKind.Class, data: num});
            num++;
        }
        for (let i of userVariables) {
            i.data = num;
            items.push(i);
            num++;
        }
        for (let i of userFunctions) {
            i.data = num;
            items.push(i);
            num++;
        }        
        for (let i of Keywords) {
            items.push({label: i, kind: CompletionItemKind.Keyword});
            num++;
        }
        return items;
    }
);

connection.onCompletionResolve((item: CompletionItem): CompletionItem => { 
    return item;
});

documents.listen(connection);
connection.listen();