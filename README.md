# Scene Management Extensions

This is the Unity package I use to manage scenes in my own projects. I work on a lot of small games so I always try to create this type of reusable, scalable and ready-to-use libraries that allow me to just drop it into any project and have it ready in no time. The package includes:
- A useful editor window that lists all the scenes in the Assets folder with buttons to easily swap between them.
- Scriptable objects that hold a reference to a specific scene, with functions to load it from UI Buttons, Events or from your own code. It allows you to load scenes and maintain references to them even if you change their name or index in the build settings.
- Fading between scenes and an easy way to create new scene transition effects.
- An easy way to create loading screens.

# How to

## Use the Scene List Window
Open the window by going to `Window` → `Scene List`. Here you will see a list of all the scenes in your project grouped by folders. You can fold each group to focus on the ones you are currently working on. Click the folder icon to highlight the folder on the project window.

The currently loaded scenes are highlighted in yellow. You can easily swap scenes, add/remove them, or hit play directly to one of them.


## Create and use Scene References

``Scene References`` are a scriptable object that holds a reference to a specific scene. This object will maintain a reference to that scene even if you change its name or its index in the build settings.

To create a new one go to the project window and do right click → `Create` → `Scene` → `Scene Reference`. Assign the scene you want in the inspector. 

The scriptable object contains functions that you can call to load the scene from UI buttons, Unity Events or your own code.


## Create a Loading Screen

A `Loading Screen` is another scriptable object that lives on your project. In the project window do right-click → `Create` → `Scene` → `Loading Screen` to create a new one. Assign the scene you want to act as the loading screen in the inspector. Now you can reference that Loading Screen from your code or events and call its functions indicating what scene you want to load using after the loading process.

### Add a Loading Progress Bar
`Loading Progress Bar` is a component that modifies the fillAmount of a UI image to show the progress of a loading screen.

Add it to an UI image with the `Image type` set to filled, and it will automatically show the progress of the loading process.


## Fade between scenes
Add a ``Screen Fader`` component to an UI Image and make sure that it covers the entire screen.

Both the ``Scene Reference`` and the ``Loading Screen`` have functions to change scenes with a transition, where you can tell them to use your screen fader. You can indicate the exact fader you want to use or let them automatically search for one in the scene.

You can create your own custom transition effect by creating a new component inheriting from the ``Scene Transition`` or ``Lerped Scene Transition`` classes, if you want to dig into the code a little.
