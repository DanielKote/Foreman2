# Foreman #

This is a simple program for generating flowcharts for production lines in the game [Factorio](https://www.factorio.com/).

Requires .Net 4.0 or higher and Visual C++ 2012 x86 to run.

For example, here's a (fairly messy) flowchart showing the optimal resources and assemblers required to make one rocket per minute:

![1 Rocket per minute.png](https://bitbucket.org/repo/ary6LR/images/1686665211-1%20Rocket%20per%20minute.png)

## Download ##

To download the latest version of Foreman please visit the "Releases" tab here on Github and download the "Release.zip" from the latest release.

## Usage ##

Run Foreman.exe. It should automatically find your Factorio installation if you used the installer. Otherwise it will ask for its location.

Once the main program is running, you can drag items onto the flowchart from the list on the left to create nodes in the flowchart. Nodes can be dragged around, deleted (by right clicking them), or, in the case of output nodes, edited by clicking the item icon. This will change the amount of that item that the node requires (and any nodes feeding into it should be updated automatically too).

Click and drag an item from the top or bottom of a node to create a new item link. Stop dragging on empty space to create a new node to consume or produce the item, or drag it onto a compatible node to link them.

By default, the program is set to display the requirements to create a fixed amount of each item. If you want to instead show items being created at a specific rate, you can change it in the top left. This also lets you see the minimum number of assemblers, miners or furnaces it would take to produce an item at that rate (as well as applying speed modules to reduce the total number).

If you get tired of connecting recipes together yourself, click "Automatically complete flowchart", and it will add in all the recipes you need to fulfil the requirements of the output nodes currently in the graph.

## Troubleshooting ##

**"Unable to load DLL 'lua52'."**

Make sure Visual C++ 2012 x86 is installed.

**Crash on load**

We've found most crashes to be related to mods implementing features in their Lua we didn't anticipate or changes to the Lua when a new version of Factorio is released. Feel free to open an issue in this respository so we can investigate and fix.

## Contributing ##

At the time of writing the only official "contributor" is myself, Rybadour here on Github and /u/salbris on Reddit. More are welcome! I don't have any official guidance such as a code style guide so feel free to suggest or create one! I doubt I'd turn away any pull requests unless they involve large rewrites of the core functionality that I would be hard-pressed to review. Generally I'd recommend looking at the issues here on Github to see if the feature you'd like to add is already listed and assigned. I'd recommend everyone to consider this repository the source of truth for the project as there are numerous copies on BitBucket. I'll be attempting to consolidate everything here.

If you would like to contribute but are having difficulty understanding the codebase feel free to reach out directly. I've also taken the liberty to create a Discord server for contributors. We could use that to discuss development and questions about the program. Discord: https://discord.gg/bYqVq2
