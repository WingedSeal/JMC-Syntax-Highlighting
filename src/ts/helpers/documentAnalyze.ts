import { getDocumentText, getImport } from "./documentHelper";

export interface ImportData {
	filename: string;
	text: string;
}

export function getVariables(text: string, root: string): string[] {
	let definedVariables: string[] = [];

	let variablePattern = /\$(\w+)\s*\??=(?!=)/g;
	let m: RegExpExecArray | null;

	let files = getImportDocumentText(text, root);
	files.push({
		filename: "main",
		text: text,
	});

	for (let i of files) {
		let text = i.text;
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
	let variables: string[] = [];

	let variablePattern = /\$([\w\.]+)/g;
	let m: RegExpExecArray | null;

	let files = getImportDocumentText(text, root);
	files.push({
		filename: "main",
		text: text,
	});

	for (let i of files) {
		let text = i.text;
		while ((m = variablePattern.exec(text)) !== null) {
			if (m[1].endsWith(".get")) {
				variables.push(m[1].slice(0, -4));
			} else {
				variables.push(m[1]);
			}
		}
	}

	let nonDuplicate = variables.filter((item, index) => {
		variables.splice(index, 1);
		const unique = !variables.includes(item) && !item.endsWith(".get");
		variables.splice(index, 0, item);
		return unique;
	});

	return nonDuplicate;
}
export function getFunctions(text: string, root: string): string[] {
	let definedFunctions: string[] = [];

	let functionPattern = /function\s*([\w\.]+)\(/g;
	let m: RegExpExecArray | null;

	let files = getImportDocumentText(text, root);
	files.push({
		filename: "main",
		text: text,
	});

	for (let i of files) {
		let text = i.text;
		while ((m = functionPattern.exec(text)) !== null) {
			definedFunctions.push(m[1]);
		}
	}

	return definedFunctions;
}

export function getClass(text: string, root: string): string[] {
	let definedClasses: string[] = [];

	let functionPattern = /class\s*([\w\.]+)/g;
	let m: RegExpExecArray | null;

	let files = getImportDocumentText(text, root);
	files.push({
		filename: "main",
		text: text,
	});

	for (let i of files) {
		let text = i.text;
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
	let datas: ImportData[] = [];
	let files = getImport(text);
	for (let file of files) {
		let text = getDocumentText(file, root).replace("\r\n","\n");
		datas.push({
			filename: file,
			text: text,
		});
	}
	return datas;
}