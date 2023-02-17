import * as vscode from "vscode";

const tokenTypes = [
	"class",
	"variable",
	"function",
	"enum",
	"enumMember",
	"method",
	"undefinedVariable",
	"keyword",
];
const tokenModifiers = ["declaration"];
export const semanticLegend = new vscode.SemanticTokensLegend(
	tokenTypes,
	tokenModifiers
);
