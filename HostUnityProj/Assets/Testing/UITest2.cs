using ML.UI;
using UnityEngine;
using FairyGUI;

public class MyUIView : UIView
{
    [UIWidget]
    int a = 1;

    void test()
    {

    }

    [UIWidget]
    static GButton btn4;

    [UIWidget("btn_Button")]
    GButton btn1;

    //[UIWidget]
    //public GButton btn5;

    //[UIWidget("btn_Label")]
    //public GButton btn2 { get; set; }

    //will be overwrite by field.
    [UIWidget]
    GButton btn_Back
    {
        get
        {
            return btn1;
        }
        set
        {
            btn1 = value;
        }
        
    }

    protected override void onShowed()
    {
        base.onShowed();
    }

    MyUIViewModel viewModel;
    int i = 100;

    public override void Bind()
    {
        viewModel = ViewModel.Get<MyUIViewModel>(this);
        this.BindGButtonTextToProperty(btn_Back, viewModel.btn_LabelText);
        this.BindGButtonToHandler(btn_Back, ()=>
        {
            Debug.Log("btn back clickeddd..");
            viewModel.btn_LabelText.Value = i++.ToString();
            //d.Dispose();
        });
    }

    public override void UnBind()
    {
        base.UnBind();
    }
}

public class MyUIViewModel : ViewModel
{
    public P<string> btn_LabelText;

    protected override void OnCreate()
    {
        btn_LabelText = P<string>.New(this);
    }

    public override void OnPropertyChanged(object sender, string propertyName)
    {
        base.OnPropertyChanged(sender, propertyName);
        Debug.Log("property changed: " + ((IProperty)sender).ObjectValue);
    }
}

public class UITest2 : MonoBehaviour {

	// Use this for initialization
	void Start () {
        UIView.RegAsset<MyUIView>("Basics", "Main");

        GUIManager.Selfie.Push<MyUIView>();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
