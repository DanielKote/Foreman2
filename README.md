# Foreman #

This is a simple program for generating flowcharts for production lines in the game [Factorio](https://www.factorio.com/).

Requires .Net 4.0 or higher and Visual C++ 2012 to run.

Here's an example of the kind of thing you can make with it:

![Foreman Production Flowchart.png](https://bitbucket.org/repo/ary6LR/images/734263546-Foreman%20Production%20Flowchart.png)

## Usage ##

Run Foreman.exe. It should automatically find your Factorio installation if you used the installer. Otherwise it will ask for its location.

Once the main program is running, you can drag items onto the flowchart from the list on the left to create nodes in the flowchart. Nodes can be dragged around, deleted (by right clicking them), or, in the case of output nodes, edited by clicking the item icon. This will change the amount of that item that the node requires (and any nodes feeding into it should be updated automatically too).

Click and drag an item from the top or bottom of a node to create a new item link. Stop dragging on empty space to create a new node to consume or produce the item, or drag it onto a compatible node to link them.

By default, the program is set to display the requirements to create a fixed amount of each item. If you want to instead show items being created at a specific rate, you can change it in the top left. This also lets you see the minimum number of assemblers, miners or furnaces it would take to produce an item at that rate (as well as applying speed modules to reduce the total number).

If you get tired of connecting recipes together yourself, click "Automatically complete flowchart", and it will add in all the recipes you need to fulfil the requirements of the output nodes currently in the graph.

## Troubleshooting ##

**"Unable to load DLL 'lua52'."**

Make sure Visual C++ 2012 is installed.

**Automatically completing the graph sometimes chooses very inefficient recipes (e.g. choosing basic oil processing over advanced oil processing when no heavy or light oil is required).**

This is because Foreman uses a very naive method for completing the graph. It doesn't try to optimise its choices at all, instead choosing the first recipe it finds for an item. This means that charts with oil processing in them aren't very useful at the moment.

Sorry about that.

## Motivate me! ##

Entirely optional, of course.

Paypal account: factorioforeman@gmail.com  
Bitcoin address: 1HACXKCkfRYNVQbTRsnpcqhs1YVwAXccnB