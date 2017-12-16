using System;
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
        ///sync load ui function
        /// </summary>
        private static Func<string, Object> _syncLoadFunc = Resources.Load;

        /// <summary>
        /// async load ui function
        /// </summary>
        private static Action<string, Action<Object>> _asyncLoadFunc = null;

        /// <summary>
        /// all pages with the union type
        /// </summary>
        private static Dictionary<string, UIBase> _allPages;

        /// <summary>
        /// control 1&gt;2&gt;3&gt;4&gt;5 each page close will back show the previus page.
        /// </summary>
        private static List<UIBase> _currentPageNodes;

        /// <summary>
        /// Sync Show UI Logic
        /// </summary>
        private static void Show(UIBase page)
        {
            //1:instance UI
            if (page.Go == null && string.IsNullOrEmpty(page.UIPath) == false)
            {
                GameObject go;
                if (_syncLoadFunc != null)
                {
                    var o = _syncLoadFunc(page.UIPath);
                    go = (o != null) ? Object.Instantiate(o) as GameObject : null;
                }
                else
                {
                    go = Object.Instantiate(Resources.Load(page.UIPath)) as GameObject;
                }

                //protected.
                if (go == null)
                {
                    Debug.LogError("[UI] Cant sync load your ui prefab.");
                    return;
                }

                AnchorUIGameObject(page, go);
                InjectorView.AutoInject(page);

                //after instance should awake init.
                page.Awake();

                //mark this ui sync ui
                page.IsAsyncUI = false;
            }

            //:animation or init when active.
            page.Active();

            //:refresh ui component.
            page.Refresh();

            //:popup this node to top if need back.
            PopNode(page);
        }

        /// <summary>
        /// Async Show UI Logic
        /// </summary>
        private static void Show(UIBase page, Action callback)
        {
            UIRoot.Instance.StartCoroutine(AsyncShow(page, callback));
        }

        private static IEnumerator AsyncShow(UIBase page, Action callback)
        {

            //1:Instance UI
            //FIX:support this is manager multi gameObject,instance by your self.
            if (page.Go == null && string.IsNullOrEmpty(page.UIPath) == false && _asyncLoadFunc != null)
            {
                var isLoading = true;
                _asyncLoadFunc(page.UIPath, o =>
                {
                    var go = o != null ? Object.Instantiate(o) as GameObject : null;
                    AnchorUIGameObject(page, go);
                    InjectorView.AutoInject(page);

                    page.Awake();
                    page.IsAsyncUI = true;
                    isLoading = false;

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
                });

                var realtime = Time.realtimeSinceStartup;
                while (isLoading)
                {
                    if (Time.realtimeSinceStartup - realtime > Timeout)
                    {
                        Debug.LogError("[UI] WTF async load your ui prefab timeout!");
                        yield break;
                    }
                    yield return null;
                }
            }
            else
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
            if (_currentPageNodes == null)
            {
                _currentPageNodes = new List<UIBase>();
            }

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
            for (int i = 0; i < _currentPageNodes.Count; i++)
            {
                if (_currentPageNodes[i].Equals(page))
                {
                    _currentPageNodes.RemoveAt(i);
                    _currentPageNodes.Add(page);
                    isFound = true;
                    break;
                }
            }

            //if dont found in old nodes
            //should add in nodelist.
            if (!isFound)
            {
                _currentPageNodes.Add(page);
            }

            //after pop should hide the old node if need.
            HideOldNodes();
        }

        private static void HideOldNodes()
        {
            if (_currentPageNodes.Count < 0)
            {
                return;
            }

            var index = _currentPageNodes.Count - 1;
            var topPage = _currentPageNodes[index];
            if (topPage.Mode == UIMode.HideOther)
            {
                //form bottm to top.
                for (int i = _currentPageNodes.Count - 2; i >= 0; i--)
                {
                    if (_currentPageNodes[i].IsActive)
                    {
                        _currentPageNodes[i].Hide();
                    }
                }
            }
        }

        public static void ClearNodes()
        {
            _currentPageNodes.Clear();
        }

        private static void ShowPage<T>(Action callback, object pageData, bool isAsync) where T : UIBase, new()
        {
            var t = typeof(T);
            var pageName = t.ToString();
            UIBase page;
            if (_allPages != null && _allPages.TryGetValue(pageName, out page))
            {
                ShowPage(pageName, page, callback, pageData, isAsync);
            }
            else
            {
                var instance = new T();
                ShowPage(pageName, instance, callback, pageData, isAsync);
            }
        }

        private static void ShowPage(string pageName, UIBase pageInstance, Action callback, object pageData, bool isAsync)
        {
            if (string.IsNullOrEmpty(pageName) || pageInstance == null)
            {
                Debug.LogError("[UI] show page error with :" + pageName + " maybe null instance.");
                return;
            }

            if (_allPages == null)
            {
                _allPages = new Dictionary<string, UIBase>();
            }

            UIBase page;
            if (_allPages.TryGetValue(pageName, out page))
            {
                page = _allPages[pageName];
            }
            else
            {
                _allPages.Add(pageName, pageInstance);
                page = pageInstance;
            }

            //before show should set this data if need. maybe.!!
            if (isAsync)
            {
                Show(page, callback);
            }
            else
            {
                Show(page);
            }
        }

        /// <summary>
        /// Sync Show Page
        /// </summary>
        public static void ShowPage<T>() where T : UIBase, new()
        {
            ShowPage<T>(null, null, false);
        }

        /// <summary>
        /// Sync Show Page With Page Data Input.
        /// </summary>
        public static void ShowPage<T>(object pageData) where T : UIBase, new()
        {
            ShowPage<T>(null, pageData, false);
        }

        public static void ShowPage(string pageName, UIBase pageInstance)
        {
            ShowPage(pageName, pageInstance, null, null, false);
        }

        public static void ShowPage(string pageName, UIBase pageInstance, object pageData)
        {
            ShowPage(pageName, pageInstance, null, pageData, false);
        }

        /// <summary>
        /// Async Show Page with Async loader bind in 'UIBind.Bind()'
        /// </summary>
        public static void ShowPage<T>(Action callback) where T : UIBase, new()
        {
            ShowPage<T>(callback, null, true);
        }

        public static void ShowPage<T>(Action callback, object pageData) where T : UIBase, new()
        {
            ShowPage<T>(callback, pageData, true);
        }

        /// <summary>
        /// Async Show Page with Async loader bind in 'UIBind.Bind()'
        /// </summary>
        public static void ShowPage(string pageName, UIBase pageInstance, Action callback)
        {
            ShowPage(pageName, pageInstance, callback, null, true);
        }

        public static void ShowPage(string pageName, UIBase pageInstance, Action callback, object pageData)
        {
            ShowPage(pageName, pageInstance, callback, pageData, true);
        }

        /// <summary>
        /// close current page in the "top" node.
        /// </summary>
        public static void ClosePage()
        {
            //Debug.Log("Back&Close PageNodes Count:" + _currentPageNodes.Count);

            if (_currentPageNodes == null || _currentPageNodes.Count <= 1)
            {
                return;
            }

            var index = _currentPageNodes.Count - 1;
            var closePage = _currentPageNodes[index];
            _currentPageNodes.RemoveAt(index);

            //show older page.
            //TODO:Sub pages.belong to root node.
            if (_currentPageNodes.Count > 0)
            {
                index = _currentPageNodes.Count - 1;
                var page = _currentPageNodes[index];
                if (page.IsAsyncUI)
                {
                    ShowPage(page.Name, page, closePage.Hide);
                }
                else
                {
                    //after show to hide().
                    ShowPage(page.Name, page);
                    closePage.Hide();
                }
            }
        }

        /// <summary>
        /// Close target page
        /// </summary>
        public static void ClosePage(UIBase target)
        {
            if (target == null || _currentPageNodes == null)
            {
                return;
            }
            if (!target.IsActive)
            {
                for (int i = 0; i < _currentPageNodes.Count; i++)
                {
                    if (_currentPageNodes[i] == target)
                    {
                        _currentPageNodes.RemoveAt(i);
                        break;
                    }
                }
                return;
            }

            var index = _currentPageNodes.Count - 1;
            if (_currentPageNodes != null && _currentPageNodes.Count >= 1 && _currentPageNodes[index] == target)
            {
                _currentPageNodes.RemoveAt(index);
                //show older page.
                //TODO:Sub pages.belong to root node.
                if (_currentPageNodes.Count > 0)
                {
                    index = _currentPageNodes.Count - 1;
                    UIBase page = _currentPageNodes[index];
                    if (page.IsAsyncUI)
                    {
                        ShowPage(page.Name, page, target.Hide);
                    }
                    else
                    {
                        ShowPage(page.Name, page);
                        target.Hide();
                    }

                    return;
                }
            }
            else if (CheckIfNeedBack(target))
            {
                for (int i = 0; i < _currentPageNodes.Count; i++)
                {
                    if (_currentPageNodes[i] == target)
                    {
                        _currentPageNodes.RemoveAt(i);
                        target.Hide();
                        break;
                    }
                }
            }

            target.Hide();
        }

        public static void ClosePage<T>() where T : UIBase
        {
            var t = typeof(T);
            var pageName = t.ToString();
            UIBase page;
            if (_allPages != null && _allPages.TryGetValue(pageName, out page))
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
            if (_allPages != null && _allPages.TryGetValue(pageName, out page))
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
            if (_allPages != null && _allPages.TryGetValue(pageName, out page))
            {
                _allPages.Remove(pageName);
                _currentPageNodes.Remove(page);
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