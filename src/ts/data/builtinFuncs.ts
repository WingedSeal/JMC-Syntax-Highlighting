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
	doc?: string;
}

interface MethodInfo {
	name: string;
	args: ArgInfo[];
	returnType: string;
	doc?: string;
}

interface BuiltInFunction {
	class: string;
	methods: MethodInfo[];
}

export function methodInfoToDoc(info: MethodInfo): string {
	const argString = info.args.flatMap((v): string => {
		const def = v.default !== undefined ? ` = ${v.default}` : "";
		return `${v.name}: ${v.type}${def}`;
	});
	return `${info.name}(${argString}): ${info.returnType} `;
}

export const BuiltInFunctions: BuiltInFunction[] = [
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
				doc: `Grant advancement, an alternative to vanilla syntax\nAvailable type are "everything", "from", "only", "through", "until"`,
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
				doc: `Revoke advancement, an alternative to vanilla syntax\nAvailable type are "everything", "from", "only", "through", "until"`,
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
				doc: `Run commands on positive change of scoreboard and reset the score.`,
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
				doc: `Run commands as player and at player when joining the World for the first time.\nRevoking all advancements will cause this to be called again.`,
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
				doc: `Run commands as player and at player when a player leave a world then join back.\nWill not run when player join the world for the first time.`,
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
				doc: `Run onDeath as player and at player's last position when the player die\nRun onRespawn as player and at player spawn location when the player respawn`,
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
						doc: "'itemId' is the unique name of this item so that it can be referenced in other Item function.",
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
				doc: `Create a custom item and save it for further use.\nonClick can only be used with "carrot_on_a_stick" or "warped_fungus_on_a_stick" itemType.`,
			},
			{
				name: "createSign",
				args: [
					{
						name: "itemId",
						type: KEYWORD,
						doc: "'itemId' is the unique name of this item so that it can be referenced in other Item function.",
					},
					{
						name: "variant",
						type: ITEM,
						doc: "variant is wood variant of the sign such as oak, spruce, etc.",
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
				doc: "Create a custom sign and save it for further use.",
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
				doc: `Create a custom spawn egg and save it for further use.\nThe spawn egg will not summon any mob unless specified in onPlace`,
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
				doc: "Give item created from Item.create to a player",
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
				doc: `Spawn item entity from Item.create.`,
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
				doc: `Use /item replace block with item from Item.create.`,
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
				doc: `Use /item replace entity with item from Item.create.`,
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
				doc: "Use formatted text on tellraw",
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
				doc: "Use formatted text on title",
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
				doc: "Use formatted text on subtitle",
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
				doc: `Use formatted text on actionbar`,
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
				doc: `Use Newton-Raphson method to perfectly calculate square root of any integer. And, like normal minecraft operators, this function will floor(round down) the result.`,
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
				doc: "Simplify integer randomization process using Linear congruential generator.",
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
				doc: `Create a scoreboard timer with 3 run mode\n- runOnce: run the commands once after the timer is over.\n- runTick: run the commands every tick if timer is over.\n- none: do not run any command.\nSelector is the entities that the game will search for when ticking down the timer. Avoid using expensive selector like @e.`,
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
				doc: "Set entity's score to start the timer.",
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
				doc: "Whether the timer of the selector is over or not.",
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
				doc: "Create a custom recipe for Crafting Table allowing NBT in result item and running function on craft",
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
				doc: `Some features in minecraft datapack require hard coding, this function will be a tool to help you. JMC will replace text that's the same as indexString with the number.\nstart is inclusive, stop is exclusive.\n\nThis do not work on Switch Case statement. Use Hardcode.switch() instead.\n\nTo do more complex task, you can use Hardcode.calc(<expression>) ANYWHERE in the function. JMC will replace the entire section with the result after replacing indexString\n\n+(add), -(subtract), *(multiply), /(divide by), **(power) are allowed in the expression. An example of expression with indexString="index" is				`,
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
				doc: "Does the same thing as Hardcode.repeat but use list to loop through instead of numbers",
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
				doc: `Similar to Hardcode.repeat() but for switch statement.`,
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
				doc: `Setup a trigger system for custom command or allowing players with no permission to click a text button. User can use the function with /trigger <objective> set <id>\n\nDo not define/create objective with the same name as objective`,
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
				doc: `Add a trigger command. (Shortcut for Trigger.setup())\n\nDo not define/create objective with the same name as objective`,
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
				doc: "Automation for making massive location check.",
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
				doc: `Setup basic carrot_on_a_stick right click detection with selected item detection. You can map any id to a series of commands. When any player right click with the item, the command matching the id will be run. While ID 0 being default which will be run if player right click with *any* Carrot on a stick that doesn't have an ID. You are allowed to setup multiple times with different id_name but that isn't recommended due to optimization issue. An example of idName is my_id for nbt {my_id:2}`,
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
				doc: "Make circle shaped particles. The higher the spread number, the less distance between particle becomes.",
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
				doc: "Make spiral shaped particles. The higher the spread number, the less distance between particle becomes.",
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
				doc: "Make cylinder shaped particles. The higher the spread number, the less distance between particle becomes.",
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
				doc: "Make line shaped particles. The higher the spread number, the less distance between particle becomes.",
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
						doc: "'interval' is distance between checks.",
					},
					{
						name: "maxIter",
						type: INTEGER,
						default: "1000",
						doc: "'maxIter' is maximum number of iteration.",
					},
					{
						name: "boxSize",
						type: FLOAT,
						default: "0.1",
						doc: "'boxSize' is the size of entity detection cube.",
					},
					{
						name: "target",
						type: TARGET_SELECTOR,
						default: "@e",
						doc: "'target' is acceptable target for collution.",
					},
					{
						name: "startAtEye",
						type: BOOLEAN,
						default: "true",
						doc: "'startAtEye' is wheter to start at the entity's eyes. If set to false it'll use the current position of the command.",
					},
					{
						name: "stopAtEntity",
						type: BOOLEAN,
						default: "true",
						doc: "'stopAtEntity' is wheter to stop the raycast when colliding with the entity.",
					},
					{
						name: "stopAtBlock",
						type: BOOLEAN,
						default: "true",
						doc: "'stopAtBlock' is wheter to stop the raycast when colliding with a block.",
					},
					{
						name: "runAtEnd",
						type: BOOLEAN,
						default: "true",
						doc: "'runAtEnd' is wheter to run onHit function even if doesn't collide with entity. (It'll run as the caster in this case.)",
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
						doc: `'modifyExecuteBeforeStep' is part of execute command that come before positioning forward. Example: "rotated ~ ~5"`,
					},
					{
						name: "modifyExecuteAfterStep",
						type: STRING,
						default: '""',
						doc: "'modifyExecuteAfterStep' is part of execute command that come after positioning forward.",
					},
					{
						name: "overrideString",
						type: STRING,
						default: '""',
						doc: "'overrideString' is string that'll be replaced with the recursion function's name (In vanilla syntax). Must be used with overideRecursion. Do not use unless it's necessary",
					},
					{
						name: "overrideRecursion",
						type: ARROW_FUNCTION,
						default: "()=>{}",
						doc: "'overrideRecursion' is function that'll overide the recursion line entirely. Must be used with overideString. Do not use unless it's necessary",
					},
				],
				returnType: EXECUTE_EXCLUDED,
				doc: "Cast simple raycast",
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
				doc: "Ignore any parsing and output the command directly. Mainly used for bypass compiler failures.",
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
				doc: "Whether the value inside NBT path is equal to the string.",
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
				doc: "Whether the value inside NBT path is equal to the value inside another NBT path.",
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
				doc: `Create template for GUI that can be configured with GUI.register and GUI.registers then create the GUI with GUI.create and used with GUI.run. GUI module doesn't work on Player's inventory.\n\nAvailable modes are entity, block and enderchest\n\nExample of template is\n
[
	"---------",
	"---A-B---",
	"---------"
]`,
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
				doc: "Map an item to an id(chatacter) in template",
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
				doc: "Map multiple items created from Item.create to an id(chatacter) in template depending on variable",
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
				doc: "Create a GUI that's been configured from GUI.template, GUI.register and GUI.registers",
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
				doc: "Run a GUI, on entity/block. This function must be run every tick, as the entity at the entity/at the block containing that GUI.",
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
				doc: "Add scoreboard objective, an alternative to vanilla syntax",
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
				doc: "Add bossbar, an alternative to vanilla syntax",
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
				doc: `Add team, an alternative to vanilla syntax

				Example of properties is {nametagVisibility: never}`,
			},
		],
	},
];
