import {
	Diagnostic,
	DiagnosticSeverity,
	Position,
	Range,
} from "vscode-languageserver/node";
import * as fs from "fs";
import { Headers } from "./data/common";

let importPattern = /@import\s*"([\w\s]*)"/g;
let headerPattern = /#(\w+)/g

let m: RegExpExecArray | null;

interface textLinePos {
	line: number;
	length: number;
}

export function getDiagnostics(text: string, filePath: string): Diagnostic[] {
	let diagnostics: Diagnostic[] = [];

	let path = filePath.split("\\");
	let filename = path.pop();
	let f = path.join("/");
	
	if (filename?.endsWith(".jmc")) {
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
	}
	else if (filename?.endsWith(".hjmc")) {
		while ((m = headerPattern.exec(text)) !== null) { 
			let header = m[1];
			let length = m.length;
			let pos = getLineByIndex(m.index, getLinePos(text));
			if (!Headers.includes(header)) {
				let startPos = Position.create(pos.line, pos.pos + 1);
				let endPos = Position.create(pos.line, pos.pos + header.length + 1);
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

function getLineByIndex(
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
