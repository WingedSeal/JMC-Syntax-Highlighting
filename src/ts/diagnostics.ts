import {
	Diagnostic,
	DiagnosticSeverity,
	Position,
	Range,
} from "vscode-languageserver/node";
import * as fs from "fs";

let importPattern = /@import\s*"([\w\s]*)"/g;
let m: RegExpExecArray | null;

interface textLinePos {
	line: number;
	length: number;
}

export function getDiagnostics(text: string, filePath: string): Diagnostic[] {
	let diagnostics: Diagnostic[] = [];

	while ((m = importPattern.exec(text)) !== null) {
		let line = getLineByIndex(text, m.index + 9, getLinePos(text));
		if (line.line > -1 && line.pos > -1) {
			let startPos = Position.create(line.line, line.pos);
			let endPos = Position.create(line.line, line.pos + m[1].length);
			let range = Range.create(startPos, endPos);
			let path = filePath.split("\\");
			path.pop();
			let f = path.join("/");
			console.log(`${f}\\${m[1]}.jmc`);
			if (!fs.existsSync(`${f}\\${m[1]}.jmc`)) {
				diagnostics.push({
					range: range,
					message: `'${m[1]}' does not exist`,
					severity: DiagnosticSeverity.Error,
				});
			}
		}
	}

	return diagnostics;
}

function getLineByIndex(
	text: string,
	index: number,
	linepos: textLinePos[]
): { line: number; pos: number } {
	for (let i of linepos) {
		if (index < i.length) {
			return { line: i.line, pos: index };
		}
		index -= i.length;
	}
	return { line: -1, pos: -1 };
}

function getLinePos(text: string): textLinePos[] {
	let textLinePos: textLinePos[] = [];
	let textLines = text.split("\n");
	for (let i = 0; i < textLines.length; i++) {
		let textLine = textLines[i];
		textLinePos.push({
			line: i,
			length: textLine.length + 1,
		});
	}
	return textLinePos;
}
