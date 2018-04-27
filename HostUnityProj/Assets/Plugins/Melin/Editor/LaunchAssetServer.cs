using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace ML
{
    internal class LaunchAssetServer : ScriptableSingleton<LaunchAssetServer>
    {
        const string kLocalAssetServerMenu = "Melin/Local Asset Server";

        int m_serverPID = 0;

        [MenuItem(kLocalAssetServerMenu, priority = 0)]
        public static void ToggleLocalServer()
        {
            if(EditorHelpers.RunCmd("where", "python").code == 1)
            {
                EditorUtility.DisplayDialog("^_^", "no python3+ found in PATH", "OK");
                return;
            }

            bool isRunning = IsRunning();
            if (!isRunning)
            {
                Run();
            }
            else
            {
                KillRunningServer();
            }
        }

        [MenuItem(kLocalAssetServerMenu, true)]
        public static bool ToggleLocalServerChecked()
        {
            bool isRunnning = IsRunning();
            Menu.SetChecked(kLocalAssetServerMenu, isRunnning);
            return true;
        }

        static bool IsRunning()
        {
            if (instance.m_serverPID == 0)
                return false;

            var process = Process.GetProcessById(instance.m_serverPID);
            if (process == null)
                return false;

            return !process.HasExited;
        }

        static void KillRunningServer()
        {
            try
            {
                if (instance.m_serverPID == 0)
                    return;

                var lastProcess = Process.GetProcessById(instance.m_serverPID);
                lastProcess.Kill();
                instance.m_serverPID = 0;
            }
            catch
            {
            }
        }

        void OnDisable()
        {
            KillRunningServer();
        }

        static void Run()
        {
            string serverroot = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
            KillRunningServer();

            ProcessStartInfo startInfo = new ProcessStartInfo("python", "-m http.server " + MelinConfig.Selfie.ServerPort);
            startInfo.WorkingDirectory = serverroot;
            startInfo.UseShellExecute = false;

            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            Process launchProcess = Process.Start(startInfo);

            if (launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0)
            {
                Log.ML.PrintError("Unable Start AssetServer process");
            }
            else
            {
                instance.m_serverPID = launchProcess.Id;
                Log.ML.Print("Local AssetServer Listen: {0}, Root Dir: {1}", MelinConfig.Selfie.ServerPort, serverroot);
            }
        }
    }
}