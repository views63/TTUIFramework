using UnityEngine;
using System.Collections;
using Tiny.UI;
using UnityEngine.UI;

public class UINotice : UIPage
{
    public UINotice() : base(UIType.PopUp, UIMode.DoNothing, UICollider.Normal)
    {
        UIPath = "UIPrefab/Notice";
    }

    protected override void Awake()
    {
        Tr.Find("content/btn_confim").GetComponent<Button>().onClick.AddListener(Hide);
    }
}
