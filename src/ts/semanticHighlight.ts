import * as vscode from "vscode";
import { ImportData, getImport, getImportDocumentText, getVariables } from "./data/common";
import { getCurrentWorkspace } from "./source";

const tokenTypes = [
	"class",
	"variable",
	"function",
	"enum",
	"enumMember",
	"method",
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
	return getVariables(text, getCurrentWorkspace()!);
}
