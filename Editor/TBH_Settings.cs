using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    class TBH_Settings : ScriptableObject {
        const  string lastSettingsPathKey = "TBHSp";
        const  string noCreationKey = "TBHnc";
        static TBH_Settings settings;
        
        public bool enabled = false;
        public TBH_Refresher.Settings refresher;
        public bool registerYamlMerge = false;
        public TBH_Hotkeys.Settings hotkeys;
        
        public void FillDefaults(OperatingSystemFamily os, string user) {
            if (os == OperatingSystemFamily.Windows && user == "Nick") {
                enabled = true;
                settings.refresher.mode = TBH_Refresher.Mode.DELAYED;
                settings.refresher.delay = 100;
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
        
        [MenuItem("Window/Twice Better/Helpers/Settings")]
        static void MenuSettings() {
            EditorPrefs.DeleteKey(noCreationKey);
            Define();
            TBH.Initialize();
            if (settings != null) Selection.activeObject = settings;
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
            var answeredNo = EditorPrefs.GetBool(noCreationKey, false);
            if (answeredNo) return;
            
            const string title = "TwiceBetter Helpers";
            if (!EditorUtility.DisplayDialog(title, "No Helpers Settings found for current user, create?", "Yes", "No")) {
                EditorPrefs.SetBool(noCreationKey, true);
                EditorUtility.DisplayDialog(title, "You can create settings by selecting Window/Twice Better/Helpers/Settings", "OK");
                return;
            }
            
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
        SerializedProperty registerYamlMerge;
        SerializedProperty hotkeys;
        
        bool initialized;
        
        void Initialize() {
            enabled = serializedObject.FindProperty(nameof(enabled));
            
            refresher = serializedObject.FindProperty(nameof(refresher));
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