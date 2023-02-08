import {
	Diagnostic,
	DiagnosticSeverity,
	Position,
	Range,
} from "vscode-languageserver/node";
import * as fs from "fs";
import {
	HEADERS,
	JSON_FILE_TYPES,
	KEYWORDS,
	SEMI_CHECKCHAR,
	VANILLA_COMMANDS,
} from "./data/common";
import {
	getLineByIndex,
	getLinePos,
	getTextByLine,
} from "./helpers/documentHelper";
import {
	getFunctions,
	getUnusedVariables,
	getVariables,
} from "./helpers/documentAnalyze";
import { BuiltInFunctions } from "./data/builtinFunctions";

const importPattern = /@import\s*"([\w\s]*)"/g;
const variablePattern = /\$([\w\.]+)/g;
const functionPattern = /\b([\w\.]+)\s*\(/g;

const headerPattern = /#(\w+)/g;

let m: RegExpExecArray | null;

export function getDiagnostics(text: string, filePath: string): Diagnostic[] {
	const diagnostics: Diagnostic[] = [];

	const path = filePath.split("\\");
	const filename = path.pop();
	const f = path.join("/");

	if (filename?.endsWith(".jmc")) {
		//import check
		while ((m = importPattern.exec(text)) !== null) {
			const line = getLineByIndex(m.index + 9, getLinePos(text));
			if (line.line > -1 && line.pos > -1) {
				const startPos = Position.create(line.line, line.pos);
				const endPos = Position.create(
					line.line,
					line.pos + m[1].length
				);
				const range = Range.create(startPos, endPos);
				if (!fs.existsSync(`${f}\\${m[1]}.jmc`)) {
					diagnostics.push({
						range: range,
						message: `ImportError: '${m[1]}' does not exist`,
						severity: DiagnosticSeverity.Error,
					});
				}
			}

			let index = m.index;
			while ((index -= 1) !== -1) {
				const current = text[m.index - 1].trim();
				if (SEMI_CHECKCHAR.includes(current)) {
					break;
				} else if (current === "") continue;
				else {
					const line = getLineByIndex(index, getLinePos(text));
					const lineText = getTextByLine(text, line.line).trim();
					if (!lineText.startsWith("//")) {
						const startPos = Position.create(line.line, line.pos);
						const endPos = Position.create(line.line, line.pos + 1);
						const range = Range.create(startPos, endPos);
						diagnostics.push({
							range: range,
							message: `Missing Semicolon`,
							severity: DiagnosticSeverity.Error,
						});
						break;
					}
				}
			}
		}

		//variable check
		while ((m = variablePattern.exec(text)) !== null) {
			const pos = getLineByIndex(m.index, getLinePos(text));
			const lineText = getTextByLine(text, pos.line).trim();
			const variables = getVariables(text, f);

			if (
				!variables.includes(m[1]) &&
				!m[1].endsWith(".get") &&
				!lineText.startsWith("//")
			) {
				const startPos = Position.create(pos.line, pos.pos);
				const endPos = Position.create(pos.line, pos.pos + m[0].length);
				const range = Range.create(startPos, endPos);

				diagnostics.push({
					range: range,
					message: `NameError: '${m[1]}' is not defined`,
					severity: DiagnosticSeverity.Error,
				});
			}
		}

		while ((m = functionPattern.exec(text)) !== null) {
			let index = m.index;
			let t = "";
			while((index -= 1) !== -1) {
				const current = text[index].trim();
				if (current === "") continue;
				else if (SEMI_CHECKCHAR.includes(current)) break;
				else t += current;
			}
			if (t.split("").reverse().join("") === "new") {
				continue;
			}



			const builtinFunc = BuiltInFunctions.flatMap((v) => {
				const methods = v.methods.flatMap((value) => {
					return `${v.class}.${value.name}`;
				});
				return methods;
			});

			const ifExists =
				getFunctions(text, f).includes(m[1]) ||
				builtinFunc.includes(m[1]);
			const isVariable = getVariables(text, f).includes(
				m[1].split(".")[0]
			);

			const pos = getLineByIndex(m.index, getLinePos(text));
			const lineText = getTextByLine(text, pos.line).trim();

			if (
				!ifExists &&
				!lineText.startsWith("//") &&
				!KEYWORDS.includes(m[1]) &&
				!isVariable
			) {
				const startPos = Position.create(pos.line, pos.pos);
				const endPos = Position.create(pos.line, pos.pos + m[0].length);
				const range = Range.create(startPos, endPos);

				diagnostics.push({
					range: range,
					message: `NameError: '${m[1]}' is not defined`,
					severity: DiagnosticSeverity.Error,
				});
			}

			if (!isVariable) {
				let index = m.index;
				while ((index -= 1) !== -1) {
					const current = text[m.index - 1].trim();
					if (SEMI_CHECKCHAR.includes(current)) {
						break;
					} else if (current === "") continue;
					else {
						const line = getLineByIndex(index, getLinePos(text));
						const lineText = getTextByLine(text, line.line).trim();
						if (!lineText.startsWith("//")) {
							const startPos = Position.create(
								line.line,
								line.pos
							);
							const endPos = Position.create(
								line.line,
								line.pos + 1
							);
							const range = Range.create(startPos, endPos);
							diagnostics.push({
								range: range,
								message: `Missing Semicolon`,
								severity: DiagnosticSeverity.Error,
							});
							break;
						}
					}
				}
			}
		}

		for (const variable of getUnusedVariables(text, f)) {
			const pattern = RegExp(`\\\$(${variable})\\b`, "g");
			while ((m = pattern.exec(text)) !== null) {
				const line = getLineByIndex(m.index, getLinePos(text));
				const lineText = getTextByLine(text, line.line).trim();
				if (!lineText.startsWith("//")) {
					const startPos = Position.create(line.line, line.pos);
					const endPos = Position.create(
						line.line,
						line.pos + m[0].length
					);
					const range = Range.create(startPos, endPos);
					diagnostics.push({
						range: range,
						message: `Unused variable ${m[1]}`,
						severity: DiagnosticSeverity.Warning,
					});
				}

				let index = m.index;
				while ((index -= 1) !== -1) {
					const current = text[m.index - 1].trim();
					if (SEMI_CHECKCHAR.includes(current)) {
						break;
					} else if (current === "") continue;
					else {
						const line = getLineByIndex(index, getLinePos(text));
						const lineText = getTextByLine(text, line.line).trim();
						if (!lineText.startsWith("//")) {
							const startPos = Position.create(
								line.line,
								line.pos
							);
							const endPos = Position.create(
								line.line,
								line.pos + 1
							);
							const range = Range.create(startPos, endPos);
							diagnostics.push({
								range: range,
								message: `Missing Semicolon`,
								severity: DiagnosticSeverity.Error,
							});
							break;
						}
					}
				}
			}
		}

		for (const keyword of VANILLA_COMMANDS) {
			const pattern = RegExp(`\\b(${keyword}).*;`, "g");
			while ((m = pattern.exec(text)) !== null) {
				let index = m.index;
				if (text[m.index + m[0].length] === '"' && text[m.index - 1] === '"') {continue;}
				while ((index -= 1) !== -1) {
					const current = text[m.index - 1].trim();
					if (SEMI_CHECKCHAR.includes(current)) {
						break;
					} else if (current === "") continue;
					else {
						const line = getLineByIndex(index, getLinePos(text));
						const lineText = getTextByLine(text, line.line).trim();
						if (!lineText.startsWith("//")) {
							const startPos = Position.create(
								line.line,
								line.pos
							);
							const endPos = Position.create(
								line.line,
								line.pos + 1
							);
							const range = Range.create(startPos, endPos);
							diagnostics.push({
								range: range,
								message: `Missing Semicolon`,
								severity: DiagnosticSeverity.Error,
							});
							break;
						}
					}
				}
			}
		}
	} else if (filename?.endsWith(".hjmc")) {
		while ((m = headerPattern.exec(text)) !== null) {
			const header = m[1];
			const pos = getLineByIndex(m.index, getLinePos(text));
			if (!HEADERS.includes(header)) {
				const startPos = Position.create(pos.line, pos.pos + 1);
				const endPos = Position.create(
					pos.line,
					pos.pos + header.length + 1
				);
				const range = Range.create(startPos, endPos);

				diagnostics.push({
					range: range,
					message: `ValueError: '${m[1]}' does not exist`,
					severity: DiagnosticSeverity.Error,
				});
			}
		}
	}

	return diagnostics;
}
