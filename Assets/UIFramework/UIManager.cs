﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

namespace TinyUI
{
    #region define

    public enum UIType
    {
        /// <summary>
        /// 可推出界面(UIMainMenu,UIRank等)
        /// </summary>
        Normal,
        /// <summary>
        /// 可推出界面(UIMainMenu,UIRank等)
        /// </summary>
        Fixed,
        /// <summary>
        /// 模式窗口
        /// </summary>
        PopUp,
        /// <summary>
        /// 独立的窗口
        /// </summary>
        None,
    }

    public enum UIMode
    {
        DoNothing,
        /// <summary>
        /// 闭其他界面
        /// </summary>
        HideOther,
        /// <summary>
        /// 点击返回按钮关闭当前,不关闭其他界面(需要调整好层级关系)
        /// </summary>   
        NeedBack,
        /// <summary>
        ///  关闭TopBar,关闭其他界面,不加入backSequence队列
        /// </summary> 
        NoNeedBack,
    }

    public enum UICollider
    {
        /// <summary>
        /// 显示该界面不包含碰撞背景
        /// </summary>
        None,
        /// <summary>
        /// 碰撞透明背景
        /// </summary>  
        Normal,
        /// <summary>
        /// 碰撞非透明背景
        /// </summary>
        WithBg,
    }
    #endregion
    
    /// <summary>
    /// Each Page Mean one UI 'window'
    /// 3 steps:
    /// instance ui > refresh ui by data > show
    /// 
    /// by chiuan
    /// 2015-09
    /// </summary>
    public class UIManager
    {
        private const float Timeout = 10.0f;

        /// <summary>
        /// all pages with the union type
        /// </summary>
        private static readonly Dictionary<string, UIBase> AllPages = new Dictionary<string, UIBase>();

        /// <summary>
        /// control 1&gt;2&gt;3&gt;4&gt;5 each page close will back show the previus page.
        /// </summary>
        private static readonly List<UIBase> CurrentPageNodes = new List<UIBase>();


        private static IEnumerator LoadAndShow(string pageName, UIBase page, Action callback)
        {
            //1:Instance UI
            //FIX:support this is manager multi gameObject,instance by your self.
            if (string.IsNullOrEmpty(page.UIPath))
            {
                yield break;
            }

            yield return null;
            
            var go = (GameObject)Object.Instantiate(Resources.Load(page.UIPath));
            InitPage(page, go);

            AllPages[pageName] = page;
            
            if (callback != null)
            {
                callback();
            }
        }

        private static void InitPage(UIBase page, GameObject go)
        {
            AnchorUIGameObject(page, go);
            InjectorView.AutoInject(page);

            page.Awake();


            //:animation active.
            page.Active();

            //:refresh ui component.
            page.Refresh();

            //:popup this node to top if need back.
            PopNode(page);
        }


