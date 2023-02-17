import * as fs from "fs";
import { getAllFilesSync } from "get-all-files";
import { SEMI_CHECKCHAR } from "../data/common";

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

export async function getCurrentCommand(
	text: string,
	offset: number
): Promise<string> {
	let index = offset;
	let currentText = "";
	while (index-- !== -1) {
		const current = text[index];
		if (SEMI_CHECKCHAR.includes(current)) {
			return currentText.split("").reverse().join("").trim();
		}
		currentText += current;
	}
	return currentText;
}
