using UnityEngine;
using TinyUI;

public class UIBattle : UIBase
{

    public UIBattle() : base(UIType.Normal, UIMode.HideOther, UICollider.None)
    {
        UIPath = "UIPrefab/UIBattle";
    }

    public override void Awake()
    {
        Tr.AddListener("btn_skill", OnClickSkillGo);
        Tr.AddListener("btn_battle", OnClickGoBattle);
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
