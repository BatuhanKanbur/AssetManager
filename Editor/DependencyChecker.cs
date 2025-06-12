#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;

namespace AssetManager.Editor
{
    public static class DependencyChecker
    {
        private const string UniTaskPath = "Packages/com.cysharp.unitask/package.json";
        private const string DefineSymbol = "UNITASK_INSTALLED";
        private static bool _isDependenciesChecked;
        private static bool _isDefineAdded;
        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            EditorApplication.delayCall += CheckDependency;
            EditorApplication.delayCall += CheckAndAddDefine;
        }
        private static void CheckAndAddDefine()
        {
            if (_isDefineAdded) return;
            if (!File.Exists(UniTaskPath)) return;
            BuildTargetGroup[] targetGroups = {
                BuildTargetGroup.Standalone,
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS,
                BuildTargetGroup.WebGL,
                BuildTargetGroup.WSA,
                BuildTargetGroup.PS4,
                BuildTargetGroup.PS5,
                BuildTargetGroup.XboxOne,
            };

            foreach (var group in targetGroups)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var defineList = defines.Split(';').ToList();

                if (defineList.Contains(DefineSymbol)) continue;
                defineList.Add(DefineSymbol);
                var newDefines = string.Join(";", defineList);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
                UnityEngine.Debug.Log($"[DependencyChecker] '{DefineSymbol}' define added to {group}");
            }
            _isDefineAdded = true;
        }
        private static void CheckDependency()
        {
            if (_isDependenciesChecked) return;
            var unitaskInstalled = File.Exists("Packages/com.cysharp.unitask/package.json");
            if (unitaskInstalled) return;
            if (EditorUtility.DisplayDialog(
                "UniTask Not Found",
                "This package depends on Cysharp's UniTask.\nWould you like to install it automatically?",
                "Yes", "No"))
            {
                AddPackage("https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask");
            }
            _isDependenciesChecked = true;
        }
        private static void AddPackage(string url)
        {
            UnityEditor.PackageManager.Client.Add(url);
        }
    }
}
#endif