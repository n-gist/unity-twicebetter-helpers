using System;
using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    static class TBH_YamlMergeRegistrator {
        const string prefsKey = "TBH_YMR_rv";
        const string gitExecutableName = "git";
        const string configstring = "config merge.unityyamlmerge.";
        const string YAMLexecutableName = "UnityYAMLMerge";

        public static void Register() {
            var installedUnityKey = EditorPrefs.GetString(prefsKey);
            if (installedUnityKey != Application.unityVersion) YamlMergeRegister();
        }
        
        static void YamlMergeRegister() {
            string YAMLexeName = YAMLexecutableName;
            try {
                var os = SystemInfo.operatingSystemFamily;
                if (os != OperatingSystemFamily.Windows && os != OperatingSystemFamily.MacOSX) return;
                if (os == OperatingSystemFamily.Windows) YAMLexeName += ".exe";
                var UnityYAMLMergePath = EditorApplication.applicationContentsPath + "/Tools" + "/" + YAMLexeName;
                
                ExecuteGitWithParams(configstring + "name \"UnityYamlMerge\"");
                ExecuteGitWithParams(configstring + "recursive binary");
                ExecuteGitWithParams(configstring + "driver \"'" + UnityYAMLMergePath + "' merge -p -h --force --fallback none %O %B %A %A\"");
                EditorPrefs.SetString(prefsKey, Application.unityVersion);
                Debug.Log($"{YAMLexeName} registered");
            } catch (Exception e) {
                Debug.Log($"Fail to register {YAMLexeName} with error: {e}");
            }
        }

        static void YamlMergeUnregister() {
            ExecuteGitWithParams("config --remove-section merge.unityyamlmerge");
            Debug.Log($"{YAMLexecutableName} unregistered");
        }

        static string ExecuteGitWithParams(string param) {
            var processInfo = new System.Diagnostics.ProcessStartInfo(gitExecutableName);

            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = Environment.CurrentDirectory;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            processInfo.CreateNoWindow = true;

            var process = new System.Diagnostics.Process();
            process.StartInfo = processInfo;
            process.StartInfo.FileName = gitExecutableName;
            process.StartInfo.Arguments = param;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0) throw new Exception(process.StandardError.ReadLine());

            return process.StandardOutput.ReadLine();
        }
    }
}