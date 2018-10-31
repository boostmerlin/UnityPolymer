using Ginkgo;
using Ginkgo.IOC;
using System;
using UnityEngine;
using DG.Tweening;
namespace Ginkgo.UI
{
    public class UGUIViewCtrlDefault : IViewCtrl
    {
        [Inject]
        IUIAssetCtrl m_assetCtrl = null;

        GameObject m_displayObj;

        public UIView OwnerView { get; set; }

        private Vector3 originScale;

        public void AdjustOrder(int index)
        {
            URoot root = URoot.Selfie;
            if (OwnerView.OnNewCanvas)
            {
                root.AdjustHierachy(m_displayObj.transform.parent, index);
            }
            else
            {
                root.AdjustHierachy(m_displayObj.transform, index);
            }
            if (OwnerView.IsWindow)
            {
                root.AdjustMask(true);
            }
        }
        public void SetPivot()
        {
            if (m_displayObj != null)
            {
                RectTransform rectTransform = m_displayObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.pivot = OwnerView.Pivot;
                }
            }
        }
        public void AttachToViewLayer()
        {
            URoot root = URoot.Selfie;
            if (OwnerView.IsWindow) //if the view is window, add mask.
            {
                root.CheckCreateMask();
                root.AddChild(m_displayObj.transform, OwnerView.OnNewCanvas);
                root.AdjustMask(true);
            }
            else
            {
                root.AddChild(m_displayObj.transform, OwnerView.OnNewCanvas);
            }
        }

        public void CloseModalWaiting()
        {
            URoot.Selfie.AdjustMask(false);
        }

        //resize to full screen and fit center.
        void reSizeFitCenter()
        {

        }

        public void FitScreen(int mode)
        {
            if (m_displayObj == null)
            {
                return;
            }
            switch (mode)
            {
                case 1://fit size.
                    reSizeFitCenter();
                    break;
            }
        }

        private void doHide(bool dispose)
        {
            var obj = m_displayObj;
            if (dispose)
            {
                UIView.StateChange(OwnerView, UIView.kDestroy);
                m_displayObj = null;
            }
            else
            {
                m_displayObj.transform.localScale = originScale;
            }

            URoot.Selfie.RemoveChild(obj.transform, dispose);
            if (OwnerView.IsWindow)
            {
                CloseModalWaiting();
            }
            UIView.StateChange(OwnerView, UIView.kHided);
        }

        public void Hide(bool animation, bool dispose)
        {
            if (animation)
            {
                hideAnimation(() =>
                {
                    doHide(dispose);
                });
            }
            else
            {
                doHide(dispose);
            }
        }

        public void Inject()
        {
            if (m_displayObj != null)
            {
                UGUIWidgetInject.Inject(OwnerView, m_displayObj);
            }
        }

        public bool LoadView()
        {
            if (m_displayObj == null)
            {
                string packageName = OwnerView.PackageName;
                string componentName = OwnerView.ComponentName;
                var gobject = m_assetCtrl.CreateObject(packageName, componentName) as GameObject;
                if (gobject != null)
                {
                    m_displayObj = gobject;
                    if (OwnerView.AutoInject)
                    {
                        Inject();
                    }
                    if (OwnerView.IsWindow)
                    {

                    }
                }
                else
                {
                    Log.Common.PrintError("UGUI m_assetCtrl.CreateObject failed. ");
                }
                return m_displayObj != null;
            }
            return false;
        }

        protected virtual void showAnimation(Action over)
        {
            originScale = m_displayObj.transform.localScale;
            m_displayObj.transform.localScale = originScale / 10;
            m_displayObj.transform.DOScale(originScale, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                over();
            });
        }

        protected virtual void hideAnimation(Action over)
        {
            m_displayObj.transform.DOScale(originScale / 10, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                over();
            });
        }

        public void Show(bool animation)
        {
            AttachToViewLayer();
            if (animation)
            {
                showAnimation(() => UIView.StateChange(OwnerView, UIView.kShowed));
            }
            else
            {
                UIView.StateChange(OwnerView, UIView.kShowed);
            }
        }
    }
}