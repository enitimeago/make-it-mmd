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
            Debug.LogError($"PluralRules.Generator.dll is missing. Please check your Git repository to make sure it has been cloned properly.");

            EditorUtility.DisplayDialog(
                "Missing DLL Detected",
                $"PluralRules.Generator.dll is missing. Please check your Git repository to make sure it has been cloned properly.",
                "OK"
            );
        }

        FileInfo fileInfo = new FileInfo(fullPath);
        if (fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            string targetPath = Path.GetFullPath(fullPath);
            if (!File.Exists(targetPath))
            {
                Debug.LogError($"The symlink '{dllPath}' exists but points to a missing or invalid target: {targetPath}. Please run 'dotnet build' from third_party/Linguini/PluralRules.Generator/ to generate it.");

                EditorUtility.DisplayDialog(
                    "Broken Symlink Detected",
                    $"The symlink '{dllPath}' exists but points to a missing or invalid target: {targetPath}. Please run 'dotnet build' from third_party/Linguini/PluralRules.Generator/ to generate it.",
                    "OK"
                );
            }
        }
    }
}
