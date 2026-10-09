// Builds Console asset bundles from a Unity project.
//
// Put this file in any folder named "Editor" inside your Unity project's Assets folder,
// then use Console > Build Asset Bundles, or build without opening the editor:
//
//   Unity.exe -batchmode -quit -projectPath "<your project>" -executeMethod ConsoleBundleBuilder.BuildAll
//
// The finished bundles land in Builds/ServerData inside the project, ready to copy into
// the Console repository's ServerData folder.

#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ConsoleBundleBuilder
{
    // Unity writes its own bookkeeping files here; only the bundles are copied out of it.
    private const string WorkFolder = "Builds/ConsoleBundles";
    private const string ReadyFolder = "Builds/ServerData";

    [MenuItem("Console/Build Asset Bundles")]
    public static void BuildFromMenu()
    {
        try
        {
            string message = Build();
            EditorUtility.DisplayDialog("Console", message, "OK");
            EditorUtility.RevealInFinder(ReadyFolder);
        }
        catch (Exception error)
        {
            EditorUtility.DisplayDialog("Console", error.Message, "OK");
        }
    }

    /// <summary>The batch-mode entry point; a failure ends Unity with an error code.</summary>
    public static void BuildAll() => Debug.Log("[Console] " + Build());

    private static string Build()
    {
        string[] names = AssetDatabase.GetAllAssetBundleNames();
        if (names.Length == 0)
            throw new InvalidOperationException(
                "No asset has an AssetBundle name yet. Select a prefab, and at the bottom of the Inspector set its AssetBundle name (for example \"mycoolhat\").");

        // A bundle from a newer editor than the game cannot be opened at all, and one from an
        // older major version can lose its materials, so the version is worth a warning.
        if (!Application.unityVersion.StartsWith("6000.", StringComparison.Ordinal))
            Debug.LogWarning(
                $"[Console] This editor is Unity {Application.unityVersion}. Gorilla Tag runs on Unity 6 (6000.x); " +
                "build with the same version as the game, or older players' copies may fail to load the bundle.");

        Directory.CreateDirectory(WorkFolder);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            WorkFolder,
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode,
            BuildTarget.StandaloneWindows64);

        if (manifest == null)
            throw new InvalidOperationException("The build failed. The reasons are in the Console window (or the editor log in batch mode).");

        Directory.CreateDirectory(ReadyFolder);
        foreach (string bundle in manifest.GetAllAssetBundles())
            File.Copy(Path.Combine(WorkFolder, bundle), Path.Combine(ReadyFolder, bundle), true);

        return $"Built {manifest.GetAllAssetBundles().Length} bundle(s) with Unity {Application.unityVersion} into {Path.GetFullPath(ReadyFolder)}. " +
               "Copy them into the Console repository's ServerData folder, then run Tools/make_asset_list.py.";
    }
}
#endif
