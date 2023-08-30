import { END_TOKEN, Lexer, TokenData, TokenType } from "../lexer";
import { HeaderParser } from "../parseHeader";
import ExtensionLogger from "../server/extlogger";

const logger = new ExtensionLogger("Helper");

export interface Settings {
	executable: string;
	rawFuncHighlight: boolean;
	boldFirstCommand: boolean;
	capitalizedClass: boolean;
}

export interface JMCFile {
	path: string;
	lexer: Lexer;
	text: string;
}

interface Position {
	line: number;
	character: number;
}

export interface HJMCFile {
	path: string;
	parser: HeaderParser;
}

export interface MacrosData {
	path: string;
	target: string;
	values: string[];
}

export interface ExtractedTokens {
	variables: FileTokens[];
	funcs: FileTokens[];
}

export interface FileTokens {
	path: string;
	tokens: TokenData[];
}

export interface StatementRange {
	start: number;
	end: number;
}

type IsDuplicate<T> = (a: T, b: T) => boolean;

/**
 * remove all duplicate value of  in the array
 * @param collection {@link Array}
 * @param isDuplicate Arrow function - {@link IsDuplicate}
 * @returns an array without duplication
 */
export const removeDuplicate = <TItems>(
	collection: TItems[],
	isDuplicate: IsDuplicate<TItems>
) =>
	collection.filter(
		(item, index, items: TItems[]) =>
			items.findIndex((secondItem) => isDuplicate(item, secondItem)) ===
			index
	);

/**
 * find the differnences between two strings
 * @param str1 text 1
 * @param str2 text 2
 * @returns pos of the changed index
 */
export async function findStringDifference(
	str1: string,
	str2: string
): Promise<number | null> {
	const minLength = Math.min(str1.length, str2.length);

	for (let i = 0; i < minLength; i++) {
		if (str1[i] !== str2[i]) {
			return i;
		}
	}

	if (str1.length !== str2.length) {
		return minLength;
	}

	return null;
}

/**
 * find the differnences between two strings
 * @param str1 text 1
 * @param str2 text 2
 * @returns pos of the changed index
 */
export function findStringDifferenceSync(
	str1: string,
	str2: string
): number | null {
	const minLength = Math.min(str1.length, str2.length);

	for (let i = 0; i < minLength; i++) {
		if (str1[i] !== str2[i]) {
			return i;
		}
	}

	if (str1.length !== str2.length) {
		return minLength;
	}

	return null;
}

/**
 * get the tokens index of by the offset provided
 * @param tokens {@link Lexer}
 * @param offset offset of the text
 * @returns index of the tokens
 */
export function getIndexByOffset(tokens: TokenData[], offset: number): number {
	return tokens.findIndex((val) => {
		return val.pos >= offset;
	});
}
/**
 * get all variables declare tokens
 * @example $bar = 3;
 * @param lexer
 * @returns the tokens of the variables - {@link TokenData}
 */
export async function getVariablesDeclare(lexer: Lexer): Promise<TokenData[]> {
	const variablesID = lexer.tokens
		.filter((v) => v.type == TokenType.VARIABLE)
		.map((v) => lexer.tokens.indexOf(v));
	return variablesID
		.filter((v) => {
			const current = lexer.tokens[v];
			return [
				TokenType.OP_DIVIDEEQ,
				TokenType.EQUAL,
				TokenType.OP_PLUSEQ,
				TokenType.OP_MINUSEQ,
				TokenType.OP_MULTIPLYEQ,
				TokenType.OP_REMAINDEREQ,
			].includes(lexer.tokens[v + 1].type);
		})
		.map((v) => lexer.tokens[v]);
}

/**
 * get all variables of the lexer
 * @example $foo
 * @param lexer {@link Lexer}
 * @returns the tokens of the variabales - {@link TokenData}
 */
export async function getVariables(lexer: Lexer): Promise<TokenData[]> {
	return lexer.tokens.filter(
		(v) =>
			v.type == TokenType.VARIABLE || v.type == TokenType.COMMAND_VARIABLE
	);
}

/**
 * get all functions declared
 * @example function bar() {}
 * @param lexer {@link Lexer}
 * @returns the tokens of the function name - {@link TokenData}
 * @returns the modified value and original value is seperated by `\0`
 */
