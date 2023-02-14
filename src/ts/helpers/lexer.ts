import { VANILLA_COMMANDS } from "../data/common";

// interface CommandToken {
// 	offset: number;
// 	length: number;
// 	value: string[];
// }

// const StopChar: string[] = [";", "{", "}", "/"];

// export function tokenizeCommand(text: string): CommandToken[] {
// 	const values: CommandToken[] = [];
// 	let parse = "";
// 	let index = 0;
// 	while ((index += 1) !== text.length) {
// 		const current = text[index];
// 		parse += current;
// 		if (StopChar.includes(current)) {
// 			parse = parse.slice(0, -1);
// 			const command = parse.split(" ").filter((v) => v.trim() !== "");
// 			if (VANILLA_COMMANDS.includes(command[0])) {
// 				values.push({
// 					value: command,
// 					length: parse.trim().length,
// 					offset: index,
// 				});
// 			}
// 			parse = "";
// 		}
// 	}
// 	return values;
// }

export enum TokenType {
	FUNCTION,
	VARIABLE,
	CLASS,
	CALL_FUNCTION,
	USE_VARIABLE,
	COMMAND,
}

export enum ErrorType {
	UNEXPECTED,
	INVALID_SYNTAX,
}

export interface Token {
	raw: string;
	trim: string;
	type: TokenType;
	length: number;
	offset: number;
	value?: string[];
}

export interface Error {
	type: ErrorType;
	message: string;
	length: number;
	offset: number;
}

export class Language {
	public tokens: Token[] = [];
	public errors: Error[] = [];
	private raw: string[];
	private rtrim: string[];
	private currentIndex = 0;

	constructor(text: string) {
		this.raw = text.split(/(;|\{|\}|\(|\)|\,|\[|\])/g);
		this.rtrim = this.raw.map((v) => v.trim());
		for (
			this.currentIndex = 0;
			this.currentIndex < this.rtrim.length;
			this.currentIndex++
		) {
			this.parseText(this.rtrim[this.currentIndex]);
		}
	}

	/**
	 * parse text
	 * @param text 
	 * @returns void
	 */
	private parseText(text: string): void {
		const current = text;
		if (current === "") return;
		if (current.startsWith("//")) return;
		else if (current.startsWith("function")) {
			this.praseDefineFunction(current);
		} else if (current.startsWith("class")) {
			this.parseClass(current);
		} else if (current.startsWith("$")) {
			this.parseVariable(current);
		} else if (current.startsWith("if") || current.startsWith("else")) {
			this.praseIfElse(current);
		} else if (VANILLA_COMMANDS.includes(current.split(" ")[0])) {
			this.parseCommand(current);
		} else if (this.rtrim[this.currentIndex + 1] === "(") {
			this.parseFunction(current);
		} else {
			return;
		}
		
	}

	/**
	 * test if text can be parsed
	 * @param text 
	 * @returns 
	 */
	private testText(text: string): boolean {
		//return text.startsWith("function") || text.startsWith("$");
		return true;
	}

	/**
	 * parse vanilla command
	 * @param text 
	 */
	private parseCommand(text: string) {
		const index = this.getOffset();
		const rawText = this.raw[this.currentIndex];
		let length = text.length;
		
		if (this.rtrim[this.currentIndex + 1] === "[") {
			let bracketCount = 1;
			this.currentIndex += 2;			
			text += "[";
			while (bracketCount !== 0) {
				const currentText = this.rtrim[this.currentIndex];
				if (currentText === "[") bracketCount++;
				else if (currentText === "]") bracketCount--;
				else {
					text += currentText;
				}
				length += this.raw[this.currentIndex].length;
				this.currentIndex++;
			}
			text += "]";
		}

		if (text.split(" ").map((v) => v.trim()).filter((v) => (v! += ""))[0] === "execute") {
			let bracketCount = 1;
			text += " ";
			
			while (bracketCount !== 0) {
				const currentText = this.rtrim[this.currentIndex];
				if (currentText === "{") bracketCount++;
				else if (currentText === "}") bracketCount--;
				else if (currentText === ";") break;
				if (bracketCount === 1) {
					text += currentText;
					length += this.raw[this.currentIndex].length;
				}
				this.currentIndex++;
			}
		}
		else {
			this.currentIndex++;
			text += " ";
			let currentText = this.rtrim[this.currentIndex];
			while (currentText !== ";") {
				currentText = this.rtrim[this.currentIndex];
				text += currentText;
				length += this.raw[this.currentIndex].length;
				if (this.rtrim[this.currentIndex] === ")") text += " ";
				this.currentIndex++;
			}
		}
		

		const digest = text
			.split(" ")
			.map((v) => v.trim())
			.filter((v) => (v! += ""));		

		this.tokens.push({
			raw: rawText,
			trim: text,
			type: TokenType.COMMAND,
			offset: index,
			length: length,
			value: digest,
		});
	}

