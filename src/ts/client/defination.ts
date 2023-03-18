import { languages, workspace } from "vscode";
import { HEADER_SELECTOR, HeaderType, SELECTOR } from "../data/common";
import { parseHJMCFile } from "../helpers/documentHelper";
import * as url from "url";
import path from "path";
import * as vscode from "vscode";
import { definedFuncs } from "./source";

export class DefinationRegister {
	public static RegisterAll() {
		this.RegisterHeaderInclude();
		this.RegisterJMCFile();
	}

	static async RegisterJMCFile() {
		languages.registerDefinitionProvider(SELECTOR, {
			async provideDefinition(document, position, token) {
				const wordRange = document.getWordRangeAtPosition(
					position,
					/[\w\.]+/g
				);
				const linePrefix = document.getText(wordRange);
				const seperated = linePrefix.split(".");
				if (seperated.length > 1 && definedFuncs !== undefined) {
					const _class = seperated[0];
					const func = seperated[1];

					const result = definedFuncs.find(
						(v) => v.className === _class && v.name === func
					);

					if (result !== undefined) {
						const doc = await vscode.workspace.openTextDocument(
							vscode.Uri.file(result.file)
						);
						const start = doc.positionAt(result.pos + 10);
						const end = doc.positionAt(
							result.pos + 10 + result.name.length
						);
						const range = new vscode.Range(start, end);

						return {
							uri: vscode.Uri.file(result.file),
							range: range,
						};
					}
				} else if (definedFuncs !== undefined) {
					const result = definedFuncs.find(
						(v) => v.name === seperated[0]
					);

					if (result !== undefined) {
						const doc = await vscode.workspace.openTextDocument(
							vscode.Uri.file(result.file)
						);
						const start = doc.positionAt(result.pos + 10);
						const end = doc.positionAt(
							result.pos + 10 + result.name.length
						);
						const range = new vscode.Range(start, end);

						return {
							uri: vscode.Uri.file(result.file),
							range: range,
						};
					}
				}
				return undefined;
			},
		});
	}

	static RegisterHeaderInclude() {
		languages.registerDefinitionProvider(HEADER_SELECTOR, {
			async provideDefinition(document, position, token) {
				// return {
				// 	range: new vscode.Range(
				// 		document.positionAt(0),
				// 		document.positionAt(5)
				// 	),
				// 	uri: document.uri,
				// };
				const currentPos = document.offsetAt(position);
				//const locations: vscode.LocationLink[] = [];
				const text = document.getText();
				const data = parseHJMCFile(text);
				const includes = data.filter(
					(v) => v.header == HeaderType.INCLUDE
				);
				for (const include of includes) {
					if (
						currentPos > include.offset + 8 &&
						currentPos < include.offset + 8 + include.length
					) {
						const p = url
							.pathToFileURL(
								path.resolve(
									`${
										workspace.workspaceFolders![0].uri
											.fsPath
									}/${include.value![0]}`
								)
							)
							.toString();
						const range = new vscode.Range(
							document.positionAt(include.offset + 9),
							document.positionAt(
								include.offset +
									9 +
									include.value![0].length +
									3
							)
						);
						// return {
						// 	range: range,
						// 	uri: vscode.Uri.parse(p)
						// };
						return [
							{
								originSelectionRange: range,
								uri: vscode.Uri.parse(p),
								range: new vscode.Range(
									new vscode.Position(0, 0),
									new vscode.Position(0, 0)
								),
							},
						];
					}
				}
				return undefined;
			},
		});
	}
}
