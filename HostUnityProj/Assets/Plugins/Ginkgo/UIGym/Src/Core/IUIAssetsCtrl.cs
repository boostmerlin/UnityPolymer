using System;

public interface IUIAssetCtrl
{
    bool LoadLocalPackage(string pkgName);
    void RemovePackage(string pkgName);
    //加载多个
    void LoadRemotePackge(string[] assetPath);
    object CreateObject(string pkgName, string componentName);
    void PrepareAssets(string pkgName, string componentName, Action callBack);
#if !USE_FGUI
    void RemoveComponent(string pkgName, string componentName);
#endif
}
