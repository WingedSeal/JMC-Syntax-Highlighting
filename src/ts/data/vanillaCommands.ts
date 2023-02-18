import { BLOCKS_ID, ITEMS_ID } from "./staticData";

export enum ValueType {
	KEYWORD,
	ENUM,
	TARGET,
	NUMBER,
	ADVANCEMENT,
	CRITERION,
	ATTRIBUTE,
	VALUE,
	UUID,
	NAME,
	ID,
	BOOLEANS,
	ITEM,
	VECTOR,
	BLOCK,
	DIMENSION,
}

export interface CommandArg {
	type: ValueType;
	value?: string[];
	optional?: boolean;
}

export interface Command {
	command: string;
	args: CommandArg[];
}

export const COMMANDS: Command[] = [
	{
		command: "advancement",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["grant", "revoke"],
			},
			{
				type: ValueType.TARGET,
			},
			{
				type: ValueType.KEYWORD,
				value: ["everything"],
			},
		],
	},
	{
		command: "advancement",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["grant", "revoke"],
			},
			{
				type: ValueType.TARGET,
			},
			{
				type: ValueType.KEYWORD,
				value: ["only"],
			},
			{
				type: ValueType.ADVANCEMENT,
			},
			{
				type: ValueType.CRITERION,
				optional: true,
			},
		],
	},
	{
		command: "advancement",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["grant", "revoke"],
			},
			{
				type: ValueType.TARGET,
			},
			{
				type: ValueType.KEYWORD,
				value: ["from", "through", "until"],
			},
			{
				type: ValueType.ADVANCEMENT,
			},
		],
	},
	{
		command: "attribute",
		args: [
			{
				type: ValueType.TARGET,
			},
			{
				type: ValueType.ATTRIBUTE,
			},
			{
				type: ValueType.KEYWORD,
				value: ["get"],
			},
			{
				type: ValueType.NUMBER,
				optional: true,
			},
		],
	},
	{
		command: "attribute",
		args: [
			{
				type: ValueType.TARGET,
			},
			{
				type: ValueType.ATTRIBUTE,
			},
			{
				type: ValueType.KEYWORD,
				value: ["base"],
			},
			{
				type: ValueType.KEYWORD,
				value: ["get"],
			},
			{
				type: ValueType.NUMBER,
				optional: true,
			},
		],
	},
	{
		command: "attribute",
		args: [
			{
				type: ValueType.TARGET,
			},
			{
				type: ValueType.ATTRIBUTE,
			},
			{
				type: ValueType.KEYWORD,
				value: ["base"],
			},
			{
				type: ValueType.KEYWORD,
				value: ["set"],
			},
			{
				type: ValueType.VALUE,
			},
		],
	},
	{
		command: "attribute",
		args: [
			{
				type: ValueType.TARGET,
			},
			{
				type: ValueType.ATTRIBUTE,
			},
			{
				type: ValueType.KEYWORD,
				value: ["modifier"],
			},
			{
				type: ValueType.KEYWORD,
				value: ["add"],
			},
			{
				type: ValueType.UUID,
			},
			{
				type: ValueType.NAME,
			},
			{
				type: ValueType.VALUE,
			},
			{
				type: ValueType.KEYWORD,
				value: ["add", "multiply", "multiply_base"],
			},
		],
	},
	{
		command: "attribute",
		args: [
			{
				type: ValueType.TARGET,
			},
			{
				type: ValueType.ATTRIBUTE,
			},
			{
				type: ValueType.KEYWORD,
				value: ["modifier"],
			},
			{
				type: ValueType.KEYWORD,
				value: ["remove"],
			},
			{
				type: ValueType.UUID,
			},
		],
	},
	{
		command: "attribute",
		args: [
			{
				type: ValueType.TARGET,
			},
			{
				type: ValueType.ATTRIBUTE,
			},
			{
				type: ValueType.KEYWORD,
				value: ["modifier"],
			},
			{
				type: ValueType.KEYWORD,
				value: ["value"],
			},
			{
				type: ValueType.KEYWORD,
				value: ["get"],
			},
			{
				type: ValueType.NUMBER,
				optional: true,
			},
		],
	},
	{
		command: "bossbar",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["add"],
			},
			{
				type: ValueType.ID,
			},
			{
				type: ValueType.NAME,
			},
		],
	},
	{
		command: "bossbar",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["get"],
			},
			{
				type: ValueType.ID,
			},
			{
				type: ValueType.KEYWORD,
				value: ["max", "players", "value", "visible"],
			},
		],
	},
	{
		command: "bossbar",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["remove"],
			},
			{
				type: ValueType.ID,
			},
		],
	},
	{
		command: "bossbar",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["set"],
			},
			{
				type: ValueType.ID,
			},
			{
				type: ValueType.KEYWORD,
				value: ["color"],
			},
			{
				type: ValueType.ENUM,
				value: [
					"blue",
					"green",
					"pink",
					"purple",
					"red",
					"white",
					"yellow",
				],
			},
		],
	},
	{
		command: "bossbar",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["set"],
			},
			{
				type: ValueType.ID,
			},
			{
				type: ValueType.KEYWORD,
				value: ["max"],
			},
			{
				type: ValueType.NUMBER,
			},
		],
	},
	{
		command: "bossbar",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["set"],
			},
			{
				type: ValueType.ID,
			},
			{
				type: ValueType.KEYWORD,
				value: ["name"],
			},
			{
				type: ValueType.NAME,
			},
		],
	},
	{
		command: "bossbar",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["set"],
			},
			{
				type: ValueType.ID,
			},
			{
				type: ValueType.KEYWORD,
				value: ["players"],
			},
			{
				type: ValueType.TARGET,
			},
		],
	},
	{
		command: "bossbar",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["set"],
			},
			{
				type: ValueType.ID,
			},
			{
				type: ValueType.KEYWORD,
				value: ["style"],
			},
			{
				type: ValueType.ENUM,
				value: [
					"notched_6",
					"notched_10",
					"notched_12",
					"notched_20",
					"progress",
				],
			},
		],
	},
	{
		command: "bossbar",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["set"],
			},
			{
				type: ValueType.ID,
			},
			{
				type: ValueType.KEYWORD,
				value: ["value"],
			},
			{
				type: ValueType.VALUE,
			},
		],
	},
	{
		command: "bossbar",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["set"],
			},
			{
				type: ValueType.ID,
			},
			{
				type: ValueType.KEYWORD,
				value: ["visible"],
			},
			{
				type: ValueType.BOOLEANS,
				value: ["true", "false"],
			},
		],
	},
	{
		command: "clear",
		args: [
			{
				type: ValueType.TARGET,
			},
			{
				type: ValueType.ITEM,
			},
			{
				type: ValueType.NUMBER,
				optional: true,
			},
		],
	},
	{
		command: "clone",
		args: [
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.KEYWORD,
				value: ["replace", "masked"],
				optional: true,
			},
			{
				type: ValueType.ENUM,
				value: ["force", "move", "normal"],
				optional: true,
			},
		],
	},
	{
		command: "clone",
		args: [
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.KEYWORD,
				value: ["filtered"],
				optional: true,
			},
			{
				type: ValueType.BLOCK,
			},
			{
				type: ValueType.ENUM,
				value: ["force", "move", "normal"],
				optional: true,
			},
		],
	},
	{
		command: "clone",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["from"],
			},
			{
				type: ValueType.DIMENSION,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
		],
	},
	{
		command: "clone",
		args: [
			{
				type: ValueType.KEYWORD,
				value: ["to"],
			},
			{
				type: ValueType.DIMENSION,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
			{
				type: ValueType.VECTOR,
			},
		],
	},
];

//const valuePattern = /[\w.]+/g;


// export function getCommand(text: string): RegExp[] {
// 	const results: RegExp[] = [];

// 	for (const command of CommandArguments) {

// 	}

// 	return results;
// }
