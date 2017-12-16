﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TinyUI;

public class GameMain : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        UIManager.ShowPage<UITopBar>();
        UIManager.ShowPage<UIMainPage>();
    }
}
