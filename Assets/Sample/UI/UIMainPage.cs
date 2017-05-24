using UnityEngine;
using System.Collections;
using Tiny.UI;
using UnityEngine.UI;

public class UIMainPage : UIBase
{

    public UIMainPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None)
    {
        UIPath = "UIPrefab/UIMain";
    }

    public override void Awake()
    {
        Tr.Find("btn_skill").GetComponent<Button>().onClick.AddListener(UIManager.ShowPage<UISkillPage>);
        Tr.Find("btn_battle").GetComponent<Button>().onClick.AddListener(UIManager.ShowPage<UIBattle>);
    }
}