export async function getFunctions(lexer: Lexer): Promise<TokenData[]> {
	const classRanges = await getClassRange(lexer);
	const funcID = lexer.tokens
		.filter((v) => v.type == TokenType.FUNCTION)
		.map((v) => lexer.tokens.indexOf(v));
	return funcID.map((v) => {
		const token = lexer.tokens[v + 1];
		let i = v + 1;
		const value: string[] = [];
		while (lexer.tokens[i].type != TokenType.LPAREN) {
			let sv = "";
			for (const range of classRanges) {
				if (token.pos > range.range[0] && token.pos < range.range[1]) {
					sv += range.name + ".";
				}
			}
			sv += lexer.tokens[i].value;
			value.push(sv);
			i++;
		}
		return {
			type: TokenType.LITERAL,
			pos: token.pos,
			value: value.join("") + "\0" + token.value,
		};
	});
}

/**
 * get all function calls
 * @example bar();
 * @param lexer {@link Lexer}
 * @returns all function calls of the tokens
 */
export async function getFunctionsCall(lexer: Lexer): Promise<TokenData[]> {
	const datas: TokenData[] = [];
	const tokens = lexer.tokens;
	const splited = await splitTokenArray(tokens);

	for (const splitedToken of splited) {
		const lastIndex = splitedToken.length - 1;
		const funcCheck = splitedToken
			.slice(lastIndex - 3, lastIndex)
			.filter((v) => v != undefined);
		if (
			funcCheck.length > 2 &&
			funcCheck[0].type == TokenType.LITERAL &&
			funcCheck[1].type == TokenType.LPAREN &&
			funcCheck[2].type == TokenType.RPAREN
		) {
			let i = lastIndex - 2;
			const value: string[] = [];
			while (i-- != 0) {
				const current = splitedToken[i];
				value.push(current.value);
				if (
					splitedToken[i - 1] &&
					splitedToken[i - 1].type == TokenType.FUNCTION &&
					current.type == TokenType.LITERAL
				) {
					break;
				} else if (
					splitedToken[i - 1] &&
					current.type == TokenType.LITERAL &&
					splitedToken[i - 1].type != TokenType.DOT
				) {
					datas.push({
						type: TokenType.LITERAL,
						value: value.reverse().join(""),
						pos: current.pos,
					});
					break;
				} else if (
					!splitedToken[i - 1] &&
					current.type == TokenType.LITERAL
				) {
					datas.push({
						type: TokenType.LITERAL,
						value: value.reverse().join(""),
						pos: current.pos,
					});
					break;
				} else if (
					splitedToken[i - 1] &&
					current.type == TokenType.LITERAL &&
					splitedToken[i - 1].type == TokenType.DOT
				) {
					value.push(splitedToken[i - 1].value);
					i--;
				} else break;
			}
		}
	}

	return datas;
}

/**
 * split an 1d token array into 2d token array, split by [",","{","}"]
 * @param arr 1d array - {@link TokenData}
 * @returns 2d array - {@link TokenData}
 */
export async function splitTokenArray(
	arr: TokenData[]
): Promise<TokenData[][]> {
	const result: TokenData[][] = [];
	let temp: TokenData[] = [];
	for (const data of arr) {
		temp.push(data);

		if (
			[TokenType.SEMI, TokenType.LCP, TokenType.RCP].includes(data.type)
		) {
			result.push(temp);
			temp = [];
			continue;
		}
	}

	return result;
}

/**
 * split an 1d token array into 2d token array, split by [",",";","{","}"]
 * @param arr 1d array - {@link TokenData}
 * @returns 2d array - {@link TokenData}
 */
export function splitTokenArraySync(arr: TokenData[]): TokenData[][] {
	const result: TokenData[][] = [];
	let temp: TokenData[] = [];
	for (const data of arr) {
		temp.push(data);

		if (
			[TokenType.SEMI, TokenType.LCP, TokenType.RCP].includes(data.type)
		) {
			result.push(temp);
			temp = [];
			continue;
		}
	}

	return result;
}

/**
 * get all functions call, see {@link getFunctionsCall}
 * @param files all files - {@link JMCFile}
 * @returns all tokens of function call in the files - {@link TokenData}
 */
export async function getAllFunctionsCall(
	files: JMCFile[]
): Promise<{ path: string; tokens: TokenData[] }[]> {
	let datas: { path: string; tokens: TokenData[] }[] = [];
	for (const file of files) {
		datas = datas.concat({
			path: file.path,
			tokens: await getFunctionsCall(file.lexer),
		});
	}
	return datas;
}

