Trying to learn 
-tool radius compensation
-tool path planning
-tool path generation to fill closed contour


My goal is to be able to generate a toolpath similar to the following image

![ToolPath in green](/Ressources/Capture%20d'écran%202024-04-26%20165647.png)


currently I am able to generate contour tracking taking into account the radius of the tool, and also detect areas impossible to reach

![Basic contour Tracking](/Ressources/Capture%20d'écran%202024-04-26%20164833.png)
![Basic contour Tracking with collision detection](/Ressources/Capture%20d'écran%202024-04-26%20165204.png)

useful links:
https://github.com/ejbosia/Fermat-Spirals/blob/main/Connected%20Fermat%20Spirals.pdf
[stackOverflow](https://stackoverflow.com/questions/1109536/an-algorithm-for-inflating-deflating-offsetting-buffering-polygons)
https://github.com/AngusJohnson/Clipper2



a big thank you to @SebLague and its great tutorial on ShapeEditor
