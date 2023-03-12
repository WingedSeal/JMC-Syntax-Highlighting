import commandData from "../minecraft-data/command.json";

interface CommandNode {
	type: string;
	name: string;
	executable: boolean;
	redirects: string[];
	children: CommandNode[];
	parser?: { parser: string; modifier?: { amount: string; type: string } };
}

export class CommandType {
	public static LITERAL = "literal";
	public static ARGUMENT = "argument";
}

export class ParserType {
	public static MC_ENTITY = "minecraft:entity";
	public static RE_LOCATION = "minecraft:resource_location";
	public static VALUE_DOUBLE = "brigadier:double";
	public static BLOCK_POS = "minecraft:block_pos";
	public static NBT_PATH = "minecraft:nbt_path";
	public static MOB_EFFECT = "minecraft:mob_effect";
	public static VALUE_STRING = "brigadier:string";
	public static TIME = "minecraft:time";
	public static FUNCTION = "minecraft:function";
}

export class CommandData {
	public static ATTR_LIST: string[] = [
		"generic.max_health",
		"generic.follow_range",
		"generic.knockback_resistance",
		"generic.movement_speed",
		"generic.attack_damage",
		"generic.armor",
		"generic.armor_toughness",
		"generic.attack_knockback",
		"generic.attack_speed",
		"generic.luck",
		"horse.jump_strength",
		"generic.flying_speed",
		"zombie.spawn_reinforcements",
	];
}

export interface ParsedResult {
	node?: CommandNode[];
	error?: { pos: number; msg: string };
	type: { type: string; length: number; value?: string; parser?: string }[];
}

export function lexCommand(text: string): string[] {
	const splited = text
		.split(/( |\{|\}|\[|\])/g)
		.filter((v) => v !== undefined && v !== "");
	const data: string[] = [];
	for (let i = 0; i < splited.length; i++) {
		const current = splited[i];
		let lbCount = 0;
		let mbCount = 0;
		if (current === "{" || current === "[") {
			let joined = "";
			if (current === "{") {
				joined += "{";
				lbCount++;
			}
			if (current === "[") {
				joined += "[";
				mbCount++;
			}
			while (lbCount !== 0 || mbCount !== 0) {
				i++;
				const c = splited[i];

				if (c === "{") {
					lbCount++;
				} else if (c === "[") {
					mbCount++;
				} else if (c === "}") {
					lbCount--;
				} else if (c === "]") {
					mbCount--;
				}
				joined += c;
			}
			data.push(joined);
		} else if (/([\w\.]+)\s*\(/g.test(current)) {
			//TODO: add namespace
			data.push(current.split("(")[0].split(".").join("/") + "()");
		} else if (current !== " ") {
			data.push(current);
		}
	}
	const result: string[] = [];
	for (let i = 0; i < data.length; i++) {
		if (data[i + 1] !== undefined) {
			if (data[i + 1].startsWith("{") || data[i + 1].startsWith("[")) {
				result.push(data[i] + data[i + 1]);
				i++;
			} else {
				result.push(data[i]);
			}
		} else {
			result.push(data[i]);
		}
	}
	return result;
}

export function parseCommand(parsed: string[]): ParsedResult | undefined {
	let currentData = parsed[0];
	let currentDepth = 0;
	let currentNode: CommandNode[] = commandData.root.children as CommandNode[];

	const result: ParsedResult = { type: [] };

	while (currentDepth++ !== parsed.length) {
		const query = currentNode.find(
			(v) =>
				(v.type === CommandType.LITERAL && v.name === currentData) ||
				v.type !== CommandType.LITERAL
		);

		if (query === undefined) {
			return result;
		}

		if (query.parser !== undefined) {
			result.type.push({
				type: query.type,
				value: query.name,
				length: currentData.length,
				parser: query.parser.parser,
			});
		} else {
			result.type.push({
				type: query.type,
				value: query.name,
				length: currentData.length,
			});
		}

		if (query.executable && currentDepth === parsed.length) {
			return result;
		} else if (query.type === CommandType.ARGUMENT) {
			const parser = query.parser!.parser;
			switch (parser) {
				case ParserType.MC_ENTITY:
					if (!/@[parse]/g.test(currentData.slice(0, 2))) {
						result.error = {
							pos: currentDepth - 1,
							msg: "unknown selector, expected [@p,@a,@s,@r,@e]",
						};
						return result;
					}
					if (query.parser!.modifier !== undefined) {
						const value = currentData.slice(0, 2);
						const modifier = query.parser!.modifier;
						if (
							modifier.amount === "single" &&
							!/@[prs]/g.test(value)
						) {
							result.error = {
								pos: currentDepth - 1,
								msg: "single selector only",
							};
							return result;
						}
					}

					break;
				case ParserType.RE_LOCATION:
					if (currentData.startsWith(":")) {
						result.error = {
							pos: currentDepth - 1,
							msg: "invalid resource path",
						};
						return result;
					}
					if (
						query.name === "attribute" &&
						!CommandData.ATTR_LIST.includes(currentData)
					) {
						result.error = {
							pos: currentDepth - 1,
							msg: "invalid attribute",
						};
						return result;
					}
					break;
				case ParserType.VALUE_DOUBLE:
					if (!/\d+(\.\d+)?|\.\d+/g.test(currentData)) {
						result.error = {
							pos: currentDepth - 1,
							msg: "invalid number",
						};
						return result;
					}
					break;
				case ParserType.BLOCK_POS:
					if (!/(?:\^|~)(?:\d*(?:\.\d+))?/g.test(currentData)) {
						result.error = {
							pos: currentDepth - 1,
							msg: "invalid pos",
						};
						return result;
					}
					break;
				case ParserType.TIME:
				case ParserType.FUNCTION:
				case ParserType.VALUE_STRING:
				case ParserType.MOB_EFFECT:
				case ParserType.NBT_PATH:
					break;
				default:
					return result;
			}
		}
		currentNode = query.children;
		currentData = parsed[currentDepth];
		//TODO: execute redirect
		if (query.redirects.length > 0) {
			const redirect = (commandData.root.children as CommandNode[]).find(
				(v) => v.name === query.redirects[0]
			);
			if (redirect !== undefined) {
				currentNode = redirect.children;
			}
		}
		if (query.name === "run") {
			currentNode = commandData.root.children as CommandNode[];
		}
	}
	result.node = currentNode;
	return result;
}
