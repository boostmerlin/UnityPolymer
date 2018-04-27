using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using ML;

public class ProtoGenerator : EditorWindow
{
    [Flags]
    enum Language
    {
        CSHARP = 1,
        GOLANG = 1 << 1,
        CPP = 1 << 2,
        JAVA = 1 << 3,
        PYTHON = 1 << 4
    }

    const string kMenuProtoGen = "Melin/Protobuf Generator";
    const string protoc = @"../Tools/bin/protoc.exe";

    const string kcsharp_out = "csharp_out";
    const string kgo_out = "go_out";
    const string kcpp_out = "cpp_out";
    const string kjava_out = "java_out";
    const string kpython_out = "python_out";

    string[] OutPathNameFlag;  

    //-I
    List<string> mProtopaths = new List<string>();

    Enum mLanguage = Language.CSHARP;

    string[] mOutputPaths;

    string mExtraOptions;
    string mExcludeProtoKeywords;

    [MenuItem(kMenuProtoGen)]
    static void Execute()
    {
        var w = GetWindow<ProtoGenerator>(true, "Proto Generate Options");
        w.Show();
    }

    void Awake()
    {
        mOutputPaths = new string[EnumUtils.Length<Language>()];

        OutPathNameFlag = new string[]
        {
            kcsharp_out,kgo_out,kcpp_out, kjava_out,kpython_out
        };
        for (int i = 0; i < OutPathNameFlag.Length; i++)
        {
            mOutputPaths[i] = EditorPrefs.GetString(OutPathNameFlag[i], mOutputPaths[i]);
        }
        mExcludeProtoKeywords = EditorPrefs.GetString("exclude_proto_keywords", mExcludeProtoKeywords);
        EditorGUIHepler.GetListPrefs(mProtopaths, "proto_path");
        mLanguage = (Language)EditorPrefs.GetInt("out_language", 1);
    }

    bool checkOutPaths()
    {
        Language lan = (Language)mLanguage;
        int i = 0;
        foreach (Language v in Enum.GetValues(typeof(Language)))
        {
            if ((lan & v) > 0)
            {
                if (!string.IsNullOrEmpty(mOutputPaths[i]))
                {
                    return true;
                }
            }
            i++;
        }
        return false;
    }

    void gen()
    {
        List<string> args = new List<string>();
        if (!checkOutPaths())
        {
            Debug.LogError("no output path specified.");
            return;
        }
        foreach (var path in mProtopaths)
        {
            if (!string.IsNullOrEmpty(path))
            {
                args.Add("--proto_path=" + path);
            }
        }
        if (args.Count == 0)
        {
            Debug.LogError("no proto_path specified.");
            return;
        }

        Language lan = (Language)mLanguage;
        int i = 0;
        foreach (Language v in Enum.GetValues(typeof(Language)))
        {
            if ((lan & v) > 0)
            {
                if (!string.IsNullOrEmpty(mOutputPaths[i]))
                {
                    args.Add("--" + OutPathNameFlag[i] + "=" + mOutputPaths[i]);
                }
                else
                {
                    Debug.LogWarning(OutPathNameFlag[i] + " Out Path is NULL, skip!");
                }
            }
            i++;
        }

        args.Add("--csharp_opt=file_extension=.pb.cs");

        if (!string.IsNullOrEmpty(mExtraOptions))
        {
            mExtraOptions = mExtraOptions.Trim();
            args.Add(mExtraOptions);
        }
        bool genany = false;
        List<string> excludes = new List<string>();
        if (!string.IsNullOrEmpty(mExcludeProtoKeywords))
        {
            var ss = mExcludeProtoKeywords.Split(';');
            excludes.AddRange(ss.Select((s) => s.Trim()));
        }
        foreach (var path in mProtopaths)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string[] files = Directory.GetFiles(path, "*.proto", SearchOption.TopDirectoryOnly);
                foreach (var f in files)
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
                    if (result.code != 0)
                    {
                        Debug.LogError(result.msg);
                    }
                    args.RemoveAt(args.Count - 1);
                }
            }
        }
        if (genany)
        {
            EditorGUIHepler.SaveListPrefs(mProtopaths, "proto_path");
            for (int j = 0; j < OutPathNameFlag.Length; j++)
            {
                if (!string.IsNullOrEmpty(mOutputPaths[j]))
                {
                    EditorPrefs.SetString(OutPathNameFlag[j], mOutputPaths[j]);
                }
            }
            EditorPrefs.SetString("exclude_proto_keywords", mExcludeProtoKeywords);
            EditorPrefs.SetInt("out_language", (int)(Language)mLanguage);
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
                if (mProtopaths.Count > 0)
                    mProtopaths.RemoveAt(mProtopaths.Count - 1);
            }
            if (GUILayout.Button("add proto path"))
            {
                mProtopaths.Add(string.Empty);
            }
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < mProtopaths.Count; i++)
            {
                mProtopaths[i] = EditorGUILayout.TextField(mProtopaths[i]);
            }
        }

        if (EditorGUIHepler.DrawHeader("其它参数，见protoc -h", "extra_options", false, false))
        {
            mExtraOptions = EditorGUILayout.TextField(mExtraOptions);
        }
        if (EditorGUIHepler.DrawHeader("排除文件关键字(;分开):", "exclude_proto_keywords", false, false))
        {
            mExcludeProtoKeywords = EditorGUILayout.TextField(mExcludeProtoKeywords);
        }
        if (EditorGUIHepler.DrawHeader("输出", "outputs", false, false))
        {
            mLanguage = EditorGUILayout.EnumMaskPopup("proto生成目标语言", mLanguage);
            int i = 0;
            foreach (Language v in Enum.GetValues(typeof(Language)))
            {
                if (((Language)mLanguage & v) > 0)
                {
                    EditorGUILayout.PrefixLabel(EnumUtils.GetString<Language>(v).ToLower() + " 输出目录：");
                    mOutputPaths[i] = EditorGUILayout.TextField(mOutputPaths[i]);
                }
                i++;
            }
            if (GUILayout.Button("Generate"))
            {
                gen();
            }
        }
    }
}
