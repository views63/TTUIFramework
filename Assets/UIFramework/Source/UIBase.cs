using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Tiny.UI
{

    /// <summary>
    /// Each Page Mean one UI 'window'
    /// 3 steps:
    /// instance ui > refresh ui by data > show
    /// 
    /// by chiuan
    /// 2015-09
    /// </summary>
    public abstract class UIBase
    {
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
        public GameObject Go { get { return _go; } }
        private GameObject _go;

        /// <summary>
        ///  this ui's transform
        /// </summary>
        public Transform Tr { get { return _tr; } }
        private Transform _tr;


        /// <summary>
        /// record this ui load mode.async or sync.
        /// </summary>
        public bool IsAsyncUI = false;

        /// <summary>
        /// this page active flag
        /// </summary>
        protected bool IsActived { get; private set; }

        /// <summary>
        /// refresh page 's data.
        /// </summary>
        public object Data { get; set; }

        #region abstract api

        /// <summary>
        /// When Instance UI Ony Once.
        /// </summary>
        public abstract void Awake();

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

        protected UIBase(UIType type = UIType.Normal, UIMode mod = UIMode.DoNothing, UICollider col = UICollider.None)
        {
            Data = null;
            Type = type;
            Mode = mod;
            Collider = col;
            Name = GetType().ToString();
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
            UIManager.Destroy(Name);
        }

        #endregion
    }
}