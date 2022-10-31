using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    static class TBH_Refresher {
        public enum Mode {INTACT, ON, OFF, DELAYED}
        [Serializable]
        public struct Settings {
            public Mode mode;
            public int delay;
        }
        static bool active;
        static int  editorPID;
        static bool focused;
        static long lastFocusTime;
        static bool refreshed;
        static bool refreshDuringPlaying;
        static Settings settings;
        
        public static void Initialize(ref Settings settings) {
            #if UNITY_EDITOR && UNITY_EDITOR_WIN
            if (settings.mode != Mode.DELAYED && active) Shutdown();
            if (settings.mode != Mode.INTACT) {
                TBH_Refresher.settings = settings;
                const string autoRefreshPrefName = "kAutoRefreshMode";
                var autoRefreshSetting = settings.mode == Mode.ON ? 1 : 0;
                if (EditorPrefs.GetInt(autoRefreshPrefName, -1) != autoRefreshSetting) EditorPrefs.SetInt(autoRefreshPrefName, autoRefreshSetting);
                
                if (settings.mode == Mode.DELAYED) {
                    if (!active) {
                        editorPID = Process.GetCurrentProcess().Id;
                        focused = editorPID == FocusedWindowProcessId();
                        EditorApplication.update += Update;
                        lastFocusTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                        refreshed = false;
                        UpdateSettings();
                        active = true;
                    }
                }
            }
            #endif
        }
        public static void Shutdown() {
            #if UNITY_EDITOR && UNITY_EDITOR_WIN
            EditorApplication.update -= Update;
            active = false;
            #endif
        }
        
        static void Update() {
            if (focused) {
                if (!refreshed) {
                    if (   (!EditorApplication.isPlaying || refreshDuringPlaying)
                        && !EditorApplication.isUpdating
                        && !EditorApplication.isCompiling) {
                            long nowTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                            if (nowTime > lastFocusTime + settings.delay) {
                                refreshed = true;
                                if (!EditorApplication.isPlaying) UpdateSettings();
                                AssetDatabase.Refresh();
                            }
                        }
                }
                if (FocusedWindowProcessId() != editorPID) focused = false;
            } else {
                if (FocusedWindowProcessId() == editorPID) {
                    lastFocusTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                    focused = true;
                    refreshed = false;
                }
            }
        }
        static void UpdateSettings() {
            refreshDuringPlaying = EditorPrefs.GetInt("ScriptCompilationDuringPlay", -1) == 0;
        }
        
        static int FocusedWindowProcessId() {
            #if UNITY_EDITOR && UNITY_EDITOR_WIN
            var foregroundHandle = GetForegroundWindow();
            if (foregroundHandle != IntPtr.Zero) {
                GetWindowThreadProcessId(foregroundHandle, out int foregroundWindowProcessId);
                return foregroundWindowProcessId;
            }
            #endif
            return -1;
        }
        
        #if UNITY_EDITOR && UNITY_EDITOR_WIN
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        static extern int GetWindowThreadProcessId(IntPtr h, out int pId);
        #endif
    }
    
    [CustomPropertyDrawer(typeof(TBH_Refresher.Settings))]
    class TBH_Refresher_Settings_PropertyDrawer : PropertyDrawer {
        bool initialized;
        SerializedProperty mode;
        SerializedProperty delay;
        
        static GUIStyle modeDropdownStyle;
        static GUIStyle shortIntStyle;
       
        void Initialize(SerializedProperty property, GUIContent label) {
            if (modeDropdownStyle == null) {
                modeDropdownStyle = new GUIStyle(EditorStyles.popup);
                modeDropdownStyle.fixedWidth = 80;
                shortIntStyle = new GUIStyle(GUI.skin.textField);
                shortIntStyle.fixedWidth = 50;
            }
            
            mode  = property.FindPropertyRelative(nameof(mode));
            delay = property.FindPropertyRelative(nameof(delay));
            
            initialized = true;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!initialized) Initialize(property, label);
            EditorGUI.BeginProperty(position, label, property);
            
            var rect = position;
            
            #if !UNITY_EDITOR_WIN
            var guiEnabled = GUI.enabled;
            GUI.enabled = false;
            #endif
            mode.intValue = (int)(TBH_Refresher.Mode)EditorGUI.EnumPopup(rect, "Auto Refresh", (TBH_Refresher.Mode)mode.intValue, modeDropdownStyle);
            if (mode.intValue == (int)TBH_Refresher.Mode.DELAYED) {
                delay.intValue = EditorGUILayout.IntField("Auto Refresh Delay", delay.intValue, shortIntStyle);
            }
            #if !UNITY_EDITOR_WIN
            GUI.enabled = guiEnabled;
            #endif
            
            
            EditorGUI.EndProperty();
        }
    }
}