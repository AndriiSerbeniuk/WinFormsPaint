# WinFormsPaint
This is a MS Paint clone with layers as in Photoshop. 
It allows to save projects into a .dat file and load them from it, export open project into an image.

Implemented tools are:
	- brush: draws a line following the cursor when it's pressed. Width, color, and alpha are modifiable;
	- eraser: works the same way as the brush, but erases all in it's way instead of drawing. Width is modifiable4
	- shape tool: draws a shape between the point where mouse button was pressed and the point where the button was released. 
		Available shapes are:
			- line: width and color are modifiable;
			- square: width, outline color, and fill color are modifiable;
			- ellipse: width, outline color, and fill color are modifiable.
			Each shape is initially stored in its own layer as an object and its colors, width, and size can be modified for as long as the layer is not rasterized;
	- text tool: draws text in the place of the mouse click. The text, its color, size, font, and style are modifiable. 
		Text tool writes text in a new layer and is treated the same way shape tool does;
	- color pick tool: allows to pick a color for the current selected tool. 
		A variation for fill color exists that is used on rectangle and ellipse tools;
	- pipette tool: allows to pick a color for the current selected tool from the canvas.
		A variation for fill color exists that is used on rectangle and ellipse tools;
	- move tool: allows to move the selected layer on the canvas.

Layer controls allow to add and remove layers, as well as moving them in front of each other. 
Layer's size can be modified. 
Shape layers can't be drawn on until rasterised.



This project was an assignment on my second year of university. 
Use of raw WinForms for an image editor isn't really appropriate, but it demonstrates a practical use of polymorphism, so I went with it.
