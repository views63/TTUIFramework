using TinyUI;
using UnityEngine.UI;

public class UINotice : UIBase
{
    public UINotice() : base(UIType.PopUp, UIMode.DoNothing, UICollider.Normal)
    {
        UIPath = "UIPrefab/Notice";
    }

    public override void Awake()
    {
        Tr.AddListener("content/btn_confim", Hide);
    }
}
