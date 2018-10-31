using Ginkgo;
using Ginkgo.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UGUIWidgetInject
{
    static Transform recursiveFind(Transform root, string name, int depth)
    {
        if(root.childCount == 0 || depth == 0)
        {
            return null;
        }
        foreach(Transform t in root)
        {
            if (t.name.Equals(name))
            {
                return t;
            }
            Transform tt = recursiveFind(t, name, depth - 1);
            if(tt != null)
            {
                return tt;
            }
        }
        return null;
    }

    static Transform findWidget(GameObject component, string fieldName, UIWidgetAttribute attribute)
    {
        Transform trans = component.transform;
        Transform child = null;
        if (!string.IsNullOrEmpty(attribute.FullName))
        {
            child = trans.Find(attribute.FullName.Replace('.', '/'));
            if(child == null)
            {
                Log.Common.PrintWarning("Can't find component name={0} on path: {1}, field is:{2}", component.name, attribute.FullName, fieldName);
            }
        }
        else
        {
            DebugTimer.BEGIN("UGUIWidgetInject");
            child = recursiveFind(trans, fieldName, attribute.Depth);
            DebugTimer.END("UGUIWidgetInject");
            if (child == null)
            {
                Log.Common.PrintWarning("Can't find component by fieldname={0}", fieldName);
            }
        }
        return child;
    }

    static void setValue(object view, object value, MemberInfo memberInfo, MemberTypes memberType)
    {
        if (memberType == MemberTypes.Field)
        {
            FieldInfo fi = memberInfo as FieldInfo;
            {
                fi.SetValue(view, value);
            }
        }
        else if (memberType == MemberTypes.Property)
        {
            PropertyInfo pi = memberInfo as PropertyInfo;
            if (pi.CanWrite)
            {
                pi.SetValue(view, value, null);
            }
        }
    }

    public static void Inject(object view, GameObject componentRoot)
    {
        if (componentRoot == null || view == null)
        {
            Log.Common.PrintWarning("FGUIWidgetInject.Inject, null componentRoot or view");
            return;
        }

        Type type = view.GetType();
        var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.NonPublic | BindingFlags.Static;
        var propinfos = type.GetProperties(bindingFlags);
        var fieldinfos = type.GetFields(bindingFlags);

        for (int i = 0; i < propinfos.Length; i++)
        {
            PropertyInfo pi = propinfos[i];
            object[] attributes = pi.GetCustomAttributes(typeof(UIWidgetAttribute), true);
            if (attributes.Length == 1)
            {
                UIWidgetAttribute att = attributes[0] as UIWidgetAttribute;
                Transform go = findWidget(componentRoot, pi.Name, att);
                if (go != null)
                {
                    var comtype = pi.PropertyType;
                    Component com = go.GetComponent(comtype);
                    if(com != null)
                        setValue(view, com, pi, pi.MemberType);
                }
            }
        }
        for (int i = 0; i < fieldinfos.Length; i++)
        {
            FieldInfo pi = fieldinfos[i];
            object[] attributes = pi.GetCustomAttributes(typeof(UIWidgetAttribute), true);
            if (attributes.Length == 1)
            {
                UIWidgetAttribute att = attributes[0] as UIWidgetAttribute;
                Transform go = findWidget(componentRoot, pi.Name, att);
                if (go != null)
                {
                    var comtype = pi.FieldType;
                    Component com = go.GetComponent(comtype);
                    if (com != null)
                        setValue(view, com, pi, pi.MemberType);
                }
            }
        }
    }
}