	/**
	 * prase If else block
	 * @param text 
	 * @returns 
	 */
	private praseIfElse(text: string) {
		const startOffset = this.getOffset();
		const rawText = this.raw[this.currentIndex];
		if (text.endsWith("if")) {
			const next = this.rtrim[this.currentIndex + 1];
			if (next !== "(") {
				this.errors.push({
					type: ErrorType.INVALID_SYNTAX,
					message: "Expected '('",
					offset: startOffset,
					length: 1,
				});
				return;
			}
			this.currentIndex += 2;
			let bracketCount = 1;
			let args: string[][] = [];
			let lastTextIndex: number | undefined;
			while (bracketCount !== 0) {
				const currentText = this.rtrim[this.currentIndex];
				if (currentText === ")") bracketCount--;
				else if (currentText === "(") bracketCount++;
				else if (currentText === "{") break;
				else {
					const condition = currentText
						.split(/(\|\||&&|==|!=)/g)
						.map((v) => v.trim())
						.filter((v) => v !== "");
					if (condition.length === 1 && lastTextIndex !== undefined) {
						const lastElem = args[lastTextIndex];
						const lastText = lastElem.slice(-1)[0];
						args[lastTextIndex][lastElem.length - 1] =
							lastText.concat(condition[0]);
					} else if (condition[0] !== undefined) {
						this.parseText(condition[0]);
						lastTextIndex = args.length;
						args.push(condition);
					}
				}

				this.currentIndex++;
			}

			if (bracketCount > 0) {
				this.errors.push({
					type: ErrorType.INVALID_SYNTAX,
					message: "Expected '('",
					length: startOffset - this.getOffset(),
					offset: this.getOffset(),
				});
			}

			args = args.filter((v) => v.length !== 0);

			// for (const arg of args) {
			// 	if (arg.length === 3) {
			// 		this.parseText(arg[0]);
			// 		this.parseText(arg[2]);
			// 	} else if (arg.length === 2) {
			// 		this.parseText(arg[0]);
			// 	}
			// }
		}
	}

	/**
	 * parse class
	 * @param text 
	 * @returns 
	 */
	private parseClass(text: string) {
		const index = this.getOffset();
		const rawText = this.raw[this.currentIndex];
		if (text.split(" ").length > 2) {
			this.errors.push({
				type: ErrorType.UNEXPECTED,
				message: "",
				length: rawText.length,
				offset: index,
			});
			return;
		}

		const funcName = text.split(" ")[1];
		this.tokens.push({
			raw: rawText,
			trim: text,
			type: TokenType.CLASS,
			length: rawText.length,
			offset: index,
			value: [funcName],
		});
	}

