import {
	ExtractedTokens,
	JMCFile,
	getFunctions,
	getVariables,
	removeDuplicate,
} from "../helpers/general";
import { TokenData } from "../lexer";

// export async function getAllVariables(files: JMCFile[]): Promise<TokenData[]> {
// 	let datas: TokenData[] = [];

// 	for (const f of files) {
// 		datas = datas.concat(await getVariables(f.lexer));
// 	}

// 	return removeDuplicate(datas, (a, b) => a.value === b.value);
// }

// export async function getAllFuncs(files: JMCFile[]): Promise<TokenData[]> {
// 	let datas: TokenData[] = [];

// 	for (const f of files) {
// 		datas = datas.concat(await getFunctions(f.lexer));
// 	}

// 	return removeDuplicate(datas, (a, b) => a.value === b.value);
// }

export async function getTokens(files: JMCFile[]): Promise<ExtractedTokens> {
	const tokens: ExtractedTokens = {
		variables: [],
		funcs: [],
	};

	for (const f of files) {
		tokens.variables.push({
			path: f.path,
			tokens: await getVariables(f.lexer),
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

export function concatFuncsTokens(ext: ExtractedTokens): TokenData[] {
	const tokens = ext.funcs;
	const datas: TokenData[] = [];

	for (const t of tokens) {
		t.tokens.forEach((v) => datas.push(v));
	}

	return removeDuplicate(datas, (a, b) => a.value === b.value);
}
