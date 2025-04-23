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
        string fullPath = Path.Combine(Application.dataPath, "../", dllPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"The required DLL file '{dllPath}' is missing. Compilation will fail until it is restored.");

            EditorUtility.DisplayDialog(
                "Missing DLL Detected",
                $"The required DLL file '{dllPath}' is missing. Compilation will fail until it is restored.",
                "OK"
            );
        }

        FileInfo fileInfo = new FileInfo(fullPath);
        if (fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            string targetPath = Path.GetFullPath(fullPath);
            if (!File.Exists(targetPath))
            {
                Debug.LogError($"The symlink '{dllPath}' exists but points to a missing or invalid target: {targetPath}.");

                EditorUtility.DisplayDialog(
                    "Broken Symlink Detected",
                    $"The symlink '{dllPath}' exists but points to a missing or invalid target: {targetPath}. Compilation will fail until it is fixed.",
                    "OK"
                );
            }
        }
    }
}
