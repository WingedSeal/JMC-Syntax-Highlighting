// import MinecraftData from "minecraft-data";

// export interface Command {
// 	root: CommandData;
// 	parsers: CommandParser[];
// }

// export interface ParserModifier {
// 	amount?: string;
// 	type?: string;
// 	min?: number;
// 	max?: number;
// }

// export interface CommandData {
// 	type: string;
// 	name: string;
// 	executable: boolean;
// 	redirects: string[];
// 	children: CommandData[];
// 	parser?: {
// 		parser: string;
// 		modifier?: ParserModifier;
// 	};
// }

// export interface CommandParser {
// 	parser: string;
// 	modifier: ParserModifier | null;
// 	examples: string[];
// }

// const mcc = MinecraftData("1.16").commands as Command;
// export const COMMANDS_TREE = (
// 	mcc.root.children as unknown as CommandData[]
// ).filter((v) => !["function", "reload"].includes(v.name));
// export const START_COMMAND = COMMANDS_TREE.map((v) => v.name);

// export const PARSERS: CommandParser[] =
// 	mcc.parsers as unknown as CommandParser[];

import cmd from "./minecraft/commands.json";

export interface ParserModifier {
	type?: string;
	amount?: string;
	min?: number;
	max?: number;
}

export interface CommandNode {
	type: string;
	children: CommandNode[];
	executable?: boolean;
	parser: string;
	properties: ParserModifier;
}

const cmd_datas = cmd as unknown as CommandNode;
export const START_COMMAND = Object.entries(cmd_datas.children)
	.map((v) => v[0])
	.filter((v) => !["function", "reload"].includes(v));
