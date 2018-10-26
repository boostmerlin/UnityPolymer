using Ginkgo;
using Ginkgo.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class UGUIWidgetInject : MonoBehaviour
{
    static bool IsGameObject(Type type)
    {
        return type == typeof(GameObject);
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
            //string[] Names = attribute.FullName.Split('.');
            //for (int i = 0; i < Names.Length; i++)
            //{
            //    string name = Names[i];
            //    if (string.IsNullOrEmpty(name))
            //    {
            //        continue;
            //    }

            //    GameObject go = trans.Find(name)
            //    if (go == null)
            //    {
            //        Log.Common.PrintWarning("Can't find component name={0} on path: {1}, field is:{2}", name, attribute.FullName, fieldName);
            //        return null;
            //    }
            //}
        }
        else
        {
            child = trans.Find(fieldName);
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
            if (IsGameObject(fi.FieldType))
            {
                fi.SetValue(view, value);
            }
        }
        else if (memberType == MemberTypes.Property)
        {
            PropertyInfo pi = memberInfo as PropertyInfo;
            if (pi.CanWrite && IsGameObject(pi.PropertyType))
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

        List<MemberInfo> members = new List<MemberInfo>(propinfos.Length + fieldinfos.Length);
        members.AddRange(propinfos);
        members.AddRange(fieldinfos);
        for (int i = 0; i < members.Count; i++)
        {
            MemberInfo pi = members[i];
            object[] attributes = pi.GetCustomAttributes(typeof(UIWidgetAttribute), true);
            if (attributes.Length == 1)
            {
                UIWidgetAttribute att = attributes[0] as UIWidgetAttribute;
                Transform go = findWidget(componentRoot, pi.Name, att);
                if (go != null)
                {
                    var comtype = pi.GetType();
                    Component com = go.GetComponent(comtype);
                    setValue(view, com, pi, pi.MemberType);
                }
            }
        }
    }
}