using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tiny.UI;
using UnityEngine.UI;
using DG.Tweening;

public class UISkillPage : UIPage {

    GameObject skillList = null;
    GameObject skillDesc = null;
    GameObject skillItem = null;
    List<UISkillItem> skillItems = new List<UISkillItem>();
    UISkillItem currentItem = null;

    public UISkillPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None)
    {
        UIPath = "UIPrefab/UISkill";
    }

    protected override void Awake()
    {
        skillList = this.CacheTransform.Find("list").gameObject;
        skillDesc = this.CacheTransform.Find("desc").gameObject;
        skillDesc.transform.Find("btn_upgrade").GetComponent<Button>().onClick.AddListener(OnClickUpgrade);

        skillItem = this.CacheTransform.Find("list/Viewport/Content/item").gameObject;
        skillItem.SetActive(false);
    }

    public override void Refresh()
    {
        //default desc deactive
        skillDesc.SetActive(false);

        //Get Skill Data.
        //NOTE:here,maybe you havent Show(...pageData),ofcause you can got your skill data from your data singleton
        UDSkill skillData = Data != null ? Data as UDSkill : GameData.Instance.playerSkill;
        
        //create skill items in list.
        for(int i=0;i< skillData.skills.Count; i++)
        {
            CreateSkillItem(skillData.skills[i]);
        }

        skillList.transform.localScale = Vector3.zero;
        skillList.transform.DOScale(Vector3.one, 0.5f).Play();
    }

    public override void Hide()
    {
        for (int i = 0; i < skillItems.Count; i++)
        {
            Object.Destroy(skillItems[i].gameObject);
        }
        skillItems.Clear();
        CacheGameObject.SetActive(false);
        //Destroy();
    }

    #region this page logic

    private void CreateSkillItem(UDSkill.Skill skill)
    {
        var go = Object.Instantiate(skillItem);
        go.transform.SetParent(skillItem.transform.parent);
        go.transform.localScale = Vector3.one;
        go.SetActive(true);

        UISkillItem item = go.AddComponent<UISkillItem>();
        item.Refresh(skill);
        skillItems.Add(item);

        //add click btn
        go.AddComponent<Button>().onClick.AddListener(OnClickSkillItem);
    }

    private void OnClickSkillItem()
    {
        UISkillItem item = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<UISkillItem>();
        ShowDesc(item);
    }

    private void ShowDesc(UISkillItem skill)
    {
        currentItem = skill;
        skillDesc.SetActive(true);
        skillDesc.transform.localPosition = new Vector3(300f, skillDesc.transform.localPosition.y, skillDesc.transform.localPosition.z);
        skillDesc.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-289.28f, -44.05f), 0.25f, true);

        RefreshDesc(skill);
    }

    private void RefreshDesc(UISkillItem skill)
    {
        skillDesc.transform.Find("content").GetComponent<Text>().text = skill.data.desc + "\n名称:" + skill.data.name + "\n等级:" + skill.data.level;
    }

    private void OnClickUpgrade()
    {
        currentItem.data.level++;
        currentItem.Refresh(currentItem.data);
        RefreshDesc(currentItem);
    }

    #endregion

}
