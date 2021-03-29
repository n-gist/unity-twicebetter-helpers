using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    static class EditorHotkeys {
        static Object[] sceneWindows;
        static Object[] hierarchyWindows;
        static System.Type sceneWindowType;
        static System.Type hierarchyWindowType;
        static KeyCode deselectKey;
        static bool deselectAlt;
        static bool deselectCtrl;
        static KeyCode frameKey;
        static bool frameAlt;
        static bool frameCtrl;
        static KeyCode cycleToolsKey;
        static bool cycleToolsAlt;
        static bool cycleToolsCtrl;
        static KeyCode noToolKey;
        static bool noToolAlt;
        static bool noToolCtrl;
        
        public static void Initialize() {
            var gehInfo = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var gehFunc = (EditorApplication.CallbackFunction)gehInfo.GetValue(null);
            gehFunc -= UnityEditor_Hotkeys_KeyPress;
            gehFunc += UnityEditor_Hotkeys_KeyPress;
            gehInfo.SetValue(null, gehFunc);
        }
        public static void Shutdown() {
            var gehInfo = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var gehFunc = (EditorApplication.CallbackFunction)gehInfo.GetValue(null);
            gehFunc -= UnityEditor_Hotkeys_KeyPress;
            gehInfo.SetValue(null, gehFunc);
        }
        public static void SetDeselectKey(KeyCode key, bool alt, bool ctrl) {
            deselectKey = key;
            deselectAlt = alt;
            deselectCtrl = ctrl;
        }
        public static void SetFrameKey(KeyCode key, bool alt, bool ctrl) {
            frameKey = key;
            frameAlt = alt;
            frameCtrl = ctrl;
        }
        public static void SetCycleToolsKey(KeyCode key, bool alt, bool ctrl) {
            cycleToolsKey = key;
            cycleToolsAlt = alt;
            cycleToolsCtrl = ctrl;
        }
        public static void SetNoToolKey(KeyCode key, bool alt, bool ctrl) {
            noToolKey = key;
            noToolAlt = alt;
            noToolCtrl = ctrl;
        }
        
        static void UnityEditor_Hotkeys_KeyPress() {
            var e = Event.current;
            if (e.type != EventType.KeyDown || e.keyCode == KeyCode.None) return;
            
            // Deselect
            if (e.keyCode == deselectKey && e.alt == deselectAlt && e.control == deselectCtrl) {
                if (Selection.activeGameObject != null) {
                    if (
                        (FindSceneWindows() && OneOfWindowsIsFocused(sceneWindows)) ||
                        (FindHierarchyWindow() && OneOfWindowsIsFocused(hierarchyWindows))
                        ) Selection.objects = null;
                }
            }
            // Frame
            if (e.keyCode == frameKey && e.alt == frameAlt && e.control == frameCtrl) {
                SceneView.FrameLastActiveSceneView();
            }
            // Cycle Tools
            if (e.keyCode == cycleToolsKey && e.alt == cycleToolsAlt && e.control == cycleToolsCtrl) {
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
            }
            // No Tool
            if (e.keyCode == noToolKey && e.alt == noToolAlt && e.control == noToolCtrl) {
                Tools.current = Tool.None;
            }
        }
        static bool OneOfWindowsIsFocused(Object[] windows) {
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
}