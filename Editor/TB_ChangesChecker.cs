﻿#if TRUE && UNITY_EDITOR && UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;

namespace twicebetter.helpers {

    [InitializeOnLoad]
    public static class TB_ChangesChecker {
        private static int  editorPID;
        private static bool focused;
        private static long lastFocusTime;
        private static bool refreshed;
        private static bool refreshDuringPlaying;

        static TB_ChangesChecker() {
            editorPID = Process.GetCurrentProcess().Id;
            focused = editorPID == FocusedWindowProcessId();
            EditorApplication.update += Update;
            lastFocusTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            refreshed = false;
        }
        
        private static void Update() {
            if (focused) {
                if (!refreshed) {
                    if (   (!EditorApplication.isPlaying || refreshDuringPlaying)
                        && !EditorApplication.isUpdating
                        && !EditorApplication.isCompiling) {
                            long nowTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                            if (nowTime > lastFocusTime + 20) {
                                refreshed = true;
                                refreshDuringPlaying = EditorPrefs.GetInt("ScriptCompilationDuringPlay", -1) == 0;
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
        
        private static int FocusedWindowProcessId() {
            var foregroundHandle = GetForegroundWindow();
            if (foregroundHandle != IntPtr.Zero) {
                GetWindowThreadProcessId(foregroundHandle, out int foregroundWindowProcessId);
                return foregroundWindowProcessId;
            }
            return -1;
        }
        
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern int GetWindowThreadProcessId(IntPtr h, out int pId);
    }
}
#endif