namespace Tiny.UI
{
    using UnityEngine;

    /// <summary>
    /// Bind Some Delegate Func For Yours.
    /// </summary>
    public class UIBind
    {
       private static bool _isBind = false;
        public static void Bind()
        {
            if (!_isBind)
            {
                _isBind = true;
                //Debug.LogWarning("Bind For UI Framework.");

                //bind for your loader api to load UI.
                UIPage.SyncLoadFunc = Resources.Load;
                //UIPage.delegateAsyncLoadUI = UILoader.Load;

            }
        }
    }
}