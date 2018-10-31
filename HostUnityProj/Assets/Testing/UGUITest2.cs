using Ginkgo.UI;
using UnityEngine;
using UnityEngine.UI;

//test ui bind.
public class MyUIView : UIView
{
    [UIWidget]
    int a = 1;

    [UIWidget]
    static Button mybtn1;

    [UIWidget("mybtn2")]
    Button btn2;

    [UIWidget]
    Text mytext;

    //will be overwrite by field.
    [UIWidget]
    Button btn_Back
    {
        get
        {
            return btn2;
        }
        set
        {
            btn2 = value;
        }
    }

    protected override void onShowed()
    {
        base.onShowed();
    }

    protected override void onCreate()
    {
        base.onCreate();
        IsWindow = true;
    }

    MyUIViewModel viewModel;
    int i = 100;

    public override void Bind()
    {
        viewModel = ViewModel.Get<MyUIViewModel>(this);
        this.BindTextToProperty(mytext, viewModel.btn_LabelText);
        this.BindButtonToHandler(mybtn1, () =>
        {
            viewModel.btn_LabelText.Value = mybtn1.name ;
        });

        this.BindButtonToHandler(btn2, () =>
        {
            viewModel.btn_LabelText.Value = btn2.name;
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

public class UGUITest2 : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        UIView.RegAsset<MyUIView>("uipackage1", "uiPanel1");
        GUIManager.Selfie.Push<MyUIView>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
