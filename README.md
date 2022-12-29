# Rai Manager

A small app I made to make it easier for people to install my VR mods. It auto-detects if the game is installed, and is made to allow for mods to be auto-updated when served via the [itch.io app](https://itch.io/app).

![Rai Manager](https://user-images.githubusercontent.com/3955124/200336525-1b9037e2-6f34-4680-9896-c73673f86fb7.png)

## Using it with your own mods

For now, this app is hardcoded to only handle mods made with [BepInEx](https://github.com/BepInEx/BepInEx). If your mod uses another mod loader, you'll need to modify the Rai Manager source code to handle that (if you do, feel free to open a PR).

Check the [ModExample](https://github.com/Raicuparta/rai-manager/tree/master/ModExample) folder in this repo to learn how to structure your files.

Use an editor that supports json schema validation (for instance, [VSCode](https://code.visualstudio.com/)) to edit your `manifest.json`. This way, you will be able to read hints about what each entry means, get auto-completion, etc. [Manifest schema here](https://github.com/Raicuparta/rai-manager/blob/master/manifest.schema.json). Note how the [example mod manifest](https://github.com/Raicuparta/rai-manager/blob/master/ModExample/manifest.json) references the schema at the top.

For guidance, you can use my [Two Forks VR repo](https://github.com/Raicuparta/two-forks-vr), or [download a release build from itch.io](https://raicuparta.itch.io/two-forks-vr) and look at how the files are laid out.

## Building Rai Manager

Note that you don't _need_ to build Rai Manager yourself to make it work with your own mod. If your mod is made with BepInEx, you can just copy all the RaiManager files from one of my mods, and replace the mod-specific files with your own mod. This works because all the mod and game info is read at runtime from the available files.

You might want to build it anyway, for instance if you wanna change the app title, the app icon, etc. You can do so by running these commands, presuming you started in the repository root folder:

```
cd RaiManager
dotnet publish
```

When you build Rai Manager with either the Debug or Release configurations (either via your IDE or using `dotnet build`), you will have a lot of dangling dlls and other files in the output folder. To compile the app to a single executable, you need to use `dotnet publish`. You should run it from the **project** folder, to make sure all the settings from the csproj file are read.

If you use Rider, you can also publish via the "run configurations" feature:

![image](https://user-images.githubusercontent.com/3955124/200339452-317d1378-1bb4-437d-9a6a-12c8cdffde6a.png)

During development, you wanna keep it in the "RaiManager" run configuration, so you can test the manager by running via the IDE. Once you wanna publish it, you need to swap to the "Publish RaiManager" run configuration, and run the project.

## License

Rai Manager: VR Mod Manager
Copyright (C) 2022 Raicuparta

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
