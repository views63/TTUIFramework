// Enhanced Hierarchy for Unity
// Version 1.3.1
// Samuel Schultze
// samuelschultze@gmail.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EnhancedHierarchy {

    internal enum MiniLabelType {
        None = 0,
        Tag = 1,
        Layer = 2,
        TagOrLayer = 3,
        LayerOrTag = 4,
    }

    internal enum DrawType {
        Active = 0,
        Static = 1,
        Lock = 2,
        Icon = 3,
        ApplyPrefab = 4
    }

    internal enum EntryMode {
        ScriptingError = 256,
        ScriptingWarning = 512,
        ScriptingLog = 1024
    }

    //GUIStyles and GUIContents used in hierarchy
    internal static class Styles {
        public static GUIStyle staticToggleStyle {
            get {
                var style = new GUIStyle("ShurikenLabel");
                style.alignment = TextAnchor.MiddleCenter;
                return style;
            }
        }
        public static GUIStyle applyPrefabStyle {
            get {
                var style = new GUIStyle("ShurikenLabel");
                style.alignment = TextAnchor.MiddleCenter;
                return style;
            }
        }
        public static GUIStyle lockToggleStyle = "IN LockButton";
        public static GUIStyle activeToggleStyle = "OL Toggle";
        public static GUIStyle minilabelStyle = "ShurikenDropdown";
        public static GUIStyle horizontalLine = "EyeDropperHorizontalLine";

        public static GUIStyle labelNormal = "PR Label";
        public static GUIStyle labelDisabled = "PR DisabledLabel";
        public static GUIStyle labelPrefab = "PR PrefabLabel";
        public static GUIStyle labelPrefabDisabled = "PR DisabledPrefabLabel";
        public static GUIStyle labelPrefabBroken = "PR BrokenPrefabLabel";
        public static GUIStyle labelPrefabBrokenDisabled = "PR DisabledBrokenPrefabLabel";

        private const string treeLineBase64 =
@"iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAJUlEQVQ4EWNgIAL8BwJcyphwSRAr
PmoAA8NoGIyGASi/DHw6AADYOwQbvk/7+AAAAABJRU5ErkJggg==";
        private const string treeEndBase64 =
@"iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAK0lEQVQ4EWNgIAL8BwJcyphwSRAr
PmoAA8MwCANGYuIbX0IiRv+oGlqHAABHsQgKWP01jwAAAABJRU5ErkJggg==";
        private const string treeMiddleBase64 =
@"iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAMUlEQVQ4EWNgIAL8BwJcyphwSRAr
PmoAA8PAhwEjMdGFLx0Qo58BnwEDHwajLmBgAADNbwwQNsi4sgAAAABJRU5ErkJggg==";

        private static Texture2D _treeLine;
        private static Texture2D _treeMiddle;
        private static Texture2D _treeEnd;
        private static Texture2D _infoIcon;
        private static Texture2D _warningIcon;
        private static Texture2D _errorIcon;

        public static Texture2D treeLine {
            get {
                if(!_treeLine)
                    _treeLine = Utility.ConvertToTexture(treeLineBase64);
                return _treeLine;
            }
        }
        public static Texture2D treeMiddle {
            get {
                if(!_treeMiddle)
                    _treeMiddle = Utility.ConvertToTexture(treeMiddleBase64);
                return _treeMiddle;
            }
        }
        public static Texture2D treeEnd {
            get {
                if(!_treeEnd)
                    _treeEnd = Utility.ConvertToTexture(treeEndBase64);
                return _treeEnd;
            }
        }
        public static Texture2D infoIcon {
            get {
                if(!_infoIcon)
                    _infoIcon = Utility.LoadIcon("console.infoicon.sml");
                if(!_infoIcon)
                    return Texture2D.blackTexture;
                return _infoIcon;
            }
        }
        public static Texture2D warningIcon {
            get {
                if(!_warningIcon)
                    _warningIcon = Utility.LoadIcon("console.warnicon.sml");
                if(!_warningIcon)
                    return Texture2D.blackTexture;
                return _warningIcon;
            }
        }
        public static Texture2D errorIcon {
            get {
                if(!_errorIcon)
                    _errorIcon = Utility.LoadIcon("console.erroricon.sml");
                if(!_errorIcon)
                    return Texture2D.blackTexture;
                return _errorIcon;
            }
        }

        public static GUIContent prefabApplyContent = new GUIContent("A", "Apply Prefab Changes");
        public static GUIContent staticContent = new GUIContent("S", "Static");
        public static GUIContent emptyStaticContent = new GUIContent(" ", "Static");
        public static GUIContent lockContent = new GUIContent("", "Lock/Unlock");
        public static GUIContent activeContent = new GUIContent("", "Enable/Disable");
        public static GUIContent tagContent = new GUIContent("", "Tag");
        public static GUIContent layerContent = new GUIContent("", "Layer");
    }

    //Main GUI class
    internal static class GUIDrawer {

        private static bool odd;
        private static GameObject go;
        private static Color currentColor;

        [InitializeOnLoadMethod]
        private static void Start() {
            Application.logMessageReceived += (message, stack, type) => {
                EditorApplication.RepaintHierarchyWindow();
            };

            EditorApplication.hierarchyWindowItemOnGUI += (instanceID, rect) => {
                try {
                    var evt = Event.current;
                    //You can change or remove this if statement if it's conflicting with other extension
                    if(evt.Equals(Event.KeyboardEvent("^h"))) {
                        Prefs.enabled = !Prefs.enabled;
                        evt.Use();
                    }

                    if(!Prefs.enabled)
                        return;

                    go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

                    if(!go)
                        return;

                    currentColor = go.GetHierarchyColor();
                    Undo.RecordObject(go, "Hierarchy");

                    if(Prefs.lineSorting)
                        ColorSort(rect);
                    if(Prefs.separator)
                        DrawSeparator(rect);
                    if(Prefs.warning)
                        DrawWarnings(rect);
                    if(Prefs.tree)
                        DrawTree(rect);

                    rect.xMin = rect.xMax - rect.height;
                    rect.x += rect.height - Prefs.offset;

                    var list = Prefs.drawOrder;
                    for(int i = 0; i < list.Count; i++)
                        switch(list[i]) {
                            case DrawType.Active:
                                DrawActiveButton(ref rect);
                                break;
                            case DrawType.Static:
                                DrawStaticButton(ref rect);
                                break;
                            case DrawType.Lock:
                                DrawLockButton(ref rect);
                                break;
                            case DrawType.Icon:
                                DrawIcon(ref rect);
                                break;
                            case DrawType.ApplyPrefab:
                                DrawPrefabApply(ref rect);
                                break;
                        }

                    if(Prefs.labelType != MiniLabelType.None)
                        DrawMiniLabel(ref rect);

                    if(Prefs.tooltip) {
                        rect.xMax = rect.xMin;
                        rect.xMin = 0f;
                        DrawTooltip(rect);
                    }
                }
                catch(Exception e) {
                    Debug.LogErrorFormat("Unexpected exception in enhanced hierarchy: {0}", e.ToString());
                }
            };
        }

        private static void DrawStaticButton(ref Rect rect) {
            rect.x -= rect.height;
            GUI.changed = false;
            GUI.Toggle(rect, go.isStatic, go.isStatic ? Styles.staticContent : Styles.emptyStaticContent, Styles.staticToggleStyle);
            if(GUI.changed)
                go.isStatic = !go.isStatic;
        }

        private static void DrawLockButton(ref Rect rect) {
            rect.x -= rect.height;

            var locked = go.hideFlags == (go.hideFlags | HideFlags.NotEditable);
            GUI.changed = false;
            GUI.Toggle(rect, locked, Styles.lockContent, Styles.lockToggleStyle);
            if(!GUI.changed)
                return;

            go.hideFlags += locked ? -8 : 8;
            InternalEditorUtility.RepaintAllViews();
        }

        private static void DrawActiveButton(ref Rect rect) {
            rect.x -= rect.height;

            GUI.changed = false;
            GUI.Toggle(rect, go.activeSelf, Styles.activeContent, Styles.activeToggleStyle);

            if(GUI.changed)
                go.SetActive(!go.activeSelf);
        }

        private static void DrawIcon(ref Rect rect) {
            var content = EditorGUIUtility.ObjectContent(go, typeof(GameObject));

            if(!content.image)
                return;

            rect.x -= rect.height;
            content.text = string.Empty;
            content.tooltip = "Change Icon";

            if(GUI.Button(rect, content, EditorStyles.label))
                Utility.ShowIconSelector(go, rect, true);
        }

        private static void DrawMiniLabel(ref Rect rect) {
            rect.x -= rect.height;

            var style = new GUIStyle(Styles.minilabelStyle);
            style.alignment = TextAnchor.MiddleRight;
            style.clipping = TextClipping.Overflow;

            style.normal.textColor =
            style.hover.textColor =
            style.focused.textColor =
            style.active.textColor = currentColor;

            switch(Prefs.labelType) {
                case MiniLabelType.Tag:
                    DrawTag(ref rect, style);
                    return;

                case MiniLabelType.Layer:
                    DrawLayer(ref rect, style);
                    return;

                case MiniLabelType.LayerOrTag:
                    if(go.tag != "Untagged" && LayerMask.LayerToName(go.layer) == "Default")
                        DrawTag(ref rect, style);
                    else
                        DrawLayer(ref rect, style);
                    return;

                case MiniLabelType.TagOrLayer:
                    if(go.tag == "Untagged" && LayerMask.LayerToName(go.layer) != "Default")
                        DrawLayer(ref rect, style);
                    else
                        DrawTag(ref rect, style);
                    return;
            }
        }

        private static void DrawPrefabApply(ref Rect rect) {
            PrefabUtility.RecordPrefabInstancePropertyModifications(go);
            var mods = PrefabUtility.GetPropertyModifications(go);

            if(mods == null || PrefabUtility.GetPrefabType(go) != PrefabType.PrefabInstance)
                return;

            mods = (from mod in mods
                    where !(mod.target as Transform) &&
                    !mod.propertyPath.Contains("m_Name") &&
                    !mod.InvalidPrefabReference(go)
                    select mod).ToArray();

            if(mods.Length == 0)
                return;

            rect.x -= rect.height;
            if(GUI.Button(rect, Styles.prefabApplyContent, Styles.applyPrefabStyle)) {
                var selected = Selection.objects;
                Selection.activeGameObject = go;
                EditorApplication.ExecuteMenuItem("GameObject/Apply Changes To Prefab");
                Selection.objects = selected;
            }
        }

        private static void DrawLayer(ref Rect rect, GUIStyle style) {
            var str = LayerMask.LayerToName(go.layer);

            if(str != "Default") {
                var size = style.CalcSize(new GUIContent(str)).x;
                rect.xMin = rect.xMax - size;
            }

            if(go.layer == 0)
                style.imagePosition = ImagePosition.ImageOnly;

            GUI.changed = false;
            EditorGUI.LabelField(rect, Styles.layerContent);
            var layer = EditorGUI.LayerField(rect, go.layer, style);

            if(GUI.changed)
                go.layer = layer;
        }

        private static void DrawTag(ref Rect rect, GUIStyle style) {
            var str = go.tag;

            if(str == "Untagged")
                str = string.Empty;

            var size = style.CalcSize(new GUIContent(str)).x;
            rect.xMin = rect.xMax - size;

            GUI.changed = false;
            EditorGUI.LabelField(rect, Styles.tagContent);
            str = EditorGUI.TagField(rect, str, style);

            if(string.IsNullOrEmpty(str))
                str = "Untagged";

            if(GUI.changed)
                go.tag = str;
        }

        private static void DrawSeparator(Rect rect) {
            rect.yMin = rect.yMax - 1f;
            EditorGUI.LabelField(rect, string.Empty, Styles.horizontalLine);
        }

        private static void DrawWarnings(Rect rect) {
            var hasInfo = false;
            var hasWarning = false;
            var hasError = false;
            var entries = Utility.GetLogs();
            var contextEntries = (from entry in entries
                                  where entry.intanceID == go.GetInstanceID()
                                  select entry).ToArray();

            for(int i = 0; i < contextEntries.Length; i++) {
                if(contextEntries[i].mode == (contextEntries[i].mode | EntryMode.ScriptingLog))
                    hasInfo = true;

                if(contextEntries[i].mode == (contextEntries[i].mode | EntryMode.ScriptingWarning))
                    hasWarning = true;

                if(contextEntries[i].mode == (contextEntries[i].mode | EntryMode.ScriptingError))
                    hasError = true;
            }

            if(!hasWarning) {
                var components = go.GetComponents<MonoBehaviour>();
                for(int i = 0; i < components.Length; i++)
                    if(!components[i])
                        hasWarning = true;
            }

            var size = EditorStyles.label.CalcSize(new GUIContent(go.name)).x;
            rect.xMin += size;
            rect.xMax = rect.xMin + rect.height;
            rect.height = 16f;
            rect.xMax = rect.xMin + rect.height;

            if(hasInfo) {
                GUI.DrawTexture(rect, Styles.infoIcon);
                rect.x += rect.width;
            }
            if(hasWarning) {
                GUI.DrawTexture(rect, Styles.warningIcon);
                rect.x += rect.width;
            }
            if(hasError) {
                GUI.DrawTexture(rect, Styles.errorIcon);
                rect.x += rect.width;
            }
        }

        private static void DrawTooltip(Rect rect) {
            var content = new GUIContent();
            content.tooltip = string.Format("Tag: {0}\nLayer: {1}", go.tag, LayerMask.LayerToName(go.layer));
            EditorGUI.LabelField(rect, content);
        }

        private static void DrawTree(Rect rect) {
            rect.xMin -= 14f;
            rect.xMax = rect.xMin + 14f;

            GUI.color = currentColor;

            if(go.transform.childCount == 0 && go.transform.parent) {
                if(Utility.LastInHierarchy(go.transform))
                    GUI.DrawTexture(rect, Styles.treeEnd);
                else
                    GUI.DrawTexture(rect, Styles.treeMiddle);
            }

            var parent = go.transform.parent;

            for(rect.x -= 14f; rect.xMin > 0f && parent.parent; rect.x -= 14f) {
                GUI.color = parent.parent.GetHierarchyColor();
                if(!parent.LastInHierarchy())
                    GUI.DrawTexture(rect, Styles.treeLine);
                parent = parent.parent;
            }

            GUI.color = Color.white;
        }

        private static void ColorSort(Rect rect) {
            Color color;

            if(EditorGUIUtility.isProSkin)
                color = new Color(0, 0, 0, 0.10f);
            else
                color = new Color(1, 1, 1, 0.20f);

            rect.xMin = 0f;

            if(odd)
                EditorGUI.DrawRect(rect, color);

            odd = !odd;
        }
    }

    //Editor preferences
    internal static class Prefs {
        private const string enabledPref = "HierarchyEnabled";
        private const string linePref = "HierarchySeparator";
        private const string treePref = "HierarchyTree";
        private const string warningPref = "HierarchyWarning";
        private const string labelPref = "HierarchyMiniLabel";
        private const string tooltipPref = "HierarchyTooltip";
        private const string offsetPref = "HierarchyOffset";
        private const string lineSortingPref = "HierarchyLineSorting";
        private const string drawOrderPref = "HierarchyDrawOrder";

        public static bool enabled {
            get {
                return EditorPrefs.GetBool(enabledPref, true);
            }
            set {
                EditorPrefs.SetBool(enabledPref, value);
            }
        }
        public static bool separator {
            get {
                return EditorPrefs.GetBool(linePref, true);
            }
            set {
                EditorPrefs.SetBool(linePref, value);
            }
        }
        public static bool lineSorting {
            get {
                return EditorPrefs.GetBool(lineSortingPref, true);
            }
            set {
                EditorPrefs.SetBool(lineSortingPref, value);
            }
        }
        public static bool tree {
            get {
                return EditorPrefs.GetBool(treePref, true);
            }
            set {
                EditorPrefs.SetBool(treePref, value);
            }
        }
        public static bool warning {
            get {
                return EditorPrefs.GetBool(warningPref, true);
            }
            set {
                EditorPrefs.SetBool(warningPref, value);
            }
        }
        public static bool tooltip {
            get {
                return EditorPrefs.GetBool(tooltipPref, true);
            }
            set {
                EditorPrefs.SetBool(tooltipPref, value);
            }
        }
        public static int offset {
            get {
                return EditorPrefs.GetInt(offsetPref);
            }
            set {
                EditorPrefs.SetInt(offsetPref, value);
            }
        }
        public static MiniLabelType labelType {
            get {
                return (MiniLabelType)EditorPrefs.GetInt(labelPref, 3);
            }
            set {
                EditorPrefs.SetInt(labelPref, (int)value);
            }
        }

        public static List<DrawType> drawOrder {
            get {
                var list = new List<DrawType>();
                if(!EditorPrefs.HasKey(drawOrderPref + "Count")) {
                    list.Add(DrawType.Active);
                    list.Add(DrawType.Static);
                    list.Add(DrawType.Lock);
                    list.Add(DrawType.Icon);
                    list.Add(DrawType.ApplyPrefab);
                    return list;
                }

                var count = EditorPrefs.GetInt(drawOrderPref + "Count");

                for(int i = 0; i < count; i++)
                    list.Add((DrawType)EditorPrefs.GetInt(drawOrderPref + i));

                drawOrder = list;
                return list;
            }
            set {
                EditorPrefs.SetInt(drawOrderPref + "Count", value.Count);
                for(int i = 0; i < value.Count; i++)
                    EditorPrefs.SetInt(drawOrderPref + i, (int)value[i]);
            }
        }

        private static ReorderableList rList;
        private static Vector2 scroll;

        [PreferenceItem("Hierarchy")]
        private static void OnPreferencesGUI() {
            scroll = EditorGUILayout.BeginScrollView(scroll, false, false);
            EditorGUILayout.Separator();
            enabled = EditorGUILayout.Toggle("Enabled (Ctrl+H)", enabled);
            EditorGUILayout.HelpBox("Hierarchy window must be selected for the shortcut to work", MessageType.None);
            GUI.enabled = enabled;
            offset = EditorGUILayout.IntField("Offset", offset);
            EditorGUILayout.Separator();

            if(rList == null) {
                rList = new ReorderableList(drawOrder, typeof(DrawType), true, false, false, false);
                rList.showDefaultBackground = false;
                rList.onChangedCallback += (newList) => { drawOrder = newList.list as List<DrawType>; };
            }

            var list = rList.list as List<DrawType>;

            DrawTypeToggle("Active toggle", DrawType.Active, list);
            DrawTypeToggle("Static toggle", DrawType.Static, list);
            DrawTypeToggle("Lock toggle", DrawType.Lock, list);
            DrawTypeToggle("GameObject icon", DrawType.Icon, list);
            DrawTypeToggle("Apply Prefab Changes Button", DrawType.ApplyPrefab, list);

            drawOrder = list;

            EditorGUILayout.Separator();

            separator = EditorGUILayout.Toggle("Separator", separator);
            tree = EditorGUILayout.Toggle("Hierarchy tree", tree);
            warning = EditorGUILayout.Toggle("Warnings", warning);
            tooltip = EditorGUILayout.Toggle("Tooltip", tooltip);
            lineSorting = EditorGUILayout.Toggle("Color sorting", lineSorting);
            labelType = (MiniLabelType)EditorGUILayout.EnumPopup("Mini label type", labelType);

            if(rList.count > 1) {
                EditorGUILayout.Separator();
                var rect = EditorGUILayout.GetControlRect(GUILayout.Height(0f));
                rect.yMax += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, "Drawing Order", EditorStyles.boldLabel);
                rList.DoLayoutList();
            }

            GUI.enabled = true;
            EditorGUILayout.EndScrollView();
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void DrawTypeToggle(string label, DrawType drawType, List<DrawType> list) {
            GUI.changed = false;
            EditorGUILayout.Toggle(label, list.Contains(drawType));

            if(!GUI.changed)
                return;

            if(list.Contains(drawType))
                list.Remove(drawType);
            else
                list.Add(drawType);
        }
    }

    //GUI and hierarchy utilities
    internal static class Utility {

        public static void ShowIconSelector(Object targetObj, Rect activatorRect, bool showLabelIcons) {
            var type = typeof(Editor).Assembly.GetType("UnityEditor.IconSelector");
            var instance = ScriptableObject.CreateInstance(type);
            var parameters = new object[3];

            parameters[0] = targetObj;
            parameters[1] = activatorRect;
            parameters[2] = showLabelIcons;

            type.InvokeMember("Init", BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, instance, parameters);
        }

        public static Texture2D ConvertToTexture(string source) {
            var bytes = Convert.FromBase64String(source);
            var texture = new Texture2D(0, 0);

            texture.LoadImage(bytes);
            texture.alphaIsTransparency = true;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.Apply();

            return texture;
        }

        public static Texture2D LoadIcon(string icon) {
            var type = typeof(EditorGUIUtility);
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var method = type.GetMethod("LoadIcon", flags);

            return method.Invoke(null, new object[] { icon }) as Texture2D;
        }

        public static Color GetHierarchyColor(this GameObject go) {
            if(!go)
                return Color.black;

            var prefabType = PrefabUtility.GetPrefabType(PrefabUtility.FindPrefabRoot(go));
            var active = go.activeInHierarchy;
            var style = active ? Styles.labelNormal : Styles.labelDisabled;

            switch(prefabType) {
                case PrefabType.PrefabInstance:
                case PrefabType.ModelPrefabInstance:
                    style = active ? Styles.labelPrefab : Styles.labelPrefabDisabled;
                    break;
                case PrefabType.MissingPrefabInstance:
                    style = active ? Styles.labelPrefabBroken : Styles.labelPrefabBrokenDisabled;
                    break;
            }

            return style.normal.textColor;
        }

        public static Color GetHierarchyColor(this Transform t) {
            if(!t)
                return Color.black;

            return t.gameObject.GetHierarchyColor();
        }

        public static bool LastInHierarchy(this Transform t) {
            if(!t)
                return true;

            return t.parent.GetChild(t.parent.childCount - 1) == t;
        }

        public static bool LastInHierarchy(this GameObject go) {
            if(!go)
                return true;

            return go.transform.LastInHierarchy();
        }

        public static bool InvalidPrefabReference(this PropertyModification mod, GameObject go) {

            var parent = PrefabUtility.FindPrefabRoot(go);
            var comps = parent.GetComponentsInChildren<Component>(true).ToList();
            var gos = (from t in comps
                       where t as Transform
                       select t.gameObject).ToList();

            if(mod.objectReference as Component)
                return !comps.Contains(mod.objectReference as Component);
            else if(mod.objectReference as GameObject)
                return !gos.Contains(mod.objectReference as GameObject);

            return false;
        }

        public static List<LogEntry> GetLogs() {
            try {
                var logEntriesType = typeof(Editor).Assembly.GetType("UnityEditorInternal.LogEntries");
                var logEntryType = typeof(Editor).Assembly.GetType("UnityEditorInternal.LogEntry");
                var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance;

                var countMethod = logEntriesType.GetMethod("GetCount", flags);
                var getEntryMethod = logEntriesType.GetMethod("GetEntryInternal", flags);
                var startMethod = logEntriesType.GetMethod("StartGettingEntries", flags);
                var endMethod = logEntriesType.GetMethod("EndGettingEntries", flags);
                var logEntryConstructor = logEntryType.GetConstructor(new Type[0]);

                var logEntry = logEntryConstructor.Invoke(null);
                var count = (int)countMethod.Invoke(null, null);

                var conditionField = logEntryType.GetField("condition");
                var errorNumField = logEntryType.GetField("errorNum");
                var fileField = logEntryType.GetField("file");
                var lineField = logEntryType.GetField("line");
                var modeField = logEntryType.GetField("mode");
                var instanceIDField = logEntryType.GetField("instanceID");
                var identifierField = logEntryType.GetField("identifier");
                var entries = new List<LogEntry>();

                startMethod.Invoke(null, null);

                for(int i = 0; i < count; i++) {
                    getEntryMethod.Invoke(null, new object[] { i, logEntry });

                    var entry = new LogEntry();

                    entry.condition = (string)conditionField.GetValue(logEntry);
                    entry.file = (string)fileField.GetValue(logEntry);
                    entry.errorNum = (int)errorNumField.GetValue(logEntry);
                    entry.line = (int)lineField.GetValue(logEntry);
                    entry.identifier = (int)identifierField.GetValue(logEntry);
                    entry.mode = (EntryMode)modeField.GetValue(logEntry);
                    entry.intanceID = (int)instanceIDField.GetValue(logEntry);

                    entries.Add(entry);
                }

                endMethod.Invoke(null, null);

                return entries;
            }
            catch {
                return new List<LogEntry>();
            }
        }
    }

    //Console Log Entry
    internal struct LogEntry {
        public string condition;
        public string file;
        public int errorNum;
        public int line;
        public int identifier;
        public int intanceID;
        public EntryMode mode;
    }
}