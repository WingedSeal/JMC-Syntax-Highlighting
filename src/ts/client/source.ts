import { workspace, ExtensionContext, languages } from "vscode";
import {
	LanguageClient,
	LanguageClientOptions,
	NotificationType,
	ServerOptions,
	TransportKind,
} from "vscode-languageclient/node";
import * as path from "path";
import * as vscode from "vscode";
import {
	DefinedFunction,
	HeaderData,
	NotificationData,
	SELECTOR,
} from "../data/common";
import { semanticLegend } from "./semanticHighlight";
import { Language, TokenType } from "../helpers/lexer";
import { CompletionRegister } from "./completion";
import { RegisterSignatureSign } from "./signature";
import { DefinationRegister } from "./defination";
import { CommandType, ParserType } from "../helpers/parseCommand";

interface ClassesMethods {
	name: string;
	methods: string[];
}

let client: LanguageClient;
export let classesMethods: ClassesMethods[] | undefined;
export let mainHeader: HeaderData[] = [];
export let definedFuncs: DefinedFunction[] | undefined;

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

	//define client
	client = new LanguageClient("jmc", "JMC", serverOptions, clientOptions);

	CompletionRegister.RegisterAll();
	RegisterSignatureSign();
	DefinationRegister.RegisterAll();

	const semanticHighlight = languages.registerDocumentSemanticTokensProvider(
		SELECTOR,
		{
			async provideDocumentSemanticTokens(document, token) {
				const builder = new vscode.SemanticTokensBuilder(
					semanticLegend
				);
				const text = document.getText();
				const language = new Language(text, mainHeader);
				for (const _var of language.tokens.sort()) {
					if (_var.type === TokenType.USE_VARIABLE) {
						const startPos = document.positionAt(_var.offset);
						const endPos = document.positionAt(
							_var.offset + _var.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "variable", ["declaration"]);
					} else if (_var.type === TokenType.MACRO) {
						const startPos = document.positionAt(_var.offset);
						const endPos = document.positionAt(
							_var.offset + _var.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "macro", ["declaration"]);
					} else if (_var.type === TokenType.VARIABLE) {
						const startPos = document.positionAt(_var.offset);
						const endPos = document.positionAt(
							_var.offset + _var.value![0].length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "variable", ["declaration"]);
					} else if (_var.type === TokenType.FUNCTION) {
						const startPos = document.positionAt(_var.offset + 9);
						const endPos = document.positionAt(
							_var.offset + _var.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "function", ["declaration"]);
					} else if (_var.type === TokenType.CLASS) {
						const startPos = document.positionAt(_var.offset + 6);
						const endPos = document.positionAt(
							_var.offset + _var.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "class", ["declaration"]);
					} else if (_var.type === TokenType.CALL_FUNCTION) {
						if (
							_var.value !== undefined &&
							_var.value.length === 1
						) {
							const startPos = document.positionAt(_var.offset);
							const endPos = document.positionAt(
								_var.offset + _var.value[0].length
							);
							const range = new vscode.Range(startPos, endPos);
							builder.push(range, "function", ["declaration"]);
						} else if (
							_var.value !== undefined &&
							_var.value.length === 2
						) {
							const startPos = document.positionAt(_var.offset);
							const endPos = document.positionAt(
								_var.offset + _var.value[0].length
							);
							const range = new vscode.Range(startPos, endPos);
							builder.push(range, "class", ["declaration"]);

							const startPos2 = document.positionAt(
								_var.offset + _var.value[0].length + 1
							);
							const endPos2 = document.positionAt(
								_var.offset +
									_var.value[0].length +
									1 +
									_var.value[1].length
							);
							const range2 = new vscode.Range(startPos2, endPos2);
							builder.push(range2, "function", ["declaration"]);
						} else if (
							_var.value !== undefined &&
							_var.value.length > 2
						) {
							const startPos = document.positionAt(_var.offset);
							const endPos = document.positionAt(
								_var.offset + _var.value[0].length
							);
							const range = new vscode.Range(startPos, endPos);
							builder.push(range, "class", ["declaration"]);

							let pos = _var.offset + _var.value[0].length + 1;
							for (const v of _var.value.slice(
								1,
								_var.value.length - 2
							)) {
								const startPos = document.positionAt(pos);
								const endPos = document.positionAt(
									pos + v.length
								);
								const range = new vscode.Range(
									startPos,
									endPos
								);
								builder.push(range, "class", ["declaration"]);
								pos += v.length + 1;
							}

							const startPos2 = document.positionAt(pos);
							const endPos2 = document.positionAt(
								pos + _var.value[_var.value.length - 1].length
							);
							const range2 = new vscode.Range(startPos2, endPos2);
							builder.push(range2, "function", ["declaration"]);
						}
					} else if (
						_var.type === TokenType.COMMAND &&
						_var.value !== undefined
					) {
						let pos = _var.offset;
						const spaces = [0].concat(
							_var.trim
								.split(/(\s+)/g)
								.filter((v) => v.startsWith(" "))
								.map((v) => v.length)
						);

						for (let i = 0; i < _var.value.length; i++) {
							const value = _var.value[i].split(";");

							const varType = value[0];
							const name = value[1];
							const length = Number.parseInt(value[2]);
							const parser: string | undefined = value[3];

							if (varType === CommandType.LITERAL) {
								const start = document.positionAt(pos);
								const end = document.positionAt(pos + length);
								const range = new vscode.Range(start, end);
								builder.push(range, "keyword", ["declaration"]);
							} else if (
								varType === CommandType.ARGUMENT &&
								parser !== undefined
							) {
								const start = document.positionAt(pos);
								const end = document.positionAt(pos + length);
								const range = new vscode.Range(start, end);
								if (parser === ParserType.FUNCTION) {
									builder.push(range, "function", [
										"declaration",
									]);
								}
								//TODO: add support for all parsertype
							}

							pos += length + spaces[i + 1];
						}
					} else if (_var.type === TokenType.NEW) {
						const startPos = document.positionAt(_var.offset + 4);
						const endPos = document.positionAt(
							_var.offset + _var.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "class", ["declaration"]);
					}
				}
				return builder.build();
			},
		},
		semanticLegend
	);

	client.start().then(() => {
		client.onNotification("data/lang", (data: NotificationData) => {
			classesMethods = data.classesMethods;
			mainHeader = data.headers;
			definedFuncs = data.funcs;
		});
		console.log("Client Started");
	});
}

export function deactivate(): Thenable<void> | undefined {
	if (!client) {
		return undefined;
	}
	return client.stop();
}
