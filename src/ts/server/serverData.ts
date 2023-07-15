import { ILogObj, Logger } from "tslog";
import {
	ExtractedTokens,
	HJMCFile,
	JMCFile,
	MacrosData,
} from "../helpers/general";
import * as vscode from "vscode-languageserver/node";

export abstract class ServerData {
	protected jmcConfigPaths: string[];
	protected jmcFiles: JMCFile[];
	protected hjmcFiles: HJMCFile[];
	protected extractedTokens: ExtractedTokens;
	protected macros: MacrosData[];

	constructor() {
		this.jmcConfigPaths = [];
		this.jmcFiles = [];
		this.hjmcFiles = [];
		this.extractedTokens = {
			variables: [],
			funcs: [],
		};
		this.macros = [];
	}
}

export interface BaseServer {
	connection: vscode.Connection;
	logger: Logger<ILogObj>;

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
