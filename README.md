# PlaymodeGraphEditor (beta version)

This project allow to construct and edit a node graph in a Unity game in playmode (at runtime).

![A calculator graph](/Documentation/Resources/CalculatorGraph.png)

## How to use

Clone the project and export the content as a package.

You can take a look at the examples to see how it works.

You can then customize the USS.

## Examples

There are two examples:
* A simple example, to interact with the graph
* A more complete example, the calculator example, which show how to:
  * Add nodes, remove nodes
  * Use the nodes (to do the computation of the graph)
  * Make custom color connections
  * Save & load the graph

## Requirements

I am using Unity 2021.3.3f1.

## Known issues

The canvas is not infinite.

When we are zooming, in or out, the TextFields are not displayed any long, when we zoom out, there is a white area instead of the normal background color of the TextFields, this is a limitation of Unity I believe, since I did not setup this and I could not fix it.

## Release Notes

### 0.0.1

- Initial release of the library.

## Credits

I am using the draw connections method of [Node_Editor_Framework](https://github.com/Seneral/Node_Editor_Framework).

The pointer I am using, to display the forbidden area is created by [Freepik - Flaticon](https://www.flaticon.com/free-icons/ui) and can be found [here](https://www.flaticon.com/free-icon/pointer_7686046?term=mouse%20cursor%20forbidden&page=1&position=16&page=1&position=16&related_id=7686046&origin=style).
