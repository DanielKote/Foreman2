# Foreman 2.0 #
![1: Foreman 2.0](https://puu.sh/Im6D4/5a42f137e2.jpg)

This is a relatively simple program for generating flowcharts for production lines in the game [Factorio](https://www.factorio.com/).

Requires .Net 4.8 or higher and Visual C++ 2019 x86 to run. I am not sure about earlier versions, sorry.

For example, here's a flowchart showing the optimal resources and assemblers required to make the first base red science in the Pyanodon mod pack (rather comparable to base Factorio rocket I would say):

![2: Base red science for Pyanodons](https://puu.sh/Im6qB/83d13bab31.png)

## Download ##

To download the latest version of Foreman 2.0 please visit the "Releases" tab here on Github and download the "Release.zip" from the latest release.

The vanilla preset is included in the release, with a couple presets (from common modpacks) available in the "Presets.zip". You can always import your own preset using your customized modpack via the foreman app (see below for "presets" heading).

## Usage ##

Run Foreman.exe. It will already have the default Factorio 1.1 preset loaded so you can start graphing right away. Click on 'add item' or 'add recipe' button to begin.

Once you have your first node, you can drag from the ingredients/products of the node to add more nodes, or just click on add item/add recipe to add a disconnected node.

If you are dragging from the ingredients/products of the node and let go, you will have an option to choose which recipe you wish to use for the new node, or if you wish to create an input/passthrough/output node. If however you are holding Ctrl when you let go you will automatically create a passthrough node without any options.

If you are dragging from the ingredients/products of a selected passthrough node while holding down Ctrl and have multiple passthrough nodes selected (and only passthrough nodes), you will automatically place down a set of new passthrough nodes connected to the old ones. This should enable for quickly laying down a bus for larger graphs.

Movement around the graph can be done by dragging with the middle mouse button, or by dragging with the right mouse button (assuming you werent pressing down on a node when you started). Dragging with the middle mouse button is recommended, and is possible even while doing other operations such as selecting / moving modes, or dragging a new connection.

Dragging with the left mouse button (from an empty location) will enable you to select a group of nodes, and doing this while holding down the Ctrl key will 'add' to the currently selected nodes, while doing the selection while holding down the Alt key will 'subtract' from the currently selected nodes.

A node (or a selection of nodes) can be moved around the graph by dragging them with the left mouse button, or by using the arrow keys. Holding shift while dragging with the mouse will limit movement to the horizontal or vertical axis, while holding shift while moving with the arrow keys will move by a major grid increment rather than the minor grid increment.

Once a group of nodes has been selected you can also Ctrl+C or Ctrl+X the group (copy/cut), and Ctrl-V afterwards to paste the nodes wherever you wish. Keep in mind that pasting nodes will deselect the currently selected nodes and select the newly pasted nodes instead.

In most cases there is a helpful tool tip available at the top left of the screen to give guidance. Dont quite rely on it though - still working on it.

### Menu ###

![3: Main Menu](https://puu.sh/Im6AT/d126c4de38.jpg)

Add new item/recipe buttons will allow you to make the first nodes, following which you can drag from the ingredients/products to add further nodes.

You can activate grid lines to snap nodes into position, and set the given graph time scale in the options right above the graph. Currently the graph summary is still in development, so you can ignore that.

In most cases you would wish to keep the graph in auto-update mode which will re-calculate the flows every time you make a change (such as adding a new recipe, linking two nodes, deleting a node, etc). However for extremely large graph chains (such as planning out the entire production chain for science in B&A mods) it would be a good idea to activate the "pause all calculations" option (under graph options) which will stop any graph flow updates. Once your entire graph is finalized, you can deactivate that option and it will calculate the flows again (which for large graphs can take over 1 second).

Graphs are loaded in and saved through the save/load buttons, though keep in mind that no auto-save is present. It is recommended to save often, as this is currently a DEV version and is (not as much anymore but still) prone to crashes. For larger graphs it is recommended to design them in parts, save each separately, then import them into the final large graph.

Clear graph will clear the current graph completely, though it WILL NOT touch your save. You will need to save/overwrite your save for that to happen.

### Item / Recipe Selection ###

![4: Item & Recipe Selection Window](https://puu.sh/Im8Lm/21fca42176.jpg)

The item and recipe selection have been modeled after the Factorio window, so should be intuitive to navigate. A few things of note:

(1) There is an additional extraction/power group that has been added by foreman. It is there to group together any 'recipe' for mining ores, pumping oil (or other liquid, including water), generating heat (for example from nuclear reactors), and producing electricity (ex: steam generators). So if you are looking for any of those, they will be in that group.

(2) The filter will search through both the dev-name of the item/recipe, as well as the translated name. If you wish to search for the recipe only, there is a checkbox for that. Otherwise it will search for a possible match in the recipe name as well as a possible match in the ingredients/products.

(3) It is recommended to leave 'Ignore assembler' and 'Show disabled' turned off. Ignore assembler will no longer check if the item/recipe has an enabled assembler for its production, while show disabled will include recipes that have been disabled. If these options are turned on, then the recipes without an enabled assembler will have a dark yellow background, while disabled recipes will have a dark red background. If the DEV option to show unavailable items/recipes is turned on (in settings), then any unavailable item/recipe will have a light purple background.

(4) If the recipe selection is based off a pre-selected item (such as after the 'add item' button, or after a drag operation on a node's input/output), then there may be several other filter options: Ingredient, Product, and Fuel. They do exactly as you would expect - filtering the possible recipes based on the item's use as an ingredient, as a product, or as fuel. So if you are planning your coal production and dont want to see all the different recipes that can use coal as fuel, then you can turn off that option.

(5) The lower buttons can be used to add a source / passthrough / output node.


### Nodes ###

![5: Node examples](https://puu.sh/Im8AG/5924f95fa4.jpg)

Nodes come in 4 varieties; source nodes that act as inputs, sink nodes that act as outputs, passthrough nodes that can be used as limiters or just to tidy up the graph, and recipe nodes that actually do stuff. The first 3 can have a specific flow set that would specify the amount of items coming in/out/through the node, while the last (recipe node) can specify the number of buildings (among many other options) that will be utilized. Any of the 4 can be set to automatic (and in fact are thus set when first placed), meaning that their flow/building count is calculated based on the optimized flow of the graph. Those nodes with set flow/building count will have a darker background, and should thus be easy to visually identify.

The item input/output boxes are usually drawn with a grey border, but appear as red if they are not connected to anything, or golden if they are receiving too much input You can drag from them to quickly establish a new linked node, or right click for options (delete all links).

The nodes themselves are usually colored in light green with a dark green border around them.

(1) If the assigned flow or building count can not be achieved (due to insufficient incoming ingredients), then the border will be colored red.

(2) If there is too many ingredients coming in (and thus will be 'stockpiled' at the node), then the border will be colored golden. 

(3) If the node uses an unobtainable or disabled recipe or building, then there will be an orange flag on the top left of the node, with a warning sign on the top left.

(4) If the node has errors (such as a recipe / item / building from another mod, assembler / fuel / module assigned that cant be used, or anything else of similar severity), then the background will be fully colored in orange with a warning sign on the top left.

For passthrough nodes, they can be set to be simply drawn, meaning they will appear as a line with two circles you can drag connections from. When you export the graph to an image the circles will not be visible, leaving the passthrough node virtually unrecognizable from a simple connection. This should allow for cleaner graphs. You can also set it to be fully drawn, which will draw the full node with item input/output boxes and flow values.

Hovering over the warning sign will list the issues, clicking on the warning sign will auto-resolve issues (WARNING: in case of errors this will quite often lead to the deletion of the node!), while right clicking on the warning sign will give a menu of possible solutions. You can of-course resolve the issue yourself, or just ignore it if you know what you are doing.

Right clicking on the node itself will give you several options, including deleting the node, copying its properties (so you can later paste it to a node / selection), and applying default assembler/ modules. If you already copied a node's properties you can also paste them to the given node/selection while specifying what exactly you wish copied (assembler, modules, fuel, beacon). You can also set the 'simple draw' options for passthrough nodes.

Left clicking on the node will lead you to the flow or recipe editor.

Left clicking on a passthrough node will also allow you to set its 'simple draw' option.

### Recipe Node Options ###

![6: Recipe node editor](https://puu.sh/Im7OQ/f3b4573b74.jpg)

Most options here are rather self-explanatory and I have taken the initial design of the recipe node editor from the HelMod Factorio mod, so any users of that will feel right at home.

You can click on any of the building (assemblers) to select which one you wish to use. If the building supports modules you can click on the module in the module option to add it to the selected modules, or click on one of the already added modules to remove it. If the building can be supported by beacons, you can select the beacon and beacon modules in much the same way.

If the selected building burns fuel (liquid or solid), the options will be available right under the building selection.

If any of the options are red, that means that they have an issue, and it is recommended not to use them. In most cases it represents that the selected building / module is not available in regular play. Note: They will still work, and in the example above you have the 'hand crafting' as an assembler option which will still work (though it is red - meaning not buildable in regular play... you have to invite friends over to use them as an assembler, which is an out-of-game action).

To set the actual number of buildings you wish to use, switch the # of assemblers from auto to fixed and set the value. The graph will then calculate all the flows knowing that there are exactly that many assemblers in that node. Keep in mind that if there arent enough ingredients being passed to the node then it will show a lower value!

Specifically for reactors you can set the number of neighbors so as to properly apply neighbor bonuses (ex: for nuclear reactors).

For the beacon, there are several values to be set:

(1) # of beacons: this specifies the average number of beacons that will affect the building, and is the value that will be used to calculate the bonuses applied to the building.

(2) / Assembler: this specifies the number of beacons you will place per placed assembler. So for example if you are building a linear setup with beacon-assembler-beacon, you would have 2 beacons placed per assembler, so you would put in 2. This is used for the 'total beacons' and the power usage calculations, and will not impact the number of buildings/assemblers necessary.

(3) Additional: this specifies the number of additional beacons you will need. So in the example above, if you need to place 2 more beacons above your rows of beacons-assembler and 2 more below your rows (in order to have 8 beacons active on each assembler), you would put in 4.

Think of the values as 'total beacons' = 'per assembler' x '# of assemblers' + 'additional'.

## Settings ##

Settings have mostly been moved to the settings form, which has been clearly broken into 3 sections:

### Presets ###

![7: Presets](https://puu.sh/Im6B4/0a6aef4421.jpg)

All the currently saved presets (in the Preset folder) are listed here. You can check their mods & difficulty options in the list on the right by clicking on the preset you wish to see. To import a new preset you must prepare your Factorio game to the settings you wish - such that if you created a brand new game with the default options it will be the kind of preset you wish to see. Once that is done, exit out of Factorio, click 'import new preset from Factorio' browse to find the Factorio location (its the main install folder with 'bin' and 'data' folders - if using the steam version it should auto-locate for you), choose the difficulties you wish to use, give the preset a name, and click import. If you are using advanced options (such as --mod-directory) that change the mod folder location from the default, you can manually search for the mod folder. Otherwise it is best to leave the 'Mod Folder Location' blank and the importer will auto-locate your mods for you.

If you have more than 1 preset currently in your list, you can compare 2 presets to see any differences between them. Rather helpful to find what changes the newly updated mods have brought that might impact your game.

### Enabled Objects ###

![8: Enabled Objects](https://puu.sh/Im6Bo/6d0473b0e1.jpg)

This is where you can set which buildings / recipes are to be enabled for your graph. Rather handy if you wish to plan for a specific science tier. Each building/recipe can be set manually by searching for it and enabling/disabling it, by loading a save file, or by selecting which science packs you wish to have available (any technology with the required science packs will be researched).

You can also allow unavailable items (and enable their use), though this is not recommended. Unavailable items are those that are uncraftable during regular play, such as the infinite pipes, cheat beacons (from bobs), and other such objects. It is highly unlikely that you will require them, so keep them off.

Recipes can also be enabled/disabled straight from the recipe selection window by right clicking on them, though keep in mind that they will disappear from the visible list if 'show disabled' is not checked.

### Graph Options ###

![9: Graph Options](https://puu.sh/In2aO/c462e226a0.jpg)

**Level of detail:** specifies how much detail you wish shown on the nodes. Low will just show the recipe name, Medium will show the assembler + beacon + modules + number of buildings, while High will add building percentages (productivity, speed, power)

**Maximum number of graphical objects:** when more than this number of nodes is visible on the screen, the graphics shift to a simple view where you will no longer see any node information or item icons. The same thing happens if you zoom out too far. If your computer can handle it, crank it up! Keep in mind that the default (300) should be more than enough for most users. On the other hand if you have a meh computer decreasing this value to 200 or 150 may help performance (visual only).

**Draw arrows to show direction on link lines**: if enabled each throughput line will have a direction arrow at the end showing the direction of flow. If dynamic link width is used (or the option is disabled), the item tabs will have a light arrow drawn inside them instead.

**Dynamic link width:** if enabled the width of the item flow between nodes will be proportional to the amounts being moved around (so expect really beefy lines from your miners to the smelters, and really thin ones from your high end electronics). Fluids and items are considered separately.

**Abbreviate science packs:** If the mod pack you are graphing for has too many science packs (ex: space exploration), using this option is recommended. It will hide any science packs from the 'required science packs' of any recipe that is required to craft a higher tier science pack required by said recipe. So for example a red+green science will be abbreviated to just green since red science is necessary to research green science.

**Show recipe tool-tip:** If turned on will show the recipe of a given node when you hover over it.

**Round building count:** If turned on will round up buildings to the nearest integer (so instead of 0.2 buildings you will see 1 building). This is visual only and doesnt impact the power consumption of the buildings!

**Lock recipe editor to top left corner:** If turned on the recipe editor panel will always show up in the top left corner, otherwise it will show up next to the node being edited.

**Flag over or under supplied nodes** If enabled, any over or under supplied nodes will have not just the border set to the appropriate color (red or gold), but also a flag will be visible in the top left corner similar to warning/error flags. Turn this on for better visibility of over/under supplied nodes.

**Guide Arrows** Useful to find any error nodes (recipe from another mod save for example), or warning nodes (disabled assemblers, uncraftable fuel, etc). Can also be used for missing link nodes (nodes where some of the inputs/outputs are not connected to anything), or over/under supplied nodes. If any exist outside the currently viewed area (where they are rather obvious, having an orange flag/background to them), there will be an arrow pointing in their direction.

**Defaults (assemblers & modules):** Should be straight forward. You can set which type of assembler you wish to be automatically assigned to newly added nodes, as well as what type of modules to give it.

**Defaults (node direction)** Placed nodes can either have inputs at the bottom and outputs at the top (Up direction), or inputs at the top and outputs at the bottom (Down direction). The set default will be applied to newly placed nodes.

**Defaults (smart direction)** If enabled, the direction of a new node placed by dragging from an item tab (which will be linked to said item tab) will be set automatically - if the new node is below the node you dragged from, then it will be pointing down, and if it is placed above the node you dragged from then it will be pointing up.

**Defaults (Simple draw passthrough nodes)** Passthrough nodes have 2 drawing options - regular and simple. Regular draw will draw the passthrough node as all other nodes - with a rounded border, input & output item boxes, and flow values. Simple draw will draw the passthrough node as a single line connecting the inputs to the outputs with no item boxes - virtually indistinguishable from a regular link line. This could be helpful in organizing the graph without unduly splitting the viewer's attention away from the nodes that actually do something (as opposed to directing flow). Passthrough nodes with set flow (instead of automatic), or passthrough nodes that are over/under supplied will be fully drawn no matter what.

**Advanced:** probably best to leave it alone (turned off) unless there is a particular need for it.

**Advanced (Enable extra productivity bonus for all entities):** to allow for miner productivity, there is an 'extra productivity' value you can set within your mining nodes. If this is turned on then all nodes (and not just the miners) will have an extra productivity that you can set. This should be used cautiously as you can accidentally copy the extra productivity to all nodes, but it is left as an option for those mods that allow mining productivity to act on non-miners (usually by creating 'invisible' beacons that apply the productivity effect)

**Advanced (Show unavailable items):** if turned on will display those items that cant be acquired in regular play (ex: infinite pipes, coins).

**Advanced (Load barreling or crating recipes):** if turned on will load the barreling / crating recipes from the preset (instead of just ignoring them). In most cases you really dont want them, so its best to just keep it off. NOTE: if you are planning on crating and wish to find the flows, please - by all means turn it on.

## Exporting the graph ##

![10: Export](https://puu.sh/Im91L/703d54d784.jpg)

Graph export remains unchanged at this moment from the original Foreman. Click on the 'Export image' button to bring up the export form, browse to select where you wish to save the resulting png file, set the scale (1x should be fine) for the image, check the transparent background if you wish to, and hit export to save the image. Dont worry - the grid will not be exported.

Additionally, be careful when exporting large graphs - this is a raster format instead of a vector one, so large graphs can quickly spiral out of control. It is highly recommended to plan out the factory in smaller sub-factories with a saved graph for each (that you can export as an image), and if you wish to plan the ENTIRE factory, then do so by importing the smaller graphs into the main graph, connecting all the nodes, doing your planning, and NOT exporting the final image (instead just saving the graph and navigating it within Foreman).

## Major changes from Foreman 1.0 ##

(1) removal of LUA code dependency (import of data now done through an automatic process that copies in the export mod, runs Factorio in the background to generate preset, and finally deletes the export mod).

(2) Item graphics have been updated to show what the item looks like in Factorio, not a rough approximation.

(3) Filtering of items/available objects should be much better.

(4) Selection of enabled/disabled objects has been streamlined - initial preset will have all objects available in the standard playthrough enabled; with the option to set the enabled status manually for each object, set the enabled status based off of an existing save file, or set the enabled status based off of the science packs you wish to be available.

(5) Burners have been handled properly (so furnaces work just fine now). In addition objects such as steam engines, nuclear reactors, and heat exchangers work, so if you wish to plan out your nuclear power plant, you can do that.

(6) Item / recipe selection panel has been designed to look and feel like the Factorio one - no need to scroll through hundreds of recipe options.

(7) Graphics have been redesigned to better handle large graphs - tested with 1000+ node graphs. Still recommended to pause updates while editing those graphs. Linear algebra can struggle with huge variable counts. But the graphics will struggle no longer!

(8) Better handling of wrong/missing recipes/items. Save files now store information about what recipes/items are used, and are loaded properly even into wrong preset (though they will be labeled as 'missing', and might not calculate correctly). This should allow for import of saves between preset versions with minimal trouble.

(9) Quality of life changes, including dragging around groups of nodes, copy/pasting nodes, copying node options between each other, and importing graphs from other saves into the current graph.

## Troubleshooting ##

(1) Make sure Visual C++ 2019 x86 is installed.

(2) Add an issue? There is likely to be quite a few bugs at the moment...

## Contributing ##

At the time of writing the only official "contributor" is myself, DanielKotes. This started out as a slight fork of the [original foreman](https://github.com/Rybadour/Foreman), with just a few changes that I didnt bother using git for. It kind of spiraled out of control to the point where it is no longer something that can be considered the original Foreman, thus the new repository.

I have mostly finished with active development and will mostly be releasing updates pertaining to keeping the software functional / fixing up any major bugs. You are free to make a fork of this project and make any changes you want; I will try to check up on any posted merge requests when I have time.