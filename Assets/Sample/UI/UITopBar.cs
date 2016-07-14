using UnityEngine;
using System.Collections;
using Tiny.UI;
using UnityEngine.UI;

public class UITopBar : UIPage
{
    public UITopBar() : base(UIType.Fixed, UIMode.DoNothing, UICollider.None)
    {
        UIPath = "UIPrefab/Topbar";
    }

    protected override void Awake()
    {
        CacheTransform.Find("btn_back").GetComponent<Button>().onClick.AddListener(ClosePage);
        CacheTransform.Find("btn_notice").GetComponent<Button>().onClick.AddListener(ShowPage<UINotice>);
    }
}
