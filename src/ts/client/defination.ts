import {
	CancellationToken,
	Definition,
	DefinitionProvider,
	Location,
	LocationLink,
	Position,
	Range,
	TextDocument,
	Uri,
} from "vscode";
import {
	ExtractedTokens,
	JMCFile,
	getAllFunctionsCall,
	getClassRange,
	getFunctions,
	getIndexByOffset,
	getVariables,
	getVariablesDeclare,
} from "../helpers/general";
import { client } from "./source";
import { TokenType } from "../lexer";
import * as vscode from "vscode";

export class definationProvider implements DefinitionProvider {
	async provideDefinition(
		document: TextDocument,
		position: Position,
		token: CancellationToken
	): Promise<Definition | LocationLink[] | null | undefined> {
		const files: JMCFile[] = await client.sendRequest("data/getFiles");
		const currFile = files.find((v) => v.path == document.uri.fsPath);

		if (currFile) {
			const index = getIndexByOffset(
				currFile.lexer,
				document.offsetAt(position)
			);
			const tokens = currFile.lexer.tokens;
			const currentToken = tokens[index - 1];
			const word = document.getText(
				document.getWordRangeAtPosition(position, /\$?[\w\.]+/g)
			);
			console.log(word);

			if (tokens[index - 2].type == TokenType.FUNCTION) {
				const datas: LocationLink[] = [];
				const funcs = await getAllFunctionsCall(files);
				const classRanges = await getClassRange(currFile.lexer);
				for (const range of classRanges) {
					if (
						currentToken.pos > range.range[0] &&
						range.range[1] > currentToken.pos &&
						currentToken.type == TokenType.LITERAL
					)
						tokens[index - 1].value = `${range.name}.${
							tokens[index - 1].value
						}`;
				}
				for (const func of funcs) {
					for (const t of func.tokens) {
						if (currentToken.value == t.value) {
							const doc = await vscode.workspace.openTextDocument(
								func.path
							);
							const start = doc.positionAt(t.pos);
							const end = doc.positionAt(t.pos + t.value.length);
							const range = new Range(start, end);
							datas.push({
								targetUri: Uri.file(func.path),
								targetRange: range,
							});
						}
					}
				}
				return datas;
			} else if (!word.startsWith("$")) {
				for (const file of files) {
					const func = await getFunctions(file.lexer);
					const doc = await vscode.workspace.openTextDocument(
						file.path
					);
					const query = func.find((v) => v.value == word);

					if (query) {
						const start = doc.positionAt(query.pos);
						const end = doc.positionAt(query.pos + 1);
						const range = new Range(start, end);

						return new Location(doc.uri, range);
					}
				}
			} else if (word.startsWith("$")) {
				const datas: LocationLink[] = [];
				for (const file of files) {
					const variables = await getVariables(file.lexer);
					const doc = await vscode.workspace.openTextDocument(
						file.path
					);
					const query = variables.find((v) => v.value == word);
					console.log(variables);

					if (query) {
						const start = doc.positionAt(query.pos + 1);
						const end = doc.positionAt(
							query.pos + query.value.length
						);
						const range = new Range(start, end);
						datas.push({
							targetUri: doc.uri,
							targetRange: range,
						});
					}
				}
				return datas;
			}
		}

		return undefined;
	}
}
