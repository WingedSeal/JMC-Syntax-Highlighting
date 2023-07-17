/* eslint-disable @typescript-eslint/no-explicit-any */
export default class ServerLogger {
	private name: string;
	constructor(name: string) {
		this.name = name;
	}

	verbose(message: any) {
		console.info(this.getStyle("VERBOSE") + message);
	}

	debug(message: any) {
		console.info(this.getStyle("DEBUG") + message);
	}

	info(message: any) {
		console.info(this.getStyle("INFO") + message);
	}

	error(message: any) {
		console.info(this.getStyle("ERROR") + message);
	}

	fatal(message: any) {
		console.info(this.getStyle("FATAL") + message);
	}

	private getStyle(level: string): string {
		const date = new Date();
		const year = date.getFullYear();
		const month = date.getMonth();
		const day = date.getDate();
		const hours = date.getHours();
		const minute = date.getMinutes();
		return `[${year}-${month}-${day}][${hours}:${minute}][${this.name}][${level}] `;
	}
}
