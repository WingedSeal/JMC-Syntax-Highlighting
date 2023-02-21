import { HeaderData, HeaderType, VANILLA_COMMANDS } from "../data/common";

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
	MACRO,
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
	public headerData: HeaderData[] = [];

	private macros: string[];
	private raw: string[];
	private rtrim: string[];
	private currentIndex = 0;

	constructor(text: string, header: HeaderData[]) {
		this.headerData = header;
		this.macros = this.headerData
			.filter((v) => v.header === HeaderType.DEFINE)
			.map((v) => v.value![0]);
		this.raw = text.split(/(;|\{|\}|\(|\)|\,|\[|\]|\/\/.*)/g);
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
	 * @order EMPTY
	 * @order COMMENT (//)
	 * @order FUNCTION (function)
	 * @order CLASS (class)
	 * @order VARIABLE ($)
	 * @order IF-ELSE (if/else)
	 * @order VANILLA_COMMANDS
	 * @order FUNCTION_CALL
	 * @param text
	 * @returns void
	 */
	private parseText(text: string): void {
		const current = text;
		if (current === "") return;
		if (current.startsWith("//")) return;
		else if (this.macros.includes(current)) {
			this.parseMacro(current);
		} else if (current.startsWith("function")) {
			this.parseDefineFunction(current);
		} else if (current.startsWith("class")) {
			this.parseClass(current);
		} else if (current.startsWith("switch")) {
			this.parseSwitch(current);
		} else if (current.startsWith("$")) {
			this.parseVariable(current);
		} else if (current.startsWith("if") || current.startsWith("else")) {
			this.praseIfElse(current);
		} else if (VANILLA_COMMANDS.includes(current.split(" ")[0])) {
			this.parseCommand(current);
		} else if (current.endsWith("=")) {
			return;
		} else if (current.startsWith('"')) {
			return;
		} else if (this.rtrim[this.currentIndex + 1] === "(") {
			this.parseCallFunction(current);
		} else {
			return;
		}
	}

	private parseMacro(text: string) {
		let offset = this.getOffset();
		const rawText = this.raw[this.currentIndex];

		const emptyLength = rawText.match(/^(\s+)/g || []);
		const empty = emptyLength !== null ? emptyLength[0].length : 0;

		if (rawText.trim().startsWith("$")) {
			offset += rawText.length;
			offset -= text.length;
		}
		else {
			offset += empty;
		}

		this.tokens.push({
			type: TokenType.MACRO,
			offset: offset,
			length: text.length,
			raw: rawText,
			trim: text,
		});
	}

	private parseSwitch(text: string) {
		const offset = this.getOffset();
		const rawText = this.raw[this.currentIndex];

		if (this.rtrim[this.currentIndex + 1] !== "(") {
			this.errors.push({
				type: ErrorType.INVALID_SYNTAX,
				offset: offset + 7,
				length: 1,
				message: "Expected '('",
			});
			return;
		}

		if (!this.checkBracket()) {
			this.errors.push({
				type: ErrorType.INVALID_SYNTAX,
				offset: offset + 7,
				length: 1,
				message: "Expected ')'",
			});
		}
		while (this.currentIndex++ !== this.rtrim.length - 1) {
			const current = this.rtrim[this.currentIndex];
			this.parseText(current);
			if (current === "{") break;
		}
		let bracketCount = 1;

		while (this.currentIndex++ !== this.rtrim.length - 1) {
			const current = this.rtrim[this.currentIndex];
			if (current.startsWith("case")) {
				const data = current.split(":").map((v) => v.trim());
				this.parseText(data[1]);
			} else {
				this.parseText(current);
			}

			if (current === "{") bracketCount++;
			else if (current === "}") bracketCount--;
			if (bracketCount === 0) break;
		}
	}

	/**
	 * parse vanilla command
	 * @param text
	 */
	private parseCommand(text: string) {
		const index = this.getOffset();
		let startRaw = this.raw[this.currentIndex];

		if (this.rtrim[this.currentIndex + 1] === "[") {
			let bracketCount = 1;
			this.currentIndex += 2;
			text += "[";
			while (
				bracketCount !== 0 &&
				this.currentIndex < this.raw.length - 1
			) {
				const currentText = this.rtrim[this.currentIndex];
				const currentRaw = this.raw[this.currentIndex];

				if (currentText === "[") bracketCount++;
				else if (currentText === "]") bracketCount--;
				else {
					text += currentText;
				}
				startRaw += currentRaw;

				this.currentIndex++;
			}
			text += "]";
		}

		const next = this.rtrim[this.currentIndex + 1];

		if (!(next === ";" || next === "{")) {
			this.currentIndex++;
			let currentText = this.rtrim[this.currentIndex];

			while (!(currentText === ";" || currentText === "{")) {
				currentText = this.rtrim[this.currentIndex];
				const currentRaw = this.raw[this.currentIndex];

				if (currentText === ";" || currentText === "{") break;
				if (currentText !== "(" && currentText !== ")") {
					text += currentText;
				}
				if (currentText === ")") text += " ";
				startRaw += currentRaw;

				this.currentIndex++;
			}
		} else if (text.startsWith("execute")) {
			let currentText = this.rtrim[this.currentIndex];
			text += " ";

			while (!(currentText === ";" || currentText === "{")) {
				currentText = this.rtrim[this.currentIndex];
				const currentRaw = this.raw[this.currentIndex];

				if (currentText === ";" || currentText === "{") break;
				if (currentText !== "(" && currentText !== ")") {
					text += currentText;
				}
				if (currentText === ")") text += " ";
				startRaw += currentRaw;

				this.currentIndex++;
			}
		}

		// if (
		// 	text
		// 		.split(" ")
		// 		.map((v) => v.trim())
		// 		.filter((v) => (v! += ""))[0] === "execute"
		// ) {
		// 	let bracketCount = 1;
		// 	text += " ";

		// 	while (
		// 		bracketCount !== 0 &&
		// 		this.currentIndex < this.raw.length - 1
		// 	) {
		// 		const currentText = this.rtrim[this.currentIndex];
		// 		if (currentText === "{") bracketCount++;
		// 		else if (currentText === "}") bracketCount--;
		// 		else if (currentText === ";") break;
		// 		if (bracketCount === 1) {
		// 			text += currentText;
		// 			length += this.raw[this.currentIndex].length;
		// 		}
		// 		this.currentIndex++;
		// 	}
		// } else {
		// 	this.currentIndex++;
		// 	text += " ";
		// 	let currentText = this.rtrim[this.currentIndex];
		// 	while (
		// 		currentText !== ";" &&
		// 		this.currentIndex < this.raw.length - 1
		// 	) {
		// 		currentText = this.rtrim[this.currentIndex];
		// 		text += currentText;
		// 		length += this.raw[this.currentIndex].length;
		// 		if (this.rtrim[this.currentIndex] === ")") text += " ";
		// 		this.currentIndex++;
		// 	}
		// }

		const digest = text
			.split(" ")
			.map((v) => v.trim())
			.filter((v) => (v! += ""));
		const emptyLength = startRaw.match(/^(\s+)/g || []);
		const empty = emptyLength !== null ? emptyLength[0].length : 0;

		this.tokens.push({
			raw: startRaw,
			trim: text,
			type: TokenType.COMMAND,
			offset: index + empty,
			length: startRaw.length - empty,
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
				const length =
					rawText.length - rawText.trim().length + text.length;
				this.errors.push({
					type: ErrorType.INVALID_SYNTAX,
					message: "Expected '('",
					offset: startOffset + length,
					length: 1,
				});
				return;
			}
			this.currentIndex += 2;
			let bracketCount = 1;
			let args: string[][] = [];
			let lastTextIndex: number | undefined;
			while (
				bracketCount !== 0 &&
				this.currentIndex < this.raw.length - 1
			) {
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

		const emptyLength = rawText.match(/^(\s+)/g || []);
		const empty = emptyLength !== null ? emptyLength[0].length : 0;

		const funcs: string[] = [];
		let startIndex = this.currentIndex;
		let bracketCount = 0;
		while (startIndex++ !== this.rtrim.length - 1) {
			const currentText = this.rtrim[startIndex];
			if (currentText === "{") bracketCount++;
			else if (currentText === "}") bracketCount--;
			else if (currentText.startsWith("function")) {
				funcs.push(currentText.split(" ")[1]);
			}
			if (bracketCount === 0) break;
		}

		const funcName = text.split(" ")[1];
		this.tokens.push({
			raw: rawText,
			trim: text,
			type: TokenType.CLASS,
			length: rawText.length - empty,
			offset: index + empty,
			value: [funcName].concat(funcs),
		});
	}

	/**
	 * prase functioncall
	 * @param text
	 * @returns
	 */
	private parseCallFunction(text: string) {
		const startIndex = this.currentIndex;
		const rawText = this.raw[startIndex];
		let offset = this.getOffset();
		const funcName = text.split(".");

		const emptyLength = rawText.match(/^(\s+)/g || []);
		const empty = emptyLength !== null ? emptyLength[0].length : 0;

		if (rawText.trim().startsWith("$")) {
			offset += rawText.length;
			offset -= text.length;
			offset -= empty;
		}

		if (rawText.trim().startsWith("case")) {
			const firstLength = rawText.split(":")[0].trim().length;
			const secondEmpty = rawText.split(":")[1].match(/^(\s+)/g || []);
			const empLength = secondEmpty !== null ? secondEmpty[0].length : 0;
			offset += firstLength + empLength + 1;
		}

		// this.currentIndex += 1;
		// let bracketCount = 1;
		// let commaCount = 0;
		// while (bracketCount !== 0 && this.currentIndex < this.rtrim.length) {
		// 	const currentText = this.rtrim[this.currentIndex];
		// 	if (currentText === ")") bracketCount--;
		// 	else if (currentText === "(") bracketCount++;
		// 	else if (currentText === ",") commaCount++;
		// 	else if (currentText === ";") return;
		// 	else if (currentText === "[") {
		// 		const startIndex = this.currentIndex - 1;
		// 		this.rtrim[this.currentIndex - 1] = this.joinBracket();
		// 		this.parseText(this.rtrim[startIndex]);
		// 	} else {
		// 		this.parseText(currentText);
		// 		args.push(currentText);
		// 	}
		// 	this.currentIndex++;
		// }
		// const endOffset = this.getOffset();
		// const length = endOffset - startOffset;

		// if (args.length - 1 !== commaCount) {
		// 	this.errors.push({
		// 		type: ErrorType.INVALID_SYNTAX,
		// 		message: "",
		// 		length: length,
		// 		offset: endOffset - 1,
		// 	});
		// 	return;
		// }

		this.tokens.push({
			raw: rawText,
			trim: text,
			type: TokenType.CALL_FUNCTION,
			length: text.length - empty,
			offset: offset + empty,
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
			const emptyLength = rawText.match(/^(\s+)/g || []);
			const empty = emptyLength !== null ? emptyLength[0].length : 0;
			this.tokens.push({
				raw: rawText,
				trim: text,
				type: TokenType.USE_VARIABLE,
				length: data[0].length,
				offset: index + empty,
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

		const emptyLength = rawText.match(/^(\s+)/g || []);
		const empty = emptyLength !== null ? emptyLength[0].length : 0;

		this.tokens.push({
			raw: rawText,
			trim: text,
			type: TokenType.VARIABLE,
			length: rawText.length - empty,
			offset: index + empty,
			value: [varName, operator, operation],
		});
	}

	/**
	 * prase function defining
	 * @param text
	 * @returns
	 */
	private parseDefineFunction(text: string) {
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

		let startIndex = this.currentIndex;
		let bracketCount = 1;
		let inClass = "";
		while (startIndex-- !== -1) {
			const currentText = this.rtrim[startIndex];
			if (currentText === "{") bracketCount--;
			else if (currentText === "}") bracketCount++;
			else if (bracketCount === 0) {
				inClass = this.rtrim[startIndex].split(" ")[1];
				break;
			}
		}

		const emptyLength = rawText.match(/^(\s+)/g || []);
		const empty = emptyLength !== null ? emptyLength[0].length : 0;
		this.tokens.push({
			raw: rawText,
			trim: text,
			type: TokenType.FUNCTION,
			length: rawText.length - empty,
			offset: index + empty,
			value: [text.split(" ")[1], inClass],
		});
	}

	/**
	 * get offset of current position
	 * @returns number
	 */
	private getOffset(): number {
		const joined = this.raw.slice(0, this.currentIndex + 1).join("");
		if (joined === undefined) {
			return (
				this.raw.slice(0, this.currentIndex).join("").length -
				this.raw[this.currentIndex].length
			);
		}
		return joined.length - this.raw[this.currentIndex].length;
	}
	/**
	 *
	 */
	private checkBracket(): boolean {
		let index = this.currentIndex;
		let bracketCount = 0;
		while (index++ !== this.rtrim.length - 1) {
			// this.errors.push({
			// 	type: ErrorType.INVALID_SYNTAX,
			// 	offset: offset + 7,
			// 	length: 1,
			// 	message: "Expected '('",
			// });
			const current = this.rtrim[index];
			if (current === "(") bracketCount++;
			else if (current === ")") bracketCount--;
			else if (bracketCount === 0) {
				return true;
			}
		}
		return false;
	}
}
