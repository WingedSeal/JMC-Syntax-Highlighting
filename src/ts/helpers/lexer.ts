import { VANILLA_COMMANDS } from "../data/common";

interface CommandToken {
	offset: number;
	length: number;
	value: string[];
}

const StopChar: string[] = [";", "{", "}", "/"];

export function tokenizeCommand(text: string): CommandToken[] {
	const values: CommandToken[] = [];
	let parse = "";
	let index = 0;
	while ((index += 1) !== text.length) {
		const current = text[index];
		parse += current;
		if (StopChar.includes(current)) {
			parse = parse.slice(0, -1);
			const command = parse.split(" ").filter((v) => v.trim() !== "");
			if (VANILLA_COMMANDS.includes(command[0])) {
				values.push({
					value: command,
					length: parse.trim().length,
					offset: index,
				});
			}
			parse = "";
		}
	}
	return values;
}
