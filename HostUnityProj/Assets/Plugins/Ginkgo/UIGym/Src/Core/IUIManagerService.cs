using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ginkgo.UI
{
    public interface IUIManagerService
    {
        IUIAssetCtrl InitManager();
        void InitLoadAssets(IUIAssetCtrl assetCtrl);
    }
}