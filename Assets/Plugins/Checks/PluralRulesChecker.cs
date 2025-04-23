using UnityEditor;
using UnityEngine;
using System.IO;

// Runs even if Make It MMD doesn't compile since it's in a different assembly.
// https://www.reddit.com/r/Unity3D/comments/nxxqa0/comment/h1hfg0y/?context=3
[InitializeOnLoad]
public class PluralRulesChecker
{
    static PluralRulesChecker()
    {
        string dllPath = "Packages/enitimeago.non-destructive-mmd/Editor/vendor/PluralRules.Generator.dll";

        if (!File.Exists(Path.Combine(Application.dataPath, "../", dllPath)))
        {
            Debug.LogError($"The required DLL file '{dllPath}' is missing. Compilation will fail until it is restored.");

            EditorUtility.DisplayDialog(
                "Missing DLL Detected",
                $"The required DLL file '{dllPath}' is missing. Compilation will fail until it is restored.",
                "OK"
            );
        }
    }
}
