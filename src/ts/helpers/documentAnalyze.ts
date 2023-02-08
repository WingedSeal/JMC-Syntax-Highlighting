import { getDocumentText, getImport } from "./documentHelper";

export interface ImportData {
	filename: string;
	text: string;
}

export function getVariables(text: string, root: string): string[] {
	const definedVariables: string[] = [];

	const variablePattern = /\$(\w+)\s*\??=(?!=)/g;
	let m: RegExpExecArray | null;

	const files = getImportDocumentText(text, root);
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

export function getUnusedVariables(
	text: string,
	root: string | undefined
): string[] {
	const variables: string[] = [];

	const variablePattern = /\$([\w\.]+)/g;
	let m: RegExpExecArray | null;

	const files = getImportDocumentText(text, root);
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
export function getFunctions(text: string, root: string): string[] {
	const definedFunctions: string[] = [];

	const functionPattern = /function\s*([\w\.]+)\(/g;
	let m: RegExpExecArray | null;

	const files = getImportDocumentText(text, root);
	files.push({
		filename: "main",
		text: text,
	});

	for (const i of files) {
		const text = i.text;
		while ((m = functionPattern.exec(text)) !== null) {
			definedFunctions.push(m[1]);
		}
	}

	return definedFunctions;
}

export function getClass(text: string, root: string): string[] {
	const definedClasses: string[] = [];

	const functionPattern = /class\s*([\w\.]+)/g;
	let m: RegExpExecArray | null;

	const files = getImportDocumentText(text, root);
	files.push({
		filename: "main",
		text: text,
	});

	for (const i of files) {
		const text = i.text;
		while ((m = functionPattern.exec(text)) !== null) {
			definedClasses.push(m[1]);
		}
	}

	return definedClasses;
}

export function getImportDocumentText(
	text: string,
	root: string | undefined
): ImportData[] {
	const datas: ImportData[] = [];
	const files = getImport(text);
	for (const file of files) {
		const text = getDocumentText(file, root).replace("\r\n", "\n");
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
