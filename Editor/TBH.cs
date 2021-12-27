using UnityEditor;
using System.Threading.Tasks;

namespace twicebetter.helpers {
    
    [InitializeOnLoad]
    static class TBH {
        static bool initialized;
        
        static TBH() {
            EditorApplication.update += DelayedInit;
        }
        static async void DelayedInit() {
            EditorApplication.update -= DelayedInit;
            await Task.Delay(200);
            TBH_Settings.Define();
            Initialize();
        }
        
        public static void Initialize() {
            TBH_Settings settings = TBH_Settings.Get();
            if (settings == null) return;
            if (!settings.enabled) {
                if (initialized) {
                    TBH_Refresher.Shutdown();
                    TBH_Hotkeys.Shutdown();
                    initialized = false;
                }
                return;
            }
            
            // Refresher
            TBH_Refresher.Initialize(ref settings.refresher);
            
            // Yaml Merge Registrator
            if (settings.registerYamlMerge) TBH_YamlMergeRegistrator.Register();
            
            // Hotkeys
            TBH_Hotkeys.Initialize(ref settings.hotkeys);
            
            initialized = true;
        }
    }
}
