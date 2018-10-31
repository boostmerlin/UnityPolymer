using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRectTransform : MonoBehaviour
{
    RectTransform rectTransform;

    public Vector2 offsetMin;
    public Vector2 offsetMax;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 pivot;
    //public Vector3 localPosition;
    public Vector2 anchoredPosition;
    public Vector2 sizeDelta;

    void Log()
    {
        if (rectTransform == null) return;

        Debug.Log("ChildCount: " + transform.childCount);

        Debug.LogFormat("rect [readonly]: {0}", rectTransform.rect.ToString());
        Debug.LogFormat("position: {0}", rectTransform.position.ToString());

        Debug.LogFormat("AnchorMin: {0}", rectTransform.anchorMin.ToString());
        Debug.LogFormat("AnchorMax: {0}", rectTransform.anchorMax.ToString());
        Debug.LogFormat("Pivot: {0}", rectTransform.pivot.ToString());
        Debug.LogFormat("localPosition: {0}", rectTransform.localPosition.ToString());
        Debug.LogFormat("offsetMin: {0}", rectTransform.offsetMin.ToString());
        Debug.LogFormat("offsetMax: {0}", rectTransform.offsetMax.ToString());
        Debug.LogFormat("sizeDelta: {0}", rectTransform.sizeDelta.ToString());
        Debug.LogFormat("anchoredPosition: {0}", rectTransform.anchoredPosition.ToString());
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;

        sizeDelta = rectTransform.sizeDelta;

        offsetMin = rectTransform.offsetMin;
        offsetMax = rectTransform.offsetMax;

        anchorMin = rectTransform.anchorMin;
        anchorMax = rectTransform.anchorMax;
        pivot = rectTransform.pivot;
        //localPosition = rectTransform.localPosition;
        anchoredPosition = rectTransform.anchoredPosition;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("LogInfo"))
        {
            Log();
        }
    }

    private void Update()
    {
        if (rectTransform == null) return;
        if (rectTransform.sizeDelta != sizeDelta)
        {
            rectTransform.sizeDelta = sizeDelta;
            offsetMin = rectTransform.offsetMin;
            offsetMax = rectTransform.offsetMax;
        }
        else
        {
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
            sizeDelta = rectTransform.sizeDelta;
        }

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
       // rectTransform.localPosition = localPosition;
        rectTransform.anchoredPosition = anchoredPosition;
    }
}
