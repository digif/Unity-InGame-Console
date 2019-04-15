# Unity-InGame-Console

Small script to generate an in-game console to show logs.

Drop the script on an empty object to add the console to the scene.

If the object is in a canvas, it will fit the rect transform of that object, otherwise it will create it's own canvas and fill up the whole screen.

You can change the text size, scrollbar width, margins size, and the shortcut key to show/hide the console within the parameters.

There's also a checkbox to show the console at the start of the scene.

The text is color codded, white for assert and logs, yellow for warnings, red for errors and exceptions.

Repeating logs appear as a green counter at the right of the repeating log.
