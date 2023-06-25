import MinecraftData from "minecraft-data";

const mcc = MinecraftData("1.19.4");

export const MC_BLOCKS = mcc.blocksArray.map((v) => v.name);
export const MC_ITEMS = mcc.itemsArray.map((v) => v.name);
