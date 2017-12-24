using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace TinyUI
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
        /// this page's type
        /// </summary>
        public UIType Type { private set; get; }

        /// <summary>
        /// how to show this page.
        /// </summary>
        public UIMode Mode { private set; get; }


        /// <summary>
        /// path to load ui
        /// </summary>
        public string UIPath = string.Empty;

        /// <summary>
        /// this ui's gameobject
        /// </summary>
        public GameObject Go
        {
            get { return _go; }
        }
#pragma warning disable 649
        private GameObject _go;
#pragma warning restore 649

        /// <summary>
        ///  this ui's transform
        /// </summary>
        public Transform Tr
        {
            get { return _tr; }
        }
#pragma warning disable 649
        private Transform _tr;
#pragma warning restore 649

        /// <summary>
        /// this page active flag
        /// </summary>
        private bool _isActived;

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
        public virtual void Refresh()
        {
        }

        /// <summary>
        /// Active this UI
        /// </summary>
        public virtual void Active()
        {
            Go.SetActive(true);
            _isActived = true;
        }

        /// <summary>
        /// Only Deactive UI wont clear Data.
        /// </summary>
        public virtual void Hide()
        {
            Go.SetActive(false);
            _isActived = false;
            //set this page's data null when hide.
        }

        #endregion

        #region public api

        protected UIBase(UIType type = UIType.Normal, UIMode mod = UIMode.DoNothing)
        {
            Type = type;
            Mode = mod;
            Name = GetType().ToString();
        }


        public override string ToString()
        {
            var str = string.Format("Name:{0}_Type:{1}_ShowMode:{2}", Name, Type, Mode);
            return str;
        }

        public bool IsActive
        {
            //fix,if this page is not only one gameObject
            //so,should check isActived too.
            get
            {
                Assert.IsNotNull(Go);
                return Go.activeSelf || _isActived;
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