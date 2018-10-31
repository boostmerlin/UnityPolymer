using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class EditorHelpers
{
    public struct CmdResult
    {
        public int code;
        public string msg;
        public override string ToString()
        {
            return string.Format("code: {0}, result: {1}", code, msg);
        }
    }
    public static CmdResult RunCmd(string cmdExe, string args)
    {
        int code = -121;
        string result = string.Empty;
        try
        {
            using (System.Diagnostics.Process myPro = new System.Diagnostics.Process())
            {
               // myPro.StartInfo.FileName = "cmd.exe";
                myPro.StartInfo.FileName = cmdExe;
                myPro.StartInfo.Arguments = args;
                myPro.StartInfo.UseShellExecute = false;
                myPro.StartInfo.CreateNoWindow = true;
                myPro.StartInfo.RedirectStandardOutput = true;
                myPro.StartInfo.RedirectStandardError = true;
                myPro.Start();
                result = myPro.StandardError.ReadToEnd();
                if(string.IsNullOrEmpty(result))
                {
                    result = myPro.StandardOutput.ReadToEnd();
                }
                //   Debug.Log(result);

                myPro.WaitForExit();
                code = myPro.ExitCode;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogErrorFormat("some error on {0} {1} width exeception: {2}", cmdExe, args, e);
        }
        return new CmdResult() { code = code, msg = result };
    }

    public static string CreateAsset(Object asset, string pathname)
    {
        string p = pathname + ".asset";
        FileInfo fi = new FileInfo(p);
        if (!fi.Directory.Exists)
        {
            fi.Directory.Create();
        }

        AssetDatabase.CreateAsset(asset, p);

        return p;
    }
    public static List<T> CollectAll<T>(string path, bool recursive, string tag) where T : Object
    {
        List<T> l = new List<T>();
        if (!Directory.Exists(path))
        {
            Debug.LogWarning("Path not exist when CollectAll assets: " + path + " " + tag);
            return l;
        }

        string[] files = Directory.GetFiles(path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            if (file.Contains(".meta")) continue;
            string nfile = file.Replace("\\", "/");
            T asset = (T)AssetDatabase.LoadAssetAtPath(nfile, typeof(T));
            if (asset == null)
            {
                if (tag == "textures")
                {
                    Debug.Log("[" + tag + "]" + "need Reimport  " + typeof(T) + ": " + nfile);//fk, have to force update again, bug?
                    AssetDatabase.ImportAsset(nfile, ImportAssetOptions.ForceUpdate);
                    asset = (T)AssetDatabase.LoadAssetAtPath(nfile, typeof(T));
                }
                else
                {
                    continue;
                }
            }
            if (asset == null)
            {
                Debug.Log("[" + tag + "]" + "Asset IS NOT " + typeof(T) + ": " + nfile);
                continue;
            }
            l.Add(asset);
        }
        return l;
    }

    public static string args(string name)
    {
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith(name))
            {
                return arg.Split(new char[] { '=' }, 2)[1];
            }
        }

        return "";
    }

    public static string CreatePrefab(GameObject go, string name, bool canDestroyOrigin = false)
    {
        string p = name + ".prefab";
        FileInfo fi = new FileInfo(p);
        if (!fi.Directory.Exists)
        {
            fi.Directory.Create();
        }
        PrefabUtility.CreatePrefab(p, go);
        if (canDestroyOrigin && !AssetDatabase.IsSubAsset(go))
        {
            Object.DestroyImmediate(go);
        }
        return p;
    }

    public static string GetProductName()
    {
        return Application.productName;
    }
}
