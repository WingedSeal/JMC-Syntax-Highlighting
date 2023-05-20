import {
	ExtractedTokens,
	FileTokens,
	JMCFile,
	getFunctions,
	getVariables,
	getVariablesDeclare,
	removeDuplicate,
} from "../helpers/general";
import { TokenData } from "../lexer";

export async function getTokens(files: JMCFile[]): Promise<ExtractedTokens> {
	const tokens: ExtractedTokens = {
		variables: [],
		funcs: [],
	};

	for (const f of files) {
		tokens.variables.push({
			path: f.path,
			tokens: await getVariablesDeclare(f.lexer),
		});
		tokens.funcs.push({
			path: f.path,
			tokens: await getFunctions(f.lexer),
		});
	}
	return tokens;
}

export function concatVariableTokens(ext: ExtractedTokens): TokenData[] {
	const tokens = ext.variables;
	const datas: TokenData[] = [];

	for (const t of tokens) {
		t.tokens.forEach((v) => datas.push(v));
	}

	return removeDuplicate(datas, (a, b) => a.value === b.value);
}

export async function getAllVariables(files: JMCFile[]): Promise<FileTokens[]> {
	const datas: FileTokens[] = [];

	for (const file of files) {
		datas.push({
			path: file.path,
			tokens: await getVariables(file.lexer),
		});
	}

	return datas;
}

export function concatFuncsTokens(ext: ExtractedTokens): TokenData[] {
	const tokens = ext.funcs;
	const datas: TokenData[] = [];

	for (const t of tokens) {
		t.tokens.forEach((v) => datas.push(v));
	}

	return removeDuplicate(datas, (a, b) => a.value === b.value);
}
