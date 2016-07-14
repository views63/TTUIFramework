using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Tiny.UI;

public class GameMain : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        UIPage.ShowPage<UITopBar>();
        UIPage.ShowPage<UIMainPage>();
    }
}
