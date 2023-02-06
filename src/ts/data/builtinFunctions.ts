const TARGET_SELECTOR = "TargetSelector";
const KEYWORD = "Keyword";
const CRITERIA = "Criteria";
const FUNCTION = "Function";
const ITEM = "Item";
const FORMATTED_STRING = "FormattedString";
const LIST_FORMATTED_STRING = "List<FormattedString>";
const JS_OBJ = "JSObject";
const INTEGER = "integer";
const STRING = "string";
const SCOREBOARD = "Scoreboard";
const SCOREBOARD_INTEGER = "ScoreboardInteger";
const OBJECTIVE = "Objective";
const BOOLEAN = "Boolean";
const JSON = "JSON";
const ARROW_FUNCTION = "ArrowFunction";
const LIST = "List";
const JS_OBJ_IF = "JSObject<integer, Function>";
const FLOAT = "float";

const EXECUTE_EXCLUDED = "ExecuteExcluded";
const JMC_FUNCTION = "JMCFunction";
const LOAD_ONLY = "LoadOnly";
const LOAD_ONCE = "LoadOnce";
const VARIABLE_OPERATION = "VariableOperation";

interface ArgInfo {
	name: string;
	type: string;
	default?: string;
}

interface MethodInfo {
	name: string;
	args: ArgInfo[];
	returnType: string;
}

interface BuiltInFunction {
	class: string;
	methods: MethodInfo[];
}

export function methodInfoToDoc(info: MethodInfo): string {
	let argString = info.args.flatMap((v): string => {
		let def = v.default !== undefined ? ` = ${v.default}` : "";
		return `${v.name}: ${v.type}${def}`;
	});
	return `${info.name}(${argString}): ${info.returnType} `;
}

