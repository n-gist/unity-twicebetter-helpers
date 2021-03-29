using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    
    [InitializeOnLoad]
    static class Helpers {
        static TB_Helpers_Settings_SO settings_SO;
        static string settings_GUID;
        
        static Helpers() {
            CheckSettings();
            Raise();
        }
        
        static void CheckSettings() {
            if (!string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(settings_GUID))) return;
            
            const string prefix = "HelpersSettings";
            var os = SystemInfo.operatingSystemFamily;
            var osstr = os.ToString();
            var user = System.Environment.UserName;
            var settingsAssetName = $"{prefix}.{osstr}.{user}.asset";
            var existingSettings = AssetDatabase.FindAssets("t:" + nameof(TB_Helpers_Settings_SO));
            var match = $"/{settingsAssetName}";
            for (int i = 0; i < existingSettings.Length; i++) {
                var existingPath = AssetDatabase.GUIDToAssetPath(existingSettings[i]);
                if (match.Equals(existingPath.Substring(existingPath.Length - match.Length))) {
                    settings_SO = AssetDatabase.LoadAssetAtPath<TB_Helpers_Settings_SO>(existingPath);
                    settings_GUID = existingSettings[i];
                    return;
                }
            }
            
            settings_SO = ScriptableObject.CreateInstance<TB_Helpers_Settings_SO>();
            settings_SO.FillDefaults(os, user);
            var path = $"Assets/{settingsAssetName}";
            AssetDatabase.CreateAsset(settings_SO, path);
            var guid = AssetDatabase.GUIDFromAssetPath(path);
            if (guid.Empty()) {
                settings_SO = null;
                settings_GUID = null;
                Debug.LogWarning("TwiceBetter Helpers: Failed to create settings");
                return;
            }
            
            settings_GUID = guid.ToString();
        }
        
        public static void SettingsModified(ScriptableObject settingsSO) {
            if (settingsSO != settings_SO) return;
            
            Raise();
        }
        
        static void Raise() {
            if (settings_SO == null) return;
            
            // Auto Refresh
            const string prefName = "kAutoRefresh";
            var autoRefreshSetting = !settings_SO.autoRefresh || settings_SO.delayedAutoRefresh ? 0 : 1;
            if (EditorPrefs.GetInt(prefName, -1) != autoRefreshSetting) EditorPrefs.SetInt(prefName, autoRefreshSetting);
            
            // Delayed Auto Refresh
            if (settings_SO.delayedAutoRefresh) {
                DelayedRefresher.Initialize();
                DelayedRefresher.SetDelay(settings_SO.delayedAutoRefreshDelay);
            } else DelayedRefresher.Shutdown();
            
            // Scene Focus Keeper
            if (settings_SO.keepSceneFocused) SceneFocusKeeper.Initialize();
            else SceneFocusKeeper.Shutdown();
            
            // Yaml Merge Registrator
            if (settings_SO.registerYamlMerge) YamlMergeRegistrator.Register();
            
            // Hotkeys
            if (settings_SO.hotkeys) {
                EditorHotkeys.Initialize();
                EditorHotkeys.SetDeselectKey(settings_SO.deselectKey, settings_SO.deselectAlt, settings_SO.deselectCtrl);
                EditorHotkeys.SetFrameKey(settings_SO.frameKey, settings_SO.frameAlt, settings_SO.frameCtrl);
                EditorHotkeys.SetCycleToolsKey(settings_SO.cycleToolsKey, settings_SO.cycleToolsAlt, settings_SO.cycleToolsCtrl);
                EditorHotkeys.SetNoToolKey(settings_SO.noToolKey, settings_SO.noToolAlt, settings_SO.noToolCtrl);
            } else EditorHotkeys.Shutdown();
            
        }
    }
}
