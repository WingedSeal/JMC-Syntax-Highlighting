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
	private header: string;
	private value: string;

	constructor(text: string) {
		this.data = [];
		this.header = "";
		this.value = "";
		for (const line of text.split(/\r?\n/g)) {
			this.header = line.split(" ")[0];
			this.value = line.split(" ").slice(1).join("");
			const headerData: HeaderData = {
				type: this.getHeaderType(),
				values: this.getValues(),
			};

			this.data.push(headerData);
		}
	}

	private getHeaderType(): HeaderType {
		switch (this.header) {
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

	private getValues(): string[] {
		const header = this.getHeaderType();
		if (
			header == HeaderType.CREDIT ||
			header == HeaderType.INCLUDE ||
			header == HeaderType.STATIC
		) {
			return [this.value];
		}
		return this.value.split(" ");
	}
}
