import * as vscode from "vscode";
import { Lexer, TokenData, TokenType } from "../lexer";

export class variableCompletion implements vscode.CompletionItemProvider {
	private done = false;
	private result:
		| vscode.CompletionItem[]
		| vscode.CompletionList<vscode.CompletionItem>
		| null
		| undefined;

	async provideCompletionItems(
		document: vscode.TextDocument,
		position: vscode.Position,
		token: vscode.CancellationToken,
		context: vscode.CompletionContext
	): Promise<
		| vscode.CompletionItem[]
		| vscode.CompletionList<vscode.CompletionItem>
		| null
		| undefined
	> {
		const tokens = new Lexer(document.getText()).tokens;
		const offset = document.offsetAt(position) - 1;
		let i = 0;
		const chunksize = 20;
		while (i < tokens.length - 1 && !this.done) {
			// const currentToken = tokens[i];
			// const nextToken = tokens[i + 1];
			// if (currentToken.pos <= offset && nextToken.pos >= offset) {
			// 	if (tokens[i - 1].type == TokenType.VARIABLE) {
			// 		return [
			// 			{
			// 				label: "get",
			// 				kind: vscode.CompletionItemKind.Function,
			// 				insertText: "get()",
			// 			},
			// 		];
			// 	}
			// }
			const previous = tokens[i - 1];
			const chunk = tokens
				.slice(i, i + chunksize)
				.filter((v) => v != undefined);
			this.result = await this.parseChunks(previous, chunk, offset);
			if (this.result) {
				return this.result;
			}

			i += chunksize;
		}

		return undefined;
	}

	private async parseChunks(
		pre: TokenData | undefined,
		chunk: TokenData[],
		offset: number
	): Promise<vscode.CompletionItem[] | undefined> {
		if (pre) chunk.unshift(pre);
		console.log(chunk);
		for (let i = 0; i < chunk.length; i++) {
			const currentToken = chunk[i];
			const nextToken = chunk[i + 1];
			if (
				nextToken &&
				currentToken.pos <= offset &&
				nextToken.pos >= offset
			) {
				if (chunk[i - 1].type == TokenType.VARIABLE) {
					this.done = true;
					return [
						{
							label: "get",
							kind: vscode.CompletionItemKind.Function,
							insertText: "get()",
						},
					];
				}
			}
		}
	}
}
