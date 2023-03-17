import * as fs from "fs";
import { getAllFilesSync } from "get-all-files";
import { HeaderType, ParsedHeaderData, SEMI_CHECKCHAR } from "../data/common";

/**
 * get all `.jmc` files in current workspace
 * @param workspaceFolder
 * @returns `string[]` all `.jmc` files full path
 */
export function getJMCFile(workspaceFolder: string): string[] {
	return getAllFilesSync(workspaceFolder)
		.toArray()
		.filter((v) => {
			return v.endsWith(".jmc");
		});
}

/**
 * get all `.hjmc` files in current workspace
 * @param workspaceFolder
 * @returns `string[]` all `.hjmc` files full path
 */
export function getHJMCFile(workspaceFolder: string): string[] {
	return getAllFilesSync(workspaceFolder)
		.toArray()
		.filter((v) => {
			return v.endsWith(".hjmc");
		});
}

/**
 * parse header in `.hjmc` file
 * @param text
 * @returns `ParsedHeaderData[]`
 */
export function parseHJMCFile(text: string): ParsedHeaderData[] {
	const data = text.split("\r\n");
	const hd: ParsedHeaderData[] = [];

	for (let i = 0; i < data.length; i++) {
		const header = data[i];
		const headerData = header.split(" ");
		switch (headerData[0]) {
			case "#define":
				hd.push({
					header: HeaderType.DEFINE,
					value: [headerData[1]].concat(
						headerData.slice(2).join(" ")
					),
					offset: getHeaderPos(data, i),
					length: header.length,
				});
				break;
			case "#include":
				hd.push({
					header: HeaderType.INCLUDE,
					value: [headerData.slice(1).join(" ").slice(1, -1)],
					offset: getHeaderPos(data, i),
					length: header.length,
				});
				break;
			case "#override_minecraft":
				hd.push({
					header: HeaderType.OVERRIDE_MINECRAFT,
					offset: getHeaderPos(data, i),
					length: header.length,
				});
				break;
			case "#credit":
				hd.push({
					header: HeaderType.CREDIT,
					value: [headerData.slice(1).join(" ").slice(1, -1)],
					offset: getHeaderPos(data, i),
					length: header.length,
				});
				break;
			case "#command":
				hd.push({
					header: HeaderType.COMMAND,
					value: headerData.slice(1),
					offset: getHeaderPos(data, i),
					length: header.length,
				});
				break;
			case "#delete":
				hd.push({
					header: HeaderType.DEL,
					value: headerData.slice(1),
					offset: getHeaderPos(data, i),
					length: header.length,
				});
				break;
			case "#uninstall":
				hd.push({
					header: HeaderType.UNINSTALL,
					offset: getHeaderPos(data, i),
					length: header.length,
				});
				break;
			case "#static":
				hd.push({
					header: HeaderType.STATIC,
					value: [headerData.slice(1).join(" ").slice(1, -1)],
					offset: getHeaderPos(data, i),
					length: header.length,
				});
				break;
		}
	}
	return hd;
}

function getHeaderPos(data: string[], pos: number): number {
	if (pos === 0) return data.slice(0, pos).join("\n").length + pos;
	return data.slice(0, pos).join("\n").length + pos + 1;
}

/**
 * get text of a file
 * @param path
 * @returns `string` file text
 */
export async function getFileText(path: string): Promise<string> {
	return new Promise((resolve, reject) => {
		resolve(fs.readFileSync(path, { encoding: "utf-8", flag: "r" }));
	});
}

export interface ImportData {
	filename: string;
	text: string;
}

/**
 * get all file text of JMC files
 * @param root root workspace folder
 * @returns
 */
export async function getAllJMCFileText(root: string): Promise<ImportData[]> {
	const datas: ImportData[] = [];
	const files = getJMCFile(root);
	for (const file of files) {
		const text = (await getFileText(file)).replace("\r\n", "\n");
		datas.push({
			filename: file,
			text: text,
		});
	}
	return datas;
}

/**
 * get the command of current position
 * @param text
 * @param offset
 * @returns
 */
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

	index = offset;
	while (index++ !== text.length - 1) {
		const current = text[index];
		if (SEMI_CHECKCHAR.includes(current)) {
			return currentText.split("").reverse().join("").trim();
		}
		currentText += current;
	}

	return currentText;
}
