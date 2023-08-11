import { MacrosData, splitTokenArraySync } from "./helpers/general";
import ExtensionLogger from "./server/extlogger";

export enum ErrorType {
	INVALID,
	UNKNWON_TOKEN,
	MISSING,
}

export interface ErrorData {
	type: ErrorType;
	message: string;
	pos: number;
	length: number;
	token?: TokenData;
}

/**
 * All tokens avaliable
 */
export enum TokenType {
	COMMENT,
	FUNCTION,
	CLASS,
	NEW,
	IF,
	ELSE,
	FOR,
	WHILE,
	DO,
	SWITCH,
	CASE,
	OLD_IMPORT,
	SEMI,
	COLON,
	ARROW_FUNC,
	LITERAL,
	INT,
	FLOAT,
	BOOL,
	STRING,
	VARIABLE,
	LPAREN,
	RPAREN,
	LCP,
	RCP,
	OP_EQ,
	OP_PLUSEQ,
	OP_MINUSEQ,
	OP_DIVIDEEQ,
	OP_MULTIPLYEQ,
	OP_REMAINDEREQ,
	EQUAL,
	NOT_EQUAL,
	AND,
	OR,
	NOT,
	GREATER_THEN,
	LESS_THEN,
	GREATER_OR_EQ_THEN,
	LESS_OR_EQ_THEN,
	DOT,
	COMMA,
	MACROS,
	OPERATION,
	COMPARASION,
	COMMAND_LITERAL,
	UNKNOWN,
	IMPORT,
	RMP,
	LMP,
	COMMAND_SELECTOR,
	MODIFIED_CP,
	MODIFIED_LPAREN,
	MODIFIED_MP,
	NBT,
	SELECTOR_INFO,
	COMMAND_INFO,
	DELETED,
	COMMAND_FINAL,
	COMMAND_INT,
	COMMAND_FLOAT,
	COMMAND_VECTOR,
	COMMAND_ITEM_STACK,
	COMMAND_STRING,
	COMMAND_INVALID,
	COMMAND_FC,
	COMMAND_VALUE,
	COMMAND_NUMBER,
	COMMAND_NAMESAPCE,
}

/**
 * use to identify a string
 */
interface Token {
	regex: RegExp;
	token: TokenType;
}

export interface TokenData {
	type: TokenType;
	pos: number;
	value: string;
}

