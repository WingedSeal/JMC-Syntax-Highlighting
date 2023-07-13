import cmd from "./minecraft/commands.json";

export interface ParserPropety {
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
	properties?: ParserPropety;
	redirect?: string[];
}

interface Parser {
	parser: string;
	properties?: ParserPropety;
	regex: RegExp;
}

//TODO:
const parsers: Parser[] = [
	{
		parser: "brigadier:double",
		regex: /^-?\d*(?:\.\d+)?(?!\.)$/,
	},
	{
		parser: "brigadier:float",
		regex: /^-?\d*(?:\.\d+)?(?!\.)$/,
	},
	{
		parser: "brigadier:long",
		regex: /^-?\d*(?:\.\d+)?(?!\.)$/,
	},
	{
		parser: "brigadier:integer",
		regex: /^\d+$/,
	},
	{
		parser: "brigadier:string",
		properties: {
			type: "phrase",
		},
		regex: /^\"(?:.|\s)*\"$/,
	},
	{
		parser: "brigadier:string",
		properties: {
			type: "word",
		},
		regex: /^\w+$/,
	},
	{
		parser: "brigadier:string",
		properties: {
			type: "greedy",
		},
		regex: /^(?:.|\s)*$/,
	},
	{
		parser: "minecraft:entity",
		properties: {
			type: "players",
			amount: "multiple",
		},
		regex: /^@[pars]/,
	},
	{
		parser: "minecraft:entity",
		properties: {
			type: "players",
			amount: "single",
		},
		regex: /^@r|@p|@s/,
	},
	{
		parser: "minecraft:entity",
		properties: {
			type: "entities",
			amount: "multiple",
		},
		regex: /^@[parse]/,
	},
	{
		parser: "minecraft:entity",
		properties: {
			type: "entities",
			amount: "single",
		},
		regex: /^@[prs]$/,
	},
	{
		parser: "minecraft:score_holder",
		properties: {
			amount: "single",
		},
		regex: /^@[prs]/,
	},
	{
		parser: "minecraft:score_holder",
		properties: {
			amount: "multiple",
		},
		regex: /^@[parse]/,
	},
];

const cmd_datas = cmd as unknown as CommandNode;
const treeHead = Object.entries(cmd_datas.children);
export const START_COMMAND = treeHead
	.map((v) => v[0])
	.filter((v) => !["function", "reload"].includes(v));
export const COMMAND_ENTITY_SELECTORS: string[] = ["@s", "@p", "@r", "@e", "@a"];

export function getNode(tokens: string[]): [string, CommandNode][] {
	let treeNode = treeHead;
	for (let i = 0; i < tokens.length + 1; i++) {
		const token = tokens[i];
		const q = treeNode.find((v) => v[0] === token);
		if (q && q[1].children) {
			treeNode = Object.entries(q[1].children);
		} else if (q && q[1].redirect) {
			treeNode = treeHead;
		} else if (
			treeNode.length === 1 &&
			treeNode[0][1].type === "argument" &&
			treeNode[0][1].children
		) {
			treeNode = Object.entries(treeNode[0][1].children);
		} else if (
			treeNode.length === 1 &&
			treeNode[0][1].redirect &&
			i !== tokens.length
		) {
			const redirect = treeNode[0][1].redirect[0];
			const qe = treeHead.find((v) => v[0] === redirect);
			if (qe) {
				treeNode = Object.entries(qe[1].children);
			} else {
				return treeNode;
			}
		} else {
			return treeNode;
		}
	}
	return [];
}

export function checkExecutable(commands: [string, CommandNode][]): boolean {
	return commands.length === 0
		? false
		: commands[commands.length - 1][1].executable ?? false;
}
