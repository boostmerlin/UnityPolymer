using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Ginkgo.UI;
using FairyGUI;
using Ginkgo;
using UniRx;

public class FGUITest1 : MonoBehaviour {

    UIView m_view;

	// Use this for initialization
	void Start ()
    {
        #region guimanager
        UIView.RegAsset<UIView>("Basics", "Main");
        UIView view = UIView.CreateView("Bag", "BagWin");
        view.IsWindow = true;
        view.OnNewLayer = true;
        //  view.Modal = true;
        //  GUIManager.Self.MinUnpopLayer = -1;
        m_view = GUIManager.Selfie.Push<UIView>();
        m_view.FillScreen = true;
        //   m_view = GUIManager.Self.Push<UIView>(true);
        //  m_view = GUIManager.Self.Push<UIView>(true);
        GUIManager.Selfie.Push(view, true);

        view = UIView.CreateView("Basics", "WindowA");
        view.IsWindow = true;
        view.DestroyWhenHide = true;
        //view.OnNewLayer = true;
        //view.FillScreen = true;
        //  view.Modal = true;
        GUIManager.Selfie.Push(view, true);

        view = UIView.CreateView("Basics", "WindowB");
        view.IsWindow = true;
        view.DestroyWhenHide = true;
      //  view.OnNewLayer = true;
        view.Modal = true;
        GUIManager.Selfie.Push(view, true);
        m_view = view;

        Observable.IntervalFrame(200).Subscribe((v) =>
        {
            GUIManager.Selfie.Pop();
            int n = GRoot.inst.numChildren;
            Debug.Log(n);
            for (int i = 0; i < n; i++)
            {
                Debug.Log(GRoot.inst.GetChildAt(i).displayObject.gameObject.name);
            }
        });
        #endregion

    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnGUI()
    {
        if(GUILayout.Button("Click Me"))
        {
            GUIManager.Selfie.Push(m_view);
        }
    }
}
