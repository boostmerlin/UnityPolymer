using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Ginkgo
{
    public static class GeneralUtils
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }

        public static void ListSwap<T>(IList<T> list, int indexA, int indexB)
        {
            T value = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = value;
        }

        public static void ListMove<T>(IList<T> list, int srcIndex, int dstIndex)
        {
            if (srcIndex == dstIndex)
            {
                return;
            }
            T item = list[srcIndex];
            list.RemoveAt(srcIndex);
            if (dstIndex > srcIndex)
            {
                dstIndex--;
            }
            list.Insert(dstIndex, item);
        }

        public static T[] Combine<T>(T[] arr1, T[] arr2)
        {
            T[] array = new T[arr1.Length + arr2.Length];
            Array.Copy(arr1, 0, array, 0, arr1.Length);
            Array.Copy(arr1, 0, array, arr1.Length, arr2.Length);
            return array;
        }

        public static bool Contains(this string str, string val, StringComparison comparison)
        {
            return str.IndexOf(val, comparison) >= 0;
        }

        public static T[] Slice<T>(this T[] arr, int start, int end)
        {
            int num = arr.Length;
            if (start < 0)
            {
                start = num + start;
            }
            if (end < 0)
            {
                end = num + end;
            }
            int num2 = end - start;
            if (num2 <= 0)
            {
                return new T[0];
            }
            int num3 = num - start;
            if (num2 > num3)
            {
                num2 = num3;
            }
            T[] array = new T[num2];
            Array.Copy(arr, start, array, 0, num2);
            return array;
        }

        public static T[] Slice<T>(this T[] arr, int start)
        {
            return arr.Slice(start, arr.Length);
        }

        public static T[] Slice<T>(this T[] arr)
        {
            return arr.Slice(0, arr.Length);
        }

        public static bool IsOverriddenMethod(MethodInfo childMethod, MethodInfo ancestorMethod)
        {
            if (childMethod == null)
            {
                return false;
            }
            if (ancestorMethod == null)
            {
                return false;
            }
            if (childMethod.Equals(ancestorMethod))
            {
                return false;
            }
            MethodInfo baseDefinition = childMethod.GetBaseDefinition();
            while (!baseDefinition.Equals(childMethod) && !baseDefinition.Equals(ancestorMethod))
            {
                MethodInfo obj = baseDefinition;
                baseDefinition = baseDefinition.GetBaseDefinition();
                if (baseDefinition.Equals(obj))
                {
                    return false;
                }
            }
            return baseDefinition.Equals(ancestorMethod);
        }

        public static bool IsObjectAlive(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is UnityEngine.Object))
            {
                return true;
            }
            UnityEngine.Object exists = (UnityEngine.Object)obj;
            return exists;
        }

        public static bool IsCallbackValid(Delegate callback)
        {
            bool flag = true;
            if (callback == null)
            {
                flag = false;
            }
            else if (!callback.Method.IsStatic)
            {
                object target = callback.Target;
                flag = GeneralUtils.IsObjectAlive(target);
                if (!flag)
                {
                    Debug.LogError(string.Format("Target for callback {0} is null.", callback.Method.Name));
                }
            }
            return flag;
        }

        public static bool IsEditorPlaying()
        {
            return false;
        }

        public static void ExitApplication()
        {
            Application.Quit();
        }

        public static bool IsDevelopmentBuildTextVisible()
        {
            return Debug.isDebugBuild;
        }

        public static bool TryParseBool(string strVal, out bool boolVal)
        {
            if (bool.TryParse(strVal, out boolVal))
            {
                return true;
            }
            string a = strVal.ToLowerInvariant().Trim();
            if (a == "off" || a == "0" || a == "false")
            {
                boolVal = false;
                return true;
            }
            if (a == "on" || a == "1" || a == "true")
            {
                boolVal = true;
                return true;
            }
            boolVal = false;
            return false;
        }

        public static bool ForceBool(string strVal)
        {
            string a = strVal.ToLowerInvariant().Trim();
            return a == "on" || a == "1" || a == "true";
        }

        public static bool TryParseInt(string str, out int val)
        {
            return int.TryParse(str, NumberStyles.Any, null, out val);
        }

        public static int ForceInt(string str)
        {
            int result = 0;
            GeneralUtils.TryParseInt(str, out result);
            return result;
        }

        public static bool TryParseLong(string str, out long val)
        {
            return long.TryParse(str, NumberStyles.Any, null, out val);
        }

        public static long ForceLong(string str)
        {
            long result = 0L;
            GeneralUtils.TryParseLong(str, out result);
            return result;
        }

        public static bool TryParseULong(string str, out ulong val)
        {
            return ulong.TryParse(str, NumberStyles.Any, null, out val);
        }

        public static ulong ForceULong(string str)
        {
            ulong result = 0uL;
            GeneralUtils.TryParseULong(str, out result);
            return result;
        }

        public static bool TryParseFloat(string str, out float val)
        {
            return float.TryParse(str, NumberStyles.Any, null, out val);
        }

        public static float ForceFloat(string str)
        {
            float result = 0f;
            GeneralUtils.TryParseFloat(str, out result);
            return result;
        }

        public static bool RandomBool()
        {
            return UnityEngine.Random.Range(0, 2) == 0;
        }

        public static float RandomSign()
        {
            return (!GeneralUtils.RandomBool()) ? 1f : -1f;
        }

        public static int UnsignedMod(int x, int y)
        {
            int num = x % y;
            if (num < 0)
            {
                num += y;
            }
            return num;
        }

        public static bool IsEven(int n)
        {
            return (n & 1) == 0;
        }

        public static bool IsOdd(int n)
        {
            return (n & 1) == 1;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func)
        {
            if (enumerable == null)
            {
                return;
            }
            foreach (T current in enumerable)
            {
                func(current);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> func)
        {
            if (enumerable == null)
            {
                return;
            }
            int num = 0;
            foreach (T current in enumerable)
            {
                func(current, num);
                num++;
            }
        }

        public static void ForEachReassign<T>(this T[] array, Func<T, T> func)
        {
            if (array == null)
            {
                return;
            }
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = func(array[i]);
            }
        }

        public static void ForEachReassign<T>(this T[] array, Func<T, int, T> func)
        {
            if (array == null)
            {
                return;
            }
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = func(array[i], i);
            }
        }

        public static bool AreArraysEqual<T>(T[] arr1, T[] arr2)
        {
            if (arr1 == arr2)
            {
                return true;
            }
            if (arr1 == null)
            {
                return false;
            }
            if (arr2 == null)
            {
                return false;
            }
            if (arr1.Length != arr2.Length)
            {
                return false;
            }
            for (int i = 0; i < arr1.Length; i++)
            {
                if (!arr1[i].Equals(arr2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool AreBytesEqual(byte[] bytes1, byte[] bytes2)
        {
            return GeneralUtils.AreArraysEqual<byte>(bytes1, bytes2);
        }

        public static T DeepClone<T>(T obj)
        {
            return (T)((object)GeneralUtils.CloneValue(obj, obj.GetType()));
        }

        private static object CloneClass(object obj, Type objType)
        {
            object obj2 = GeneralUtils.CreateNewType(objType);
            FieldInfo[] fields = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] array = fields;
            for (int i = 0; i < array.Length; i++)
            {
                FieldInfo fieldInfo = array[i];
                fieldInfo.SetValue(obj2, GeneralUtils.CloneValue(fieldInfo.GetValue(obj), fieldInfo.FieldType));
            }
            return obj2;
        }

        private static object CloneValue(object src, Type type)
        {
            if (src != null && type != typeof(string) && type.IsClass)
            {
                if (!type.IsGenericType)
                {
                    return GeneralUtils.CloneClass(src, type);
                }
                if (src is IDictionary)
                {
                    IDictionary dictionary = src as IDictionary;
                    IDictionary dictionary2 = GeneralUtils.CreateNewType(type) as IDictionary;
                    Type type2 = type.GetGenericArguments()[0];
                    Type type3 = type.GetGenericArguments()[1];
                    foreach (DictionaryEntry dictionaryEntry in dictionary)
                    {
                        dictionary2.Add(GeneralUtils.CloneValue(dictionaryEntry.Key, type2), GeneralUtils.CloneValue(dictionaryEntry.Value, type3));
                    }
                    return dictionary2;
                }
                if (src is IList)
                {
                    IList list = src as IList;
                    IList list2 = GeneralUtils.CreateNewType(type) as IList;
                    Type type4 = type.GetGenericArguments()[0];
                    foreach (object current in list)
                    {
                        list2.Add(GeneralUtils.CloneValue(current, type4));
                    }
                    return list2;
                }
            }
            return src;
        }

        private static object CreateNewType(Type type)
        {
            object obj = Activator.CreateInstance(type);
            if (obj == null)
            {
                throw new SystemException(string.Format("Unable to instantiate type {0} with default constructor.", type.Name));
            }
            return obj;
        }

        public static void DeepReset<T>(T obj)
        {
            Type typeFromHandle = typeof(T);
            T t = Activator.CreateInstance<T>();
            if (t == null)
            {
                throw new SystemException(string.Format("Unable to instantiate type {0} with default constructor.", typeFromHandle.Name));
            }
            FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] array = fields;
            for (int i = 0; i < array.Length; i++)
            {
                FieldInfo fieldInfo = array[i];
                fieldInfo.SetValue(obj, fieldInfo.GetValue(t));
            }
        }

        public static void CleanNullObjectsFromList<T>(List<T> list)
        {
            int i = 0;
            while (i < list.Count)
            {
                T t = list[i];
                if (t == null)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        public static void CleanDeadObjectsFromList<T>(List<T> list) where T : Component
        {
            int i = 0;
            while (i < list.Count)
            {
                T t = list[i];
                if (t)
                {
                    i++;
                }
                else
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void CleanDeadObjectsFromList(List<GameObject> list)
        {
            int i = 0;
            while (i < list.Count)
            {
                GameObject exists = list[i];
                if (exists)
                {
                    i++;
                }
                else
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static string SafeFormat(string format, params object[] args)
        {
            string result;
            if (args.Length == 0)
            {
                result = format;
            }
            else
            {
                result = string.Format(format, args);
            }
            return result;
        }

        //去除UTF-8的BOM
        public static string Bytes2Utf8String(byte[] buffer)
        {
            if (buffer == null)
            {
                return string.Empty;
            }
        //    byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
        //    byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] BOM = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
            if (buffer[0] == BOM[0]
                && buffer[1] == BOM[1]
                && buffer[2] == BOM[2])
            {
                return Encoding.UTF8.GetString(buffer, 3, buffer.Length - 3);
            }
            else
            {
                return Encoding.UTF8.GetString(buffer);
            }
        }

       /// <summary>
       /// FNV Hash
       /// FNV_prime的取值: 
       /// 32 bit FNV_prime = 2 ^ 24 + 2 ^ 8 + 0x93 = 16777619
       /// offset_basis的取值: 
       /// 32 bit offset_basis = 2166136261
       /// </summary>
        public static int HashFNV(string data)
        {
            int p = 16777619;
            uint basis = 2166136261;
            int hash = (int)basis;
            for (int i = 0; i < data.Length; i++)
                hash = (hash ^ data[i]) * p;
            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;
            return hash;
        }

        /// <summary>
        /// RS Hash
        /// </summary>
        public static int HashRS(string data)
        {
            int b = 378551;
            int a = 63689;
            int hash = 0;
            for (int i = 0; i < data.Length; i++)
            {
                hash = hash * a + data[i];
                a = a * b;
            }
            return (hash & 0x7FFFFFFF);
        }
    }
}