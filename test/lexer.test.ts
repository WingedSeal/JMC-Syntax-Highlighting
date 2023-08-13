import { TokenData, TokenType } from "../src/ts/lexer";
import { describe, expect, test } from "@jest/globals";
import LexerHelper from "./helper/lexer";

describe("Tokenizing", () => {
	describe("Basic", () => {
		test("Comment", () => {
			const text = `//test\nhello;`;
			const expected: TokenData[] = [
				{ type: TokenType.COMMENT, pos: 0, value: "//test" },
				{ type: TokenType.LITERAL, pos: 7, value: "hello" },
				{ type: TokenType.SEMI, pos: 12, value: ";" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Function", () => {
			const text = `function amogus() {}`;
			const expected: TokenData[] = [
				{ type: TokenType.FUNCTION, pos: 0, value: "function" },
				{ type: TokenType.LITERAL, pos: 9, value: "amogus" },
				{ type: TokenType.LPAREN, pos: 15, value: "(" },
				{ type: TokenType.RPAREN, pos: 16, value: ")" },
				{ type: TokenType.LCP, pos: 18, value: "{" },
				{ type: TokenType.RCP, pos: 19, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Class", () => {
			const text = `class test {}`;
			const expected: TokenData[] = [
				{ type: TokenType.CLASS, pos: 0, value: "class" },
				{ type: TokenType.LITERAL, pos: 6, value: "test" },
				{ type: TokenType.LCP, pos: 11, value: "{" },
				{ type: TokenType.RCP, pos: 12, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("New", () => {
			const text = `new advancements(rct) {
    "criteria": {
    }
}`;
			const expected: TokenData[] = [
				{ type: TokenType.NEW, pos: 0, value: "new" },
				{ type: TokenType.LITERAL, pos: 4, value: "advancements" },
				{ type: TokenType.LPAREN, pos: 16, value: "(" },
				{ type: TokenType.LITERAL, pos: 17, value: "rct" },
				{ type: TokenType.RPAREN, pos: 20, value: ")" },
				{ type: TokenType.LCP, pos: 22, value: "{" },
				{ type: TokenType.STRING, pos: 28, value: '"criteria"' },
				{ type: TokenType.COLON, pos: 38, value: ":" },
				{ type: TokenType.LCP, pos: 40, value: "{" },
				{ type: TokenType.RCP, pos: 46, value: "}" },
				{ type: TokenType.RCP, pos: 48, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("IfElse", () => {
			const text = `if (1 == 1) {} else {}`;
			const expected: TokenData[] = [
				{ type: TokenType.IF, pos: 0, value: "if" },
				{ type: TokenType.LPAREN, pos: 3, value: "(" },
				{ type: TokenType.INT, pos: 4, value: "1" },
				{ type: TokenType.EQUAL, pos: 6, value: "==" },
				{ type: TokenType.INT, pos: 9, value: "1" },
				{ type: TokenType.RPAREN, pos: 10, value: ")" },
				{ type: TokenType.LCP, pos: 12, value: "{" },
				{ type: TokenType.RCP, pos: 13, value: "}" },
				{ type: TokenType.ELSE, pos: 15, value: "else" },
				{ type: TokenType.LCP, pos: 20, value: "{" },
				{ type: TokenType.RCP, pos: 21, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Switch", () => {
			const text = `switch ($gun) {case 1: how;}`;
			const expected: TokenData[] = [
				{ type: TokenType.SWITCH, pos: 0, value: "switch" },
				{ type: TokenType.LPAREN, pos: 7, value: "(" },
				{ type: TokenType.VARIABLE, pos: 8, value: "$gun" },
				{ type: TokenType.RPAREN, pos: 12, value: ")" },
				{ type: TokenType.LCP, pos: 14, value: "{" },
				{ type: TokenType.CASE, pos: 15, value: "case" },
				{ type: TokenType.INT, pos: 20, value: "1" },
				{ type: TokenType.COLON, pos: 21, value: ":" },
				{ type: TokenType.LITERAL, pos: 23, value: "how" },
				{ type: TokenType.SEMI, pos: 26, value: ";" },
				{ type: TokenType.RCP, pos: 27, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Import", () => {
			const text = `import "*";`;
			const expected: TokenData[] = [
				{ type: TokenType.IMPORT, pos: 0, value: "import" },
				{ type: TokenType.STRING, pos: 7, value: '"*"' },
				{ type: TokenType.SEMI, pos: 10, value: ";" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Function Call", () => {
			const text = `guns.test();`;
			const expected: TokenData[] = [
				{ type: TokenType.LITERAL, pos: 0, value: "guns" },
				{ type: TokenType.DOT, pos: 4, value: "." },
				{ type: TokenType.LITERAL, pos: 5, value: "test" },
				{ type: TokenType.LPAREN, pos: 9, value: "(" },
				{ type: TokenType.RPAREN, pos: 10, value: ")" },
				{ type: TokenType.SEMI, pos: 11, value: ";" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("For", () => {
			const text = `for (x;x;x;) {}`;
			const expected: TokenData[] = [
				{ type: TokenType.FOR, pos: 0, value: "for" },
				{ type: TokenType.LPAREN, pos: 4, value: "(" },
				{ type: TokenType.LITERAL, pos: 5, value: "x" },
				{ type: TokenType.SEMI, pos: 6, value: ";" },
				{ type: TokenType.LITERAL, pos: 7, value: "x" },
				{ type: TokenType.SEMI, pos: 8, value: ";" },
				{ type: TokenType.LITERAL, pos: 9, value: "x" },
				{ type: TokenType.SEMI, pos: 10, value: ";" },
				{ type: TokenType.RPAREN, pos: 11, value: ")" },
				{ type: TokenType.LCP, pos: 13, value: "{" },
				{ type: TokenType.RCP, pos: 14, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("While", () => {
			const text = `while (x;x;x;) {}`;
			const expected: TokenData[] = [
				{ type: TokenType.WHILE, pos: 0, value: "while" },
				{ type: TokenType.LPAREN, pos: 6, value: "(" },
				{ type: TokenType.LITERAL, pos: 7, value: "x" },
				{ type: TokenType.SEMI, pos: 8, value: ";" },
				{ type: TokenType.LITERAL, pos: 9, value: "x" },
				{ type: TokenType.SEMI, pos: 10, value: ";" },
				{ type: TokenType.LITERAL, pos: 11, value: "x" },
				{ type: TokenType.SEMI, pos: 12, value: ";" },
				{ type: TokenType.RPAREN, pos: 13, value: ")" },
				{ type: TokenType.LCP, pos: 15, value: "{" },
				{ type: TokenType.RCP, pos: 16, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Do", () => {
			const text = `do {} while (x;x;x;)`;
			const expected: TokenData[] = [
				{ type: TokenType.DO, pos: 0, value: "do" },
				{ type: TokenType.LCP, pos: 3, value: "{" },
				{ type: TokenType.RCP, pos: 4, value: "}" },
				{ type: TokenType.WHILE, pos: 6, value: "while" },
				{ type: TokenType.LPAREN, pos: 12, value: "(" },
				{ type: TokenType.LITERAL, pos: 13, value: "x" },
				{ type: TokenType.SEMI, pos: 14, value: ";" },
				{ type: TokenType.LITERAL, pos: 15, value: "x" },
				{ type: TokenType.SEMI, pos: 16, value: ";" },
				{ type: TokenType.LITERAL, pos: 17, value: "x" },
				{ type: TokenType.SEMI, pos: 18, value: ";" },
				{ type: TokenType.RPAREN, pos: 19, value: ")" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Multiline String", () => {
			const text = `\`amogus 
a\``;
			const expected: TokenData[] = [
				{
					type: TokenType.MULTILINE_STRING,
					pos: 0,
					value: "`amogus \na`",
				},
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});
	});

	describe.skip("Command", () => {
		test("item", () => {
			const text = `item modify entity @s weapon.mainhand gun:gun_interval_changed;`;
			const expected: TokenData[] = [
				{ type: 42, pos: 0, value: "item" },
				{ type: 42, pos: 5, value: "modify" },
				{ type: 42, pos: 12, value: "entity" },
				{ type: 47, pos: 19, value: "@s" },
				{ type: 63, pos: 22, value: "weapon.mainhand" },
				{ type: 61, pos: 38, value: "gun:gun_interval_changed" },
				{ type: 9, pos: 62, value: ";" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("give", () => {
			const text = `give @s minecraft:carrot_on_a_stick{ammo:1b,id:1,maxAmmo:25,ammo_count:0,isMagazine:1b};`;
			const expected: TokenData[] = [
				{ type: 42, pos: 0, value: "give" },
				{ type: 47, pos: 5, value: "@s" },
				{
					type: 59,
					pos: 8,
					value: "minecraft:carrot_on_a_stick{ammo:1b,id:1,maxAmmo:25,ammo_count:0,isMagazine:1b}",
				},
				{ type: 9, pos: 87, value: ";" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Command with Vars", () => {
			const text = `execute store result entity @s Item.tag.ammo int 2 run $temp.get();`;
			const expected: TokenData[] = [
				{ type: 42, pos: 0, value: "execute" },
				{ type: 42, pos: 8, value: "store" },
				{ type: 42, pos: 14, value: "result" },
				{ type: 42, pos: 21, value: "entity" },
				{ type: 47, pos: 28, value: "@s" },
				{ type: 63, pos: 31, value: "Item.tag.ammo" },
				{ type: 42, pos: 45, value: "int" },
				{ type: 64, pos: 49, value: "2" },
				{ type: 42, pos: 51, value: "run" },
				{ type: 61, pos: 55, value: "$temp.get()" },
				{ type: 9, pos: 66, value: ";" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});
	});
});
