using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ginkgo.UI
{
    public class UIView : MComponent, IBinding
    {
        public const int kHided = 0;
        public const int kShowed = 1;
        public const int kDispose = 2;

        static Dictionary<Type, KeyValuePair<string, string>> viewRespaths = new Dictionary<Type, KeyValuePair<string, string>>();

        private IViewCtrl m_viewCtrl;
        public string ViewCtrlId { get; set; }

        public string Name { get; set; }
        public int ViewId { get; private set; }
        public bool IsVisible { get; private set; }
        public bool AutoInject { get; set; }

        static int ViewIdGenerator;

        /// <summary>
        /// 相同的Layer只有最顶端会显示，不同的Layer可以同时显示
        /// </summary>
        public bool OnNewLayer { get; set; }
        public string PackageName { get; protected set; }
        public string ComponentName { get; protected set; }

        public bool Modal
        {
            get; set;
        }

        bool m_modalWaiting;
        public bool ModalWaiting
        {
            get
            {
                return m_modalWaiting;
            }
            set
            {
                if (!value)
                {
                    m_viewCtrl.CloseModalWaiting();
                }
                m_modalWaiting = value;
            }
        }

        public Vector2 Pivot
        {
            get;set;
        }

        public bool ShowAnimation { get; set; }
        public bool HideAnimation { get; set; }

        /// <summary>
        /// View 是否是窗口，标题，模态，关闭等特性
        /// </summary>
        public bool IsWindow { get; set; }

        bool m_fillScreen;
        public bool FillScreen
        {
            get
            {
                return m_fillScreen;
            }
            set
            {
                m_fillScreen = value;
                m_viewCtrl.FitScreen(m_fillScreen ? 1 : 0);
            }
        }

        public bool DestroyWhenHide { get; set; }

        //protected ViewModel viewModel { private get; set; }

        //public T GetViewModel<T>() where T : ViewModel
        //{
        //    return viewModel as T;
        //}

        public UIView()
        {
            ViewId = ++ViewIdGenerator;
            ViewCtrlId = "default";
            AutoInject = true;
            Pivot = new Vector2(0.5f, 0.5f);
        }

        public static UIView CreateView(string pkgName, string componentName)
        {
            UIView v = new UIView
            {
                ComponentName = componentName,
                PackageName = pkgName,
            };
            v.setName();
            v.onCreate();

            return v;
        }

        public void __adjustOrder(int index)
        {
            m_viewCtrl.AdjustOrder(index);
        }

        public static UIView CreateView(Type type)
        {
            KeyValuePair<string, string> info;
            if (viewRespaths.TryGetValue(type, out info))
            {
                UIView v = Activator.CreateInstance(type, null) as UIView;
                if (v == null)
                {
                    Log.Common.PrintWarning("Create View Failed for Wrong UIView Type: " + type.Name);
                    return null;
                }
                v.ComponentName = info.Value;
                v.PackageName = info.Key;
                v.setName();
                v.onCreate();
                return v;
            }
            Log.Common.PrintWarning("Create View Failed for No Bind info: " + type.Name);
            return null;
        }

        public static T CreateView<T>() where T : UIView
        {
            var v = CreateView(typeof(T));
            return (T)v;
        }

        public static void RegAsset<T>(string pkgName, string componentName) where T : UIView
        {
            var kv = new KeyValuePair<string, string>(pkgName, componentName);
            viewRespaths[typeof(T)] = kv;
        }

        protected virtual void onCreate()
        {
            m_viewCtrl = Container.ResolveNew<IViewCtrl>(ViewCtrlId);
            m_viewCtrl.OwnerView = this;
            ShowAnimation = GUIConfig.Selfie.animationEnable;
            HideAnimation = ShowAnimation;
        }

        /// <summary>
        /// just before the view come visible.
        /// </summary>
        protected virtual void onShow(bool newobj)
        {
            if(newobj)
            {
                Bind();
            }
        }
        /// <summary>
        /// just before the view come invisible.
        /// </summary>
        protected virtual void onHide()
        {

        }

        /// <summary>
        /// after the view showed.
        /// </summary>
        protected virtual void onShowed()
        {

        }

        /// <summary>
        /// after the view hided.
        /// </summary>
        protected virtual void onHided()
        {

        }

        protected virtual void onDestroy()
        {
            UnBind();
            Dispose();
        }

        public void ___StateChange(int state)
        {
            switch (state)
            {
                case kHided:
                    onHided();
                    break;
                case kShowed:
                    onShowed();
                    break;
                case kDispose:
                    onDestroy();
                    break;
            }
        }

        void setName()
        {
            Name = string.Format("{0}@{1}", ComponentName, PackageName);
        }

        public void Show()
        {
            onShow(m_viewCtrl.LoadView());
            m_viewCtrl.Show(ShowAnimation);
        }

        public void Hide(bool animation)
        {
            onHide();
            m_viewCtrl.Hide(animation, DestroyWhenHide);
        }

        public virtual void Bind()
        {
            
        }

        public virtual void UnBind()
        {
            
        }
    }
}
