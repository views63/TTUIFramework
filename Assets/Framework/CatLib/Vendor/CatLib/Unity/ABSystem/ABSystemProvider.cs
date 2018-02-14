using System;
using System.Collections;
using CatLib;
using CatLib.API.MonoDriver;
using Tangzx.ABSystem;
using UnityEngine;
using UnityEngine.Networking;
using IServiceProvider = CatLib.IServiceProvider;

namespace Catlib.Runtime.ABSystem
{
    public class ABSystemProvider : IServiceProvider
    {
        private static WaitForSeconds _waitFor5 = new WaitForSeconds(5);
        
        /// <summary>
        /// Mono驱动器初始化
        /// </summary>
        /// <returns>迭代器</returns>
        public void Init()
        {
           var  abs = App.Make<IABSystem>();
            InitMainThreadGroup(abs);
        }

        /// <summary>
        /// 注册路由条目
        /// </summary>
        public void Register()
        {
            App.Singleton<AssetBundleManager>().Alias<IABSystem>();
        }


        /// <summary>
        /// 初始化主线程组
        /// </summary>
        /// <param name="abs"></param>
        private void InitMainThreadGroup(IABSystem abs)
        {
            var driver = App.Make<IMonoDriver>();

            driver.StartCoroutine(CheckMethod(abs.CheckUnusedBundle));
           var set= new SortSet<IUpdate, int>();
            set.Add((IUpdate)abs,1);
            driver.Attach(set);
            
            abs.Init(null);
        }

        private IEnumerator CheckMethod(Action checkUnusedBundle)
        {
            checkUnusedBundle();
            yield return _waitFor5;
            
            var driver = App.Make<IMonoDriver>();
            driver.StartCoroutine(CheckMethod(checkUnusedBundle));
        }
    }
}