using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Ginkgo.UI
{
    public class URoot : MLSingleton<URoot>
    {
        public Canvas CanvasRoot { get; private set; }
        public CanvasScaler CanvasScaler { get; private set; }
        //ToDO:
        public Camera UICamera { get; set; }

        public Transform m_maskObject;

        public Transform MaskObject
        {
            get
            {
                return m_maskObject;
            }
            set
            {
                if(m_maskObject != null)
                {
                    Destroy(m_maskObject.gameObject);
                }
                m_maskObject = value;
            }
        }

        //Canvas _activeCanvas;
        //public Canvas ActiveCanvas
        //{
        //    get
        //    {
        //        return _activeCanvas;
        //    }
        //}

        private Transform m_rootTransform;

        private List<GameObject> mExtraCanvas = new List<GameObject>();

        public void CheckCreateMask()
        {
            if(m_maskObject != null)
            {
                return;
            }
            GameObject mask = new GameObject("__BlackMask");
            var image = mask.AddComponent<RawImage>();
            image.color = new Color(0, 0, 0, 0.39f);
            RectTransform t = mask.GetComponent<RectTransform>();
            if(t != null)
            {
                m_maskObject = t;
                t.anchorMin = Vector2.zero;
                t.anchorMax = Vector2.one;
                t.offsetMin = t.offsetMax = Vector2.zero;
                t.anchoredPosition = Vector2.zero;
                t.SetParent(m_rootTransform, false);
            }
        }

        public void AdjustMask(bool enable)
        {
            int lastSiblingIndex = m_rootTransform.childCount - 1;
            if (enable)
            {
                m_maskObject.SetSiblingIndex(lastSiblingIndex - 1);
            }
            if (enable != m_maskObject.gameObject.activeSelf)
            {
                m_maskObject.gameObject.SetActive(enable);
            }
        }

        public void AdjustHierachy(Transform transform, int index)
        {
            if(index < 0)
            {
                Log.Common.PrintWarning("URoot AdjustHierachy erro index: {0}", index);
                return;
            }

            int finalSiblingIndex = index;
            foreach(var obj in mExtraCanvas)
            {
                Transform trans = obj.transform;
                if(trans.childCount == 0 && index >= trans.GetSiblingIndex())
                {
                    finalSiblingIndex++;
                }
            }

            if(m_maskObject && index >= m_maskObject.GetSiblingIndex())
            {
                finalSiblingIndex++;
            }
            
            int max = m_rootTransform.childCount;
            if(finalSiblingIndex > max)
            {
                Log.Common.PrintWarning("Something wrong, finalSiblingIndex calculate error. ");
                finalSiblingIndex = max;
            }
            transform.SetSiblingIndex(finalSiblingIndex);
        }

        public void RemoveChild(Transform transform, bool dispose)
        {
            if (transform)
            {
                if (dispose)
                {
                    Destroy(transform.gameObject);
                }
                else
                {
                    transform.SetParent(null, false);
                }
            }
        }

        public void AddChild(Transform transform, bool onNewCanvas)
        {
            if (transform)
            {
                Transform rootTrans = m_rootTransform;
                if (onNewCanvas)
                {
                    bool findAvail = false;
                    foreach(var canvasObj in mExtraCanvas)
                    {
                        Transform trans = canvasObj.transform;
                        if (trans.childCount == 0)
                        {
                            trans.SetAsLastSibling();
                            findAvail = true;
                            rootTrans = trans;
                            break;
                        }
                    }
                    if (!findAvail)
                    {
                        GameObject go = new GameObject("Canvas");
                        mExtraCanvas.Add(go);
                        go.AddComponent<Canvas>();
                        rootTrans = go.transform;
                    }

                    rootTrans.SetParent(m_rootTransform, false);
                }
                transform.SetParent(rootTrans, false);
            }
        }

        protected override void onInitialize()
        {
            var go = GameObject.FindGameObjectWithTag("CanvasRoot");
            if (go)
            {
                CanvasRoot = go.GetComponent<Canvas>();
                CanvasScaler = go.GetComponent<CanvasScaler>();
                m_rootTransform = go.transform;
            }
        }
    }
}
