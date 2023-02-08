import * as fs from "fs";

export interface TextLinePos {
	line: number;
	length: number;
}

export function getLineByIndex(
	index: number,
	linepos: TextLinePos[]
): { line: number; pos: number } {
	for (const i of linepos) {
		if (index < i.length) {
			return { line: i.line, pos: index };
		}
		index -= i.length;
	}
	return { line: -1, pos: -1 };
}

export function getLinePos(text: string): TextLinePos[] {
	const textLinePos: TextLinePos[] = [];
	const textLines = text.split("\n");
	for (let i = 0; i < textLines.length; i++) {
		const textLine = textLines[i];
		textLinePos.push({
			line: i,
			length: textLine.length + 1,
		});
	}
	return textLinePos;
}

export function getImport(text: string): string[] {
	const importPattern = /@import\s*\"(.+)\"/g;
	let m: RegExpExecArray | null;
	const files: string[] = [];

	while ((m = importPattern.exec(text)) !== null) {
		files.push(`${m[1]}.jmc`);
	}
	return files;
}

export function getDocumentText(
	path: string,
	root: string | undefined
): string {
	return fs.readFileSync(`${root}/${path}`, "utf-8");
}

export function getTextByLine(text: string, line: number): string {
	return text.split("\n")[line];
}
