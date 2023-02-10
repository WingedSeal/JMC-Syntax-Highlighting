import { getDocumentText, getImport } from "./documentHelper";

export interface ImportData {
	filename: string;
	text: string;
}

export async function getVariables(text: string, root: string): Promise<string[]> {
	const definedVariables: string[] = [];

	const variablePattern = /\$([\w.]+)\s*\??=(?!=)/g;
	let m: RegExpExecArray | null;

	const files = await getImportDocumentText(root);
	files.push({
		filename: "main",
		text: text,
	});

	for (const i of files) {
		const text = i.text;
		while ((m = variablePattern.exec(text)) !== null) {
			definedVariables.push(m[1]);
		}
	}

	return definedVariables;
}

export async function getUnusedVariables(
	text: string,
	root: string
): Promise<string[]> {
	const variables: string[] = [];

	const variablePattern = /\$([\w\.]+)/g;
	let m: RegExpExecArray | null;

	const files = await getImportDocumentText(root);
	files.push({
		filename: "main",
		text: text,
	});

	for (const i of files) {
		const text = i.text;
		while ((m = variablePattern.exec(text)) !== null) {
			if (m[1].endsWith(".get")) {
				variables.push(m[1].slice(0, -4));
			} else {
				variables.push(m[1]);
			}
		}
	}

	const nonDuplicate = variables.filter((item, index) => {
		variables.splice(index, 1);
		const unique = !variables.includes(item) && !item.endsWith(".get");
		variables.splice(index, 0, item);
		return unique;
	});

	return nonDuplicate;
}
export async function getFunctions(text: string, root: string): Promise<string[]> {
	const definedFunctions: string[] = [];

	const functionPattern = /function\s*([\w\.]+)\s*\(/g;
	const classPattern = /\bclass\s*([\w.]+)\b/g;
	let m: RegExpExecArray | null;
	let mn: RegExpExecArray | null;

	const files = await getImportDocumentText(root);
	files.push({
		filename: "main",
		text: text,
	});

	for (const i of files) {
		const text = i.text;
		while ((m = functionPattern.exec(text)) !== null) {
			definedFunctions.push(m[1]);
		}

		let t = "";
		let bracketCount = 0;
		let started = false;
		while ((m = classPattern.exec(text)) !== null) {
			let index = m.index + m[0].length;
			while ((index += 1) !== text.length - 1) {
				const current = text[index];
				if (current === "{") {
					started = true;
					bracketCount++;
				}
				else if (started) {
					if (current === "{") { 
						bracketCount++;
					}
					if (current === "}") { 
						bracketCount--;
					}
					if (bracketCount === 0) break;
					t += current;
				}
			}

			while ((mn = functionPattern.exec(t)) !== null) {
				if (!mn[1].startsWith(m[1] + ".") && !definedFunctions.includes(`${m[1]}.${mn[1]}`)) {
					definedFunctions.push(`${m[1]}.${mn[1]}`);
				}
			}

			t = "";
		}
	}

	return definedFunctions;
}

export async function getClass(text: string, root: string): Promise<string[]> {
	const definedClasses: string[] = [];

	const classPattern = /class\s*([\w\.]+)/g;
	let m: RegExpExecArray | null;

	const files = await getImportDocumentText(root);
	files.push({
		filename: "main",
		text: text,
	});

	for (const i of files) {
		const text = i.text;
		while ((m = classPattern.exec(text)) !== null) {
			definedClasses.push(m[1]);
		}
	}

	return definedClasses;
}

export async function getImportDocumentText(
	root: string
): Promise<ImportData[]> {
	const datas: ImportData[] = [];
	const files = getImport(root);
	for (const file of files) {
		const text = (await getDocumentText(file)).replace("\r\n", "\n");
		datas.push({
			filename: file,
			text: text,
		});
	}
	return datas;
}

export function getCurrentCommand(text: string, offset: number): string {
	let str = "";
	const stopChar: string[] = ["{", ";"];
	let index = offset;

	while ((index -= 1) !== -1) {
		const current = text[index];
		if (current === "\n") {
			continue;
		} else if (stopChar.includes(current)) {
			break;
		} else {
			str += current;
		}
	}

	str = str.split("").reverse().join("").trim();

	index = offset - 1;

	while ((index += 1) !== text.length) {
		const current = text[index];
		if (current === "\n") {
			continue;
		} else if (stopChar.includes(current)) {
			break;
		} else {
			str += current;
		}
	}
	return str;
}
