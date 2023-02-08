import {
	Diagnostic,
	DiagnosticSeverity,
	Position,
	Range,
} from "vscode-languageserver/node";
import * as fs from "fs";
import { HEADERS } from "./data/common";
import {
	getLineByIndex,
	getLinePos,
	getTextByLine,
} from "./helpers/documentHelper";
import { getUnusedVariables, getVariables } from "./helpers/documentAnalyze";

const importPattern = /@import\s*"([\w\s]*)"/g;
const variablePattern = /\$([\w\.]+)/g;

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
				const endPos = Position.create(line.line, line.pos + m[1].length);
				const range = Range.create(startPos, endPos);
				if (!fs.existsSync(`${f}\\${m[1]}.jmc`)) {
					diagnostics.push({
						range: range,
						message: `ImportError: '${m[1]}' does not exist`,
						severity: DiagnosticSeverity.Error,
					});
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
