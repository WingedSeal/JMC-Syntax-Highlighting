import itemsData from "../minecraft-data/items.json";
import blocksData from "../minecraft-data/blocks.json";

export const ITEMS_ID = itemsData.map((v) => v.name);
export const BLOCKS_ID = blocksData.map((v) => v.name);

export const SELECTORS: string[] = ["@p", "@a", "@r", "@s", "@e"];
export const ATTRIBUTE_LIST: string[] = [
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
export const BOOLEANS = ["true", "false"];
