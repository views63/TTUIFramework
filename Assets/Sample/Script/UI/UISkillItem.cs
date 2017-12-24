﻿using TinyUI;
using UnityEngine;
using UnityEngine.UI;

public class UISkillItem : Block
{

    public UDSkill.Skill data = null;

    [UIPath("title")]
    private Text title;

    public UISkillItem(Transform tr)
    {
        Init(this, tr);
    }

    public void Refresh(UDSkill.Skill skill)
    {
        data = skill;
        title.text = string.Format("{0}[lv.{1}]", skill.name, skill.level);
    }
}
