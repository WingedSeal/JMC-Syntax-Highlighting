import {
	ExtractedTokens,
	FileTokens,
	JMCFile,
	getFunctions,
	getVariables,
	getVariablesDeclare,
	removeDuplicate,
} from "./general";
import { TokenData } from "../lexer";

export interface CFData {
	classes: string[];
	funcs: string[];
}

export interface ServerSettings {
	maxNumberOfProblems: number;
}

/**
 * helper for {@link ExtractedTokens}
 */
export class ExtractedTokensHelper {
	/**
	 *
	 * @param files
	 * @returns
	 */
	static async getTokens(files: JMCFile[]): Promise<ExtractedTokens> {
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

	static concatVariableTokens(ext: ExtractedTokens): TokenData[] {
		const tokens = ext.variables;
		const datas: TokenData[] = [];

		for (const t of tokens) {
			t.tokens.forEach((v) => datas.push(v));
		}

		return removeDuplicate(datas, (a, b) => a.value === b.value);
	}

	static async getAllVariables(files: JMCFile[]): Promise<FileTokens[]> {
		const datas: FileTokens[] = [];

		for (const file of files) {
			datas.push({
				path: file.path,
				tokens: await getVariables(file.lexer),
			});
		}

		return datas;
	}

	static concatFuncsTokens(ext: ExtractedTokens): TokenData[] {
		const tokens = ext.funcs;
		const datas: TokenData[] = [];

		for (const t of tokens) {
			t.tokens.forEach((v) => datas.push(v));
		}

		return removeDuplicate(datas, (a, b) => a.value === b.value);
	}
}

/**
 * Helper for hirarchy
 */
export class HirarchyHelper {
	/**
	 *
	 * @param funcs
	 * @returns
	 */
	static async getFirstHirarchy(funcs: string[]): Promise<CFData> {
		const result: CFData = {
			classes: [],
			funcs: [],
		};

		for (const func of funcs) {
			const splited = func.split(".");
			if (splited.length == 1) result.funcs.push(splited[0]);
			else result.classes.push(splited[0]);
		}
		result.funcs = removeDuplicate(result.funcs, (a, b) => a === b);
		result.classes = removeDuplicate(result.classes, (a, b) => a === b);
		return result;
	}

	static async getHirarchy(funcs: string[], query: string[]) {
		const splitedFuncs = funcs.map((v) => v.split("."));
		const result: CFData = {
			classes: [],
			funcs: [],
		};

		for (let i = 0; i < query.length; i++) {
			const current = query[i];
			const q = splitedFuncs.filter((v) => v[i] == current);
			if (q.length === 0) break;
			else if (i === query.length - 1) {
				for (const r of q) {
					if (r.length === query.length + 1) {
						result.funcs.push(r[query.length]);
					} else if (r.length > query.length) {
						result.classes.push(r[query.length]);
					}
				}
				result.funcs = removeDuplicate(result.funcs, (a, b) => a === b);
				result.classes = removeDuplicate(
					result.classes,
					(a, b) => a === b
				);
				return result;
			}
		}
	}
}
