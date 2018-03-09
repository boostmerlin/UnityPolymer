using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.UI
{
    public interface IViewCtrl
    {
        void AttachToViewLayer();
        bool LoadView();
        void Inject();
        UIView OwnerView { get; set; }
        void Show(bool animation);
        void Hide(bool animation, bool dispose);
        void CloseModalWaiting();
        void AdjustOrder(int index);
        void FitScreen(int mode);
    }
}
