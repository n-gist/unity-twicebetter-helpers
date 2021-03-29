using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    class TB_Helpers_Settings_SO : ScriptableObject {
        public bool autoRefresh = true;
        public bool delayedAutoRefresh = false;
        public int  delayedAutoRefreshDelay;
        public bool keepSceneFocused = false;
        public bool registerYamlMerge = false;
        public bool hotkeys = true;
        public KeyCode deselectKey = KeyCode.Escape;
        public bool deselectAlt;
        public bool deselectCtrl;
        public KeyCode frameKey = KeyCode.None;
        public bool frameAlt;
        public bool frameCtrl;
        public KeyCode cycleToolsKey = KeyCode.None;
        public bool cycleToolsAlt;
        public bool cycleToolsCtrl;
        public KeyCode noToolKey = KeyCode.None;
        public bool noToolAlt;
        public bool noToolCtrl;
        
        public TB_Helpers_Settings_SO() {}
        public void FillDefaults(OperatingSystemFamily os, string user) {
            if (os == OperatingSystemFamily.Windows && user == "Nick") {
                delayedAutoRefresh = true;
                delayedAutoRefreshDelay = 30;
                frameKey = KeyCode.F7;
                frameAlt = true;
                frameCtrl = true;
                cycleToolsKey = KeyCode.F7;
                cycleToolsCtrl = true;
                noToolKey = KeyCode.F8;
                noToolCtrl = true;
            }
            if (os == OperatingSystemFamily.MacOSX) {
                cycleToolsKey = KeyCode.F14;
                noToolKey = KeyCode.F13;
            }
        }
        void OnValidate() {
            if (delayedAutoRefreshDelay < 0) delayedAutoRefreshDelay = 0;
            Helpers.SettingsModified(this);
        }
    }
    
    [CustomEditor(typeof(TB_Helpers_Settings_SO))]
    class TB_Settings_SO_Editor : Editor {
        SerializedProperty autoRefresh;
        SerializedProperty delayedAutoRefresh;
        SerializedProperty delayedAutoRefreshDelay;
        SerializedProperty keepSceneFocused;
        SerializedProperty registerYamlMerge;
        SerializedProperty hotkeys;
        SerializedProperty deselectKey;
        SerializedProperty deselectAlt;
        SerializedProperty deselectCtrl;
        SerializedProperty frameKey;
        SerializedProperty frameAlt;
        SerializedProperty frameCtrl;
        SerializedProperty cycleToolsKey;
        SerializedProperty cycleToolsAlt;
        SerializedProperty cycleToolsCtrl;
        SerializedProperty noToolKey;
        SerializedProperty noToolAlt;
        SerializedProperty noToolCtrl;
        
        bool initialized;
        GUIStyle hotkeyLabel;
        GUIStyle shortIntStyle;
        GUIStyle hotkeyDropdownStyle;
        GUIStyle hotkeyModifierStyle;
        
        void Initialize() {
            autoRefresh = serializedObject.FindProperty(nameof(autoRefresh));
            delayedAutoRefresh = serializedObject.FindProperty(nameof(delayedAutoRefresh));
            delayedAutoRefreshDelay = serializedObject.FindProperty(nameof(delayedAutoRefreshDelay));
            keepSceneFocused = serializedObject.FindProperty(nameof(keepSceneFocused));
            registerYamlMerge = serializedObject.FindProperty(nameof(registerYamlMerge));
            
            hotkeys = serializedObject.FindProperty(nameof(hotkeys));
            deselectKey = serializedObject.FindProperty(nameof(deselectKey));
            deselectAlt = serializedObject.FindProperty(nameof(deselectAlt));
            deselectCtrl = serializedObject.FindProperty(nameof(deselectCtrl));
            frameKey = serializedObject.FindProperty(nameof(frameKey));
            frameAlt = serializedObject.FindProperty(nameof(frameAlt));
            frameCtrl = serializedObject.FindProperty(nameof(frameCtrl));
            cycleToolsKey = serializedObject.FindProperty(nameof(cycleToolsKey));
            cycleToolsAlt = serializedObject.FindProperty(nameof(cycleToolsAlt));
            cycleToolsCtrl = serializedObject.FindProperty(nameof(cycleToolsCtrl));
            noToolKey = serializedObject.FindProperty(nameof(noToolKey));
            noToolAlt = serializedObject.FindProperty(nameof(noToolAlt));
            noToolCtrl = serializedObject.FindProperty(nameof(noToolCtrl));
            
            hotkeyLabel = new GUIStyle(GUI.skin.label);
            hotkeyLabel.fixedWidth = 100;
            shortIntStyle = new GUIStyle(GUI.skin.textField);
            shortIntStyle.fixedWidth = 50;
            hotkeyDropdownStyle = new GUIStyle(EditorStyles.popup);
            hotkeyDropdownStyle.fixedWidth = 120;
            hotkeyModifierStyle = new GUIStyle(GUI.skin.toggle);
            hotkeyModifierStyle.fixedWidth = 40;
            
            initialized = true;
        }
        
        public override void OnInspectorGUI() {
            if (!initialized) Initialize();
            
            serializedObject.Update();
            var guiEnabled = GUI.enabled;
            
            if (delayedAutoRefresh.boolValue) GUI.enabled = false;
            EditorGUILayout.PropertyField(autoRefresh);
            if (delayedAutoRefresh.boolValue) GUI.enabled = true;
            
            EditorGUILayout.PropertyField(delayedAutoRefresh);
            
            if (!delayedAutoRefresh.boolValue) GUI.enabled = false;
            EditorGUI.indentLevel++;
            delayedAutoRefreshDelay.intValue = EditorGUILayout.IntField("Delay", delayedAutoRefreshDelay.intValue, shortIntStyle);
            EditorGUI.indentLevel--;
            if (!delayedAutoRefresh.boolValue) GUI.enabled = true;
            
            EditorGUILayout.PropertyField(keepSceneFocused);
            
            EditorGUILayout.PropertyField(registerYamlMerge);
            
            EditorGUILayout.PropertyField(hotkeys);
            if (hotkeys.boolValue) {
                HotkeyProperty("Deselect", deselectKey, deselectAlt, deselectCtrl);
                HotkeyProperty("Frame", frameKey, frameAlt, frameCtrl);
                HotkeyProperty("Cycle Tools", cycleToolsKey, cycleToolsAlt, cycleToolsCtrl);
                HotkeyProperty("No Tool", noToolKey, noToolAlt, noToolCtrl);
            }
            
            GUI.enabled = guiEnabled;
            serializedObject.ApplyModifiedProperties();
        }
        
        void HotkeyProperty(string label, SerializedProperty key, SerializedProperty alt, SerializedProperty ctrl) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.Label(label, hotkeyLabel);
            key.intValue = (int)(KeyCode)EditorGUILayout.EnumPopup((KeyCode)key.intValue, hotkeyDropdownStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(hotkeyLabel.fixedWidth + 20);
            alt.boolValue = GUILayout.Toggle(alt.boolValue, "Alt", hotkeyModifierStyle);
            ctrl.boolValue = GUILayout.Toggle(ctrl.boolValue, "Ctrl", hotkeyModifierStyle);
            EditorGUILayout.EndHorizontal();
        }
    }
}