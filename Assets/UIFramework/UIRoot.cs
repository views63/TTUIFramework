#region

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#endregion

namespace TinyUI
{
    /// <summary>
    ///     Init The UI Root
    ///     UIRoot
    ///       --UICamera
    ///       --FixedRoot
    ///       --NormalRoot
    ///       --PopupRoot
    ///       --EventSystem
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        private static UIRoot _instance;
        public Transform FixedRoot { private set; get; }
        public Transform NormalRoot { private set; get; }
        public Transform PopupRoot { private set; get; }
        public Transform Root { private set; get; }
        public Camera UICamera { private set; get; }

        public static UIRoot Instance
        {
            get
            {
                if (_instance == null)
                {
                    InitRoot();
                }
                return _instance;
            }
        }

        private static void InitRoot()
        {
            var go = new GameObject("UIRoot");
            DontDestroyOnLoad(go);
            
            go.layer = LayerMask.NameToLayer("UI");
            _instance = go.AddComponent<UIRoot>();
            go.AddComponent<RectTransform>();
            _instance.Root = go.transform;

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.pixelPerfect = true;
            var camObj = new GameObject("UICamera");
            camObj.layer = LayerMask.NameToLayer("UI");
            camObj.transform.parent = go.transform;
            camObj.transform.localPosition = new Vector3(0, 0, -100f);
            var uiCamera = camObj.AddComponent<Camera>();
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.orthographic = true;
            uiCamera.farClipPlane = 200f;
            canvas.worldCamera = uiCamera;
            _instance.UICamera = uiCamera;
            uiCamera.cullingMask = 1 << 5;
            uiCamera.nearClipPlane = -50f;
            uiCamera.farClipPlane = 50f;
            uiCamera.allowMSAA = false;
            uiCamera.allowHDR = false;
            
            //add audio listener
            camObj.AddComponent<AudioListener>();

            var cs = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1136f, 640f);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            ////add auto scale camera fix size.
            //TTCameraScaler tcs = go.AddComponent<TTCameraScaler>();
            //tcs.scaler = cs;

            //set the raycaster
            //GraphicRaycaster gr = go.AddComponent<GraphicRaycaster>();

            var fixedRoot = CreateSubCanvasForRoot(go.transform, 250);
            fixedRoot.name = "FixedRoot";
            _instance.FixedRoot = fixedRoot.transform;

            var normalRoot = CreateSubCanvasForRoot(go.transform, 0);
            normalRoot.name = "NormalRoot";
            _instance.NormalRoot = normalRoot.transform;

            var popupRoot = CreateSubCanvasForRoot(go.transform, 500);
            popupRoot.name = "PopupRoot";
            _instance.PopupRoot = popupRoot.transform;

            // clear Canvas
            var canObj = GameObject.Find("Canvas");
            if (canObj != null)
            {
                DestroyImmediate(canObj);
            }

            //add Event System
            var esObj = GameObject.Find("EventSystem");
            if (esObj != null)
            {
                DestroyImmediate(esObj);
            }

            var eventObj = new GameObject("EventSystem");
            eventObj.layer = LayerMask.NameToLayer("UI");
            eventObj.transform.SetParent(go.transform);
            eventObj.transform.localPosition = Vector3.zero;
            eventObj.transform.localRotation=Quaternion.identity;
            eventObj.transform.localScale = Vector3.one;

            eventObj.AddComponent<EventSystem>();
            var inputModule = eventObj.AddComponent<StandaloneInputModule>();
            inputModule.forceModuleActive = true;
            
        }

        private static GameObject CreateSubCanvasForRoot(Transform root, int sort)
        {
            var go = new GameObject("canvas");
            go.transform.parent = root;
            go.layer = LayerMask.NameToLayer("UI");

            var can = go.AddComponent<Canvas>();
            var rect = go.GetComponent<RectTransform>();
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.localScale = Vector3.one;
            can.overrideSorting = true;
            can.sortingOrder = sort;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}