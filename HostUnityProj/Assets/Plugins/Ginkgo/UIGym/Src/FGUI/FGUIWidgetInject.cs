using FairyGUI;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Ginkgo.UI
{
    public class FGUIWidgetInject
    {
        static bool IsGObject(Type type)
        {
            return type.IsSubclassOf(typeof(GObject));
        }

        public static void Inject(object view, GObject componentRoot)
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
                    GObject go = findWidget(componentRoot, pi.Name, att);
                    if(go != null)
                    {
                        setValue(view, go, pi, pi.MemberType);
                    }
                }
            }
        }

        static void setValue(object view, GObject value, MemberInfo memberInfo, MemberTypes memberType)
        {
            if(memberType == MemberTypes.Field)
            {
                FieldInfo fi = memberInfo as FieldInfo;
                if (IsGObject(fi.FieldType))
                {
                    fi.SetValue(view, value);
                }
            }
            else if(memberType == MemberTypes.Property)
            {
                PropertyInfo pi = memberInfo as PropertyInfo;
                if(pi.CanWrite && IsGObject(pi.PropertyType))
                {
                    pi.SetValue(view, value, null);
                }
            }
        }

        static GObject findWidget(GObject component, string fieldName, UIWidgetAttribute attribute)
        {
            if (component.asCom == null)
            {
                return null;
            }

            GObject obj = component;
            if (!string.IsNullOrEmpty(attribute.FullName))
            {
                string[] Names = attribute.FullName.Split('.');
                for (int i = 0; i < Names.Length; i++)
                {
                    string name = Names[i];
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    GObject go = obj.asCom.GetChild(name);
                    if (go == null)
                    {
                        Log.Common.PrintWarning("Can't find component name={0} on path: {1}, field is:{2}", name, attribute.FullName, fieldName);
                        return null;
                    }
                    obj = go;
                }
            }
            else
            {
                obj = obj.asCom.GetChild(fieldName);
                if (obj == null)
                {
                    Log.Common.PrintWarning("Can't find component by fieldname={0}", fieldName);
                    return null;
                }
            }
            return obj;
        }
    }
}
