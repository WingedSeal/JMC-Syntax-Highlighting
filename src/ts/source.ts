import { workspace, ExtensionContext, languages, DocumentSelector, LanguageConfiguration } from "vscode";
import {
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    TransportKind,
} from "vscode-languageclient/node";
import * as path from "path";
import { BuiltInFunctions } from "./data/builtinFunctions";
import * as vscode from "vscode";


const selector: DocumentSelector = {
    language: 'jmc',
    scheme: 'file'
}
let client: LanguageClient;

export async function activate(context: ExtensionContext) {
    //setup client
    let clientOptions: LanguageClientOptions = {
        documentSelector: [{scheme: "file", language: "jmc"}],
        synchronize: {
            fileEvents: workspace.createFileSystemWatcher("**/.clientsrc")
        }
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
    client = new LanguageClient(
        "jmc",
        "Testing",
        serverOptions,
        clientOptions
    )

    const builtinFunctionsCompletion = languages.registerCompletionItemProvider(
        selector,
        {
            provideCompletionItems(document, position, token, context) {
                const linePrefix = document.lineAt(position).text.substring(0, position.character);
                for (let i of BuiltInFunctions) {
                    if (linePrefix.endsWith(`${i.class}.`)) {
                        var methods: vscode.CompletionItem[] = [];
                        for (let method of i.methods) {
                            methods.push(new vscode.CompletionItem(method, vscode.CompletionItemKind.Method));
                        }
                        return methods;
                    }
                }
            }
        },
        "."
    )
    client.start();
}

export function deactivate(): Thenable<void> | undefined {
    if (!client) {
        return undefined;
    }
    return client.stop();
}