/**
 * get all classes inside the tokens
 * @param lexer {@link Lexer}
 * @returns all tokens meet the requirement - {@link TokenData}
 */
export async function getClasses(lexer: Lexer): Promise<TokenData[]> {
	const classID = lexer.tokens
		.filter((v) => v.type == TokenType.CLASS)
		.map((v) => lexer.tokens.indexOf(v));
	return classID.map((v) => lexer.tokens[v + 1]);
}

/**
 * get the range of the class between "{" and "}"
 * @param lexer {@link Lexer}
 * @returns return the name of the class, and the start offset and end offset of the class
 */
export async function getClassRange(
	lexer: Lexer
): Promise<{ name: string; range: number[] }[]> {
	const datas: { name: string; range: number[] }[] = [];

	const classes = lexer.tokens.filter((v) => v.type == TokenType.CLASS);
	for (const cls of classes) {
		const currentIndex = lexer.tokens.indexOf(cls);
		let index = currentIndex + 2;

		//const start = lexer.tokens[currentIndex];

		let parenCount = 0;
		for (; index < lexer.tokens.length; index++) {
			const current = lexer.tokens[index];
			if (current.type == TokenType.LCP) parenCount++;
			else if (current.type == TokenType.RCP) parenCount--;
			if (parenCount == 0) {
				datas.push({
					name: lexer.tokens[currentIndex + 1].value,
					range: [currentIndex, current.pos],
				});
				break;
			}
		}
	}

	return datas;
}

/**
 * change offset to position data
 * @param offset offset of the text
 * @param text the text of the file
 * @returns line and character pos - {@link Position}
 */
export async function offsetToPosition(
	offset: number,
	text: string
): Promise<Position> {
	let line = 0;
	let character = 0;

	for (let i = 0; i < offset; i++) {
		if (text[i] === "\n") {
			line++;
			character = 0;
		} else {
			character++;
		}
	}

	return { line, character };
}

/**
 * get current statement
 * @example function foo() {
 * @example $bar = 3;
 * @example foo.bar();
 * @param lexer {@link Lexer}
 * @param token the currentToken - {@link TokenData}
 * @returns `undefined` or the statement - {@link TokenData}
 */
export async function getCurrentStatement(
	lexer: Lexer,
	token: TokenData
): Promise<TokenData[] | undefined> {
	const splited = await splitTokenArray(lexer.tokens);
	for (const arr of splited) {
		for (const tarr of arr) {
			if (tarr.pos == token.pos) return arr;
		}
	}
}

/**
 * split string into tokenizable string
 * @param text the string
 * @returns splited string
 */
export async function splitTokenString(text: string): Promise<string[]> {
	return text.split(Lexer.splitPattern).filter((v) => v != undefined);
}

/**
 * get the literal with string init
 * @example foo.bar();
 * @param statement see {@link getCurrentStatement}
 * @param start `optional` it indicate the start of the searching
 * @returns string of the literal with dot or `undefined`
 */
export async function getLiteralWithDot(
	statement: TokenData[],
	start?: TokenData
): Promise<string | undefined> {
	let str = "";
	let i = 0;

	if (!start) {
		for (; i < statement.length; i++) {
			const ct = statement[i];
			const next = statement[i + 1];
			if (next && next.type == TokenType.DOT) {
				i += 1;
				str += `${ct.value}.`;
			} else {
				str += ct.value;
				break;
			}
		}

		let temp = "";

		for (let i = 0; i != -1; i--) {
			const ct = statement[i];
			const next = statement[i - 1];
			if (next && next.type == TokenType.DOT) {
				i -= 1;
				temp += `${ct.value}.`;
			} else {
				break;
			}
		}

		return temp + str;
	} else {
		const index = statement.findIndex((v) => v.pos == start.pos);

		let str = "";

		//loop forward
		for (let i = index; i < statement.length; i++) {
			const current = statement[i];
			const next = statement[i + 1];
			if (!current) break;

			if (
				next &&
				current.type == TokenType.DOT &&
				next.type != TokenType.LITERAL
			) {
				break;
			} else if (
				next &&
				current.type == TokenType.LITERAL &&
				next.type != TokenType.DOT
			) {
				str += current.value;
				break;
			}

			str += current.value;
		}

		const temp: string[] = [];

		//loop backward
		for (let i = index - 1; i != -1; i--) {
			const current = statement[i];
			const next = statement[i - 1];
			if (!current) break;
			if (
				next &&
				current.type == TokenType.DOT &&
				next.type != TokenType.LITERAL
			) {
				break;
			} else if (
				next &&
				current.type == TokenType.LITERAL &&
				next.type != TokenType.DOT
			) {
				temp.push(current.value);
				break;
			} else if (
				next &&
				current.type != TokenType.LITERAL &&
				next.type != TokenType.DOT &&
				current.type != TokenType.DOT &&
				next.type != TokenType.LITERAL
			) {
				break;
			}
			temp.push(current.value);
		}

		return temp.reverse().join("") + str;
	}
}

