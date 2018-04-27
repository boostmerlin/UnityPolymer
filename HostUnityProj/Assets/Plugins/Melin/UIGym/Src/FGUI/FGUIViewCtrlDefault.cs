using System;
using UnityEngine;
using FairyGUI;
using ML.IOC;
using DG.Tweening;

namespace ML.UI
{
    public class FGUIWindowDefault : Window
    {
        public FGUIWindowDefault(UIView view) : base()
        {
            m_view = view;
        }
        UIView m_view;
        protected override void OnInit()
        {
            base.OnInit();
            displayObject.onTouchBegin.AddCapture(touchBegin);
        }
        private void touchBegin(EventContext context)
        {
            if (this.isShowing && bringToFontOnClick)
            {
                GUIManager.Selfie.BringViewToFront(m_view.ViewId);
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
            displayObject.onTouchBegin.RemoveCapture(touchBegin);
        }

        protected override void DoHideAnimation()
        {
            GUIManager.Selfie.Pop();
        }
    }

    public class FGUIViewCtrlDefault : IViewCtrl
    {
        UIView m_theView;
        GComponent m_displayObj;

        [Inject]
        IUIAssetCtrl m_assetCtrl = null;
        public UIView OwnerView
        {
            get
            {
                return m_theView;
            }
            set
            {
                m_theView = value;
            }
        }

        void reSizeFitCenter()
        {
            m_displayObj.Center();
            m_displayObj.SetSize(GRoot.inst.width, GRoot.inst.height);
        }

        void handleSizeChanged()
        {
            if(m_displayObj == null)
            {
                return;
            }
            if (m_theView.FillScreen)
            {
                reSizeFitCenter();
            }
            else
            {
                if (m_theView.IsWindow)
                {
                    m_displayObj.Center();
                }
            }
        }

        public void AttachToViewLayer()
        {
            UnityEngine.Assertions.Assert.IsTrue(m_displayObj != null);
            GComponent obj = m_displayObj;
            setPivot();
            if (!m_theView.IsWindow)
            {
                GRoot.inst.AddChild(obj);
            }
            else
            {
                Window win = obj as Window;
                win.modal = m_theView.Modal;
                if (m_theView.ModalWaiting)
                {
                    win.ShowModalWait();
                }
                win.Show();
            }
            handleSizeChanged();
        }

        public bool LoadView()
        {
            if (m_displayObj == null)
            {
                string packageName = m_theView.PackageName;
                string componentName = m_theView.ComponentName;
                var gobject = m_assetCtrl.CreateObject(packageName, componentName) as GObject;
                if (gobject != null)
                {
                    m_displayObj = gobject.asCom;
                    if(m_theView.AutoInject)
                    {
                        Inject();
                    }
                    if (m_theView.IsWindow)
                    {
                        FGUIWindowDefault win = new FGUIWindowDefault(m_theView);
                        win.contentPane = m_displayObj;
                        m_displayObj = win;
                    }
                    GRoot.inst.onSizeChanged.Add(handleSizeChanged);
                }
                else
                {
                    Log.ML.PrintError("m_assetCtrl.CreateObject failed. ");
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
           // LoadView();
            AttachToViewLayer();
            if (animation)
            {
                showAnimation(() => m_theView.___StateChange(UIView.kShowed));
            }
            else
            {
                m_theView.___StateChange(UIView.kShowed);
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

        void doHide(bool dispose)
        {
            var obj = m_displayObj;
            if (dispose)
            {
                m_theView.___StateChange(UIView.kDispose);
                GRoot.inst.onSizeChanged.Remove(handleSizeChanged);
                m_displayObj = null;
            }
            if (obj is Window)
            {
                GRoot.inst.HideWindowImmediately((Window)obj, dispose);
            }
            else
            {
                GRoot.inst.RemoveChild(obj, dispose);
            }
            m_theView.___StateChange(UIView.kHided);
        }

        public void CloseModalWaiting()
        {
            if (m_theView.IsWindow && m_displayObj != null)
            {
                ((Window)m_displayObj).CloseModalWait();
            }
        }

        public void AdjustOrder(int index)
        {
            int current = GRoot.inst.GetChildIndex(m_displayObj);
            if (current == -1)
            {
                return;
            }
            if (GRoot.inst.hasModalWindow)
            {
                int modelIndex = GRoot.inst.GetChildIndex(GRoot.inst.modalLayer);
                if (index >= modelIndex)
                {
                    index = index + 1;
                }
            }
            if (current != index)
            {
                GRoot.inst.SetChildIndex(m_displayObj, index);
            }
        }

        void setPivot()
        {
            if (m_displayObj != null)
            {
                m_displayObj.SetPivot(m_theView.Pivot.x, m_theView.Pivot.y);
            }
        }

        public void FitScreen(int mode)
        {
            if(m_displayObj == null)
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

        public void Inject()
        {
            if (m_displayObj != null)
            {
                FGUIWidgetInject.Inject(m_theView, m_displayObj);
            }
        }
    }
}