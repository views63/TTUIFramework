﻿using TinyUI;

public class UITopBar : UIBase
{
    public UITopBar() : base(UIType.Fixed, UIMode.DoNothing)
    {
        UIPath = "UIPrefab/Topbar";
    }

    public override void Awake()
    {
        Tr.AddListener("btn_back", UIManager.ClosePage);
        Tr.AddListener("btn_notice", UIManager.ShowPage<UINotice>);
    }
}
