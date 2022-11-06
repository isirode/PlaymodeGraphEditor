# PlaymodeGraphEditor (beta version)

This project allow to construct and edit a node graph in a Unity game in playmode (at runtime).

![A calculator graph](/Documentation/Resources/CalculatorGraph.png)

I've added support (in progress) for the EditorWindow system as well.


![A calculator graph in an EditorWindow](/Documentation/Resources/EditorWindowCalculatorGraph.png)

## How to use

Clone the project and export the content as a package.

You can take a look at the examples to see how it works.

You can then customize the USS.

## Examples

There are two graph examples:
* A simple example, to interact with the graph
* A more complete example, the calculator example, which show how to:
  * Add nodes, remove nodes
  * Use the nodes (to do the computation of the graph)
  * Make custom color connections
  * Save & load the graph
  
There is also a EditorWindow example, that you can access via "PlaymodeGraphEditor/Open calculator example"

## Requirements

I am using Unity 2021.3.3f1.

## Known issues

The canvas is not infinite.

When we are zooming, in or out, the TextFields are not displayed any long, when we zoom out, there is a white area instead of the normal background color of the TextFields, this is a limitation of Unity I believe, since I did not setup this and I could not fix it.

If you create an EditorWindow, make sure that the backgrounds are transparent, for the canvases. Otherwise, you will not see the lines of the graph.
I opened a [thread](https://forum.unity.com/threads/draw-unity-gl-on-top-of-ui-in-editorwindow.1357673/) in Unity's forum for this.

I've encounter some issues while exporting the package, sometimes, Unity does not include necessary ASM definitions.

In an EditorWindow, the drag & drop of the line is a little bugged, it will show the line only after a few seconds, the updates are also only done once every few seconds. I am working at fixing these issue.

## Release Notes

### 0.0.2

- Added support for EditorWindow.

### 0.0.1

- Initial release of the library.

## Credits

I am using the draw connections method of [Node_Editor_Framework](https://github.com/Seneral/Node_Editor_Framework).

The pointer I am using, to display the forbidden area is created by [Freepik - Flaticon](https://www.flaticon.com/free-icons/ui) and can be found [here](https://www.flaticon.com/free-icon/pointer_7686046?term=mouse%20cursor%20forbidden&page=1&position=16&page=1&position=16&related_id=7686046&origin=style).
