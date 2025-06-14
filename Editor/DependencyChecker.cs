#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace AssetManager.Editor
{
    [InitializeOnLoad]
    public static class DependencyChecker
    {
        private const string UniTaskGitUrl = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask";
        private const string UniTaskPath = "Packages/com.cysharp.unitask";
        private const string AddressablesPath = "Packages/com.unity.addressables";
        private const string AssetManagerAsmdefPath = "Packages/com.batuhankanbur.assetmanager/Runtime/AssetManager.asmdef";
        private const string DefineSymbol = "ASSETMANAGER_INITIALIZED";

        private static AddRequest _currentRequest;

        static DependencyChecker()
        {
            EditorApplication.update += Run;
        }

        private static void Run()
        {
            EditorApplication.update -= Run;
            CheckAndInstallDependencies();
        }

        private static void CheckAndInstallDependencies()
        {
            var allGood =
                IsPackageInstalled(UniTaskPath) &&
                IsPackageInstalled(AddressablesPath) &&
                AsmdefHasReferences("UniTask", "Unity.Addressables", "UniTask.Addressables", "Unity.ResourceManager") &&
                HasDefineSymbol(DefineSymbol);

            if (allGood)
            {
                Debug.Log("[DependencyChecker] All dependencies verified.");
                return;
            }

            Debug.Log("[DependencyChecker] Dependencies missing or incomplete. Fixing...");

            if (!IsPackageInstalled(UniTaskPath))
            {
                Debug.Log("[DependencyChecker] Installing UniTask...");
                _currentRequest = Client.Add(UniTaskGitUrl);
                EditorApplication.update += WaitForUniTask;
                return;
            }

            if (!IsPackageInstalled(AddressablesPath))
            {
                Debug.Log("[DependencyChecker] Installing Addressables...");
                _currentRequest = Client.Add("com.unity.addressables");
                EditorApplication.update += WaitForAddressables;
                return;
            }

            AfterInstall();
        }

        private static void WaitForUniTask()
        {
            if (!_currentRequest.IsCompleted) return;
            EditorApplication.update -= WaitForUniTask;

            if (_currentRequest.Status == StatusCode.Success)
            {
                Debug.Log("[DependencyChecker] UniTask installed.");
                if (!IsPackageInstalled(AddressablesPath))
                {
                    _currentRequest = Client.Add("com.unity.addressables");
                    EditorApplication.update += WaitForAddressables;
                }
                else
                {
                    AfterInstall();
                }
            }
            else
            {
                Debug.LogError($"[DependencyChecker] Failed to install UniTask: {_currentRequest.Error.message}");
            }
        }

        private static void WaitForAddressables()
        {
            if (!_currentRequest.IsCompleted) return;
            EditorApplication.update -= WaitForAddressables;

            if (_currentRequest.Status == StatusCode.Success)
            {
                Debug.Log("[DependencyChecker] Addressables installed.");
                AfterInstall();
            }
            else
            {
                Debug.LogError($"[DependencyChecker] Failed to install Addressables: {_currentRequest.Error.message}");
            }
        }

        private static void AfterInstall()
        {
            AddDefine();
            UpdateAsmdef();
            Debug.Log("[DependencyChecker] Setup completed.");
        }

        private static void AddDefine()
        {
            BuildTargetGroup[] groups =
            {
                BuildTargetGroup.Standalone,
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS,
                BuildTargetGroup.WebGL,
                BuildTargetGroup.WSA,
                BuildTargetGroup.PS4,
                BuildTargetGroup.PS5,
                BuildTargetGroup.XboxOne
            };

            foreach (var group in groups)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();
                if (!defines.Contains(DefineSymbol))
                {
                    defines.Add(DefineSymbol);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines));
                    Debug.Log($"[DependencyChecker] Define added: {DefineSymbol} for {group}");
                }
            }
        }

        private static void UpdateAsmdef()
        {
            if (!File.Exists(AssetManagerAsmdefPath))
            {
                Debug.LogWarning($"[DependencyChecker] asmdef file not found at {AssetManagerAsmdefPath}");
                return;
            }

            var asmdefText = File.ReadAllText(AssetManagerAsmdefPath);
            var asmdef = JsonUtility.FromJson<AsmdefData>(asmdefText);
            var refs = asmdef.references.ToList();
            bool changed = false;

            void TryAdd(string asmName)
            {
                if (!refs.Contains(asmName))
                {
                    refs.Add(asmName);
                    changed = true;
                    Debug.Log($"[DependencyChecker] Added asmdef reference: {asmName}");
                }
            }

            TryAdd("UniTask");
            TryAdd("Unity.Addressables");
            TryAdd("UniTask.Addressables");
            TryAdd("Unity.ResourceManager");

            if (changed)
            {
                asmdef.references = refs.ToArray();
                File.WriteAllText(AssetManagerAsmdefPath, JsonUtility.ToJson(asmdef, true));
                AssetDatabase.Refresh();
                Debug.Log("[DependencyChecker] asmdef updated.");
            }
        }

        private static bool IsPackageInstalled(string path) => Directory.Exists(path);

        private static bool AsmdefHasReferences(params string[] requiredRefs)
        {
            if (!File.Exists(AssetManagerAsmdefPath)) return false;

            var asmdefText = File.ReadAllText(AssetManagerAsmdefPath);
            var asmdef = JsonUtility.FromJson<AsmdefData>(asmdefText);
            var refs = asmdef.references ?? Array.Empty<string>();

            return requiredRefs.All(r => refs.Contains(r));
        }

        private static bool HasDefineSymbol(string symbol)
        {
            BuildTargetGroup[] groups =
            {
                BuildTargetGroup.Standalone,
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS,
                BuildTargetGroup.WebGL,
                BuildTargetGroup.WSA,
                BuildTargetGroup.PS4,
                BuildTargetGroup.PS5,
                BuildTargetGroup.XboxOne
            };
            return groups.All(group => PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Contains(symbol));
        }

        [Serializable]
        private class AsmdefData
        {
            public string name;
            public string[] references = Array.Empty<string>();
        }
    }
}
#endif
