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
        Tr.Find("btn_skill").GetComponent<Button>().onClick.AddListener(ShowPage<UISkillPage>);
        Tr.Find("btn_battle").GetComponent<Button>().onClick.AddListener(ShowPage<UIBattle>);
    }
}
