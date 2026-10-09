# Making Console asset bundles

Console assets are Unity asset bundles. They can only be built in the Unity Editor, so this
folder holds what you need to build them and add them to `ServerData`.

## 1. Use the right Unity version

Build with the same Unity version as Gorilla Tag (Unity 6, `6000.x`). To see the exact
version, open `%USERPROFILE%\AppData\LocalLow\Another Axiom\Gorilla Tag\Player.log` after
playing once and look for the line starting `Initialize engine version:`. A bundle built
with a newer editor than the game will not load at all; one from an older version may lose
its materials. Use URP shaders (the game uses the Universal Render Pipeline).

## 2. Add the build script

Copy `Editor/ConsoleBundleBuilder.cs` into your Unity project, into any folder named
`Editor` under `Assets` (for example `Assets/Editor/`).

## 3. Mark what goes in a bundle

Select a prefab, and at the very bottom of the Inspector set its **AssetBundle** name, for
example `mycoolhat`. Everything with the same name goes into the same bundle. The prefab's
own name is what the Console spawner shows and spawns.

## 4. Build

In the editor: **Console > Build Asset Bundles**. Or without opening it:

```
Unity.exe -batchmode -quit -projectPath "C:\path\to\project" -executeMethod ConsoleBundleBuilder.BuildAll
```

The bundles appear in `Builds/ServerData` inside your project.

## 5. Add them to this repository

1. Copy the files from `Builds/ServerData` into `ServerData/` here.
2. Run `python3 Tools/make_asset_list.py` from the repository root. It updates
   `ServerData/ConsoleAssets.json` and `ConsoleAssets.txt`, which the menu's asset spawner
   reads, with each bundle's Unity version and size.
3. Commit and push. The spawner picks the new bundles up the next time the menu starts.
