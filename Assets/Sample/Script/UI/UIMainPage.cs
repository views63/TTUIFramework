using TinyUI;

public class UIMainPage : UIBase
{

    public UIMainPage() : base(UIType.Normal, UIMode.HideOther)
    {
        UIPath = "UIPrefab/UIMain";
    }

    public override void Awake()
    {
        Tr.AddListener("btn_skill", UIManager.ShowPage<UISkillPage>);
        Tr.AddListener("btn_battle", UIManager.ShowPage<UIBattle>);
    }
}
