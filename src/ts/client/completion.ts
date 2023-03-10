import * as vscode from "vscode";
import { languages } from "vscode";
import { HEADERS, HEADER_SELECTOR, JSON_FILE_TYPES, SELECTOR } from "../data/common";
import { getCurrentCommand } from "../helpers/documentHelper";
import { BuiltInFunctions } from "../data/builtinFunctions";
import { classesMethods } from "./source";
import { getAllFiles } from "get-all-files";
import { CommandType, lexCommand, parseCommand } from "../helpers/parseCommand";

export class CompletionRegister {
	public static RegisterAll() {
		this.RegisterBuiltinFunction();
		this.RegisterClassMethods();
		this.RegisterFileImport();
		this.RegisterHeader();
		this.RegisterImport();
		this.RegisterVariableFunction();
        this.RegisterNewKeyword();
        this.RegisterVanillaCommand();
	}

	public static RegisterBuiltinFunction() {
		languages.registerCompletionItemProvider(
			SELECTOR,
			{
				async provideCompletionItems(
					document,
					position,
					token,
					context
				) {
					const linePrefix = await getCurrentCommand(
						document.getText(),
						document.offsetAt(position)
					);
					for (const i of BuiltInFunctions) {
						if (linePrefix.endsWith(`${i.class}.`)) {
							const methods: vscode.CompletionItem[] = [];
							for (const method of i.methods) {
								const item = new vscode.CompletionItem(
									method.name,
									vscode.CompletionItemKind.Method
								);
								methods.push(item);
							}
							return methods;
						}
					}
				},
			},
			"."
		);
	}

	public static RegisterClassMethods() {
		languages.registerCompletionItemProvider(
			SELECTOR,
			{
				async provideCompletionItems(
					document,
					position,
					token,
					context
				) {
					if (classesMethods === undefined) return;
					const linePrefix = await getCurrentCommand(
						document.getText(),
						document.offsetAt(position)
					);
					for (const item of classesMethods) {
						const cItems: vscode.CompletionItem[] = [];
						if (linePrefix.endsWith(`${item.name}.`)) {
							item.methods.forEach((v) => {
								cItems.push({
									label: v,
									kind: vscode.CompletionItemKind.Function,
								});
							});
							return cItems;
						}
					}
					return undefined;
				},
			},
			"."
		);
	}

	public static RegisterVariableFunction() {
		languages.registerCompletionItemProvider(
			SELECTOR,
			{
				async provideCompletionItems(
					document,
					position,
					token,
					context
				) {
					const linePrefix = await getCurrentCommand(
						document.getText(),
						document.offsetAt(position)
					);
					if (/\$(\w+)\./g.test(linePrefix)) {
						return [
							{
								label: "get",
								kind: vscode.CompletionItemKind.Method,
							},
						];
					}
					return undefined;
				},
			},
			"."
		);
	}

	public static RegisterHeader() {
		languages.registerCompletionItemProvider(
			HEADER_SELECTOR,
			{
				provideCompletionItems(document, position, token, c) {
					const headers: vscode.CompletionItem[] = [];
					for (const i of HEADERS) {
						headers.push({
							label: i,
							kind: vscode.CompletionItemKind.Keyword,
						});
					}
					return headers;
				},
			},
			"#"
		);
	}

	public static RegisterImport() {
		languages.registerCompletionItemProvider(
			SELECTOR,
			{
				provideCompletionItems(document, position, token, c) {
					return [
						{
							label: "import",
							kind: vscode.CompletionItemKind.Keyword,
						},
					];
				},
			},
			"@"
		);
	}

	public static RegisterFileImport() {
		languages.registerCompletionItemProvider(
			SELECTOR,
			{
				async provideCompletionItems(document, position, token, c) {
					const linePrefix = await getCurrentCommand(
						document.getText(),
						document.offsetAt(position)
					);
					if (linePrefix.endsWith("@import ")) {
						const path = document.uri.fsPath.split("\\");
						path.pop();
						const folder = path.join("/");

						const files: string[] = await getAllFiles(
							folder
						).toArray();
						const items: vscode.CompletionItem[] = [];
						for (const i of files) {
							if (i.endsWith(".jmc")) {
								items.push({
									label: `"${i
										.slice(folder.length + 1)
										.slice(0, -4)}"`,
									kind: vscode.CompletionItemKind.Module,
								});
							}
						}
						return items;
					}
					return undefined;
				},
			},
			" "
		);
	}

    public static RegisterNewKeyword() {
        languages.registerCompletionItemProvider(
            SELECTOR,
            {
                async provideCompletionItems(document, position, token, context) {
                    const linePrefix = await getCurrentCommand(
                        document.getText(),
                        document.offsetAt(position)
                    );
                    if (linePrefix.endsWith("new ")) {
                        const items: vscode.CompletionItem[] = [];
                        for (const i of JSON_FILE_TYPES) {
                            items.push({
                                label: i,
                                kind: vscode.CompletionItemKind.Value,
                            });
                        }
                        return items;
                    }
                    return undefined;
                },
            },
            " "
        );        
    }

    public static RegisterVanillaCommand() {
        languages.registerCompletionItemProvider(
            SELECTOR,
            {
                async provideCompletionItems(document, position, token, context) {
                    const text = document.getText().trim();
                    const linePrefix = await getCurrentCommand(
                        text,
                        document.offsetAt(position)
                    );
    
                    const items: vscode.CompletionItem[] = [];
    
                    const r = parseCommand(lexCommand(linePrefix));
                    if (r !== undefined && r.node !== undefined) {
                        for (const node of r.node) {
                            switch (node.type) {
                                case CommandType.LITERAL:
                                    items.push({
                                        label: node.name,
                                        kind: vscode.CompletionItemKind.Keyword,
                                    });
                                    break;
                                case CommandType.ARGUMENT:
                                    break;
                            }
                        }
                    }
    
                    return items.length > 0 ? items : undefined;
                },
            },
            " "
        );        
    }
}
