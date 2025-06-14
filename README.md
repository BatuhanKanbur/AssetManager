# AssetManager

**A powerful yet lightweight asset management system for Unity**  
by [Batuhan Kanbur](https://batuhankanbur.com)  

> _"Simplicity is the soul of efficiency."_ – Austin Freeman

---

## 🚀 Usage Example
```csharp
Sprite gameIcon = await AssetManager<Sprite>.LoadAsync(iconAssetReference);
```
---

## ✨ Overview

`AssetManager` is a clean and modular wrapper around **Unity Addressables**, designed to provide an **async-ready**, streamlined interface for loading, releasing, and managing asset lifecycles.

Built with modern Unity development in mind, it uses [`Cysharp/UniTask`](https://github.com/Cysharp/UniTask) for efficient async operations and is structured to support scalable, readable, and testable code.

---

## 📦 Features

- ✅ Clean and minimal API for Addressables
- ⚡ `UniTask`-powered asynchronous loading
- ♻️ Proper asset release & lifecycle management
- 🧪 Lightweight and easy to integrate
- 🧩 Editor & Runtime assembly separation

---

## 🛠 Requirements

- **Unity** 2020.3 or newer
- ✅ [Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- ✅ [Cysharp UniTask](https://github.com/Cysharp/UniTask)

---

## 📥 Installation

### Option 1: Unity Package Manager (Git URL)

In Unity:

1. Open **Window → Package Manager**
2. Click the **+** icon → **Add package from Git URL**
3. Paste the URL:https://github.com/BatuhanKanbur/AssetManager.git

---

## 🔄 Dependencies

### 1. Addressables  
This package **requires** Unity Addressables. It will be listed as a dependency and installed automatically by Unity if missing.

### 2. UniTask (Optional but Recommended)  
If not already installed, you’ll be prompted in the Unity Editor to add it via:

```csharp
UnityEditor.PackageManager.Client.Add("https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask");

