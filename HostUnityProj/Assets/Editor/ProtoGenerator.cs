using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using Ginkgo;
using Google.Protobuf;

class PyMsgIdGenerator : PythonGenerator
{
    Dictionary<int, string> ids = new Dictionary<int, string>();
    protected override void generateBody()
    {
        var types = ReflectionUtil.FindTypesInAssembly(new string[] { "Assembly-CSharp" }, typeof(IMessage));
        foreach (var t in types)
        {
            int msgid = Ginkgo.Net.NetMsg.HashMsgID(t.FullName);
            if(ids.ContainsKey(msgid))
            {
                Debug.LogWarning("! Hash conflict, this: " + t.FullName + " has same msgid with: " + ids[msgid]);
            }
            ids.Add(msgid, t.FullName);
            WriteComment(t.AssemblyQualifiedName);
            Debug.LogFormat("Generate msg: name={0}, fullname={1},msgid={2}", t.Name, t.FullName, msgid);
            WriteLine(string.Format("{0} = {1}", t.Name, msgid));
        }
    }
}

public class ProtoGenerator : EditorWindow
{
    const string kMenuProtoGen = "Ginkgo/Protobuf Generator";
    const string protoc = @"../Tools/bin/protoc.exe";

    const string kcsharp_out = "csharp_out";
    const string kgo_out = "go_out";
    const string kcpp_out = "cpp_out";
    const string kjava_out = "java_out";
    const string kpython_out = "python_out";

    string[] mOutPathNameFlag;

    //-I
    List<string> mProtopaths = new List<string>();

    Enum mLanguage = Language.CSHARP;

    string[] mOutputPaths;

    string mExtraOptions;
    string mExcludeProtoKeywords;

    bool isGenerateMsgId;

    enum BoolOption
    {
        GEN_MSGID,
        COUNT
    }
    List<bool> mBooleanOptions = new List<bool>();

    [MenuItem(kMenuProtoGen, priority = 1)]
    static void Execute()
    {
        var w = GetWindow<ProtoGenerator>(true, "Protobuf Generate Options");
        w.Show();
    }

    void Awake()
    {
        mOutputPaths = new string[EnumUtils.Length<Language>()];

        mOutPathNameFlag = new string[]
        {
            kcsharp_out,kgo_out,kcpp_out, kjava_out,kpython_out
        };
        for (int i = 0; i < mOutPathNameFlag.Length; i++)
        {
            mOutputPaths[i] = EditorPrefs.GetString(mOutPathNameFlag[i], mOutputPaths[i]);
        }
        mExcludeProtoKeywords = EditorPrefs.GetString("exclude_proto_keywords", mExcludeProtoKeywords);
        EditorGUIHepler.GetListPrefs(mProtopaths, "proto_path");
        EditorGUIHepler.GetListPrefs(mBooleanOptions, "bool_options");
        if (mBooleanOptions.Count == 0)
        {
            for (int i = 0; i < (int)BoolOption.COUNT; i++)
            {
                mBooleanOptions.Add(false);
            }
        }
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
                    args.Add("--" + mOutPathNameFlag[i] + "=" + mOutputPaths[i]);
                }
                else
                {
                    Debug.LogWarning(mOutPathNameFlag[i] + " Out Path is NULL, skip!");
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
        //one proto at a time.
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
            EditorGUIHepler.SaveListPrefs(mBooleanOptions, "bool_options");
            for (int j = 0; j < mOutPathNameFlag.Length; j++)
            {
                if (!string.IsNullOrEmpty(mOutputPaths[j]))
                {
                    EditorPrefs.SetString(mOutPathNameFlag[j], mOutputPaths[j]);
                }
            }

            EditorPrefs.SetString("exclude_proto_keywords", mExcludeProtoKeywords);
            EditorPrefs.SetInt("out_language", (int)(Language)mLanguage);

            AssetDatabase.Refresh();

            if (mBooleanOptions[(int)BoolOption.GEN_MSGID]
                && ((Language)mLanguage & Language.CSHARP) > 0)
            {
                isGenerateMsgId = true;
            }
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
#if UNITY_2017_3_OR_NEWER || UNITY_2018
            mLanguage = EditorGUILayout.EnumFlagsField("proto生成目标语言", mLanguage);
#else
            mLanguage = EditorGUILayout.EnumMaskField("proto生成目标语言", mLanguage);
#endif
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
        }
        if (((Language)mLanguage & Language.CSHARP) > 0
            && EditorGUIHepler.DrawHeader("其它选项:", "generate_options", false, false))
        {
            mBooleanOptions[(int)BoolOption.GEN_MSGID] = EditorGUILayout.Toggle("生成消息ID定义:", mBooleanOptions[(int)BoolOption.GEN_MSGID]);
            EditorGUILayout.HelpBox("需要生成csharp版本才能使用！", MessageType.Warning);
        }
        if (!EditorApplication.isCompiling && GUILayout.Button("Generate"))
        {
            gen();
        }

        if (!EditorApplication.isCompiling && isGenerateMsgId)
        {
            isGenerateMsgId = false;
            Language lan = (Language)mLanguage;
            int i = 0;
            foreach (Language v in Enum.GetValues(typeof(Language)))
            {
                if ((lan & v) > 0)
                {
                    if (!string.IsNullOrEmpty(mOutputPaths[i]))
                    {
                        string fileName = Path.Combine(mOutputPaths[i], Application.productName + "MsgDefine");
                        CodeGenerator cg = null;
                        if (v == Language.PYTHON)
                        {
                            cg = new PyMsgIdGenerator
                            {
                                fileName = fileName
                            };
                        }
                        if (cg != null)
                        {
                            Debug.Log("Generate MsgId Defines file of : " + v);
                            cg.Run();
                        }
                    }
                    else
                    {
                        Debug.LogWarning(mOutPathNameFlag[i] + " Out Path is NULL, skip generate msgid!");
                    }
                }
                i++;
            }
        }
    }
}

