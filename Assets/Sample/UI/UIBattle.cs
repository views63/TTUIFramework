﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tiny.UI;
using UnityEngine.UI;

public class UIBattle : UIBase
{

    public UIBattle() : base(UIType.Normal, UIMode.HideOther, UICollider.None)
    {
        UIPath = "UIPrefab/UIBattle";
    }

    public override void Awake()
    {
        Tr.Find("btn_skill").GetComponent<Button>().onClick.AddListener(OnClickSkillGo);
        Tr.Find("btn_battle").GetComponent<Button>().onClick.AddListener(OnClickGoBattle);
    }

    private void OnClickSkillGo()
    {
        //goto skill upgrade page.
        UIManager.ShowPage<UISkillPage>();
    }

    private void OnClickGoBattle()
    {
        Debug.Log("should load your battle scene!");
    }
}
