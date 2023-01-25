import * as vscode from "vscode";
import { getVariables } from "./data/common";
import { getCurrentFile } from "./source";

const tokenTypes = [
	"class",
	"variable",
	"function",
	"enum",
	"enumMember",
	"method",
	"undefinedVariable"
];
const tokenModifiers = ["declaration"];
export const semanticLegend = new vscode.SemanticTokensLegend(
	tokenTypes,
	tokenModifiers
);

interface SemanticToken {
	range: vscode.Range;
	type: string;
	modifier: string[] | undefined;
}

export function getVariablesClient(text: string): string[] {
	return getVariables(text, getCurrentFile()!);
}
