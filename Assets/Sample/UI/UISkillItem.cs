using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UISkillItem : MonoBehaviour {

    public UDSkill.Skill data = null;

    public void Refresh(UDSkill.Skill skill)
    {
        data = skill;
        transform.Find("title").GetComponent<Text>().text = skill.name + "[lv." + skill.level + "]";
    }
}