const Tokens: Token[] = [
	{
		regex: /^\/\/.*$/,
		token: TokenType.COMMENT,
	},
	{
		regex: /^function$/,
		token: TokenType.FUNCTION,
	},
	{
		regex: /^@[parse]$/,
		token: TokenType.COMMAND_SELECTOR,
	},
	{
		regex: /^class$/,
		token: TokenType.CLASS,
	},
	{
		regex: /^new$/,
		token: TokenType.NEW,
	},
	{
		regex: /^if$/,
		token: TokenType.IF,
	},
	{
		regex: /^else$/,
		token: TokenType.ELSE,
	},
	{
		regex: /^for$/,
		token: TokenType.FOR,
	},
	{
		regex: /^while$/,
		token: TokenType.WHILE,
	},
	{
		regex: /^do$/,
		token: TokenType.DO,
	},
	{
		regex: /^switch$/,
		token: TokenType.SWITCH,
	},
	{
		regex: /^case$/,
		token: TokenType.CASE,
	},
	{
		regex: /^@import$/,
		token: TokenType.OLD_IMPORT,
	},
	{
		regex: /^import$/,
		token: TokenType.IMPORT,
	},
	{
		regex: /^;$/,
		token: TokenType.SEMI,
	},
	{
		regex: /^\:$/,
		token: TokenType.COLON,
	},
	{
		regex: /^\=\>$/,
		token: TokenType.ARROW_FUNC,
	},
	{
		regex: /(^"([\w' ]|(\\"))*"$)|(^'([\w" ]|(\\'))*'$)/,
		token: TokenType.STRING,
	},
	{
		regex: /^\d+$/,
		token: TokenType.INT,
	},
	{
		regex: /^-?\d*\.?\d+$/,
		token: TokenType.FLOAT,
	},
	{
		regex: /^true|false$/,
		token: TokenType.BOOL,
	},
	{
		regex: /^\$[a-zA-Z0-9_]+$/,
		token: TokenType.VARIABLE,
	},
	{
		regex: /^\($/,
		token: TokenType.LPAREN,
	},
	{
		regex: /^\)$/,
		token: TokenType.RPAREN,
	},
	{
		regex: /^\{$/,
		token: TokenType.LCP,
	},
	{
		regex: /^\}$/,
		token: TokenType.RCP,
	},
	{
		regex: /^\[$/,
		token: TokenType.LMP,
	},
	{
		regex: /^\]$/,
		token: TokenType.RMP,
	},
	{
		regex: /^=$/,
		token: TokenType.OP_EQ,
	},
	{
		regex: /^\+=$/,
		token: TokenType.OP_PLUSEQ,
	},
	{
		regex: /^-=$/,
		token: TokenType.OP_MINUSEQ,
	},
	{
		regex: /^\*=$/,
		token: TokenType.OP_MULTIPLYEQ,
	},
	{
		regex: /^\/=$/,
		token: TokenType.OP_DIVIDEEQ,
	},
	{
		regex: /^%=$/,
		token: TokenType.OP_REMAINDEREQ,
	},
	{
		regex: /^==$/,
		token: TokenType.EQUAL,
	},
	{
		regex: /^!=$/,
		token: TokenType.NOT_EQUAL,
	},
	{
		regex: /^&&$/,
		token: TokenType.AND,
	},
	{
		regex: /^\|\|$/,
		token: TokenType.OR,
	},
	{
		regex: /^>$/,
		token: TokenType.GREATER_THEN,
	},
	{
		regex: /^>=$/,
		token: TokenType.GREATER_OR_EQ_THEN,
	},
	{
		regex: /^<$/,
		token: TokenType.LESS_THEN,
	},
	{
		regex: /^<=$/,
		token: TokenType.LESS_OR_EQ_THEN,
	},
	{
		regex: /^!$/,
		token: TokenType.NOT,
	},
	{
		regex: /^\.$/,
		token: TokenType.DOT,
	},
	{
		regex: /^\,$/,
		token: TokenType.COMMA,
	},
	{
		regex: /^[a-zA-Z_][a-zA-Z0-9_]*$/,
		token: TokenType.LITERAL,
	},
	{
		regex: /^.+$/,
		token: TokenType.UNKNOWN,
	},
];

/**
 * operators
 */
export const TOKEN_OPERATION: TokenType[] = [
	TokenType.OP_DIVIDEEQ,
	TokenType.OP_EQ,
	TokenType.OP_MINUSEQ,
	TokenType.OP_MULTIPLYEQ,
	TokenType.OP_PLUSEQ,
	TokenType.OP_REMAINDEREQ,
];

export const END_TOKEN: TokenType[] = [
	TokenType.SEMI,
	TokenType.LCP,
	TokenType.RCP,
];

const TokenPatterns: TokenType[][] = [
	//Function pattern
	[
		TokenType.FUNCTION,
		TokenType.LITERAL,
		TokenType.LPAREN,
		TokenType.RPAREN,
		TokenType.LCP,
	],
	[TokenType.VARIABLE, TokenType.OPERATION, TokenType.INT],
	[TokenType.VARIABLE, TokenType.OPERATION, TokenType.BOOL],
];

/**
 * deprecated tokens
 */
export const DEPRECATED: TokenType[] = [TokenType.OLD_IMPORT];
/**
 * lexer of jmc
 */
export class Lexer {
	tokens: TokenData[];

	private logger: ExtensionLogger;
	private raw: string[];
	private trimmed: string[];
	private currentIndex: number;
	private macros: string[];

