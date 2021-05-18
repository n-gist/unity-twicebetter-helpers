using System;
using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    static class TBH_Hotkeys {
        [Serializable]
        public struct Hotkey {
            public KeyCode key;
            public bool shift;
            public bool alt;
            public bool ctrl;
        }
        [Serializable]
        public struct Settings {
            public bool enabled;
            public Hotkey deselect;
            public Hotkey frame;
            public Hotkey cycleTools;
            public Hotkey noTool;
        }
        static bool initialized;
        static UnityEngine.Object[] sceneWindows;
        static UnityEngine.Object[] hierarchyWindows;
        static System.Type sceneWindowType;
        static System.Type hierarchyWindowType;
        static Settings settings;
        
        public static void Initialize(ref Settings settings) {
            TBH_Hotkeys.settings = settings;
            if (settings.enabled) {
                if (initialized) return;
                var gehInfo = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                var gehFunc = (EditorApplication.CallbackFunction)gehInfo.GetValue(null);
                gehFunc -= UnityEditor_Hotkeys_KeyPress;
                gehFunc += UnityEditor_Hotkeys_KeyPress;
                gehInfo.SetValue(null, gehFunc);
                initialized = true;
            } else {
                if (!initialized) return;
                Shutdown();
            }
        }
        public static void Shutdown() {
            var gehInfo = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var gehFunc = (EditorApplication.CallbackFunction)gehInfo.GetValue(null);
            gehFunc -= UnityEditor_Hotkeys_KeyPress;
            gehInfo.SetValue(null, gehFunc);
            initialized = false;
        }
        
        static void UnityEditor_Hotkeys_KeyPress() {
            var e = Event.current;
            if (e.type != EventType.KeyDown || e.keyCode == KeyCode.None) return;
            
            if (HotkeyPressed(e, ref settings.deselect)) {
                if (Selection.activeGameObject != null) {
                    if (
                        (FindSceneWindows() && OneOfWindowsIsFocused(sceneWindows)) ||
                        (FindHierarchyWindow() && OneOfWindowsIsFocused(hierarchyWindows))
                        ) Selection.objects = null;
                }
                return;
            }
            if (HotkeyPressed(e, ref settings.frame)) {
                SceneView.FrameLastActiveSceneView();
                return;
            }
            if (HotkeyPressed(e, ref settings.cycleTools)) {
                switch (Tools.current) {
                    case Tool.Move:
                        Tools.current = Tool.Rotate;
                        break;
                    case Tool.Rotate:
                        Tools.current = Tool.Scale;
                        break;
                    default:
                        Tools.current = Tool.Move;
                        break;
                }
                return;
            }
            if (HotkeyPressed(e, ref settings.noTool)) {
                Tools.current = Tool.None;
                return;
            }
        }
        static bool HotkeyPressed(Event e, ref Hotkey hotkey) {
            if (e.keyCode == hotkey.key && e.shift == hotkey.shift && e.alt == hotkey.alt && e.control == hotkey.ctrl) return true;
            return false;
        }
        static bool OneOfWindowsIsFocused(UnityEngine.Object[] windows) {
            var focusedWindow = EditorWindow.focusedWindow;
            if (focusedWindow == null) return false;
            for (int i = 0; i < windows.Length; i++) if (windows[i] == focusedWindow) return true;
            return false;
        }
        static bool FindSceneWindows() {
            if (sceneWindows != null && sceneWindows.Length > 0 && sceneWindows[0] != null) return true;
            if (sceneWindowType == null) sceneWindowType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.SceneView");

            sceneWindows = Resources.FindObjectsOfTypeAll(sceneWindowType);
            return sceneWindows != null;
        }
        static bool FindHierarchyWindow() {
            if (hierarchyWindows != null && hierarchyWindows.Length > 0 && hierarchyWindows[0] != null) return true;
            if (hierarchyWindowType == null) hierarchyWindowType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            
            hierarchyWindows = Resources.FindObjectsOfTypeAll(hierarchyWindowType);
            return hierarchyWindows != null;
        }
    }
    
    [CustomPropertyDrawer(typeof(TBH_Hotkeys.Settings))]
    class TBH_Hotkeys_Settings_PropertyDrawer : PropertyDrawer {
        bool initialized;
        SerializedProperty enabled;
        SerializedProperty[] hotkeys;
        
        void Initialize(SerializedProperty property, GUIContent label) {
            enabled = property.FindPropertyRelative(nameof(enabled));
            
            hotkeys = new SerializedProperty[4];
            hotkeys[0] = property.FindPropertyRelative("deselect");
            hotkeys[1] = property.FindPropertyRelative("frame");
            hotkeys[2] = property.FindPropertyRelative("cycleTools");
            hotkeys[3] = property.FindPropertyRelative("noTool");
            
            initialized = true;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!initialized) Initialize(property, label);
            EditorGUI.BeginProperty(position, label, property);
            
            EditorGUI.PropertyField(position, enabled, new GUIContent("Hotkeys"));
            if (enabled.boolValue) {
                for (int i = 0; i < hotkeys.Length; i++) EditorGUILayout.PropertyField(hotkeys[i]);
            }
            
            EditorGUI.EndProperty();
        }
    }
    
    [CustomPropertyDrawer(typeof(TBH_Hotkeys.Hotkey))]
    class TBH_Hotkeys_Hotkey_PropertyDrawer : PropertyDrawer {
        bool initialized;
        SerializedProperty key;
        SerializedProperty shift;
        SerializedProperty alt;
        SerializedProperty ctrl;
        
        static GUIStyle hotkeyLabel;
        static GUIStyle hotkeyDropdownStyle;
        static GUIStyle hotkeyModifierStyle;
        
        void Initialize(SerializedProperty property, GUIContent label) {
            if (hotkeyLabel == null) {
                hotkeyLabel = new GUIStyle(GUI.skin.label);
                hotkeyLabel.fixedWidth = 100;
                hotkeyLabel.padding.left += 15;
                hotkeyDropdownStyle = new GUIStyle(EditorStyles.popup);
                hotkeyDropdownStyle.fixedWidth = 140;
                hotkeyModifierStyle = new GUIStyle(GUI.skin.toggle);
                hotkeyModifierStyle.fixedWidth = 50;
            }
            
            key    = property.FindPropertyRelative("key");
            shift  = property.FindPropertyRelative("shift");
            alt    = property.FindPropertyRelative("alt");
            ctrl   = property.FindPropertyRelative("ctrl");
            
            initialized = true;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!initialized) Initialize(property, label);
            EditorGUI.BeginProperty(position, label, property);
            
            var rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;
            
            GUI.Label(rect, label, hotkeyLabel);
            rect.x += hotkeyLabel.fixedWidth;
            rect.width -= hotkeyLabel.fixedWidth;
            key.intValue = (int)(KeyCode)EditorGUI.EnumPopup(rect, (KeyCode)key.intValue, hotkeyDropdownStyle);
            rect.y += EditorGUIUtility.singleLineHeight - 1;
            hotkeyModifierStyle.fixedWidth = 49;
            rect.width = 53;
            shift.boolValue = GUI.Toggle(rect, shift.boolValue, "Shift", hotkeyModifierStyle);
            rect.x += 53;
            hotkeyModifierStyle.fixedWidth = 39;
            rect.width = 43;
            alt.boolValue = GUI.Toggle(rect, alt.boolValue, "Alt", hotkeyModifierStyle);
            rect.x += 43;
            hotkeyModifierStyle.fixedWidth = 40;
            rect.width = 40;
            ctrl.boolValue = GUI.Toggle(rect, ctrl.boolValue, "Ctrl", hotkeyModifierStyle);
            
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight - 3;
        }
    }
}