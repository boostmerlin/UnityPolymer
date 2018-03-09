using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ML { 
    public class EnumUtils
    {
        private static Dictionary<Type, Dictionary<string, object>> s_enumCache = new Dictionary<Type, Dictionary<string, object>>();

        public static string GetString<T>(T enumVal)
        {
            string text = enumVal.ToString();
            FieldInfo field = enumVal.GetType().GetField(text);
            DescriptionAttribute[] array = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (array.Length > 0)
            {
                return array[0].Description;
            }
            return text;
        }

        public static bool TryGetEnum<T>(string str, StringComparison comparisonType, out T result)
        {
            Type typeFromHandle = typeof(T);
            Dictionary<string, object> map;
            EnumUtils.s_enumCache.TryGetValue(typeFromHandle, out map);
            object obj;
            if (map != null && map.TryGetValue(str, out obj))
            {
                result = (T)((object)obj);
                return true;
            }
            IEnumerator enumerator = Enum.GetValues(typeFromHandle).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    T t = (T)((object)enumerator.Current);
                    bool flag = false;
                    string s = EnumUtils.GetString<T>(t);
                    if (s.Equals(str, comparisonType))
                    {
                        flag = true;
                        result = t;
                    }
                    else
                    {
                        FieldInfo field = t.GetType().GetField(t.ToString());
                        DescriptionAttribute[] array = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i].Description.Equals(str, comparisonType))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (flag)
                    {
                        if (map == null)
                        {
                            map = new Dictionary<string, object>();
                            EnumUtils.s_enumCache.Add(typeFromHandle, map);
                        }
                        if (!map.ContainsKey(str))
                        {
                            map.Add(str, t);
                        }
                        result = t;
                        return true;
                    }
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            result = default(T);
            return false;
        }

        public static T GetEnum<T>(string str)
        {
            return EnumUtils.GetEnum<T>(str, StringComparison.Ordinal);
        }

        public static T GetEnum<T>(string str, StringComparison comparisonType)
        {
            T result;
            if (EnumUtils.TryGetEnum<T>(str, comparisonType, out result))
            {
                return result;
            }
            string message = string.Format("EnumUtils.GetEnum() - \"{0}\" has no matching value in enum {1}", str, typeof(T));
            throw new ArgumentException(message);
        }

        public static bool TryGetEnum<T>(string str, out T outVal)
        {
            return EnumUtils.TryGetEnum<T>(str, StringComparison.Ordinal, out outVal);
        }

        public static T Parse<T>(string str)
        {
            return (T)((object)Enum.Parse(typeof(T), str));
        }

        public static T SafeParse<T>(string str)
        {
            T result;
            try
            {
                result = (T)((object)Enum.Parse(typeof(T), str));
            }
            catch (Exception)
            {
                result = default(T);
            }
            return result;
        }

        public static bool TryCast<T>(object inVal, out T outVal)
        {
            outVal = default(T);
            bool result;
            try
            {
                outVal = (T)((object)inVal);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public static int Length<T>()
        {
            return Enum.GetValues(typeof(T)).Length;
        }
    }
}