        private static void InjectUIBaseData(UIBase page, GameObject go)
        {
            var type = page.GetType().BaseType;
            type.GetField("_go", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(page, go);
            type.GetField("_tr", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(page, go.transform);
        }


        private static void AnchorUIGameObject(UIBase page, GameObject ui)
        {
            if (UIRoot.Instance == null || ui == null)
            {
                return;
            }

            InjectUIBaseData(page, ui);

            //check if this is ugui or (ngui)?
            Vector3 anchorPos;
            Vector3 scale;
            var sizeDel = Vector2.zero;
            var rect = ui.GetComponent<RectTransform>();
            if (rect != null)
            {
                anchorPos = rect.anchoredPosition;
                sizeDel = rect.sizeDelta;
                scale = rect.localScale;
            }
            else
            {
                anchorPos = ui.transform.localPosition;
                scale = ui.transform.localScale;
            }

            //Debug.Log("anchorPos:" + anchorPos + "|sizeDel:" + sizeDel);

            if (page.Type == UIType.Fixed)
            {
                page.Tr.SetParent(UIRoot.Instance.FixedRoot);
            }
            else if (page.Type == UIType.Normal)
            {
                page.Tr.SetParent(UIRoot.Instance.NormalRoot);
            }
            else if (page.Type == UIType.PopUp)
            {
                page.Tr.SetParent(UIRoot.Instance.PopupRoot);
            }

            if (rect != null)
            {
                rect.anchoredPosition = anchorPos;
                rect.sizeDelta = sizeDel;
                rect.localScale = scale;
            }
            else
            {
                page.Tr.localPosition = anchorPos;
                page.Tr.localScale = scale;
            }
        }


        #region static api

        private static bool CheckIfNeedBack(UIBase page)
        {
            if (page == null)
            {
                return false;
            }

            if (page.Type == UIType.Fixed || page.Type == UIType.PopUp || page.Type == UIType.None)
            {
                return false;
            }
            else if (page.Mode == UIMode.NoNeedBack || page.Mode == UIMode.DoNothing)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// make the target node to the top.
        /// </summary>
        private static void PopNode(UIBase page)
        {
            if (page == null)
            {
                Debug.LogError("[UI] page popup is null.");
                return;
            }

            //sub pages should not need back.
            if (CheckIfNeedBack(page) == false)
            {
                return;
            }

            var isFound = false;
            for (int i = 0; i < CurrentPageNodes.Count; i++)
            {
                if (CurrentPageNodes[i].Equals(page))
                {
                    CurrentPageNodes.RemoveAt(i);
                    CurrentPageNodes.Add(page);
                    isFound = true;
                    break;
                }
            }

            //if dont found in old nodes
            //should add in nodelist.
            if (!isFound)
            {
                CurrentPageNodes.Add(page);
            }

            //after pop should hide the old node if need.
            HideOldNodes();
        }

        private static void HideOldNodes()
        {
            if (CurrentPageNodes.Count < 0)
            {
                return;
            }

            var index = CurrentPageNodes.Count - 1;
            var topPage = CurrentPageNodes[index];
            if (topPage.Mode == UIMode.HideOther)
            {
                //form bottm to top.
                for (int i = CurrentPageNodes.Count - 2; i >= 0; i--)
                {
                    if (CurrentPageNodes[i].IsActive)
                    {
                        CurrentPageNodes[i].Hide();
                    }
                }
            }
        }

        public static void ClearNodes()
        {
            CurrentPageNodes.Clear();
        }

        private static void ShowPage(string pageName, UIBase pageInstance, Action callback, bool isLoad)
        {
            if (isLoad)
            {
                Show(pageInstance, callback);
            }
            else
            {
                UIRoot.Instance.StartCoroutine(LoadAndShow(pageName, pageInstance, callback));
            }
        }

        private static void Show(UIBase page, Action callback)
        {
            //:animation active.
            page.Active();

            //:refresh ui component.
            page.Refresh();

            //:popup this node to top if need back.
            PopNode(page);

            if (callback != null)
            {
                callback();
            }
        }

        /// <summary>
        /// Sync Show Page
        /// </summary>
        public static void ShowPage<T>() where T : UIBase, new()
        {
            ShowPage<T>(null);
        }


        public static void ShowPage<T>(Action callback) where T : UIBase, new()
        {
            var t = typeof(T);
            var pageName = t.ToString();
            UIBase page;
            if (AllPages != null && AllPages.TryGetValue(pageName, out page))
            {
                ShowPage(pageName, page, callback, isLoad: true);
            }
            else
            {
                page = new T();
                ShowPage(pageName, page, callback, isLoad: false);
            }
        }


        /// <summary>
        /// close current page in the "top" node.
        /// </summary>
        public static void ClosePage()
        {
            //Debug.Log("Back&Close PageNodes Count:" + _currentPageNodes.Count);

            if (CurrentPageNodes == null || CurrentPageNodes.Count <= 1)
            {
                return;
            }

            var index = CurrentPageNodes.Count - 1;
            var closePage = CurrentPageNodes[index];
            CurrentPageNodes.RemoveAt(index);

            //show older page.
            //TODO:Sub pages.belong to root node.
            if (CurrentPageNodes.Count > 0)
            {
                index = CurrentPageNodes.Count - 1;
                var page = CurrentPageNodes[index];
                ShowPage(page.Name, page, closePage.Hide, isLoad: true);
            }
        }

        /// <summary>
        /// Close target page
        /// </summary>
        public static void ClosePage(UIBase target)
        {
            if (target == null || CurrentPageNodes == null)
            {
                return;
            }
            if (!target.IsActive)
            {
                for (int i = 0; i < CurrentPageNodes.Count; i++)
                {
                    if (CurrentPageNodes[i] == target)
                    {
                        CurrentPageNodes.RemoveAt(i);
                        break;
                    }
                }
                return;
            }

            var index = CurrentPageNodes.Count - 1;
            if (CurrentPageNodes != null && CurrentPageNodes.Count >= 1 && CurrentPageNodes[index] == target)
            {
                CurrentPageNodes.RemoveAt(index);
                //show older page.
                //TODO:Sub pages.belong to root node.
                if (CurrentPageNodes.Count > 0)
                {
                    index = CurrentPageNodes.Count - 1;
                    UIBase page = CurrentPageNodes[index];
                    ShowPage(page.Name, page, target.Hide, isLoad: true);
                }
            }
            else if (CheckIfNeedBack(target))
            {
                for (int i = 0; i < CurrentPageNodes.Count; i++)
                {
                    if (CurrentPageNodes[i] == target)
                    {
                        CurrentPageNodes.RemoveAt(i);
                        target.Hide();
                        break;
                    }
                }
            }
        }

        public static void ClosePage<T>() where T : UIBase
        {
            var t = typeof(T);
            var pageName = t.ToString();
            UIBase page;
            if (AllPages != null && AllPages.TryGetValue(pageName, out page))
            {
                ClosePage(page);
            }
            else
            {
                Debug.LogError(pageName + "haven't shown yet!");
            }
        }

        public static void ClosePage(string pageName)
        {
            UIBase page;
            if (AllPages != null && AllPages.TryGetValue(pageName, out page))
            {
                ClosePage(page);
            }
            else
            {
                Debug.LogError(pageName + " haven't shown yet!");
            }
        }

        public static void Destroy(string pageName)
        {
            UIBase page;
            if (AllPages != null && AllPages.TryGetValue(pageName, out page))
            {
                AllPages.Remove(pageName);
                CurrentPageNodes.Remove(page);
                Object.Destroy(page.Go);
            }
            else
            {
                Debug.LogError(pageName + " haven't shown yet");
            }
        }

        #endregion
    }
}