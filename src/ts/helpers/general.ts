import { Lexer, TokenData, TokenType } from "../lexer";
import { currnetFile } from "../server/server";

export interface JMCFile {
	path: string;
	lexer: Lexer;
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

export async function getVariables(lexer: Lexer): Promise<TokenData[]> {
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

export async function getFunctions(lexer: Lexer): Promise<TokenData[]> {
	const classRanges = await getClassRange(lexer);
	const funcID = lexer.tokens
		.filter((v) => v.type == TokenType.FUNCTION)
		.map((v) => lexer.tokens.indexOf(v));
	return funcID.map((v) => {
		const token = lexer.tokens[v + 1];
		for (const range of classRanges) {
			if (v > range.range[0] && v < range.range[1]) {
				token.value = `${range.name}.${token.value}`;
				return token;
			}
		}
		return token;
	});
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
		let index = currentIndex + 3;

		const start = lexer.tokens[currentIndex + 1];
		let end: TokenData | undefined;

		let parenCount = 1;
		for (; index < lexer.tokens.length; index++) {
			const current = lexer.tokens[index];
			if (current.type == TokenType.LCP) parenCount++;
			else if (current.type == TokenType.RCP) parenCount--;
			if (parenCount == 0) {
				end = current;
				break;
			}
		}

		if (end) {
			datas.push({
				name: start.value,
				range: [currentIndex, lexer.tokens.indexOf(end)],
			});
		}
	}

	return datas;
}
