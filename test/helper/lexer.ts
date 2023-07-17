import { MacrosData } from "../../src/ts/helpers/general";
import { Lexer, TokenData } from "../../src/ts/lexer";

export default class LexerHelper {
	tokens: TokenData[];
	expected: TokenData[];

	constructor(
		text: string,
		expected: TokenData[],
		macros: MacrosData[] = []
	) {
		const lexer = new Lexer(text, macros);
		this.tokens = lexer.tokens;
		this.expected = expected;
	}
}
