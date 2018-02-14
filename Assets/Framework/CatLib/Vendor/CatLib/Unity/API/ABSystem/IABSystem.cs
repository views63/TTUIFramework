using System;
using System.Collections;
using Tangzx.ABSystem;

namespace Catlib.Runtime.ABSystem
{
    public interface IABSystem
    {
        void CheckUnusedBundle();
        UnityEngine.Coroutine StartCoroutine(IEnumerator routine);
        void Init(Action callback=null);
        AssetBundleLoader Load(string path, AssetBundleManager.LoadAssetCompleteHandler handler = null);
    }
}