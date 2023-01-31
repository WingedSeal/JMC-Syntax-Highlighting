import {
	Diagnostic,
	DiagnosticSeverity,
	Position,
	Range,
} from "vscode-languageserver/node";
import * as fs from "fs";
import {
	Headers,
	getLineByIndex,
	getLinePos,
	getVariables,
	getUnusedVariables,
	getTextByLine,
} from "./data/common";

let importPattern = /@import\s*"([\w\s]*)"/g;
let variablePattern = /\$([\w\.]+)/g;

let headerPattern = /#(\w+)/g;

let m: RegExpExecArray | null;

export function getDiagnostics(text: string, filePath: string): Diagnostic[] {
	let diagnostics: Diagnostic[] = [];

	let path = filePath.split("\\");
	let filename = path.pop();
	let f = path.join("/");

	if (filename?.endsWith(".jmc")) {
		//import check
		while ((m = importPattern.exec(text)) !== null) {
			let line = getLineByIndex(m.index + 9, getLinePos(text));
			if (line.line > -1 && line.pos > -1) {
				let startPos = Position.create(line.line, line.pos);
				let endPos = Position.create(line.line, line.pos + m[1].length);
				let range = Range.create(startPos, endPos);
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
			let pos = getLineByIndex(m.index, getLinePos(text));
			var lineText = getTextByLine(text, pos.line).trim();
			let variables = getVariables(text, f);

			if (
				!variables.includes(m[1]) &&
				!m[1].endsWith(".get") &&
				!lineText.startsWith("//")
			) {
				let startPos = Position.create(pos.line, pos.pos);
				let endPos = Position.create(pos.line, pos.pos + m[0].length);
				let range = Range.create(startPos, endPos);

				diagnostics.push({
					range: range,
					message: `NameError: '${m[1]}' is not defined`,
					severity: DiagnosticSeverity.Error,
				});
			}
		}

		for (let variable of getUnusedVariables(text, f)) {
			let pattern = RegExp(`\\\$(${variable})\\b`, "g");
			while ((m = pattern.exec(text)) !== null) {
				var line = getLineByIndex(m.index, getLinePos(text));
				var lineText = getTextByLine(text, line.line).trim();
				if (!lineText.startsWith("//")) {
					let startPos = Position.create(line.line, line.pos);
					let endPos = Position.create(
						line.line,
						line.pos + m[0].length
					);
					let range = Range.create(startPos, endPos);
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
			let header = m[1];
			let length = m.length;
			let pos = getLineByIndex(m.index, getLinePos(text));
			if (!Headers.includes(header)) {
				let startPos = Position.create(pos.line, pos.pos + 1);
				let endPos = Position.create(
					pos.line,
					pos.pos + header.length + 1
				);
				let range = Range.create(startPos, endPos);

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
