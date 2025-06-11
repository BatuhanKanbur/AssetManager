#if UNITY_EDITOR
using System.IO;
using UnityEditor;

public static class DependencyChecker
{
    [InitializeOnLoadMethod]
    private static void OnEditorLoad()
    {
        EditorApplication.delayCall += CheckDependency;
    }
    private static void CheckDependency()
    {
        var unitaskInstalled = File.Exists("Packages/com.cysharp.unitask/package.json");
        if (unitaskInstalled) return;
        if (EditorUtility.DisplayDialog(
            "UniTask Not Found",
            "This package depends on Cysharp's UniTask.\nWould you like to install it automatically?",
            "Yes", "No"))
        {
            AddPackage("https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask");
        }
    }
    private static void AddPackage(string url)
    {
        UnityEditor.PackageManager.Client.Add(url);
    }
}
#endif
