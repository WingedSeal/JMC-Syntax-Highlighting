export const keywords: Array<string> = [
	"function",
	"new",
	"if",
	"else",
	"while",
	"for",
	"do",
	"switch",
	"case",
];

export const VanillaKeywords: Array<string> = [
	"advancement",
	"attribute",
	"bossbar",
	"clear",
	"clear",
	"clone",
	"data",
	"datapack",
	"defaultgamemode",
	"difficulty",
	"effect",
	"enchant",
	"execute",
	"experience",
	"fill",
	"fillbiome",
	"foceload",
	"function",
	"gamemode",
	"gamerule",
	"give",
	"item",
	"kill",
	"loot",
	"me",
	"msg",
	"particle",
	"place",
	"playsound",
	"recipe",
	"say",
	"schedule",
	"scoreboard",
	"setblock",
	"spawnpoint",
	"setidletimeout",
	"setworldspawn",
	"spectate",
	"spreadplayers",
	"stopsound",
	"summon",
	"tag",
	"team",
	"teammsg",
	"teleport",
	"tell",
	"tellraw",
	"time",
	"title",
	"tm",
	"tp",
	"trigger",
	"weather",
	"worldborder",
	"xp",
];

export const Headers: Array<string> = [
	"define",
	"credit",
	"include",
	"command",
	"override_minecraft",
	"static",
];

export interface textLinePos {
	line: number;
	length: number;
}

export function getLineByIndex(
	index: number,
	linepos: textLinePos[]
): { line: number; pos: number } {
	for (let i of linepos) {
		if (index < i.length) {
			return { line: i.line, pos: index };
		}
		index -= i.length;
	}
	return { line: -1, pos: -1 };
}

export function getLinePos(text: string): textLinePos[] {
	let textLinePos: textLinePos[] = [];
	let textLines = text.split("\n");
	for (let i = 0; i < textLines.length; i++) {
		let textLine = textLines[i];
		textLinePos.push({
			line: i,
			length: textLine.length + 1,
		});
	}
	return textLinePos;
}
