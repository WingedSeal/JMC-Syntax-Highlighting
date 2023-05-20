import { MacrosData, splitTokenArray } from "./helpers/general";

interface ParserModifier {
	amount?: string;
	type?: string;
	min?: number;
	max?: number;
}

export enum ErrorType {
	INVALID,
	UNKNWON_TOKEN,
	MISSING,
}

export interface ErrorData {
	type: ErrorType;
	message: string;
	token?: TokenData;
}

export enum TokenType {
	COMMENT,
	FUNCTION,
	CLASS,
	NEW,
	IF,
	ELSE,
	SWITCH,
	CASE,
	IMPORT,
	SEMI,
	LITERAL,
	INT,
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
}

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
		regex: /^switch$/,
		token: TokenType.SWITCH,
	},
	{
		regex: /^case$/,
		token: TokenType.CASE,
	},
	{
		regex: /^@import$/,
		token: TokenType.IMPORT,
	},
	{
		regex: /^;$/,
		token: TokenType.SEMI,
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
];

const StatementPatterns: TokenType[][] = [
	[TokenType.FUNCTION, TokenType.LITERAL, TokenType.LPAREN, TokenType.RPAREN],
];

interface CommandData {
	type: string;
	name: string;
	executable: boolean;
	redirects: string[];
	childrens: CommandData[];
	parser: {
		parser: string;
		modifier?: ParserModifier;
	};
}

export class Lexer {
	public tokens: TokenData[];
	private raw: string[];
	private trimmed: string[];
	private currentIndex: number;
	private macros: string[];

	/**
	 *
	 * @param text
	 */
	constructor(text: string, macros: MacrosData[]) {
		this.currentIndex = 0;
		this.raw = text
			.split(/(\/\/.*)|(\s|\;|\{|\}|\(|\)|\|\||&&|==|!=|!|,|\.)/m)
			.filter((v) => v != undefined);
		this.trimmed = this.raw.map((v) => v.trim());
		this.macros = macros.map((v) => v.target);
		this.tokens = this.Tokenize();
	}

	/**
	 *
	 * @returns
	 */
	private Tokenize(): TokenData[] {
		const datas: TokenData[] = [];

		for (let i = 0; i < this.trimmed.length; i++) {
			const current = this.trimmed[i];
			const result = Tokens.find((v) => v.regex.test(current));
			const pos = this.GetPosition();
			if (this.macros.includes(current)) {
				datas.push({
					type: TokenType.MACROS,
					pos: pos,
					value: this.trimmed[this.currentIndex],
				});
			} else if (result != undefined) {
				datas.push({
					type: result.token,
					pos: pos,
					value: this.trimmed[this.currentIndex],
				});
			}
			this.currentIndex++;
		}

		return datas;
	}

	/**
	 *
	 * @returns
	 */
	private GetPosition(): number {
		const t = this.raw.slice(0, this.currentIndex).join("");

		return t.length;
	}
}

export class ErrorLexer {
	private lexer: Lexer;
	private index: number;
	private tokens: TokenData[][];

	constructor(lexer: Lexer) {
		this.lexer = lexer;
		this.tokens = [];
		splitTokenArray(this.lexer.tokens).then((v: TokenData[][]) => {
			this.tokens = v.map((v) =>
				v.filter((e) => e.type != TokenType.COMMENT)
			);
		});
		this.index = 0;
	}

	getErrors(): ErrorData[] {
		const datas: ErrorData[] = [];
		for (; this.index < this.tokens.length; this.index++) {
			const tokenType = this.tokens[this.index].map((v) => v.type);
		}
		return datas;
	}
}