export async function getPreviousStatementStart(
	text: string,
	offset: number
): Promise<number> {
	let leftCurrentStatement = false;
	for (let i = offset; i >= 0; i--) {
		const c = text[i];

		if (leftCurrentStatement && c === ";") {
			return i;
		}

		leftCurrentStatement =
			(!leftCurrentStatement && c === ";") || leftCurrentStatement;
	}
	return -1;
}

export async function getNextStatementEnd(
	text: string,
	offset: number
): Promise<number> {
	let leftCurrentStatement = false;
	for (let i = offset; i < text.length; i++) {
		const c = text[i];

		if (leftCurrentStatement && c === ";") {
			return i;
		}

		leftCurrentStatement =
			(!leftCurrentStatement && c === ";") || leftCurrentStatement;
	}
	return -1;
}

// //#region commands
// export function joinBrackets(tokens: TokenData[]): TokenData[] {
// 	const datas: TokenData[] = [];

// 	for (let i = 0; i < tokens.length; i++) {
// 		const token = tokens[i];
// 		if (token.type === TokenType.LCP) {
// 			let counter = 0;
// 			let text = "";
// 			for (; i < tokens.length; i++) {
// 				const c = tokens[i];
// 				if (c.type === TokenType.LCP) counter++;
// 				else if (c.type === TokenType.RCP) counter--;
// 				if (counter === 0) {
// 					text += c.value;
// 					break;
// 				}
// 				text += c.value;
// 			}
// 			datas.push({
// 				type: TokenType.MODIFIED_CP,
// 				pos: token.pos,
// 				value: text,
// 			});
// 		} else if (token.type === TokenType.LMP) {
// 			let counter = 0;
// 			let text = "";
// 			for (; i < tokens.length; i++) {
// 				const c = tokens[i];
// 				if (c.type === TokenType.LMP) counter++;
// 				else if (c.type === TokenType.RMP) counter--;
// 				if (counter === 0) {
// 					text += c.value;
// 					break;
// 				}
// 				text += c.value;
// 			}
// 			datas.push({
// 				type: TokenType.MODIFIED_MP,
// 				pos: token.pos,
// 				value: text,
// 			});
// 		} else datas.push(token);
// 	}

// 	return datas;
// }

// export function joinFunction(tokens: TokenData[]): TokenData[] {
// 	for (let i = 0; i < tokens.length; i++) {
// 		const token = tokens[i];
// 		if (
// 			token.type === TokenType.LITERAL &&
// 			tokens[i + 1] &&
// 			tokens[i + 2] &&
// 			tokens[i + 1].type === TokenType.LPAREN &&
// 			tokens[i + 2].type === TokenType.RPAREN
// 		) {
// 			token.value = `${token.value}()`;
// 			token.type = TokenType.COMMAND_FC;
// 			delete tokens[i + 1];
// 			delete tokens[i + 2];
// 			i += 2;
// 		} else if (
// 			token.type === TokenType.VARIABLE &&
// 			tokens[i + 1].value === ".get" &&
// 			tokens[i + 2].type === TokenType.LPAREN &&
// 			tokens[i + 3].type === TokenType.RPAREN
// 		) {
// 			token.value = `${token.value}.get()`;
// 			token.type = TokenType.COMMAND_FC;
// 			delete tokens[i + 1];
// 			delete tokens[i + 2];
// 			delete tokens[i + 3];
// 			i += 3;
// 		}
// 	}

// 	return tokens.filter((v) => v);
// }