	/**
	 * create a new lexer
	 * @param text text of a file
	 * @param macros see {@link MacrosData}
	 */
	constructor(text: string, macros: MacrosData[]) {
		this.logger = new ExtensionLogger("Lexer");
		try {
			this.currentIndex = 0;
			this.raw = text
				.split(
					/(\/\/.*)|(-?\d*\.?\d+)|(["\'].*["\'])|(\s|\;|\{|\}|\[|\]|\(|\)|\|\||&&|==|!=|!|,|\.|\:|\=\>|\=)/m
				)
				.filter((v) => v != undefined);
			this.trimmed = this.raw.map((v) => v.trim());
			this.macros = macros.map((v) => v.target);
			this.tokens = this.init();
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
		} catch (e: any) {
			this.logger.fatal(e);
			console.log(e);
			throw new e();
		}
	}

	/**
	 * get the token type of a string
	 * @param text the string to be tokenized
	 * @returns see {@link TokenType}
	 */
	static getTokenType(text: string): TokenType {
		return (
			Tokens.find((v) => v.regex.test(text))?.token ?? TokenType.UNKNOWN
		);
	}

	/**
	 * initialize the tokens list
	 * @returns tokenzied array
	 */
	private init(): TokenData[] {
		const datas: TokenData[] = [];
		for (; this.currentIndex < this.trimmed.length; this.currentIndex++) {
			const result = this.tokenize(
				this.trimmed[this.currentIndex],
				this.GetPosition(this.currentIndex),
				datas
			);
			if (result) datas.push(result);
		}
		return datas;
	}

	/**
	 * tokenize string
	 * @param current the text going to tokenize
	 * @param pos the offset of the text
	 * @param datas the previous tokenized datas
	 * @returns tokenzied data
	 */
	tokenize(
		current: string,
		pos: number,
		datas: TokenData[]
	): TokenData | null {
		const result = Tokens.find((v) => v.regex.test(current));
		if (this.macros.includes(current)) {
			return {
				type: TokenType.MACROS,
				pos: pos,
				value: current,
			};
		} else if (result) {
			if (
				result.token == TokenType.SWITCH &&
				datas[datas.length - 1] &&
				datas[datas.length - 1].type == TokenType.DOT
			) {
				result.token = TokenType.LITERAL;
			}
			return {
				type: result.token,
				pos: pos,
				value: current,
			};
		}
		return null;
	}

	/**
	 * get the position of the current index
	 * @returns offset of the index
	 */
	private GetPosition(index: number): number {
		const t = this.raw.slice(0, index).join("");

		return t.length;
	}
}

/**
 * @todo
 */
export class ErrorLexer {
	private lexer: Lexer;
	private index: number;
	private tokens: TokenData[][];

	constructor(lexer: Lexer) {
		this.lexer = lexer;
		this.tokens = splitTokenArraySync(this.lexer.tokens);
		this.index = 0;
	}

	getErrors(): ErrorData[] {
		const datas: ErrorData[] = [];
		const splited = this.tokens.map((v) => {
			const value = v[v.length - 1];
			if (
				v[0].type == TokenType.FUNCTION &&
				value.type != TokenType.LCP
			) {
				datas.push({
					type: ErrorType.MISSING,
					message: "Missing Token '{'",
					pos: value.pos + value.value.length + 1,
					length: 1,
					token: value,
				});
			} else if (v.length == 1 && END_TOKEN.includes(value.type)) {
				return v;
			} else if (value.type != TokenType.SEMI) {
				datas.push({
					type: ErrorType.MISSING,
					message: "Missing Semicolon",
					pos: value.pos + value.value.length + 1,
					length: 1,
					token: value,
				});
			}
			return v;
		});
		return datas;
	}

	// getErrors(): ErrorData[] {
	// 	const datas: ErrorData[] = [];
	// 	for (; this.index < this.tokens.length; this.index++) {
	// 		const token = this.tokens[this.index];
	// 		const firstType = token[0].type;
	// 		const query = TokenPatterns.filter((v) => v[0] == firstType);
	// 		if (query.length > 0) {
	// 			for (const q of query) {
	// 				switch (firstType) {
	// 					default: {
	// 						for (let i = 0; i < token.length; i++) {
	// 							const cToken = token[i].type;
	// 							const rToken = q[i];
	// 							if (cToken != rToken) {
	// 								datas.push({
	// 									type: ErrorType.INVALID,
	// 									message: `Expected ${TokenType[rToken]}, but got ${TokenType[cToken]}`,
	// 								});
	// 							}
	// 						}
	// 						if (q.length > token.length) {
	// 							const expect = q
	// 								.slice(token.length - 1)
	// 								.map((v) => TokenType[v])
	// 								.join(",");
	// 							datas.push({
	// 								type: ErrorType.MISSING,
	// 								message: `Missing values: ${expect}`,
	// 							});
	// 						}
	// 						break;
	// 					}
	// 				}
	// 			}
	// 		}
	// 	}
	// 	return datas;
	// }
}
