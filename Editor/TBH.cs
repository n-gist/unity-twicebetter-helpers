using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    
    [InitializeOnLoad]
    static class TBH {
        static bool initialized;
        
        static TBH() {
            TBH_Settings.Define();
            Initialize();
        }
        
        public static void Initialize() {
            TBH_Settings settings = TBH_Settings.Get();
            if (settings == null) return;
            if (!settings.enabled) {
                if (initialized) {
                    TBH_Refresher.Shutdown();
                    TBH_SceneFocusKeeper.Shutdown();
                    TBH_Hotkeys.Shutdown();
                    initialized = false;
                }
                return;
            }
            
            // Refresher
            TBH_Refresher.Initialize(ref settings.refresher);
            
            // Scene Focus Keeper
            if (settings.keepSceneFocused) TBH_SceneFocusKeeper.Initialize();
            else TBH_SceneFocusKeeper.Shutdown();
            
            // Yaml Merge Registrator
            if (settings.registerYamlMerge) TBH_YamlMergeRegistrator.Register();
            
            // Hotkeys
            TBH_Hotkeys.Initialize(ref settings.hotkeys);
            
            initialized = true;
        }
    }
}
