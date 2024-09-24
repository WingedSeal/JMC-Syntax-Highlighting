using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Extension.Server
{
    public static class FunctionsContextManager
    {
        public static Dictionary<string, (string name,string usage)[]> contexts = new()
        {
            {"Player",new (string name,string usage)[]
            {
                ("join","Player.join(function: Function) -> LoadOnce"),
                ("rejoin", "Player.rejoin(function: Function) -> LoadOnce"),
                ("die", "Player.die(onDeath: Function = ()=>{}, onRespawn: Function = ()=>{}) -> LoadOnce"),
                ("firstJoin", "Player.firstJoin(function: Function) -> LoadOnce"),
                ("onEvent", "Player.onEvent(criteria: Criteria, function: Function) -> LoadOnly")
            }},
            {"Item",new (string name,string usage)[]
            {
                ("create", "Item.create(\r\n    itemId: Keyword,\r\n    itemType: Item,\r\n    displayName: FormattedString = \"\",\r\n    lore: List<FormattedString> = [],\r\n    nbt: JSObject = {},\r\n    onClick: Function = ()=>{}\r\n) -> LoadOnly"),
                ("createSign", "Item.createSign(\r\n    itemId: Keyword,\r\n    variant: Keyword,\r\n    displayName: FormattedString = \"\",\r\n    lore: List<FormattedString> = [],\r\n    texts: List<FormattedString> = [],\r\n    nbt: JSObject = {},\r\n    onClick: Function = ()=>{}\r\n) -> LoadOnly"),
                ("createSpawnEgg", "Item.createSpawnEgg(\r\n    itemId: Keyword,\r\n    mobType: Keyword,\r\n    displayName: FormattedString = \"\",\r\n    onPlace: Function = ()=>{},\r\n    lore: List<FormattedString> = [],\r\n    nbt: JSObject = {}\r\n) -> LoadOnly"),
                ("give", "Item.give(itemId: Keyword, selector: TargetSelector = @s, amount: integer = 1) -> JMCFunction"),
                ("clear", "Item.clear(itemId: Keyword, selector: TargetSelector = @s, amount: integer = -1) -> JMCFunction"),
                ("summon", "Item.summon(itemId: Keyword, pos: string, count: integer = 1, nbt: JSObject = {}) -> JMCFunction"),
                ("replaceBlock", "Item.replaceBlock(itemId: Keyword, pos: string, slot: string, count: integer = 1) -> JMCFunction"),
                ("replaceEntity", "Item.replaceEntity(itemId: Keyword, selector: TargetSelector, slot: string, count: integer = 1) -> JMCFunction"),
            }},
            {"Text",new (string name,string usage)[]
            {
                ("tellraw","Text.tellraw(selector: TargetSelector, message: FormattedString) -> JMCFunction"),
                ("title","Text.title(selector: TargetSelector, message: FormattedString) -> JMCFunction"),
                ("subtitle","Text.subtitle(selector: TargetSelector, message: FormattedString) -> JMCFunction"),
                ("actionbar","Text.actionbar(selector: TargetSelector, message: FormattedString) -> JMCFunction"),

            }},
            {"Math",new (string name,string usage)[]
            {
                ("sqrt","Math.sqrt(n: Scoreboard) -> VariableOperation"),
                ("random","Math.random(min: ScoreboardInteger = 1, max: ScoreboardInteger = 2147483647) -> VariableOperation"),
            }},
            {"Timer",new (string name,string usage)[]
            {
                ("add","Timer.add(\r\n    objective: Objective,\r\n    mode: Keyword,\r\n    selector: TargetSelector,\r\n    function: Function = ()=>{}\r\n) -> LoadOnly"),
                ("set","Timer.set(objective: Objective, selector: TargetSelector, tick: ScoreboardInteger) -> JMCFunction"),
                ("isOver","Timer.isOver(objective: Objective, selector: TargetSelector = @s) -> Boolean"),
            }},
            {"Recipe",new (string name,string usage)[]
            {
                ("table","Recipe.table(recipe: JSON, baseItem: Item = knowledge_book, onCraft: Function = ()=>{}) -> LoadOnly"),
            }},
            {"Hardcode",new (string name,string usage)[]
            {
                ("repeat","Hardcode.repeat(\r\n    indexString: string,\r\n    function: ArrowFunction,\r\n    start: integer,\r\n    stop: integer,\r\n    step: integer = 1\r\n) -> ExecuteExcluded"),
                ("repeatList","Hardcode.repeatList(\r\n    indexString: string,\r\n    function: ArrowFunction,\r\n    strings: List<string>\r\n) -> ExecuteExcluded"),
                ("repeatLists","Hardcode.repeatLists(\r\n    indexStrings: List<string>,\r\n    function: ArrowFunction,\r\n    stringLists: List<List<string>>\r\n) -> ExecuteExcluded"),
                ("switch","Hardcode.switch(\r\n    switch: Scoreboard,\r\n    indexString: string,\r\n    function: ArrowFunction,\r\n    count: integer,\r\n    begin_at: integer = 1\r\n) -> ExecuteExcluded"),
            }},
            {"Trigger",new (string name,string usage)[]
            {
                ("setup","Trigger.setup(objective: Keyword, triggers: JSObject<integer, Function>) -> LoadOnly"),
                ("add","Trigger.add(objective: Keyword, function: Function) -> LoadOnly"),
            }},
            {"Predicate",new (string name,string usage)[]
            {
                ("locations","Predicate.locations(\r\n    name: string,\r\n    predicate: JSON,\r\n    xMin: integer,\r\n    xMax: integer,\r\n    yMin: integer,\r\n    yMax: integer,\r\n    zMin: integer,\r\n    zMax: integer\r\n) -> LoadOnly"),
            }},
            {"RightClick",new (string name,string usage)[]
            {
                ("setup","RightClick.setup(idName: Keyword, functionMap: JSObject<integer, Function>) -> LoadOnly"),
            }},
            {"Particle",new (string name,string usage)[]
            {
                ("circle","Particle.circle(\r\n    particle: string,\r\n    radius: float,\r\n    spread: integer,\r\n    speed: integer = 1,\r\n    count: integer = 1,\r\n    mode: Keyword = normal\r\n) -> JMCFunction"),
                ("spiral","Particle.spiral(\r\n    particle: string,\r\n    radius: float,\r\n    height: float,\r\n    spread: integer,\r\n    speed: integer = 1,\r\n    count: integer = 1,\r\n    mode: Keyword = normal\r\n) -> JMCFunction"),
                ("helix","Particle.helix(\r\n    particle: string,\r\n    radius: float,\r\n    height: float,\r\n    spread: integer,\r\n    speed: integer = 1,\r\n    count: integer = 1,\r\n    mode: Keyword = normal\r\n) -> JMCFunction"),
                ("cylinder","Particle.cylinder(\r\n    particle: string,\r\n    radius: float,\r\n    height: float,\r\n    spreadXZ: integer,\r\n    spreadY: integer,\r\n    speed: integer = 1,\r\n    count: integer = 1,\r\n    mode: Keyword = normal\r\n) -> JMCFunction"),
                ("line","Particle.line(\r\n    particle: string,\r\n    distance: float,\r\n    spread: integer,\r\n    speed: integer = 1,\r\n    count: integer = 1,\r\n    mode: Keyword = normal\r\n) -> JMCFunction"),
                ("square","Particle.square(\r\n    particle: string,\r\n    length: float,\r\n    spread: integer,\r\n    align: Keyword = corner,\r\n    speed: integer = 1,\r\n    count: integer = 1,\r\n    mode: Keyword = normal\r\n) -> JMCFunction"),
                ("cube","Particle.square(\r\n    particle: string,\r\n    length: float,\r\n    spread: integer,\r\n    align: Keyword = corner,\r\n    speed: integer = 1,\r\n    count: integer = 1,\r\n    mode: Keyword = normal\r\n) -> JMCFunction"),
                ("sphere","Particle.sphere(\r\n    particle: string,\r\n    radius: float,\r\n    spread: integer,\r\n    speed: integer = 1,\r\n    count: integer = 1,\r\n    mode: Keyword = normal\r\n) -> JMCFunction"),
            }},
            {"Raycast",new (string name,string usage)[]
            {
                ("simple","Raycast.simple(\r\n    onHit: Function,\r\n    onStep: Function = ()=>{},\r\n    onBeforeStep: Function = ()=>{},\r\n    interval: float = 0.1,\r\n    maxIter: integer = 1000,\r\n    boxSize: float = 0.1,\r\n    target: TargetSelector = @e,\r\n    startAtEye: boolean = true,\r\n    stopAtEntity: boolean = true,\r\n    stopAtBlock: boolean = true,\r\n    runAtEnd: boolean = false,\r\n    casterTag: Keyword = __self__,\r\n    removeCasterTag: boolean = true,\r\n    modifyExecuteBeforeStep: string = \"\",\r\n    modifyExecuteAfterStep: string = \"\",\r\n    overideString: string = \"\",\r\n    overideRecursion: ArrowFunction = ()=>{}\r\n) -> ExecuteExcluded"),
            }},
            {"JMC",new (string name,string usage)[]
            {
                ("put","JMC.put(command: string) -> JMCFunction"),
                ("python","JMC.python(pythonCode: string, env: string = \"\") -> JMCFunction"),
            }},
            {"String",new (string name,string usage)[]
            {
                ("isEqual","String.isEqual(source: string, path: Keyword, string: string) -> Boolean"),
            }},
            {"Object",new (string name,string usage)[]
            {
                ("isEqual","Object.isEqual(source1: string, path1: Keyword, source2: string, path2: Keyword) -> Boolean"),
            }},
            {"GUI",new (string name,string usage)[]
            {
                ("template","GUI.template(name: Keyword, template: List<string>, mode: Keyword) -> LoadOnly"),
                ("register","GUI.register(\r\n    name: Keyword,\r\n    id: string,\r\n    item: Item,\r\n    displayName: FormattedString = \"\",\r\n    lore: List<FormattedString> = [],\r\n    nbt: JSObject = {},\r\n    onClick: Function = ()=>{},\r\n    onClickAsGUI: Function = ()=>{}\r\n) -> LoadOnly"),
                ("registers","GUI.registers(\r\n    name: Keyword,\r\n    id: string,\r\n    items: List<Keyword>,\r\n    variable: Scoreboard,\r\n    onClick: Function = ()=>{},\r\n    onClickAsGUI: Function = ()=>{}\r\n) -> LoadOnly"),
                ("create","GUI.create(name: Keyword) -> LoadOnly"),
                ("run","GUI.run(name: Keyword) -> JMCFunction"),
            }},
            {"Advancemet",new (string name,string usage)[]
            {
                ("grant","Advancement.grant(target: TargetSelector, type: Keyword, advancement: Keyword, namespace: Keyword = \"\") -> JMCFunction"),
                ("revoke","Advancement.revoke(target: TargetSelector, type: Keyword, advancement: Keyword, namespace: Keyword = \"\") -> JMCFunction"),
            }},
            {"Scoreboard",new (string name,string usage)[]
            {
                ("add","Scoreboard.add(objective: Keyword, criteria: Criteria = dummy, displayName: FormattedString = \"\") -> JMCFunction"),
            }},
            {"Bossbar",new (string name,string usage)[]
            {
                ("add","Bossbar.add(team: Keyword, displayName: FormattedString = \"\") -> JMCFunction"),
                ("setName","Bossbar.setName(team: Keyword, displayName: FormattedString = \"\") -> JMCFunction"),
            }},
            {"Team",new (string name,string usage)[]
            {
                ("add","Team.add(id: Keyword, name: FormattedString = \"\", properties: JSObject<Keyword, Keyword> = {}) -> LoadOnly"),
                ("prefix","Team.prefix(team: Keyword, prefix: FormattedString) -> JMCFunction"),
                ("suffix","Team.suffix(team: Keyword, suffix: FormattedString) -> JMCFunction"),
            }},
            {"Entity",new (string name,string usage)[]
            {
                ("launch","Entity.launch(power: float = 1) -> JMCFunction"),
            }},
            {"Array",new (string name,string usage)[]
            {
                ("forEach","Array.forEach(target: string, path: string, function: Function) -> JMCFunction"),
            }},
            {"TextProp",new (string name,string usage)[]
            {
                ("clickCommand",""),
                ("suggestCommand",""),
                ("clickURL",""),
                ("clickPage",""),
                ("clipboard",""),
                ("hoverText",""),
                ("hoverItem",""),
                ("hoverEntity",""),
                ("font",""),
                ("keybind",""),
                ("nbt",""),
            }},
            {"TextProps",new (string name,string usage)[]
            {
                ("clickCommand",""),
                ("suggestCommand",""),
                ("clickURL",""),
                ("clickPage",""),
                ("clipboard",""),
                ("hoverText",""),
                ("hoverItem",""),
                ("hoverEntity",""),
                ("font",""),
                ("keybind",""),
                ("nbt",""),
            }},
            {"Tag",new (string name,string usage)[]
            {
                ("update","Tag.update(\r\n    selector: TargetSelector,\r\n    tag: Keyword,\r\n    removeFrom: TargetSelector = @e\r\n) -> JMCFunction"),
            }},
        };
    }
}
