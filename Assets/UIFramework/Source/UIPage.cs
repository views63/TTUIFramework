namespace Tiny.UI
{
    using System;
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using Object = UnityEngine.Object;

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
    public abstract class UIPage
    {
        private const float Timeout = 10.0f;

        /// <summary>
        ///sync load ui function
        /// </summary>
        public static Func<string, Object> SyncLoadFunc = null;

        /// <summary>
        /// async load ui function
        /// </summary>
        public static Action<string, Action<Object>> AsyncLoadFunc = null;

        /// <summary>
        /// all pages with the union type
        /// </summary>
        public static Dictionary<string, UIPage> AllPages { get; private set; }

        /// <summary>
        /// control 1&gt;2&gt;3&gt;4&gt;5 each page close will back show the previus page.
        /// </summary>
        public static List<UIPage> CurrentPageNodes { get; private set; }

        public string Name { private set; get; }

        /// <summary>
        /// this page's id
        /// </summary>
        public int ID = -1;

        /// <summary>
        /// this page's type
        /// </summary>
        public UIType Type { private set; get; }

        /// <summary>
        /// how to show this page.
        /// </summary>
        public UIMode Mode { private set; get; }

        /// <summary>
        /// the background collider mode
        /// </summary>
        public UICollider Collider { private set; get; }

        /// <summary>
        /// path to load ui
        /// </summary>
        public string UIPath = string.Empty;

        /// <summary>
        /// this ui's gameobject
        /// </summary>
        public GameObject Go { private set; get; }
        /// <summary>
        ///  this ui's transform
        /// </summary>
        public Transform Tr { private set; get; }

        /// <summary>
        /// record this ui load mode.async or sync.
        /// </summary>
        private bool _isAsyncUI = false;

        /// <summary>
        /// this page active flag
        /// </summary>
        protected bool IsActived { get; private set; }

        /// <summary>
        /// refresh page 's data.
        /// </summary>
        protected object Data { get; private set; }

        #region abstract api

        /// <summary>
        /// When Instance UI Ony Once.
        /// </summary>
        protected abstract void Awake();

        #endregion

        #region virtual api

        /// <summary>
        /// Show UI Refresh Eachtime.
        /// </summary>
        public virtual void Refresh() { }

        /// <summary>
        /// Active this UI
        /// </summary>
        public virtual void Active()
        {
            Go.SetActive(true);
            IsActived = true;
        }

        /// <summary>
        /// Only Deactive UI wont clear Data.
        /// </summary>
        public virtual void Hide()
        {
            Go.SetActive(false);
            IsActived = false;
            //set this page's data null when hide.
            Data = null;
        }

        #endregion

        #region public api

        public UIPage(UIType type = UIType.Normal, UIMode mod = UIMode.DoNothing, UICollider col = UICollider.None)
        {
            Data = null;
            Type = type;
            Mode = mod;
            Collider = col;
            Name = GetType().ToString();

            //when create one page.
            //bind special delegate .
            UIBind.Bind();
            //Debug.LogWarning("[UI] create page:" + ToString());
        }

        /// <summary>
        /// Sync Show UI Logic
        /// </summary>
        protected void Show()
        {
            //1:instance UI
            if (Go == null && string.IsNullOrEmpty(UIPath) == false)
            {
                GameObject go;
                if (SyncLoadFunc != null)
                {
                    var o = SyncLoadFunc(UIPath);
                    go = (o != null) ? Object.Instantiate(o) as GameObject : null;
                }
                else
                {
                    go = Object.Instantiate(Resources.Load(UIPath)) as GameObject;
                }

                //protected.
                if (go == null)
                {
                    Debug.LogError("[UI] Cant sync load your ui prefab.");
                    return;
                }

                AnchorUIGameObject(go);

                //after instance should awake init.
                Awake();

                //mark this ui sync ui
                _isAsyncUI = false;
            }

            //:animation or init when active.
            Active();

            //:refresh ui component.
            Refresh();

            //:popup this node to top if need back.
            PopNode(this);
        }

        /// <summary>
        /// Async Show UI Logic
        /// </summary>
        protected void Show(Action callback)
        {
            UIRoot.Instance.StartCoroutine(AsyncShow(callback));
        }

        private IEnumerator AsyncShow(Action callback)
        {
            //1:Instance UI
            //FIX:support this is manager multi gameObject,instance by your self.
            if (Go == null && string.IsNullOrEmpty(UIPath) == false)
            {
                GameObject go = null;
                var isLoading = true;
                AsyncLoadFunc(UIPath, o =>
                {
                    go = o != null ? Object.Instantiate(o) as GameObject : null;
                    AnchorUIGameObject(go);
                    Awake();
                    _isAsyncUI = true;
                    isLoading = false;

                    //:animation active.
                    Active();

                    //:refresh ui component.
                    Refresh();

                    //:popup this node to top if need back.
                    PopNode(this);

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
                Active();

                //:refresh ui component.
                Refresh();

                //:popup this node to top if need back.
                PopNode(this);

                if (callback != null)
                {
                    callback();
                }
            }
        }

        public bool CheckIfNeedBack()
        {
            if (Type == UIType.Fixed || Type == UIType.PopUp || Type == UIType.None)
            {
                return false;
            }
            else if (Mode == UIMode.NoNeedBack || Mode == UIMode.DoNothing)
            {
                return false;
            }
            return true;
        }

        protected void AnchorUIGameObject(GameObject ui)
        {
            if (UIRoot.Instance == null || ui == null)
            {
                return;
            }

            Go = ui;
            Tr = ui.transform;

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

            if (Type == UIType.Fixed)
            {
                Tr.SetParent(UIRoot.Instance.FixedRoot);
            }
            else if (Type == UIType.Normal)
            {
                Tr.SetParent(UIRoot.Instance.NormalRoot);
            }
            else if (Type == UIType.PopUp)
            {
                Tr.SetParent(UIRoot.Instance.PopupRoot);
            }


            if (rect != null)
            {
                rect.anchoredPosition = anchorPos;
                rect.sizeDelta = sizeDel;
                rect.localScale = scale;
            }
            else
            {
                Tr.localPosition = anchorPos;
                Tr.localScale = scale;
            }
        }

        public override string ToString()
        {
            var str = string.Format(">Name:{0},ID:{1},Type:{2},ShowMode:{3},Collider:{4}", Name, ID, Type, Mode, Collider);
            return str;
        }

        public bool IsActive
        {
            //fix,if this page is not only one gameObject
            //so,should check isActived too.
            get
            {
                var ret = Go != null && Go.activeSelf;
                return ret || IsActived;
            }
        }

        /// <summary>
        /// Destroy UI and clear Data.
        /// </summary>
        public void Destroy()
        {
            AllPages.Remove(Name);
            CurrentPageNodes.Remove(this);
            Data = null;
            Object.Destroy(Go);
        }

        #endregion

        #region static api

        private static bool CheckIfNeedBack(UIPage page)
        {
            return page != null && page.CheckIfNeedBack();
        }

        /// <summary>
        /// make the target node to the top.
        /// </summary>
        private static void PopNode(UIPage page)
        {
            if (CurrentPageNodes == null)
            {
                CurrentPageNodes = new List<UIPage>();
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

        private static void ShowPage<T>(Action callback, object pageData, bool isAsync) where T : UIPage, new()
        {
            var t = typeof(T);
            var pageName = t.ToString();
            UIPage page;
            if (AllPages != null && AllPages.TryGetValue(pageName, out page))
            {
                ShowPage(pageName, page, callback, pageData, isAsync);
            }
            else
            {
                var instance = new T();
                ShowPage(pageName, instance, callback, pageData, isAsync);
            }
        }

        private static void ShowPage(string pageName, UIPage pageInstance, Action callback, object pageData, bool isAsync)
        {
            if (string.IsNullOrEmpty(pageName) || pageInstance == null)
            {
                Debug.LogError("[UI] show page error with :" + pageName + " maybe null instance.");
                return;
            }

            if (AllPages == null)
            {
                AllPages = new Dictionary<string, UIPage>();
            }

            UIPage page;
            if (AllPages.TryGetValue(pageName, out page))
            {
                page = AllPages[pageName];
            }
            else
            {
                AllPages.Add(pageName, pageInstance);
                page = pageInstance;
            }

            //if active before,wont active again.
            //if (page.isActive() == false)
            {
                //before show should set this data if need. maybe.!!
                page.Data = pageData;
                if (isAsync)
                {
                    page.Show(callback);
                }
                else
                {
                    page.Show();
                }
            }
        }

        /// <summary>
        /// Sync Show Page
        /// </summary>
        public static void ShowPage<T>() where T : UIPage, new()
        {
            ShowPage<T>(null, null, false);
        }

        /// <summary>
        /// Sync Show Page With Page Data Input.
        /// </summary>
        public static void ShowPage<T>(object pageData) where T : UIPage, new()
        {
            ShowPage<T>(null, pageData, false);
        }

        public static void ShowPage(string pageName, UIPage pageInstance)
        {
            ShowPage(pageName, pageInstance, null, null, false);
        }

        public static void ShowPage(string pageName, UIPage pageInstance, object pageData)
        {
            ShowPage(pageName, pageInstance, null, pageData, false);
        }

        /// <summary>
        /// Async Show Page with Async loader bind in 'UIBind.Bind()'
        /// </summary>
        public static void ShowPage<T>(Action callback) where T : UIPage, new()
        {
            ShowPage<T>(callback, null, true);
        }

        public static void ShowPage<T>(Action callback, object pageData) where T : UIPage, new()
        {
            ShowPage<T>(callback, pageData, true);
        }

        /// <summary>
        /// Async Show Page with Async loader bind in 'UIBind.Bind()'
        /// </summary>
        public static void ShowPage(string pageName, UIPage pageInstance, Action callback)
        {
            ShowPage(pageName, pageInstance, callback, null, true);
        }

        public static void ShowPage(string pageName, UIPage pageInstance, Action callback, object pageData)
        {
            ShowPage(pageName, pageInstance, callback, pageData, true);
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
                if (page._isAsyncUI)
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
        public static void ClosePage(UIPage target)
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
                    UIPage page = CurrentPageNodes[index];
                    if (page._isAsyncUI)
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
            else if (target.CheckIfNeedBack())
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

            target.Hide();
        }

        public static void ClosePage<T>() where T : UIPage
        {
            var t = typeof(T);
            var pageName = t.ToString();
            UIPage page;
            if (AllPages != null && AllPages.TryGetValue(pageName, out page))
            {
                ClosePage(page);
            }
            else
            {
                Debug.LogError(pageName + "havnt show yet!");
            }
        }

        public static void ClosePage(string pageName)
        {
            UIPage page;
            if (AllPages != null && AllPages.TryGetValue(pageName, out page))
            {
                ClosePage(page);
            }
            else
            {
                Debug.LogError(pageName + " havnt show yet!");
            }
        }

        #endregion

    }
}