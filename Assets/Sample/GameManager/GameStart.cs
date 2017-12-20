using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TinyUI;

public class GameStart : MonoBehaviour
{
    private void Awake()
    {
        UIManager.ShowPage<UITopBar>();
        UIManager.ShowPage<UIMainPage>();
        Destroy(gameObject);
    }
}
