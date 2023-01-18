interface BuiltInFunction {
	class: string;
	methods: Array<string>;
}

export const BuiltInFunctions: Array<BuiltInFunction> = [
	{
		class: "Advancement",
		methods: ["grant", "revoke"],
	},
	{
		class: "Player",
		methods: ["onEvent", "firstJoin", "rejoin", "die"],
	},
	{
		class: "Item",
		methods: ["give", "summon", "replaceBlock", "replaceEntity"],
	},
	{
		class: "Text",
		methods: ["tellraw", "title", "subtitle", "actionbar"],
	},
	{
		class: "Math",
		methods: ["sqrt", "random"],
	},
	{
		class: "Timer",
		methods: ["add", "set", "isOver"],
	},
	{
		class: "Recipe",
		methods: ["table"],
	},
	{
		class: "Hardcode",
		methods: ["repeat", "repeatList", "switch"],
	},
	{
		class: "Trigger",
		methods: ["setup", "add"],
	},
	{
		class: "Predicate",
		methods: ["locations"],
	},
	{
		class: "RightClick",
		methods: ["setup"],
	},
	{
		class: "Particle",
		methods: ["circle", "spiral", "cylinder", "line"],
	},
	{
		class: "Raycast",
		methods: ["simple"],
	},
	{
		class: "JMC",
		methods: ["put"],
	},
	{
		class: "String",
		methods: ["isEqual"],
	},
	{
		class: "Object",
		methods: ["isEqual"],
	},
	{
		class: "GUI",
		methods: ["template", "register", "registers", "create", "run"],
	},
	{
		class: "Scoreboard",
		methods: ["add"],
	},
];
