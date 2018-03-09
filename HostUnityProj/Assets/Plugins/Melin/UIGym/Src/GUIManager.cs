using System.Collections.Generic;
using ML.IOC;
using UniRx;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ML.UI
{
    /// <summary>
    /// 不同layer上的UI可以同时显示，相同layer总是显示最后一个而隐藏前一个ui
    /// traceback=true,不会删除前一个push的view，pop时候会重新显示。
    /// </summary>
    public sealed class GUIManager : MLSingleton<GUIManager>
    {
        class ViewContext
        {
            public UIView uiView;
            public bool traceback;
        }

        [Inject]
        IUIManagerService m_manipulator = null;
        IUIAssetCtrl m_assetCtrl = null;
        //list top always visible.
        List<List<UIView>> m_layerToViews;
        Dictionary<int, ViewContext> m_idToViews;

        Queue<int> pushQueue;
        HashSet<int> readySet;

        /// <summary>
        /// 不可弹出的层的最小值，小于该值的层不会被POP，同时MinUnpoppableLayer+1层为默认的Push层，
        /// 比如 当0层作为背景层，如果还需要到Push到0层，需要 MinUnpoppableLayer = GUIManager.POP_ALL，
        /// 主要是使用方便上的考虑
        /// </summary>
        public int MinUnpoppableLayer { get; set; }
        public const int POP_ALL = -1;

        protected override void onInitialize()
        {
            m_layerToViews = new List<List<UIView>>(3);
            m_idToViews = new Dictionary<int, ViewContext>();
            pushQueue = new Queue<int>();
            readySet = new HashSet<int>();
            m_assetCtrl = m_manipulator.InitManager();
            m_manipulator.InitLoadAssets(m_assetCtrl);
            //work on main thread?
            Observable.EveryEndOfFrame().Subscribe((_) =>
            {
                if(pushQueue.Count > 0)
                {
                    int id = pushQueue.Peek();
                    if (readySet.Remove(id))
                    {
                        pushQueue.Dequeue();
                        innerPush(id);
                    }
                }
            });
        }

#if UNITY_EDITOR
        public List<List<UIView>> GetLayerViews()
        {
            return m_layerToViews;
        }
#endif

        public bool Check()
        {
            return m_manipulator != null && m_assetCtrl != null;
        }

        public int LayerNumber
        {
            get
            {
                return m_layerToViews.Count;
            }
        }

        public T Push<T>(bool traceback = false, bool onNewLayer = false) where T : UIView
        {
            var view = UIView.CreateView<T>();
            if (view != null)
            {
                view.OnNewLayer = onNewLayer;
                Push(view, traceback);
            }

            return view;
        }

        public void BringViewToLayer(int viewId, int layer)
        {
            int targetLayer = layer >= 0 ? layer : m_layerToViews.Count + layer;

            if (targetLayer >= 0 && targetLayer < m_layerToViews.Count)
            {
                ViewContext viewContext = getViewContext(viewId);
                if(viewContext != null)
                {
                    int sublayer;
                    int currentViewLayer = findLayerForView(viewContext.uiView, out sublayer);
                    if(currentViewLayer != -1 && currentViewLayer != targetLayer)
                    {
                        var currentViews = m_layerToViews[currentViewLayer];
                        m_layerToViews.RemoveAt(currentViewLayer);
                        m_layerToViews.Insert(targetLayer, currentViews);
                        viewContext.uiView.__adjustOrder(targetLayer);
#if UNITY_EDITOR
                        EditorUtility.SetDirty(this);
#endif
                    }
                }
            }
        }
        public void BringViewToFront(int viewId)
        {
            BringViewToLayer(viewId, -1);
        }

        public void Pop(int layer = -1)
        {
            int targetLayer = layer >= 0 ? layer : m_layerToViews.Count + layer;
            
            if(m_layerToViews.Count > 0 
                && (targetLayer <= MinUnpoppableLayer))
            {
                Log.ML.Print("Pop Layer is bottom most, omit popping.");
                return;
            }

            if(targetLayer >=0 && targetLayer < m_layerToViews.Count)
            {
                var viewsOnLayer = m_layerToViews[targetLayer];
                if(viewsOnLayer.Count == 0)
                {
                    Log.ML.PrintWarning("Something wrong ? no view on Layer: " + targetLayer);
                    return;
                }
                var view = viewsOnLayer[viewsOnLayer.Count - 1];
                viewsOnLayer.RemoveAt(viewsOnLayer.Count - 1);

                int viewId = view.ViewId;
                readySet.Remove(viewId);
                if(pushQueue.Contains(viewId))
                {
                    var datas = pushQueue.ToArray();
                    pushQueue.Clear();
                    foreach (var d in datas)
                    {
                        if(d != viewId)
                        {
                            pushQueue.Enqueue(d);
                        }
                    }
                }
                if (view.DestroyWhenHide)
                {
                    m_idToViews.Remove(viewId);
                }
                view.Hide(view.HideAnimation);
                if(viewsOnLayer.Count > 0)
                {
                    view = viewsOnLayer[viewsOnLayer.Count - 1];
                    view.Show();
                }
                else
                {
                    m_layerToViews.RemoveAt(targetLayer);
                }
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        int findLayerForView(UIView view, out int subLayer)
        {
            subLayer = -1;
            for (int i = 0; i < m_layerToViews.Count; i++)
            {
                subLayer = m_layerToViews[i].FindIndex((v) =>
                {
                    return v.ViewId == view.ViewId;
                });
                if(subLayer != -1)
                {
                    return i;
                }
            }
            return -1;
        }

        ViewContext getViewContext(int viewId)
        {
            ViewContext viewContext;
            if (!m_idToViews.TryGetValue(viewId, out viewContext))
            {
                Log.ML.PrintWarning("Get View null, view has been disposed after push ?");
                return null;
            }
            return viewContext;
        }

        void innerPush(int viewId)
        {
            var viewContext = getViewContext(viewId);
            if(viewContext == null)
            {
                return;
            }

            UIView view = viewContext.uiView;

            List<UIView> viewsOfLayer = null;
            int defaultPushLayer = MinUnpoppableLayer + 1;
            if (view.OnNewLayer 
                || m_layerToViews.Count == 0 
                || defaultPushLayer > m_layerToViews.Count - 1)
            {
                viewsOfLayer = new List<UIView>();
                m_layerToViews.Add(viewsOfLayer);
            }
            else
            {
                //find view already on layer?
                //var sv = from layers in m_layerToViews
                //from v in layers
                //where view == v
                //select v;
                int sublayer;
                int layer = findLayerForView(view, out sublayer);
                if(layer != -1)
                {
                    viewsOfLayer = m_layerToViews[layer];
                    if(viewsOfLayer.Count - 1 == sublayer)
                    {
                        Log.ML.Print(true, "View {0}:{1} already on top.", view.ViewId, view.Name);
                        return;
                    }
                    else
                    {
                        viewsOfLayer.RemoveAt(sublayer);
                    }
                }

                //default push on last layer.
                int lastLayer = m_layerToViews.Count - 1;
                if (viewsOfLayer == null)
                {
                    viewsOfLayer = m_layerToViews[lastLayer];
                }
                int last = viewsOfLayer.Count - 1;
                if (last >= 0)
                {
                    UIView lastTopView = viewsOfLayer[last];
                    lastTopView.Hide(false);
                    if(!viewContext.traceback)
                    {
                        viewsOfLayer.RemoveAt(last);
                    }
                }
            }
            viewsOfLayer.Add(view);
            view.Show();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void Push(UIView view, bool traceback=false)
        {
            if (view == null)
            {
                Log.ML.PrintWarning("?? Push null UIView...");
                return;
            }
            if (!m_idToViews.ContainsKey(view.ViewId))
            {
                m_idToViews.Add(view.ViewId, new ViewContext() {
                    uiView = view,
                    traceback = traceback
                });
            }
            
            if (!pushQueue.Contains(view.ViewId) && !readySet.Contains(view.ViewId))
            {
                pushQueue.Enqueue(view.ViewId);
                m_assetCtrl.PrepareAssets(view.PackageName, view.ComponentName, () =>
                {
                    readySet.Add(view.ViewId);
                });
            }
            else
            {
                Log.ML.Print("Multiple Push On view: " + view.Name);
            }
        }

        public T GetView<T>() where T : UIView
        {
            var type = typeof(T);
            foreach (var kv in m_idToViews)
            {
                if(kv.Value.GetType() == type)
                {
                    return kv.Value.uiView as T;
                }
            }
            return null;
        }

        public UIView GetView(int viewId)
        {
            ViewContext view;
            if (m_idToViews.TryGetValue(viewId, out view))
            {
                return view.uiView;
            }
            else
            {
                return null;
            }
        }

    }
}
