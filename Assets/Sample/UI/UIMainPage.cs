﻿using Tiny.UI;

public class UIMainPage : UIBase
{

    public UIMainPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None)
    {
        UIPath = "UIPrefab/UIMain";
    }

    public override void Awake()
    {
        Tr.AddListener("btn_skill", UIManager.ShowPage<UISkillPage>);
        Tr.AddListener("btn_battle", UIManager.ShowPage<UIBattle>);
    }
}
