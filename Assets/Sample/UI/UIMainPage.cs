using UnityEngine;
using System.Collections;
using Tiny.UI;
using UnityEngine.UI;

public class UIMainPage : UIPage
{

    public UIMainPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None)
    {
        UIPath = "UIPrefab/UIMain";
    }

    protected override void Awake()
    {
        CacheTransform.Find("btn_skill").GetComponent<Button>().onClick.AddListener(ShowPage<UISkillPage>);
        CacheTransform.Find("btn_battle").GetComponent<Button>().onClick.AddListener(ShowPage<UIBattle>);
    }
}
