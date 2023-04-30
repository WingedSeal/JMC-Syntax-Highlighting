import * as vscode from "vscode";
import { Lexer, TokenType } from "../lexer";

const tokenTypes = ["class", "interface", "enum", "function", "variable"];
const tokenModifiers = ["declaration", "documentation"];
export const semanticLegend = new vscode.SemanticTokensLegend(
	tokenTypes,
	tokenModifiers
);

export const semanticProvider: vscode.DocumentSemanticTokensProvider = {
	async provideDocumentSemanticTokens(document, token) {
		const builder = new vscode.SemanticTokensBuilder(semanticLegend);
		const tokens = new Lexer(document.getText()).tokens;
		for (let i = 0; i < tokens.length; i++) {
			const token = tokens[i];
			switch (token.type) {
				case TokenType.CLASS:
					if (
						tokens[i + 1] !== undefined &&
						tokens[i + 1].type === TokenType.LITERAL
					) {
						const current = tokens[i + 1];
						const startPos = document.positionAt(current.pos);
						const endPos = document.positionAt(
							current.pos + current.value.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "class", ["declaration"]);
					}
					break;
				case TokenType.VARIABLE: {
					const startPos = document.positionAt(token.pos);
					const endPos = document.positionAt(
						token.pos + token.value.length
					);
					const range = new vscode.Range(startPos, endPos);
					builder.push(range, "variable", ["declaration"]);
					break;
				}
				case TokenType.LITERAL: {
					if (tokens[i + 1].type == TokenType.LPAREN) {
						const startPos = document.positionAt(token.pos);
						const endPos = document.positionAt(
							token.pos + token.value.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "function", ["declaration"]);
					} else if (tokens[i + 1].type == TokenType.DOT) {
						const startPos = document.positionAt(token.pos);
						const endPos = document.positionAt(
							token.pos + token.value.length
						);
						const range = new vscode.Range(startPos, endPos);
						builder.push(range, "class", ["declaration"]);
					}

					break;
				}
			}
		}

		return builder.build();
	},
};
