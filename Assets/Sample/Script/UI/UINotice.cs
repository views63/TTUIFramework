using TinyUI;
using UnityEngine.UI;

public class UINotice : UIBase
{
    public UINotice() : base(UIType.PopUp, UIMode.DoNothing)
    {
        UIPath = "Assets.Sample.UIPrefab.Notice.prefab";
    }

    public override void Awake()
    {
        Tr.AddListener("content/btn_confim", Hide);
    }
}
