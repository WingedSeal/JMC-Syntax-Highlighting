/* eslint-disable @typescript-eslint/no-explicit-any */
export default class ExtensionLogger {
	private name: string;
	level: number;
	constructor(name: string, level = 2) {
		this.name = name;
		this.level = level;
	}

	verbose(message: any) {
		if (this.level <= 0) console.info(this.getStyle("VERBOSE") + message);
	}

	debug(message: any) {
		if (this.level <= 1) console.info(this.getStyle("DEBUG") + message);
	}

	info(message: any) {
		if (this.level <= 2) console.info(this.getStyle("INFO") + message);
	}

	error(message: any) {
		if (this.level <= 3) console.info(this.getStyle("ERROR") + message);
	}

	fatal(message: any) {
		if (this.level <= 4) console.info(this.getStyle("FATAL") + message);
	}

	private getStyle(level: string): string {
		const date = new Date();
		const year = date.getFullYear();
		const month = date.getMonth();
		const day = date.getDate();
		const hours = date.getHours();
		const minute = date.getMinutes();
		return `[${year}-${month}-${day} ${hours}:${minute}] [${this.name}] [${level}] `;
	}
}
