#if TRUE && UNITY_EDITOR
namespace twicebetter {
    using UnityEngine;
    using UnityEditor;

    [InitializeOnLoad]
    public static class TB_UnityEditor_Hotkeys {
        static TB_UnityEditor_Hotkeys() {
            System.Reflection.FieldInfo gehInfo = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            EditorApplication.CallbackFunction gehValue = (EditorApplication.CallbackFunction)gehInfo.GetValue(null);
            gehValue += UnityEditor_Hotkeys_KeyPress;
            gehInfo.SetValue(null, gehValue);
        }
        static void UnityEditor_Hotkeys_KeyPress() {
            var e = Event.current;
            if (!(e.control) || e.type != EventType.KeyDown) return;
            
            
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
    }
}
#endif