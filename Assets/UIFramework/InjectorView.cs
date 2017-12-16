#region

using System;
using System.Reflection;
using TinyUI;
using UnityEngine;

#endregion

public class InjectorView
{
    private static readonly Type TypeGameObject;
    private static readonly Type TypeComponent;

    static InjectorView()
    {
        TypeGameObject = typeof(GameObject);
        TypeComponent = typeof(Component);
    }

    public static void AutoInject(UIBase view)
    {
        var tr = view.Tr;
        var allViewFields = view.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        AutoInject(view, allViewFields, tr);
    }

    public static void AutoInject(Block block)
    {
        var tr = block.Tr;
        var allViewFields = block.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        AutoInject(block, allViewFields, tr);
    }

    private static void AutoInject(object obj, FieldInfo[] allViewFields, Transform tr)
    {
        foreach (var viewfield in allViewFields)
        {
            var injections = viewfield.GetCustomAttributes(typeof(UIPath), true) as UIPath[];
            if (injections != null && injections.Length > 0)
            {
                var uiPath = injections[0].Path;
                var target = tr.Find(uiPath);
                if (target == null)
                {
                    Debug.LogWarning("the UI component injection failed for this attribute. check the UI component is set correctly: " + uiPath);
                    continue;
                }

                var fieldType = viewfield.FieldType;
                var go = target.gameObject;
                if (fieldType == TypeGameObject)
                {
                    viewfield.SetValue(obj, go);
                }
                else if (fieldType == TypeComponent || fieldType.IsSubclassOf(TypeComponent))
                {
                    viewfield.SetValue(obj, go.GetComponent(fieldType) ?? go.AddComponent(fieldType));
                }
            }
        }
    }
}