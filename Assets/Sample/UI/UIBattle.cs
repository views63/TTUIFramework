using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tiny.UI;
using UnityEngine.UI;

public class UIBattle : UIPage
{

    public UIBattle() : base(UIType.Normal, UIMode.HideOther, UICollider.None)
    {
        UIPath = "UIPrefab/UIBattle";
    }

    protected override void Awake()
    {
        this.CacheTransform.Find("btn_skill").GetComponent<Button>().onClick.AddListener(OnClickSkillGo);
        this.CacheTransform.Find("btn_battle").GetComponent<Button>().onClick.AddListener(OnClickGoBattle);
    }

    private void OnClickSkillGo()
    {
        //goto skill upgrade page.
        ShowPage<UISkillPage>();
    }

    private void OnClickGoBattle()
    {
        Debug.Log("should load your battle scene!");
    }
}
