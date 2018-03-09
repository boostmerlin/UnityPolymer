using System;
using UniRx;
using FairyGUI;

namespace ML.UI
{
    public class FGUIAssetsCtrl : IUIAssetCtrl
    {
        object IUIAssetCtrl.CreateObject(string pkgName, string componentName)
        {
            return UIPackage.CreateObject(pkgName, componentName);
        }

        public bool LoadLocalPackage(string pkgName)
        {
            return UIPackage.AddPackage(GUIConfig.Instance.LocalUIAssetsPath + "/" + pkgName) != null;
        }

        void IUIAssetCtrl.LoadRemotePackge(IAssetPath assetPath)
        {
        }

        void remoteLoad(string pkgName, Action<int> action)
        {
            string text = UI.GUIConfig.Instance.RemoteUIAssetsRootPath;
            string ext = MelinConfig.Instance.bundleExt;

            WWWLoader.GetAssetBundle(string.Format("{0}/{1}{2}", text, pkgName, ext))
                .Subscribe((ab) =>
                {
                    if (UIPackage.AddPackage(ab) != null)
                    {
                        action(0);
                    }
                    else
                    {
                        Log.ML.PrintWarning("UIPackage.AddPackage(AssetBundle) error: " + pkgName);
                        action(1);
                    }
                },
                (except) =>
                {
                    Log.ML.PrintWarning("Load UI Assets {0} error: {1}", pkgName, except.Message);
                    action(2);
                });
        }

        void IUIAssetCtrl.RemovePackage(string pkgName)
        {
            UIPackage.RemovePackage(pkgName);
        }

        void IUIAssetCtrl.PrepareAssets(string pkgName, string componentName, Action callBack)
        {
            if (UIPackage.GetByName(pkgName) != null)
            {
                callBack();
            }
            else
            {
                if (GUIConfig.Instance.checkRemoteAssetFirst)
                {
                    remoteLoad(pkgName, (i) =>
                    {
                        if (i != 0)
                        {
                            if (LoadLocalPackage(pkgName))
                            {
                                callBack();
                            }
                            else
                            {
                                Log.ML.PrintWarning("LoadLocalPackage failed: " + pkgName);
                            }
                        }
                        else
                        {
                            callBack();
                        }
                    });
                }
                else
                {
                    if (LoadLocalPackage(pkgName))
                    {
                        callBack();
                    }
                    else
                    {
                        remoteLoad(pkgName, (i) =>
                        {
                            if (i == 0)
                            {
                                callBack();
                            }
                        });
                    }
                }
            }
        }
    }
}