// export function getTokensByRange(
// 	tokens: TokenData[],
// 	range: StatementRange
// ): TokenData[] {
// 	return tokens.filter((v) => v.pos <= range.end && v.pos >= range.start);
// }

// export function joinNBT(tokens: TokenData[]): TokenData[] {
// 	const datas: TokenData[] = [];
// 	for (let i = 0; i < tokens.length; i++) {
// 		const current = tokens[i];
// 		const next = tokens[i + 1];
// 		if (
// 			next &&
// 			next.type == TokenType.MODIFIED_CP &&
// 			current.type == TokenType.LITERAL
// 		) {
// 			datas.push({
// 				type: TokenType.NBT,
// 				pos: current.pos,
// 				value: current.value + next.value,
// 			});
// 			i++;
// 		} else if (
// 			next &&
// 			next.type == TokenType.MODIFIED_MP &&
// 			current.type == TokenType.SELECTOR
// 		) {
// 			datas.push({
// 				type: TokenType.SELECTOR_INFO,
// 				pos: current.pos,
// 				value: current.value + next.value,
// 			});
// 			i++;
// 		} else datas.push(current);
// 	}
// 	return datas;
// }

// export function joinDot(tokens: TokenData[]): TokenData[] {
// 	const datas: TokenData[] = [];
// 	for (let i = 0; i < tokens.length; i++) {
// 		const current = tokens[i];
// 		const dLastIndex = datas.length - 1;
// 		const dCurrent = datas[dLastIndex];

// 		//continue if it is the start
// 		if (dLastIndex === -1) {
// 			datas.push(current);
// 			continue;
// 		}

// 		//join dots
// 		if (dCurrent) {
// 			if (
// 				dCurrent.type === TokenType.LITERAL &&
// 				current.type === TokenType.DOT
// 			) {
// 				datas[dLastIndex].value += ".";
// 			} else if (
// 				dCurrent.value.endsWith(".") &&
// 				(current.type === TokenType.LITERAL ||
// 					current.type === TokenType.COMMAND_LITERAL)
// 			) {
// 				datas[dLastIndex].value += current.value;
// 			} else datas.push(current);
// 		}
// 	}

// 	return datas;
// }

// export function joinNamespace(tokens: TokenData[]): TokenData[] {
// 	const datas: TokenData[] = [];

// 	for (let i = 0; i < tokens.length; i++) {
// 		const f = tokens[i];
// 		const s = tokens[i + 1];
// 		const t = tokens[i + 2];
// 		if (s && t && f.value === "minecraft" && s.type === TokenType.COLON) {
// 			datas.push({
// 				type: TokenType.COMMAND_NAMESAPCE,
// 				pos: f.pos,
// 				value: `${f.value}${s.value}${t.value}`,
// 			});
// 			i += 2;
// 		} else if (s && t && s.type === TokenType.COLON) {
// 			datas.push({
// 				type: TokenType.COMMAND_NAMESAPCE,
// 				pos: f.pos,
// 				value: `${f.value}${s.value}${t.value}`,
// 			});
// 			i += 2;
// 		} else datas.push(f);
// 	}

// 	return datas;
// }

// export function joinNumber(tokens: TokenData[]): TokenData[] {
// 	const datas: TokenData[] = [];

// 	for (let i = 0; i < tokens.length; i++) {
// 		const token = tokens[i];
// 		if (
// 			!isNaN(+token.value) &&
// 			tokens[i + 1] &&
// 			["l", "s", "b"].includes(tokens[i + 1].value)
// 		) {
// 			token.value += tokens[i + 1].value;
// 			datas.push(token);
// 			i++;
// 		} else datas.push(token);
// 	}

// 	return datas;
// }

// /**
//  * turns tokens into a recognizable commands
//  * @param tokens lexer tokens
//  * @returns	tokens that parsed
//  */
// export function joinCommandData(tokens: TokenData[]): TokenData[] {
// 	logger.verbose(`parsing tokens ${JSON.stringify(tokens)}`);

// 	const jd = joinDot(tokens);
// 	const jb = joinBrackets(jd);
// 	const jn = joinNBT(jb);
// 	const jf = joinFunction(jn);
// 	const jna = joinNamespace(jf);
// 	const jnum = joinNumber(jna);

// 	//remove undefined
// 	return jnum.filter((v) => v);
// }
// //#endregion
