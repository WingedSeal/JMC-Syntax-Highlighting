export interface ClassesMethods {
	name: string;
	methods: string[];
}

export interface ConfigData {
	namespace: string;
	description: string;
	pack_format: string;
	target: string;
	output: string;
}

export interface MacroData {
	macro: string;
	value: string;
}

export enum HeaderType {
	DEFINE,
	OVERRIDE_MINECRAFT,
	CREDIT,
	DEL,
	INCLUDE,
	UNINSTALL,
	STATIC,
	COMMAND,
}

export interface ParsedHeaderData {
	header: HeaderType;
	value?: string[];
	offset: number;
	length: number;
}

export interface HeaderData {
	header: HeaderType;
	value?: string[];
}

export const KEYWORDS: Array<string> = [
	"function",
	"new",
	"if",
	"else",
	"while",
	"for",
	"do",
	"switch",
	"case",
	"true",
	"false",
];

export const SEMI_CHECKCHAR: string[] = [";", "{", "}", "[", "]"];

export const VANILLA_COMMANDS: Array<string> = [
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

export const HEADERS: Array<string> = [
	"define",
	"credit",
	"include",
	"command",
	"override_minecraft",
	"static",
	"uninstall",
];

export const JSON_FILE_TYPES = [
	"advancements",
	"dimension",
	"dimension_type",
	"loot_tables",
	"predicates",
	"recipes",
	"item_modifiers",
	"structures",
	"worldgen/biome",
	"worldgen/configured_carver",
	"worldgen/configured_feature",
	"worldgen/configured_structure_feature",
	"worldgen/configured_surface_builder",
	"worldgen/noise_settings",
	"worldgen/processor_list",
	"worldgen/template_pool",
];
