﻿#if USE_FGUI
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace Ginkgo.UI
{
    public class FGUIService : IUIManagerService
    {
        void IUIManagerService.InitLoadAssets(IUIAssetCtrl assetCtrl)
        {
            var config = GUIConfig.Selfie;
            var assets = config.preloadAssets;
            for(int i = 0; i < assets.Length; i++)
            {
                assetCtrl.LoadLocalPackage(assets[i]);
            }
        }

        IUIAssetCtrl IUIManagerService.InitManager()
        {
            var config = GUIConfig.Selfie;
            GRoot.inst.SetContentScaleFactor(config.designResoWidth, config.designResoHeight, config.screenMatchMode);
            var container = MSystem.Container;
            var assetCtrl = new FGUIAssetsCtrl();
            container.RegisterInstance<IUIAssetCtrl>(assetCtrl, false);
            container.Register<IViewCtrl, FGUIViewCtrlDefault>("default");
            return assetCtrl;
        }
    }
}
#endif