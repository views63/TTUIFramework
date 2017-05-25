using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class ExtendMethod
{
    public static void AddListener(this Button btn, UnityAction act)
    {
        btn.onClick.AddListener(act); 
    }

    public static void AddListener(this InputField input, UnityAction<string> act)
    {
        input.onEndEdit.AddListener(act); 
    }

    public static void AddListener(this Transform tr, string path, UnityAction action)
    {
        tr.Find(path).GetComponent<Button>().AddListener(action);
    }
}