export const BuiltInFunctions: Array<BuiltInFunction> = [
	{
		class: "Advancement",
		methods: [
			{
				name: "grant",
				args: [
					{
						name: "target",
						type: TARGET_SELECTOR,
					},
					{
						name: "type",
						type: KEYWORD,
					},
					{
						name: "advancement",
						type: KEYWORD,
					},
					{
						name: "namespace",
						type: KEYWORD,
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "revoke",
				args: [
					{
						name: "target",
						type: TARGET_SELECTOR,
					},
					{
						name: "type",
						type: KEYWORD,
					},
					{
						name: "advancement",
						type: KEYWORD,
					},
					{
						name: "namespace",
						type: KEYWORD,
						default: "",
					},
				],
				returnType: JMC_FUNCTION,
			},
		],
	},
	{
		class: "Player",
		methods: [
			{
				name: "onEvent",
				args: [
					{
						name: "criteria",
						type: CRITERIA,
					},
					{
						name: "function",
						type: FUNCTION,
					},
				],
				returnType: LOAD_ONLY,
			},
			{
				name: "firstJoin",
				args: [
					{
						name: "function",
						type: FUNCTION,
					},
				],
				returnType: LOAD_ONCE,
			},
			{
				name: "reJoin",
				args: [
					{
						name: "function",
						type: FUNCTION,
					},
				],
				returnType: LOAD_ONCE,
			},
			{
				name: "die",
				args: [
					{
						name: "onDeath",
						type: FUNCTION,
						default: "()=>{}",
					},
					{
						name: "onRespawn",
						type: FUNCTION,
						default: "() => {}",
					},
				],
				returnType: LOAD_ONCE,
			},
		],
	},
	{
		class: "Item",
		methods: [
			{
				name: "create",
				args: [
					{
						name: "itemId",
						type: KEYWORD,
					},
					{
						name: "itemType",
						type: ITEM,
					},
					{
						name: "displayName",
						type: FORMATTED_STRING,
					},
					{
						name: "lore",
						type: LIST_FORMATTED_STRING,
						default: "[]",
					},
					{
						name: "nbt",
						type: JS_OBJ,
						default: "{}",
					},
					{
						name: "onClick",
						type: FUNCTION,
						default: "()=>{}",
					},
				],
				returnType: LOAD_ONLY,
			},
			{
				name: "createSign",
				args: [
					{
						name: "itemId",
						type: KEYWORD,
					},
					{
						name: "variant",
						type: ITEM,
					},
					{
						name: "displayName",
						type: FORMATTED_STRING,
					},
					{
						name: "texts",
						type: LIST_FORMATTED_STRING,
						default: "[]",
					},
					{
						name: "nbt",
						type: JS_OBJ,
						default: "{}",
					},
					{
						name: "onClick",
						type: FUNCTION,
						default: "()=>{}",
					},
				],
				returnType: LOAD_ONLY,
			},
			{
				name: "createSpawnEgg",
				args: [
					{
						name: "itemId",
						type: KEYWORD,
					},
					{
						name: "mobType",
						type: KEYWORD,
					},
					{
						name: "displayName",
						type: FORMATTED_STRING,
					},
					{
						name: "onPlace",
						type: FUNCTION,
						default: "()=>{}",
					},
					{
						name: "lore",
						type: LIST_FORMATTED_STRING,
						default: "[]",
					},
					{
						name: "nbt",
						type: JS_OBJ,
						default: "{}",
					},
				],
				returnType: LOAD_ONLY,
			},
			{
				name: "give",
				args: [
					{
						name: "itemId",
						type: KEYWORD,
					},
					{
						name: "selector",
						type: TARGET_SELECTOR,
						default: "@s",
					},
					{
						name: "amount",
						type: INTEGER,
						default: "1",
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "summon",
				args: [
					{
						name: "itemId",
						type: KEYWORD,
					},
					{
						name: "pos",
						type: STRING,
					},
					{
						name: "count",
						type: INTEGER,
						default: "1",
					},
					{
						name: "nbt",
						type: JS_OBJ,
						default: "{}",
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "replaceBlock",
				args: [
					{
						name: "itemId",
						type: KEYWORD,
					},
					{
						name: "pos",
						type: STRING,
					},
					{
						name: "slot",
						type: STRING,
					},
					{
						name: "count",
						type: INTEGER,
						default: "1",
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "replaceEntity",
				args: [
					{
						name: "itemId",
						type: KEYWORD,
					},
					{
						name: "selector",
						type: TARGET_SELECTOR,
					},
					{
						name: "slot",
						type: STRING,
					},
					{
						name: "count",
						type: INTEGER,
						default: "1",
					},
				],
				returnType: JMC_FUNCTION,
			},
		],
	},
	{
		class: "Text",
		methods: [
			{
				name: "tellraw",
				args: [
					{
						name: "selector",
						type: TARGET_SELECTOR,
					},
					{
						name: "message",
						type: FORMATTED_STRING,
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "title",
				args: [
					{
						name: "selector",
						type: TARGET_SELECTOR,
					},
					{
						name: "message",
						type: FORMATTED_STRING,
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "subtitle",
				args: [
					{
						name: "selector",
						type: TARGET_SELECTOR,
					},
					{
						name: "message",
						type: FORMATTED_STRING,
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "actionbar",
				args: [
					{
						name: "selector",
						type: TARGET_SELECTOR,
					},
					{
						name: "message",
						type: FORMATTED_STRING,
					},
				],
				returnType: JMC_FUNCTION,
			},
		],
	},
	{
		class: "Math",
		methods: [
			{
				name: "sqrt",
				args: [
					{
						name: "n",
						type: SCOREBOARD,
					},
				],
				returnType: VARIABLE_OPERATION,
			},
			{
				name: "random",
				args: [
					{
						name: "min",
						type: INTEGER,
						default: "1",
					},
					{
						name: "max",
						type: INTEGER,
						default: "2147483647",
					},
				],
				returnType: VARIABLE_OPERATION,
			},
		],
	},
	{
		class: "Timer",
		methods: [
			{
				name: "add",
				args: [
					{
						name: "objective",
						type: OBJECTIVE,
					},
					{
						name: "mode",
						type: KEYWORD,
					},
					{
						name: "selector",
						type: TARGET_SELECTOR,
					},
					{
						name: "function",
						type: FUNCTION,
						default: "()=>{}",
					},
				],
				returnType: LOAD_ONLY,
			},
			{
				name: "set",
				args: [
					{
						name: "objective",
						type: OBJECTIVE,
					},
					{
						name: "selector",
						type: TARGET_SELECTOR,
					},
					{
						name: "tick",
						type: SCOREBOARD_INTEGER,
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "isOver",
				args: [
					{
						name: "objective",
						type: OBJECTIVE,
					},
					{
						name: "selector",
						type: TARGET_SELECTOR,
						default: "@s",
					},
				],
				returnType: BOOLEAN,
			},
		],
	},
	{
		class: "Recipe",
		methods: [
			{
				name: "methods",
				args: [
					{
						name: "recipe",
						type: JSON,
					},
					{
						name: "baseItem",
						type: ITEM,
						default: "knowledge_book",
					},
					{
						name: "onCraft",
						type: FUNCTION,
						default: "()=>{}",
					},
				],
				returnType: LOAD_ONLY,
			},
		],
	},
	{
		class: "Hardcode",
		methods: [
			{
				name: "repeat",
				args: [
					{
						name: "indexString",
						type: STRING,
					},
					{
						name: "function",
						type: ARROW_FUNCTION,
					},
					{
						name: "start",
						type: INTEGER,
					},
					{
						name: "stop",
						type: INTEGER,
					},
					{
						name: "step",
						type: INTEGER,
						default: "1",
					},
				],
				returnType: EXECUTE_EXCLUDED,
			},
			{
				name: "repeatList",
				args: [
					{
						name: "indexString",
						type: STRING,
					},
					{
						name: "function",
						type: ARROW_FUNCTION,
					},
					{
						name: "strings",
						type: LIST,
					},
				],
				returnType: EXECUTE_EXCLUDED,
			},
			{
				name: "switch",
				args: [
					{
						name: "switch",
						type: SCOREBOARD,
					},
					{
						name: "indexString",
						type: STRING,
					},
					{
						name: "function",
						type: ARROW_FUNCTION,
					},
					{
						name: "count",
						type: INTEGER,
					},
				],
				returnType: EXECUTE_EXCLUDED,
			},
		],
	},
	{
		class: "Trigger",
		methods: [
			{
				name: "setup",
				args: [
					{
						name: "objective",
						type: STRING,
					},
					{
						name: "triggers",
						type: JS_OBJ_IF,
					},
				],
				returnType: LOAD_ONLY,
			},
			{
				name: "add",
				args: [
					{
						name: "objective",
						type: KEYWORD,
					},
					{
						name: "function",
						type: FUNCTION,
					},
				],
				returnType: LOAD_ONLY,
			},
		],
	},
	{
		class: "Predicate",
		methods: [
			{
				name: "locations",
				args: [
					{
						name: "name",
						type: STRING,
					},
					{
						name: "predicate",
						type: JSON,
					},
					{
						name: "xMin",
						type: INTEGER,
					},
					{
						name: "xMax",
						type: INTEGER,
					},
					{
						name: "yMin",
						type: INTEGER,
					},
					{
						name: "yMax",
						type: INTEGER,
					},
					{
						name: "zMin",
						type: INTEGER,
					},
					{
						name: "zMax",
						type: INTEGER,
					},
				],
				returnType: LOAD_ONLY,
			},
		],
	},
	{
		class: "RightClick",
		methods: [
			{
				name: "setup",
				args: [
					{
						name: "idName",
						type: KEYWORD,
					},
					{
						name: "functionMap",
						type: JS_OBJ_IF,
					},
				],
				returnType: LOAD_ONLY,
			},
		],
	},
	{
		class: "Particle",
		methods: [
			{
				name: "circle",
				args: [
					{
						name: "particle",
						type: STRING,
					},
					{
						name: "radius",
						type: FLOAT,
					},
					{
						name: "spread",
						type: INTEGER,
					},
					{
						name: "speed",
						type: INTEGER,
						default: "1",
					},
					{
						name: "count",
						type: INTEGER,
						default: "1",
					},
					{
						name: "mode",
						type: KEYWORD,
						default: "normal",
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "spiral",
				args: [
					{
						name: "particle",
						type: STRING,
					},
					{
						name: "radius",
						type: FLOAT,
					},
					{
						name: "height",
						type: FLOAT,
					},
					{
						name: "spread",
						type: INTEGER,
					},
					{
						name: "speed",
						type: INTEGER,
						default: "1",
					},
					{
						name: "count",
						type: INTEGER,
						default: "1",
					},
					{
						name: "mode",
						type: KEYWORD,
						default: "normal",
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "cylinder",
				args: [
					{
						name: "particle",
						type: STRING,
					},
					{
						name: "radius",
						type: FLOAT,
					},
					{
						name: "height",
						type: FLOAT,
					},
					{
						name: "spreadXZ",
						type: INTEGER,
					},
					{
						name: "spreadY",
						type: INTEGER,
					},
					{
						name: "speed",
						type: INTEGER,
						default: "1",
					},
					{
						name: "count",
						type: INTEGER,
						default: "1",
					},
					{
						name: "mode",
						type: KEYWORD,
						default: "normal",
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "line",
				args: [
					{
						name: "particle",
						type: STRING,
					},
					{
						name: "distance",
						type: FLOAT,
					},
					{
						name: "spread",
						type: INTEGER,
					},
					{
						name: "speed",
						type: INTEGER,
						default: "1",
					},
					{
						name: "count",
						type: INTEGER,
						default: "1",
					},
					{
						name: "mode",
						type: KEYWORD,
						default: "normal",
					},
				],
				returnType: JMC_FUNCTION,
			},
		],
	},
	{
		class: "Raycast",
		methods: [
			{
				name: "simple",
				args: [
					{
						name: "onHit",
						type: FUNCTION,
					},
					{
						name: "onStep",
						type: FUNCTION,
						default: "()=>{}",
					},
					{
						name: "onBeforeStep",
						type: FUNCTION,
						default: "()=>{}",
					},
					{
						name: "interval",
						type: FLOAT,
						default: "0.1",
					},
					{
						name: "maxIter",
						type: INTEGER,
						default: "1000",
					},
					{
						name: "boxSize",
						type: FLOAT,
						default: "0.1",
					},
					{
						name: "target",
						type: TARGET_SELECTOR,
						default: "@e",
					},
					{
						name: "startAtEye",
						type: BOOLEAN,
						default: "true",
					},
					{
						name: "stopAtEntity",
						type: BOOLEAN,
						default: "true",
					},
					{
						name: "stopAtBlock",
						type: BOOLEAN,
						default: "true",
					},
					{
						name: "runAtEnd",
						type: BOOLEAN,
						default: "true",
					},
					{
						name: "casterTag",
						type: KEYWORD,
						default: "__self__",
					},
					{
						name: "removeCasterTag",
						type: KEYWORD,
						default: "true",
					},
					{
						name: "modifyExecuteBeforeStep",
						type: STRING,
						default: '""',
					},
					{
						name: "modifyExecuteAfterStep",
						type: STRING,
						default: '""',
					},
					{
						name: "overrideString",
						type: STRING,
						default: '""',
					},
					{
						name: "overrideRecursion",
						type: ARROW_FUNCTION,
						default: "()=>{}",
					},
				],
				returnType: EXECUTE_EXCLUDED,
			},
		],
	},
	{
		class: "JMC",
		methods: [
			{
				name: "put",
				args: [
					{
						name: "command",
						type: STRING,
					},
				],
				returnType: JMC_FUNCTION,
			},
		],
	},
	{
		class: "String",
		methods: [
			{
				name: "isEqual",
				args: [
					{
						name: "type",
						type: KEYWORD,
					},
					{
						name: "source",
						type: STRING,
					},
					{
						name: "path",
						type: KEYWORD,
					},
					{
						name: "string",
						type: STRING,
					},
				],
				returnType: BOOLEAN,
			},
		],
	},
	{
		class: "Object",
		methods: [
			{
				name: "isEqual",
				args: [
					{
						name: "type1",
						type: KEYWORD,
					},
					{
						name: "source1",
						type: STRING,
					},
					{
						name: "path1",
						type: KEYWORD,
					},
					{
						name: "type2",
						type: KEYWORD,
					},
					{
						name: "source2",
						type: STRING,
					},
					{
						name: "path2",
						type: KEYWORD,
					},
				],
				returnType: BOOLEAN,
			},
		],
	},
	{
		class: "GUI",
		methods: [
			{
				name: "template",
				args: [
					{
						name: "name",
						type: KEYWORD,
					},
					{
						name: "template",
						type: "List<string>",
					},
					{
						name: "mode",
						type: KEYWORD,
					},
				],
				returnType: LOAD_ONLY,
			},
			{
				name: "register",
				args: [
					{
						name: "name",
						type: KEYWORD,
					},
					{
						name: "id",
						type: STRING,
					},
					{
						name: "item",
						type: ITEM,
					},
					{
						name: "displayName",
						type: FORMATTED_STRING,
						default: '""',
					},
					{
						name: "lore",
						type: "List<FormattedString>",
						default: "[]",
					},
					{
						name: "nbt",
						type: JS_OBJ,
						default: "{}",
					},
					{
						name: "onClick",
						type: FUNCTION,
						default: "()=>{}",
					},
					{
						name: "onClickAsGUI",
						type: FUNCTION,
						default: "()=>{}",
					},
				],
				returnType: LOAD_ONLY,
			},
			{
				name: "registers",
				args: [
					{
						name: "name",
						type: KEYWORD,
					},
					{
						name: "id",
						type: STRING,
					},
					{
						name: "items",
						type: "List<Keyword>",
					},
					{
						name: "variable",
						type: SCOREBOARD,
					},
					{
						name: "onClick",
						type: FUNCTION,
						default: "()=>{}",
					},
					{
						name: "onClickAsGUI",
						type: FUNCTION,
						default: "()=>{}",
					},
				],
				returnType: LOAD_ONLY,
			},
			{
				name: "create",
				args: [
					{
						name: "name",
						type: KEYWORD,
					},
				],
				returnType: LOAD_ONLY,
			},
			{
				name: "run",
				args: [
					{
						name: "name",
						type: KEYWORD,
					},
				],
				returnType: JMC_FUNCTION,
			},
		],
	},
	{
		class: "Advancement",
		methods: [
			{
				name: "grant",
				args: [
					{
						name: "target",
						type: TARGET_SELECTOR,
					},
					{
						name: "type",
						type: KEYWORD,
					},
					{
						name: "advancement",
						type: KEYWORD,
					},
					{
						name: "namespace",
						type: KEYWORD,
						default: '""',
					},
				],
				returnType: JMC_FUNCTION,
			},
			{
				name: "revoke",
				args: [
					{
						name: "target",
						type: TARGET_SELECTOR,
					},
					{
						name: "type",
						type: KEYWORD,
					},
					{
						name: "advancement",
						type: KEYWORD,
					},
					{
						name: "namespace",
						type: KEYWORD,
						default: '""',
					},
				],
				returnType: JMC_FUNCTION,
			},
		],
	},
	{
		class: "Scoreboard",
		methods: [
			{
				name: "add",
				args: [
					{
						name: "objective",
						type: KEYWORD,
					},
					{
						name: "criteria",
						type: CRITERIA,
						default: "dummy",
					},
					{
						name: "displayName",
						type: FORMATTED_STRING,
						default: '""',
					},
				],
				returnType: JMC_FUNCTION,
			},
		],
	},
	{
		class: "Bossbar",
		methods: [
			{
				name: "add",
				args: [
					{
						name: "team",
						type: KEYWORD,
					},
					{
						name: "displayName",
						type: FORMATTED_STRING,
						default: '""',
					},
				],
				returnType: JMC_FUNCTION,
			},
		],
	},
	{
		class: "Team",
		methods: [
			{
				name: "add",
				args: [
					{
						name: "id",
						type: KEYWORD,
					},
					{
						name: "name",
						type: FORMATTED_STRING,
						default: '""',
					},
					{
						name: "propeties",
						type: "SObject<Keyword, Keyword>",
						default: "{}",
					},
				],
				returnType: JMC_FUNCTION,
			},
		],
	},
];
