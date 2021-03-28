#if FALSE && UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace twicebetter.helpers {
    
    [InitializeOnLoad]
    public class TB_YamlMergeRegistrator {
        const string prefsKey = "TB_YMR_rv";
        const string YAMLexecutableName = "UnityYAMLMerge";
        const string gitExecutableName = "git";
        const string configstring = "config merge.unityyamlmerge.";

        static TB_YamlMergeRegistrator() {
            var installedUnityKey = EditorPrefs.GetString(prefsKey);
            if (installedUnityKey != Application.unityVersion) YamlMergeRegister();
        }
        
        // [MenuItem("Window/Twice Better/YamlMerge registration")]
        static void YamlMergeRegister() {
            try {
                var os = SystemInfo.operatingSystemFamily;
                if (os != OperatingSystemFamily.Windows && os != OperatingSystemFamily.MacOSX) return;
                if (os == OperatingSystemFamily.Windows) YAMLexecutableName += ".exe";
                var UnityYAMLMergePath = EditorApplication.applicationContentsPath + "/Tools" + "/" + YAMLexecutableName;
                
                ExecuteGitWithParams(configstring + "name \"UnityYamlMerge\"");
                ExecuteGitWithParams(configstring + "recursive binary");
                ExecuteGitWithParams(configstring + "driver \"'" + UnityYAMLMergePath + "' merge -p -h --force --fallback none %O %B %A %A\"");
                EditorPrefs.SetString(prefsKey, Application.unityVersion);
                Debug.Log($"{YAMLexecutableName} registered");
            } catch (Exception e) {
                Debug.Log($"Fail to register {YAMLexecutableName} with error: {e}");
            }
        }

        // [MenuItem("Window/Twice Better/YamlMerge unregistration")]
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
#endif