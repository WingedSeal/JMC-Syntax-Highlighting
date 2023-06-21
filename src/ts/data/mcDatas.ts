import mc_blocks from "./minecraft/blocks.json";
import mc_items from "./minecraft/items.json";

interface BlockData {
	name: string;
}

interface ItemData {
	name: string;
}

export const MC_BLOCKS: BlockData[] = mc_blocks.map((v) => {
	return {
		name: v.name,
	};
});
export const MC_ITEMS: ItemData[] = mc_items.map((v) => {
	return {
		name: v.name,
	};
});
