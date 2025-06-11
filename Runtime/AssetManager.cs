// <summary>
// AssetManager - A powerful yet lightweight asset management system for Unity,
// developed by Batuhan Kanbur (https://batuhankanbur.com).
// Source: https://github.com/BatuhanKanbur
//
// This package simplifies working with Unity Addressables by providing a clean,
// async-ready interface for loading, releasing, and managing asset lifecycles.
//
// Requirements:
// - Unity Addressables
// - Cysharp UniTask
//
// "Simplicity is the soul of efficiency." – Austin Freeman
// </summary>
#if UNITASK_PRESENT
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class AssetManager<T> where T : Object
{
    private static readonly Dictionary<object, object> AssetCache = new();
    private static bool _initialized;
    static AssetManager()
    {
        Init();
        Application.quitting += Dispose;
    }
    private static async void Init()
    {
        if (_initialized) return;
        SceneManager.activeSceneChanged += OnSceneChanged;
        await Addressables.InitializeAsync(false);
        _initialized = true;
    }
    private static void Dispose()
    {
        if (!_initialized) return;
        SceneManager.activeSceneChanged -= OnSceneChanged;
        Application.quitting -= Dispose;
        AssetCache.Clear();
        _initialized = false;
    }
    private static void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        AssetCache.Clear();
        Resources.UnloadUnusedAssets();
    }
    public static async UniTask<T> LoadAsset(object assetReference)
    {
        if (AssetCache.TryGetValue(assetReference, out var cachedAsset))
            return (T)cachedAsset;
        await UniTask.WaitUntil(()=> _initialized);
        var handle = Addressables.LoadAssetAsync<T>(assetReference);
        await handle.ToUniTask();
        if (handle.Status != AsyncOperationStatus.Succeeded)
            throw new Exception($"[AssetManager] Failed to load asset: {assetReference}");
        AssetCache[assetReference] = handle.Result;
        return handle.Result;
    }

    public static async UniTask<List<T>> LoadAssets(object assetReference)
    {
        if (AssetCache.TryGetValue(assetReference, out var cachedList))
            return (List<T>)cachedList;
        await UniTask.WaitUntil(()=> _initialized);
        var handle = Addressables.LoadAssetsAsync<T>(assetReference, null);
        await handle.ToUniTask();
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var resultList = new List<T>(handle.Result);
            AssetCache[assetReference] = resultList;
            return resultList;
        }
        Debug.LogError($"[AssetManager] Failed to load assets: {assetReference}\n{handle.OperationException}");
        return new List<T>();
    }

    public static void ReleaseAssets(List<object> assetReferences)
    {
        if (assetReferences == null || assetReferences.Count == 0) return;
        foreach (var reference in assetReferences)
        {
            if (!AssetCache.TryGetValue(reference, out var asset))
            {
                Debug.LogWarning($"[AssetManager] Asset not found in cache for release: {reference}");
                continue;
            }
            Addressables.Release(asset);
            AssetCache.Remove(reference);
        }
    }
}
#endif


