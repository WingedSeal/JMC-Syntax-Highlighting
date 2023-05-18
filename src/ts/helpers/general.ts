import { Lexer, TokenData, TokenType } from "../lexer";
import { HeaderParser } from "../parseHeader";

export interface JMCFile {
	path: string;
	lexer: Lexer;
}

export interface HJMCFile {
	path: string;
	parser: HeaderParser;
}

export interface ExtractedTokens {
	variables: FileTokens[];
	funcs: FileTokens[];
}

interface FileTokens {
	path: string;
	tokens: TokenData[];
}

type IsDuplicate<T> = (a: T, b: T) => boolean;
export const removeDuplicate = <TItems>(
	collection: TItems[],
	isDuplicate: IsDuplicate<TItems>
) =>
	collection.filter(
		(item, index, items: TItems[]) =>
			items.findIndex((secondItem) => isDuplicate(item, secondItem)) ===
			index
	);

export function getIndexByOffset(lexer: Lexer, offset: number): number {
	return lexer.tokens.findIndex((val) => {
		return val.pos > offset;
	});
}

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

export async function getVariables(lexer: Lexer) {
	return lexer.tokens.filter((v) => v.type == TokenType.VARIABLE);
}

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
			value: value.join(""),
		};
	});
}

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

export async function getClasses(lexer: Lexer): Promise<TokenData[]> {
	const classID = lexer.tokens
		.filter((v) => v.type == TokenType.CLASS)
		.map((v) => lexer.tokens.indexOf(v));
	return classID.map((v) => lexer.tokens[v + 1]);
}

export async function getClassRange(
	lexer: Lexer
): Promise<{ name: string; range: number[] }[]> {
	const datas: { name: string; range: number[] }[] = [];

	const classes = lexer.tokens.filter((v) => v.type == TokenType.CLASS);
	for (const cls of classes) {
		const currentIndex = lexer.tokens.indexOf(cls);
		let index = currentIndex + 2;

		const start = lexer.tokens[currentIndex];

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
