import * as fs from "fs";
import { getAllFilesSync } from "get-all-files";

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

export function getImport(workspaceFolder: string): string[] {
	// const importPattern = /@import\s*\"(.+)\"/g;
	// let m: RegExpExecArray | null;
	// const files: string[] = [];

	// while ((m = importPattern.exec(text)) !== null) {
	// 	files.push(`${m[1]}.jmc`);
	// }
	// return files;
	return getAllFilesSync(workspaceFolder)
		.toArray()
		.filter((v) => {
			return v.endsWith(".jmc");
		});
}

export async function getDocumentText(path: string): Promise<string> {
	return new Promise((resolve, reject) => {
		resolve(fs.readFileSync(path, { encoding: "utf-8", flag: "r" }));
	}); 
}

export function getTextByLine(text: string, line: number): string {
	const t = text.split("\n")[line];
	return t;
}
