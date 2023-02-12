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
	"keyword"
];
const tokenModifiers = ["declaration"];
export const semanticLegend = new vscode.SemanticTokensLegend(
	tokenTypes,
	tokenModifiers
);

export async function getVariablesClient(text: string): Promise<string[]> {
	return await getVariables(
		text,
		vscode.workspace.workspaceFolders![0].uri.fsPath
	);
}

export async function getFunctionsClient(text: string): Promise<string[]> {
	return await getFunctions(
		text,
		vscode.workspace.workspaceFolders![0].uri.fsPath
	);
}

export async function getClassClient(text: string): Promise<string[]> {
	return await getClasses(
		text,
		vscode.workspace.workspaceFolders![0].uri.fsPath
	);
}
