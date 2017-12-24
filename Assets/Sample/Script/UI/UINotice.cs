using TinyUI;
using UnityEngine.UI;

public class UINotice : UIBase
{
    public UINotice() : base(UIType.PopUp, UIMode.DoNothing)
    {
        UIPath = "UIPrefab/Notice";
    }

    public override void Awake()
    {
        Tr.AddListener("content/btn_confim", Hide);
    }
}
