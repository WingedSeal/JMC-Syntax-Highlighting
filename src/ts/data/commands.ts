import * as mcc from "./minecraft/command.json";

export interface ParserModifier {
	amount?: string;
	type?: string;
	min?: number;
	max?: number;
}

export interface CommandData {
	type: string;
	name: string;
	executable: boolean;
	redirects: string[];
	childrens: CommandData[];
	parser?: {
		parser: string;
		modifier?: ParserModifier;
	};
}

export interface CommandParser {
	parser: string;
	modifier: ParserModifier | null;
	examples: string[];
}

export const COMMANDS_TREE = (
	mcc.root.children as unknown as CommandData[]
).filter((v) => !["function", "reload"].includes(v.name));
export const START_COMMAND = COMMANDS_TREE.map((v) => v.name);

export const PARSERS: CommandParser[] =
	mcc.parsers as unknown as CommandParser[];
