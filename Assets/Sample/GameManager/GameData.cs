using System.Collections.Generic;

public class GameData
{
    private static GameData m_instance;

    public static GameData Instance
    {
        get { return m_instance ?? (m_instance = new GameData()); }
    }

    private GameData()
    {
        //NOTE : this is Test Init Here.
        playerSkill = new UDSkill();
        playerSkill.skills = new List<UDSkill.Skill>();

        for (int i = 0; i < 50; i++)
        {
            var skill = new UDSkill.Skill {
                                              name = "skill_" + i,
                                              level = 1,
                                              desc = "这是个牛逼的技能"
                                          };
            playerSkill.skills.Add(skill);
        }
    }

    public UDSkill playerSkill;
}