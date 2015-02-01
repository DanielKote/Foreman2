# Foreman #

This is a simple program for generating flowcharts for production lines in the game [Factorio](https://www.factorio.com/).

Requires .Net 4.0 or higher and Visual C++ 2012 to run.

# Usage #

Run Foreman.exe. It should automatically find your Factorio installation if you used the installer. Otherwise it will ask for its location.

Once the main program is running, you can drag items onto the flowchart from the list on the left to create nodes in the flowchart. Nodes can be dragged around, deleted (by right clicking them), or, in the case of output nodes, edited by clicking the item icon. This will change the amount of that item that the node requires (and any nodes feeding into it should be updated automatically too).

Click and drag an item from the top or bottom of a node to create a new item link. Stop dragging on empty space to create a new node to consume or produce the item, or drag it onto a compatible node to link them.

By default, the program is set to display the requirements to create a fixed amount of each item. If you want to instead show items being created at a specific rate, you can change it in the top left. This also lets you see the minimum number of assemblers, miners or furnaces it would take to produce an item at that rate (as well as applying speed modules to reduce the total number).

# Troubleshooting #

**"Unable to load DLL 'lua52'."**

Make sure Visual C++ 2012 is installed.