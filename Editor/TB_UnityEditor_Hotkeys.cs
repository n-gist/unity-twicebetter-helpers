#if TRUE && UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    
    [InitializeOnLoad]
    static class TB_UnityEditor_Hotkeys {
        static Object[] sceneWindows;
        static Object[] hierarchyWindows;
        static System.Type sceneWindowType;
        static System.Type hierarchyWindowType;
        
        static TB_UnityEditor_Hotkeys() {
            System.Reflection.FieldInfo gehInfo = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            EditorApplication.CallbackFunction gehValue = (EditorApplication.CallbackFunction)gehInfo.GetValue(null);
            gehValue += UnityEditor_Hotkeys_KeyPress;
            gehInfo.SetValue(null, gehValue);
        }
        
        static void UnityEditor_Hotkeys_KeyPress() {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;
            
            switch (e.keyCode) {
                case KeyCode.Escape:
                if (Selection.activeGameObject == null) break;
                    if (
                        (FindSceneWindows() && OneOfWindowsIsFocused(sceneWindows)) ||
                        (FindHierarchyWindow() && OneOfWindowsIsFocused(hierarchyWindows))
                        ) Selection.objects = null;
                    break;
            }
            
            if (!(e.control)) return;
            
            switch (e.keyCode) {
                case KeyCode.F7:
                    if (e.alt) {
                        SceneView.FrameLastActiveSceneView();
                        break;
                    }
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
                    break;
                case KeyCode.F8:
                    Tools.current = Tool.None;
                    break;
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
#endif