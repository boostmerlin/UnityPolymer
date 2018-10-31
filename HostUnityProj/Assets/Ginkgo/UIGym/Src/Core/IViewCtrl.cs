using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ginkgo.UI
{
    /// <summary>
    /// Control View Behavior.
    /// </summary>
    public interface IViewCtrl
    {
        void AttachToViewLayer();
        //返回加载的是缓存还是新的object.
        bool LoadView();
        void Inject();
        UIView OwnerView { get; set; }
        void Show(bool animation);
        void Hide(bool animation, bool dispose);
        void CloseModalWaiting();
        void AdjustOrder(int index);
        void FitScreen(int mode);
        void SetPivot();
    }
}
