#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetManager.Editor
{
    public static class DependencyChecker
    {
        private const string UniTaskUpmPath = "Packages/com.cysharp.unitask/package.json";
        private const string AssetManagerAsmdefPath = "Packages/com.batuhankanbur.assetmanager/Runtime/AssetManager.asmdef";
        private const string DefineSymbol = "UNITASK_INSTALLED";
        private static readonly Dependency[] Dependencies = new Dependency[]
        {
            new Dependency("UniTask", "Packages/com.cysharp.unitask"),
            new Dependency("Unity.Addressables", "Packages/com.unity.addressables"),
            new Dependency("UniTask.Addressables", "Packages/com.cysharp.unitask"),
            new Dependency("Unity.ResourceManager", "Packages/com.unity.addressables")
        };
        private static bool _isDependenciesChecked;
        private static bool _isDefineAdded;
        private static bool _isAsmdefChecked;
        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            EditorApplication.delayCall += CheckDependency;
            EditorApplication.delayCall += CheckAndAddDefine;
            EditorApplication.delayCall += CheckAsmdef;
        }
        private static void CheckAndAddDefine()
        {
            if (_isDefineAdded) return;
            if (!File.Exists(UniTaskUpmPath)) return;
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
            EditorApplication.delayCall -= CheckAndAddDefine;
        }
        private static void CheckDependency()
        {
            if (_isDependenciesChecked) return;
            var unitaskInstalled = File.Exists(UniTaskUpmPath);
            if (unitaskInstalled) return;
            if (EditorUtility.DisplayDialog(
                "UniTask Not Found",
                "This package depends on Cysharp's UniTask.\nWould you like to install it automatically?",
                "Yes", "No"))
            {
                AddPackage("https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask");
            }
            _isDependenciesChecked = true;
            EditorApplication.delayCall -= CheckAndAddDefine;
        }
        private static void CheckAsmdef()
        {
            if (_isAsmdefChecked) return;
            if (!File.Exists(AssetManagerAsmdefPath))
            {
                Debug.LogWarning($"AssetManager.asmdef not found at: {AssetManagerAsmdefPath}");
                return;
            }
            var asmdefJson = File.ReadAllText(AssetManagerAsmdefPath);
            var asmdef = JsonUtility.FromJson<AsmdefData>(asmdefJson);
            var changed = false;
            foreach (var dep in Dependencies)
            {
                if (Directory.Exists(dep.DirectoryPath))
                {
                    if (asmdef.references.Contains(dep.AssemblyName)) continue;
                    var refs = asmdef.references.ToList();
                    refs.Add(dep.AssemblyName);
                    asmdef.references = refs.ToArray();
                    changed = true;
                    Debug.Log($"[DependencyChecker] Added reference: {dep.AssemblyName}");
                }
                else
                {
                    Debug.Log($"[DependencyChecker] Package not found for: {dep.AssemblyName}, skipping.");
                }
            }
            if(!changed) return;
            File.WriteAllText(AssetManagerAsmdefPath, JsonUtility.ToJson(asmdef, true));
            AssetDatabase.Refresh();
            EditorApplication.delayCall -= CheckAsmdef;
            Debug.Log("[DependencyChecker] AssetManager.asmdef updated.");
            _isAsmdefChecked = true;
        }
        private static void AddPackage(string url)
        {
            UnityEditor.PackageManager.Client.Add(url);
        }
    }
    [Serializable]
    internal class AsmdefData
    {
        public string name;
        public string[] references = Array.Empty<string>();
    }
    internal class Dependency
    {
        public readonly string AssemblyName;
        public readonly string DirectoryPath;

        public Dependency(string name, string path)
        {
            AssemblyName = name;
            DirectoryPath = path;
        }
    }
}

#endif