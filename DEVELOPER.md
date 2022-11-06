# Developer

If you want to test it, you can clone the project and take a look at the examples.

## Tests

There are no tests for now.

## How to participate

Open a PR explaining your changes and why it should be merged.

## WIP

Features I am currently working on:
- [ ] Nothing actually

## TODO

### Features

The features are split by responsability.

- [ ] Node
  - [x] Allow to drag a node
  - [x] Notify if one of his values has changed
  - [x] Notify when it is dragged
  - [ ] Instantiating by C# code
  - [ ] Indicate that a node is selected
  - [ ] Limit the boilerplate by providing a way to access values directly
  - [ ] Add tooltips
  
- [x] Connections
  - [x] Add a connection
  - [x] Remove a connection
  - [x] Provide an example of how to limit the connections to a node
  - [x] Provide a way to customize the connection's color
  
- [ ] Graph
  - [x] Provide an example of how to save it
  - [x] Provide an example of how to load it
  - [x] Add nodes
  - [x] Remove nodes
  - [ ] Duplicate a node
  - [ ] Select multiple nodes
  - [ ] Drag multiple nodes
  - [ ] Add a legend system
  - [ ] Support undo / redo
  
- [ ] Subgraph

- [ ] Recursivity
  - [ ] Detect recursivity
  - [ ] Block recursivity
  - [ ] Allow recursivity (with a control node)

- [ ] Editor mode
  - [x] Allow to use the graph in Editor mode
    - [ ] Fix the current issues (lines on top of UI)	
  - [ ] Provide an example of how to use a saved (in Editor mode) graph in Playmode 

### Design

- [ ] Is ValueType what I want, notably for the output knobs ?
  - It is not extensible
  - The logic checking the values is done with knowledge of the nodes supported types : it does not bring type checking between the connections

### Code quality

- [ ] Split the BaseNode file and add regions
- [ ] Find a way to handle assets correctly
  - If we ignore the other assets, they will not be included on clone
  - If we include the other assets, the git versionning become bad and duplicated

### Publishing

- [ ] Find an automation to publish the package
- [ ] FIXME : when exporting the package from the 'Isirode' main folder, the ASM definition of Seneral NodeEditorFramework is not included
	Which cause a lot of problems
	They are included in other ASM definitions though

### Tests

- [ ] Add tests

## Publishing

You can create a Unity package as usual.
