using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    class TBH_Settings : ScriptableObject {
        // static string GUID;
        const  string lastSettingsPathKey = "TBHSp";
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
        
        public static void   Define() {
            settings = null;
            
            var lastSettingsPath = EditorPrefs.GetString(lastSettingsPathKey, null);
            settings = AssetDatabase.LoadAssetAtPath<TBH_Settings>(lastSettingsPath);
            
            if (settings == null) DefineBySearching();
            
            if (settings == null) DefineByCreation();
        }
        static        void   DefineBySearching() {
            var existingPaths = AssetDatabase.FindAssets("t:" + nameof(TBH_Settings));
            var settingsAssetName = SettingsAssetName();
            if (existingPaths.Length == 0) existingPaths = AssetDatabase.FindAssets(settingsAssetName);
            var match = $"/{settingsAssetName}.asset";
            for (int i = 0; i < existingPaths.Length; i++) {
                var existingPath = AssetDatabase.GUIDToAssetPath(existingPaths[i]);
                if (match.Equals(existingPath.Substring(existingPath.Length - match.Length))) {
                    settings = AssetDatabase.LoadAssetAtPath<TBH_Settings>(existingPath);
                    if (settings != null) EditorPrefs.SetString(lastSettingsPathKey, existingPath);
                    return;
                }
            }
        }
        static        void   DefineByCreation() {
            settings = ScriptableObject.CreateInstance<TBH_Settings>();
            settings.FillDefaults(SystemInfo.operatingSystemFamily, System.Environment.UserName);
            var path = "Assets/" + SettingsAssetName() + ".asset";
            AssetDatabase.CreateAsset(settings, path);
            EditorPrefs.SetString(lastSettingsPathKey, path);
        }
        static        string SettingsAssetName() {
            return "HelpersSettings." + SystemInfo.operatingSystemFamily.ToString() + "." + System.Environment.UserName;
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