import { TokenData } from "../src/ts/lexer";
import { describe, expect, test } from "@jest/globals";
import LexerHelper from "./helper/lexer";

describe("Tokenizing Pass Case", () => {
	describe("Basic", () => {
		test("Comment", () => {
			const text = `//test\nhello;`;
			const expected: TokenData[] = [
				{ type: 0, pos: 0, value: "//test" },
				{ type: 12, pos: 7, value: "hello" },
				{ type: 9, pos: 12, value: ";" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Function", () => {
			const text = `function amogus() {}`;
			const expected: TokenData[] = [
				{ type: 1, pos: 0, value: "function" },
				{ type: 12, pos: 9, value: "amogus" },
				{ type: 18, pos: 15, value: "(" },
				{ type: 19, pos: 16, value: ")" },
				{ type: 20, pos: 18, value: "{" },
				{ type: 21, pos: 19, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Class", () => {
			const text = `class test {}`;
			const expected: TokenData[] = [
				{ type: 2, pos: 0, value: "class" },
				{ type: 12, pos: 6, value: "test" },
				{ type: 20, pos: 11, value: "{" },
				{ type: 21, pos: 12, value: "}" },
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
				{ type: 3, pos: 0, value: "new" },
				{ type: 12, pos: 4, value: "advancements" },
				{ type: 18, pos: 16, value: "(" },
				{ type: 12, pos: 17, value: "rct" },
				{ type: 19, pos: 20, value: ")" },
				{ type: 20, pos: 22, value: "{" },
				{ type: 16, pos: 28, value: '"criteria"' },
				{ type: 10, pos: 38, value: ":" },
				{ type: 20, pos: 40, value: "{" },
				{ type: 21, pos: 46, value: "}" },
				{ type: 21, pos: 48, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("IfElse", () => {
			const text = `if (1 == 1) {} else {}`;
			const expected: TokenData[] = [
				{ type: 4, pos: 0, value: "if" },
				{ type: 18, pos: 3, value: "(" },
				{ type: 13, pos: 4, value: "1" },
				{ type: 28, pos: 6, value: "==" },
				{ type: 13, pos: 9, value: "1" },
				{ type: 19, pos: 10, value: ")" },
				{ type: 20, pos: 12, value: "{" },
				{ type: 21, pos: 13, value: "}" },
				{ type: 5, pos: 15, value: "else" },
				{ type: 20, pos: 20, value: "{" },
				{ type: 21, pos: 21, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Switch", () => {
			const text = `switch ($gun) {case 1: how;}`;
			const expected: TokenData[] = [
				{ type: 6, pos: 0, value: "switch" },
				{ type: 18, pos: 7, value: "(" },
				{ type: 17, pos: 8, value: "$gun" },
				{ type: 19, pos: 12, value: ")" },
				{ type: 20, pos: 14, value: "{" },
				{ type: 7, pos: 15, value: "case" },
				{ type: 13, pos: 20, value: "1" },
				{ type: 10, pos: 21, value: ":" },
				{ type: 12, pos: 23, value: "how" },
				{ type: 9, pos: 26, value: ";" },
				{ type: 21, pos: 27, value: "}" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Import", () => {
			const text = `import "*";`;
			const expected: TokenData[] = [
				{ type: 44, pos: 0, value: "import" },
				{ type: 43, pos: 7, value: '"*"' },
				{ type: 9, pos: 10, value: ";" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});

		test("Function Call", () => {
			const text = `guns.test();`;
			const expected: TokenData[] = [
				{ type: 12, pos: 0, value: "guns" },
				{ type: 37, pos: 4, value: "." },
				{ type: 12, pos: 5, value: "test" },
				{ type: 18, pos: 9, value: "(" },
				{ type: 19, pos: 10, value: ")" },
				{ type: 9, pos: 11, value: ";" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});
	});

	describe("Command", () => {
		test("item", () => {
			const text = `item modify entity @s weapon.mainhand gun:gun_interval_changed;`;
			const expected: TokenData[] = [
				{ type: 42, pos: 0, value: "item" },
				{ type: 42, pos: 5, value: "modify" },
				{ type: 42, pos: 12, value: "entity" },
				{ type: 47, pos: 19, value: "@s" },
				{ type: 61, pos: 22, value: "weapon.mainhand" },
				{ type: 61, pos: 38, value: "gun:gun_interval_changed" },
				{ type: 9, pos: 62, value: ";" },
			];
			const helper = new LexerHelper(text, expected);
			expect(helper.tokens).toEqual(expected);
		});
	});
});
