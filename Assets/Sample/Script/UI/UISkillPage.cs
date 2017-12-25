using UnityEngine;
using System.Collections.Generic;
using TinyUI;
using UnityEngine.UI;
using DG.Tweening;

public class UISkillPage : UIBase
{
    [UIPath("list")] 
    GameObject skillList;

    [UIPath("desc")] 
    GameObject skillDesc;

    [UIPath("list/Viewport/Content/item")] 
    GameObject skillItem;
    
    [UIPath("list/Viewport/Content")] 
    Transform grid;

    [UIPath("desc/content")] 
    Text content;


    UISkillItem currentItem = null;
    private UDSkill Data;

    public UISkillPage() : base(UIType.Normal, UIMode.HideOther)
    {
        UIPath = "Assets.Sample.UIPrefab.UISkill.prefab";
    }

    public override void Awake()
    {
        Tr.AddListener("desc/btn_upgrade", OnClickUpgrade);
        skillItem.SetActive(false);
    }

    public override void Refresh()
    {
        //default desc deactive
        skillDesc.SetActive(false);


        //Get Skill Data.
        //NOTE:here,maybe you havent Show(...pageData),ofcause you can got your skill data from your data singleton
        if (Data == null)
        {
            Data = GameData.Instance.playerSkill;
            //create skill items in list.
            foreach (var skill in Data.skills)
            {
                CreateSkillItem(skill, grid);
            }
        }


        grid.localPosition = Vector3.zero;
        grid.localScale = Vector3.zero;
        grid.DOScale(Vector3.one, 0.5f).Play();
    }

    public override void Hide()
    {
        Go.SetActive(false);
    }

    #region this page logic

    private void CreateSkillItem(UDSkill.Skill skill, Transform parent)
    {
        var go = Object.Instantiate(skillItem);
        go.transform.SetParent(parent);
        go.transform.localScale = Vector3.one;
        go.SetActive(true);

        UISkillItem item = new UISkillItem(go.transform);
        item.Refresh(skill);

        //add click btn
        go.AddComponent<Button>().AddListener(() => ShowDesc(item));
    }

    private void ShowDesc(UISkillItem skill)
    {
        currentItem = skill;
        skillDesc.SetActive(true);
        var pos = skillDesc.transform.localPosition;
        skillDesc.transform.localPosition = new Vector3(300f, pos.y, pos.z);
        skillDesc.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-289.28f, -44.05f), 0.25f, true);

        RefreshDesc(skill);
    }

    private void RefreshDesc(UISkillItem skill)
    {
        content.text = string.Format("{0}\n名称:{1}\n等级:{2}", skill.data.desc, skill.data.name, skill.data.level);
    }

    private void OnClickUpgrade()
    {
        currentItem.data.level++;
        currentItem.Refresh(currentItem.data);
        RefreshDesc(currentItem);
    }

    #endregion
}