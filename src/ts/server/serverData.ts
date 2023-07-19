import {
	ExtractedTokens,
	HJMCFile,
	JMCFile,
	MacrosData,
} from "../helpers/general";
import * as vscode from "vscode-languageserver/node";
import {
	SemanticTokenModifiers,
	SemanticTokenTypes,
} from "../data/semanticDatas";
import ExtensionLogger from "./extlogger";

export abstract class ServerData {
	protected jmcConfigPaths: string[];
	protected jmcFiles: JMCFile[];
	protected hjmcFiles: HJMCFile[];
	protected extractedTokens: ExtractedTokens;
	protected macros: MacrosData[];
	protected initResult: vscode.InitializeResult;
	protected logger: ExtensionLogger;

	constructor() {
		this.jmcConfigPaths = [];
		this.jmcFiles = [];
		this.hjmcFiles = [];
		this.extractedTokens = {
			variables: [],
			funcs: [],
		};
		this.macros = [];
		this.initResult = {
			capabilities: {
				textDocumentSync: vscode.TextDocumentSyncKind.Incremental,
				// Tell the client that this server supports code completion.
				completionProvider: {
					resolveProvider: true,
					triggerCharacters: [".", "#", " ", "/"],
				},
				signatureHelpProvider: {
					triggerCharacters: ["(", ",", " "],
					retriggerCharacters: [",", " "],
				},
				semanticTokensProvider: {
					legend: {
						tokenTypes: SemanticTokenTypes,
						tokenModifiers: SemanticTokenModifiers,
					},
					full: true,
				},
				definitionProvider: true,
				workspace: {
					workspaceFolders: {
						supported: true,
					},
				},
			},
		};
		this.logger = new ExtensionLogger("JMCServer");
	}
}

export interface BaseServer {
	connection: vscode.Connection;

	//funcs
	onInitialize(
		params: vscode.InitializeParams
	): Promise<vscode.InitializeResult>;
	onInitialized(params: vscode.InitializedParams): Promise<void>;
	onDidChangeWatchedFiles(
		params: vscode.DidChangeWatchedFilesParams
	): Promise<void>;
	onDidChangeConfiguration(
		change: vscode.DidChangeConfigurationParams
	): Promise<void>;
	onCompletion(
		arg: vscode.CompletionParams
	): Promise<vscode.CompletionItem[] | undefined>;
	onSemanticHighlightFull(
		params: vscode.SemanticTokensParams
	): Promise<vscode.SemanticTokens>;
	onCompletionResolve(
		item: vscode.CompletionItem
	): Promise<vscode.CompletionItem>;
	onSignatureHelp(
		params: vscode.SignatureHelpParams
	): Promise<vscode.SignatureHelp | null | undefined>;
	onDefinition(
		params: vscode.DefinitionParams
	): Promise<vscode.Definition | vscode.LocationLink[] | null | undefined>;
}
