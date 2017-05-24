using Tiny.UI;
using UnityEngine.UI;

public class UITopBar : UIBase
{
    public UITopBar() : base(UIType.Fixed, UIMode.DoNothing, UICollider.None)
    {
        UIPath = "UIPrefab/Topbar";
    }

    public override void Awake()
    {
        Tr.Find("btn_back").GetComponent<Button>().onClick.AddListener(UIManager.ClosePage);
        Tr.Find("btn_notice").GetComponent<Button>().onClick.AddListener(UIManager.ShowPage<UINotice>);
    }
}
