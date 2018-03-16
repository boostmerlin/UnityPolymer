using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public static class EditorGUIHepler
{
    public static T GetValue<T>(string key)
    {
        Type t = typeof(T);
        if (t == typeof(float))
        {
            return (T)((object)EditorPrefs.GetFloat(key));
        }
        else if (t == typeof(bool))
        {
            return (T)((object)EditorPrefs.GetBool(key));
        }
        else if (t == typeof(string))
        {
            return (T)((object)EditorPrefs.GetString(key));
        }
        else if (t == typeof(int))
        {
            return (T)((object)EditorPrefs.GetInt(key));
        }
        return default(T);
    }

    public static void SetValue<T>(string key, T v)
    {
        Type t = typeof(T);
        if (t == typeof(float))
        {
            EditorPrefs.SetFloat(key, (float)(object)v);
        }
        else if (t == typeof(bool))
        {
            EditorPrefs.SetBool(key, (bool)(object)v);
        }
        else if (t == typeof(string))
        {
            EditorPrefs.SetString(key, (string)(object)v);
        }
        else if (t == typeof(int))
        {
            EditorPrefs.SetInt(key, (int)(object)v);
        }
    }

    static public void GetListPrefs<T>(List<T> list, string key)
    {
        if (list == null) return;
        int n = EditorPrefs.GetInt(key);
        for (int i = 0; i < n; i++)
        {
            string keykey = key + (i + 1).ToString();
            if (EditorPrefs.HasKey(keykey))
            {
                list.Add(GetValue<T>(keykey));
            }
        }
    }

    static public void SaveListPrefs<T>(List<T> list, string key)
    {
        if (list == null) return;
        int n = list.Count;
        EditorPrefs.SetInt(key, n);
        for (int i = 0; i < n; i++)
        {
            string keykey = key + (i + 1).ToString();
            SetValue<T>(keykey, list[i]);
        }
    }

    static public bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
    {
        bool state = EditorPrefs.GetBool(key, true);

        if (!minimalistic) GUILayout.Space(3f);
        if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            if (state) text = "\u25BC" + (char)0x200a + text;
            else text = "\u25BA" + (char)0x200a + text;

            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
        }

        if (GUI.changed) EditorPrefs.SetBool(key, state);

        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }
}
