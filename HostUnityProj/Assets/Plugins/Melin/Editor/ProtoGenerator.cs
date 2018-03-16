using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class ProtoGenerator : EditorWindow
{
    const string kMenuProtoGen = "Melin/Protobuf Generator";

    const string protoc = @"../Tools/bin/protoc.exe";

    List<string> protopaths = new List<string>();

    string csharp_out;
    string extra_options;
    string exclude_proto_keywords;

    [MenuItem(kMenuProtoGen)]
    static void Execute()
    {
        var w = GetWindow<ProtoGenerator>(true, "Proto Generate Options");
        w.Show();
    }

    void Awake()
    {
        csharp_out = EditorPrefs.GetString("csharp_out", csharp_out);
        exclude_proto_keywords = EditorPrefs.GetString("exclude_proto_keywords", exclude_proto_keywords);
        EditorGUIHepler.GetListPrefs(protopaths, "proto_path");
    }

    void gen()
    {
        List<string> args = new List<string>();
        if (string.IsNullOrEmpty(csharp_out))
        { return; }
        if (protopaths.Count == 0)
        {
            return;
        }
        foreach (var path in protopaths)
        {
            if(!string.IsNullOrEmpty(path))
            {
                args.Add("--proto_path=" + path);
            }
        }
        if (args.Count == 0)
        {
            return;
        }
        args.Add("--csharp_out=" + csharp_out);
        args.Add("--csharp_opt=file_extension=.pb.cs");
        if (!string.IsNullOrEmpty(extra_options))
        {
            extra_options = extra_options.Trim();
            args.Add(extra_options);
        }
        bool genany = false;
        List<string> excludes = new List<string>();
        if(!string.IsNullOrEmpty(exclude_proto_keywords))
        {
            var ss = exclude_proto_keywords.Split(';');
            excludes.AddRange(ss.Select((s) => s.Trim()));
        }
        foreach (var path in protopaths)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string[] files = Directory.GetFiles(path, "*.proto", SearchOption.TopDirectoryOnly);
                foreach(var f in files)
                {
                    var ff = f.Replace("\\", "/");
                    if (excludes.Any((s) => ff.Contains(s)))
                    {
                        continue;
                    }
                    args.Add(ff);
                    Debug.Log("generate proto of file: " + ff);
                    genany = true;
                    var result = EditorHelpers.RunCmd(protoc, string.Join(" ", args.ToArray()));
                    if(result.code != 0)
                    {
                        Debug.LogError(result.msg);
                    }
                    args.RemoveAt(args.Count - 1);
                }
            }
        }
        if (genany)
        {
            EditorGUIHepler.SaveListPrefs(protopaths, "proto_path");
            EditorPrefs.SetString("csharp_out", csharp_out);
            EditorPrefs.SetString("exclude_proto_keywords", exclude_proto_keywords);
            AssetDatabase.Refresh();
        }
    }

    void OnGUI()
    {
        if (EditorGUIHepler.DrawHeader("协议文件目录", "proto_path", false, false))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("remove proto path"))
            {
                if (protopaths.Count > 0)
                    protopaths.RemoveAt(protopaths.Count - 1);
            }
            if (GUILayout.Button("add proto path"))
            {
                protopaths.Add(string.Empty);
            }
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < protopaths.Count; i++)
            {
                protopaths[i] = EditorGUILayout.TextField(protopaths[i]);
            }
        }
        if (EditorGUIHepler.DrawHeader("c# 输出目录", "csharp_out", false, false))
        {
            csharp_out = EditorGUILayout.TextField(csharp_out);
        }
        if (EditorGUIHepler.DrawHeader("其它参数，见protoc -h", "extra_options", false, false))
        {
            extra_options = EditorGUILayout.TextField(extra_options);
        }
        if (EditorGUIHepler.DrawHeader("排除文件关键字(;分开):", "exclude_proto_keywords", false, false))
        {
            exclude_proto_keywords = EditorGUILayout.TextField(exclude_proto_keywords);
        }
        if (GUILayout.Button("Generate"))
        {
            gen();
        }
    }
}
