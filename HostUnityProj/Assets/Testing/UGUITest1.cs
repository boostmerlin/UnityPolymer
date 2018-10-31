using Ginkgo.UI;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UGUITest1 : MonoBehaviour
{
    UIView m_view;
    // Use this for initialization
    void Start()
    {
        UIView.RegAsset<UIView>("uipackage1", "bgPanel");
        //GUIManager.Selfie.MinUnpoppableLayer = GUIManager.POP_ALL;
        m_view = GUIManager.Selfie.Push<UIView>();
        GUIManager.Selfie.Pop();
        //m_view.FillScreen = true;
        //m_view = GUIManager.Selfie.Push<UIView>(true);
        UIView view = UIView.CreateView("uipackage1", "uiPanel1");
        view.DestroyWhenHide = true;
        GUIManager.Selfie.Push(view, true);
        view = UIView.CreateView("uipackage1", "uiPanel2");
        view.OnNewLayer = true;
        GUIManager.Selfie.Push(view, true);
        view = UIView.CreateView("uipackage1", "uiPanel3");
        view.OnNewLayer = true;
        m_view = view;
        view.OnNewCanvas = true;
        view.IsWindow = true;
        GUIManager.Selfie.Push(view, true);

        //view = UIView.CreateView("uipackage1", "uiPanel2");
        //view.IsWindow = true;
        //view.DestroyWhenHide = true;
        //GUIManager.Selfie.Push(view, true);

        Observable.IntervalFrame(200).Subscribe((v) =>
        {
            GUIManager.Selfie.Pop();
        });
    }

    private void OnGUI()
    {
        if (GUILayout.Button("add previous view"))
        {
            GUIManager.Selfie.Push(m_view);
        }

        if (GUILayout.Button("add new view"))
        {
            UIView view = UIView.CreateView("uipackage1", "uiPanel1");
            GUIManager.Selfie.Push(view);
        }

        if (GUILayout.Button("Clean"))
        {
            GUIManager.Selfie.Clean();
        }
    }
}
