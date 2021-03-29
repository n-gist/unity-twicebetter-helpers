using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;

namespace twicebetter.helpers {
    static class DelayedRefresher {
        private static int  editorPID;
        private static bool focused;
        private static long lastFocusTime;
        private static bool refreshed;
        private static bool refreshDuringPlaying;
        private static int  delay = 0;

        public static void Initialize() {
            #if UNITY_EDITOR && UNITY_EDITOR_WIN
            Shutdown();
            editorPID = Process.GetCurrentProcess().Id;
            focused = editorPID == FocusedWindowProcessId();
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            lastFocusTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            refreshed = false;
            UpdateSettings();
            #endif
        }
        public static void SetDelay(int ms) {
            delay = ms;
        }
        public static void Shutdown() {
            EditorApplication.update -= Update;
        }
        
        static void Update() {
            if (focused) {
                if (!refreshed) {
                    if (   (!EditorApplication.isPlaying || refreshDuringPlaying)
                        && !EditorApplication.isUpdating
                        && !EditorApplication.isCompiling) {
                            long nowTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                            if (nowTime > lastFocusTime + delay) {
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
}