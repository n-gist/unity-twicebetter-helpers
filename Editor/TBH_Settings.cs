using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    class TBH_Settings : ScriptableObject {
        static string GUID;
        static TBH_Settings settings;
        
        public bool enabled = false;
        public TBH_Refresher.Settings refresher;
        public bool keepSceneFocused = false;
        public bool registerYamlMerge = false;
        public TBH_Hotkeys.Settings hotkeys;
        
        public TBH_Settings() {}
        
        public void FillDefaults(OperatingSystemFamily os, string user) {
            if (os == OperatingSystemFamily.Windows && user == "Nick") {
                enabled = true;
                settings.refresher.mode = TBH_Refresher.Mode.DELAYED;
                settings.refresher.delay = 30;
                settings.hotkeys.enabled = true;
                settings.hotkeys.deselect.key = KeyCode.Escape;
                settings.hotkeys.frame.key = KeyCode.F7;
                settings.hotkeys.frame.alt = true;
                settings.hotkeys.frame.ctrl = true;
                settings.hotkeys.cycleTools.key = KeyCode.F7;
                settings.hotkeys.cycleTools.ctrl = true;
                settings.hotkeys.noTool.key = KeyCode.F8;
                settings.hotkeys.noTool.ctrl = true;
            }
        }
        
        public static void         Define() {
            if (!string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(GUID))) return;
            
            const string prefix = "HelpersSettings";
            var os = SystemInfo.operatingSystemFamily;
            var osstr = os.ToString();
            var user = System.Environment.UserName;
            var settingsAssetName = $"{prefix}.{osstr}.{user}.asset";
            var existingSettings = AssetDatabase.FindAssets("t:" + nameof(TBH_Settings));
            var match = $"/{settingsAssetName}";
            for (int i = 0; i < existingSettings.Length; i++) {
                var existingPath = AssetDatabase.GUIDToAssetPath(existingSettings[i]);
                if (match.Equals(existingPath.Substring(existingPath.Length - match.Length))) {
                    settings = AssetDatabase.LoadAssetAtPath<TBH_Settings>(existingPath);
                    GUID = existingSettings[i];
                    return;
                }
            }
            
            settings = ScriptableObject.CreateInstance<TBH_Settings>();
            settings.FillDefaults(os, user);
            var path = $"Assets/{settingsAssetName}";
            AssetDatabase.CreateAsset(settings, path);
            var guid = AssetDatabase.GUIDFromAssetPath(path);
            if (guid.Empty()) {
                settings = null;
                GUID = null;
                Debug.LogWarning("TwiceBetter Helpers: Failed to define settings");
                return;
            }
            
            GUID = guid.ToString();
        }
        public static TBH_Settings Get() => settings;
        
        void OnValidate() {
            if (this == settings) TBH.Initialize();
        }
    }
    
    [CustomEditor(typeof(TBH_Settings))]
    class TB_Settings_SO_Editor : Editor {
        SerializedProperty enabled;
        SerializedProperty refresher;
        SerializedProperty keepSceneFocused;
        SerializedProperty registerYamlMerge;
        SerializedProperty hotkeys;
        
        bool initialized;
        
        void Initialize() {
            enabled = serializedObject.FindProperty(nameof(enabled));
            
            refresher = serializedObject.FindProperty(nameof(refresher));
            keepSceneFocused = serializedObject.FindProperty(nameof(keepSceneFocused));
            registerYamlMerge = serializedObject.FindProperty(nameof(registerYamlMerge));
            hotkeys = serializedObject.FindProperty(nameof(hotkeys));
            
            initialized = true;
        }
        
        public override void OnInspectorGUI() {
            if (!initialized) Initialize();
            
            serializedObject.Update();
            var guiEnabled = GUI.enabled;
            
            EditorGUILayout.PropertyField(enabled);
            EditorGUILayout.Space(10);
            
            if (enabled.boolValue) {
                
                EditorGUILayout.PropertyField(refresher);
                EditorGUILayout.Space(10);
                
                EditorGUILayout.PropertyField(keepSceneFocused);
                EditorGUILayout.Space(10);
                
                EditorGUILayout.PropertyField(registerYamlMerge);
                EditorGUILayout.Space(10);
                
                EditorGUILayout.PropertyField(hotkeys);
                EditorGUILayout.Space(10);
                
            }
            
            GUI.enabled = guiEnabled;
            serializedObject.ApplyModifiedProperties();
        }
    }
}