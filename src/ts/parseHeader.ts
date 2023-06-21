export enum HeaderType {
	DEFINE,
	BIND,
	CREDIT,
	INCLUDE,
	COMMAND,
	DEL,
	OVERRIDE_MINECRAFT,
	UNINSTALL,
	STATIC,
	UNKNOWN,
}

export interface HeaderData {
	type: HeaderType;
	values: string[];
}

export class HeaderParser {
	public data: HeaderData[];

	constructor(text: string) {
		this.data = [];
		let i = 0;
		for (const line of text.split(/\r?\n/g)) {
			this.data.push(HeaderParser.parseText(line));
			i++;
		}
	}

	static parseText(line: string): HeaderData {
		const header = line.split(" ")[0];
		const value = line.split(" ").slice(1).join(" ");
		const headerData: HeaderData = {
			type: this.getHeaderType(header),
			values: this.getValues(header, value),
		};
		return headerData;
	}

	private static getHeaderType(header: string): HeaderType {
		switch (header) {
			case "#define":
				return HeaderType.DEFINE;
			case "#bind":
				return HeaderType.BIND;
			case "#credit":
				return HeaderType.CREDIT;
			case "#include":
				return HeaderType.INCLUDE;
			case "#command":
				return HeaderType.COMMAND;
			case "#del":
				return HeaderType.DEL;
			case "#override_minecraft":
				return HeaderType.OVERRIDE_MINECRAFT;
			case "#uninstall":
				return HeaderType.UNINSTALL;
			case "#static":
				return HeaderType.STATIC;
			default:
				return HeaderType.UNKNOWN;
		}
	}

	private static getValues(headerS: string, value: string): string[] {
		const header = this.getHeaderType(headerS);
		if (
			header == HeaderType.CREDIT ||
			header == HeaderType.INCLUDE ||
			header == HeaderType.STATIC
		) {
			return [value];
		}
		return value.split(" ");
	}
}
