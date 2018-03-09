using System;
using System.Collections.Generic;
using UnityEngine;

public interface IAssetPath
{
    string[] GetRemoteAssetPath(string pkgName);
}

public interface IUIAssetCtrl
{
    bool LoadLocalPackage(string pkgName);
    void RemovePackage(string pkgName);
    void LoadRemotePackge(IAssetPath assetPath);
    object CreateObject(string pkgName, string componentName);
    void PrepareAssets(string pkgName, string componentName, Action callBack);
}
