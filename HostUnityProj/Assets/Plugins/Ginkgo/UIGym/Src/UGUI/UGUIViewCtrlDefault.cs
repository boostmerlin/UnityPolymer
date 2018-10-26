using Ginkgo;
using Ginkgo.IOC;
using Ginkgo.UI;
using System;
using UnityEngine;
using DG.Tweening;

public class UGUIViewCtrlDefault : IViewCtrl
{
    [Inject]
    IUIAssetCtrl m_assetCtrl = null;

    GameObject m_displayObj;

    public UIView OwnerView { get; set; }

    public void AdjustOrder(int index)
    {
        throw new System.NotImplementedException();
    }

    public void AttachToViewLayer()
    {
        throw new System.NotImplementedException();
    }

    public void CloseModalWaiting()
    {
        throw new System.NotImplementedException();
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
        m_displayObj.SetScale(0.1f, 0.1f);
        m_displayObj.TweenScale(new Vector2(1, 1), 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            over();
        });
    }

    protected virtual void hideAnimation(Action over)
    {
        m_displayObj.TweenScale(new Vector2(0.1f, 0.1f), 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            over();
        });
    }

    public void Show(bool animation)
    {
        AttachToViewLayer();
        if (animation)
        {
            showAnimation(() => OwnerView.___StateChange(UIView.kShowed));
        }
        else
        {
            OwnerView.___StateChange(UIView.kShowed);
        }
    }
}