	/**
	 * prase functioncall
	 * @param text 
	 * @returns 
	 */
	private parseFunction(text: string) {
		const args: string[] = [];

		const startIndex = this.currentIndex;
		const rawText = this.raw[startIndex];
		const startOffset = this.getOffset();
		const funcName = text.split(".");

		this.currentIndex += 1;
		let bracketCount = 1;
		let commaCount = 0;
		let lastTextIndex: number | undefined;
		while (bracketCount !== 0 && this.currentIndex < this.rtrim.length) {
			const currentText = this.rtrim[this.currentIndex];
			//TODO:
			if (currentText === ")") bracketCount--;
			else if (currentText === "(") bracketCount++;
			else if (currentText === ",") commaCount++;
			else if (currentText === ";") return;
			else if (currentText === "[") {
				const startIndex = this.currentIndex - 1;
				this.rtrim[this.currentIndex - 1] = this.joinBracket();
				this.parseText(this.rtrim[startIndex]);
			}
			else {
				this.parseText(currentText);
				args.push(currentText);
			}
			lastTextIndex = undefined;
			this.currentIndex++;
		}
		const endOffset = this.getOffset();
		const length = endOffset - startOffset;

		if (args.length - 1 !== commaCount) {
			this.errors.push({
				type: ErrorType.INVALID_SYNTAX,
				message: "",
				length: length,
				offset: endOffset - 1,
			});
			return;
		}

		this.tokens.push({
			raw: rawText,
			trim: text,
			type: TokenType.CALL_FUNCTION,
			length: length,
			offset: startOffset,
			value: funcName,
		});
	}

	/**
	 * parse defining variables
	 * @param text 
	 * @returns 
	 */
	private parseVariable(text: string) {
		const index = this.getOffset();
		const rawText = this.raw[this.currentIndex];

		const data = text.split(" ").filter((v) => v !== "");
		data[1] = data.slice(1).join("");
		if (data[1] === undefined) return;

		const eData = data[1].split(/(\?=|=)/g).filter((v) => v !== "");
		if (eData[0] !== "=" && eData[0] !== "?=") {
			this.tokens.push({
				raw: rawText,
				trim: text,
				type: TokenType.USE_VARIABLE,
				length: rawText.length,
				offset: index,
				value: [data[0]],
			});
			return;
		}

		const operator = eData[0];
		const operation = eData[1];

		if (operator === undefined || operation === undefined) {
			this.errors.push({
				type: ErrorType.INVALID_SYNTAX,
				message: "",
				length: rawText.length,
				offset: index,
			});
			return;
		}

		const varName = data[0];
		this.parseText(operation);

		this.tokens.push({
			raw: rawText,
			trim: text,
			type: TokenType.VARIABLE,
			length: rawText.length,
			offset: index,
			value: [varName, operator, operation],
		});
	}

	/**
	 * prase function defining
	 * @param text 
	 * @returns 
	 */
	private praseDefineFunction(text: string) {
		const index = this.getOffset();
		const rawText = this.raw[this.currentIndex];
		if (text.split(" ").length > 2) {
			this.errors.push({
				type: ErrorType.UNEXPECTED,
				message: "",
				length: rawText.length,
				offset: index,
			});
			return;
		}

		const funcName = text.split(" ")[1];
		this.tokens.push({
			raw: rawText,
			trim: text,
			type: TokenType.FUNCTION,
			length: rawText.length,
			offset: index,
			value: [funcName],
		});
	}

	/**
	 * get offset of current position
	 * @returns number
	 */
	private getOffset(): number {
		const joined = this.raw.slice(0, this.currentIndex + 1).join("");
		if (joined === undefined) { 
			return this.raw.slice(0, this.currentIndex).join("").length - this.raw[this.currentIndex].length;
		}
		return joined.length - this.raw[this.currentIndex].length;
	}

	/**
	 * 
	 * @returns 
	 */
	private joinBracket(): string {
		const startIndex = this.currentIndex;
		let cText = this.rtrim[startIndex - 1];
		cText += "[";
		let mbracketCount = 1;
		this.currentIndex++;
		while (mbracketCount !== 0) {
			const pText = this.rtrim[this.currentIndex];
			if (pText === "[") mbracketCount++;
			else if (pText === "]") mbracketCount--;
			else {
				cText += pText;
			}
			this.currentIndex++;
		}
		cText += "]";
		return cText;
	}
}
