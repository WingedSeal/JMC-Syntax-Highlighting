import * as vscode from "vscode";
import {
	getClass as getClasses,
	getFunctions,
	getVariables,
} from "./helpers/documentAnalyze";

const tokenTypes = [
	"class",
	"variable",
	"function",
	"enum",
	"enumMember",
	"method",
	"undefinedVariable",
];
const tokenModifiers = ["declaration"];
export const semanticLegend = new vscode.SemanticTokensLegend(
	tokenTypes,
	tokenModifiers
);

export function getVariablesClient(text: string): string[] {
	return getVariables(text, vscode.workspace.workspaceFolders![0].uri.fsPath);
}

export function getFunctionsClient(text: string): string[] {
	return getFunctions(text, vscode.workspace.workspaceFolders![0].uri.fsPath);
}

export function getClassClient(text: string): string[] {
	return getClasses(text, vscode.workspace.workspaceFolders![0].uri.fsPath);
}
