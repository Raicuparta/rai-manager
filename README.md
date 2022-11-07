# Rai Manager

A small app I made to make it easier for people to install my VR mods. It auto-detects if the game is installed, and is made to allow for mods to be auto-updated when served via the [itch.io app](https://itch.io/app).

![Rai Manager](https://user-images.githubusercontent.com/3955124/200336525-1b9037e2-6f34-4680-9896-c73673f86fb7.png)

## Using it with your own mods

For now, this app is hardcoded to only handle mods made with [BepInEx](https://github.com/BepInEx/BepInEx). If your mod uses another mod loader, you'll need to modify the Rai Manager source code to handle that (if you do, feel free to open a PR).

Check the [ModExample](https://github.com/Raicuparta/rai-manager/tree/master/ModExample) folder in this repo to learn how to structure your files.

Use an editor that supports json schema validation (for instance, [VSCode](https://code.visualstudio.com/)) to edit your `manifest.json`. This way, you will be able to read hints about what each entry means, get auto-completion, etc. [Manifest schema here](https://github.com/Raicuparta/rai-manager/blob/master/manifest.schema.json). Note how the [example mod manifest](https://github.com/Raicuparta/rai-manager/blob/master/ModExample/manifest.json) references the schema at the top.

For guidance, you can use my [Two Forks VR repo](https://github.com/Raicuparta/two-forks-vr), or [download a release build from itch.io](https://raicuparta.itch.io/two-forks-vr) and look at how the files are laid out.

## Building Rai Manager

Note that you don't *need* to build Rai Manager yourself to make it work with your own mod. If your mod is made with BepInEx, you can just copy all the RaiManager files from one of my mods, and replace the mod-specific files with your own mod. This works because all the mod and game info is read at runtime from the available files.

You might want to build it anyway, for instance if you wanna change the app title, the app icon, etc.

There are two run configurations:

![image](https://user-images.githubusercontent.com/3955124/200339452-317d1378-1bb4-437d-9a6a-12c8cdffde6a.png)

During development, you wanna keep it in the "RaiManager" run configuration, so you can test the manager by running via the IDE. Once you wanna publish it, you need to swap to the "Publish RaiManager" run configuration, and run the project. This will create a neat little publish build without all the dangling dlls and stuff.
