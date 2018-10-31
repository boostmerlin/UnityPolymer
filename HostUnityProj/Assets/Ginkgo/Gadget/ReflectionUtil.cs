using System.Collections.Generic;
using System.Reflection;
using System;

namespace Ginkgo
{
    public static class ReflectionUtil
    {
        public static void LogAssemblyInfo()
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            Log.Common.Print("GetCallingAssembly: " + assembly.FullName);

            assembly = Assembly.GetEntryAssembly();

            if (assembly != null)
            {
                Log.Common.Print("GetEntryAssembly: " + assembly.FullName);
            }
            assembly = Assembly.GetExecutingAssembly();
            Log.Common.Print("GetExecutingAssembly: " + assembly.FullName);

#if UNITY_EDITOR
            var unityAssemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();
            foreach(var ua in unityAssemblies)
            {
                Log.Common.Print(true, "Unity Assemblies? " + ua.name);
            }
#endif
        }

        public static Assembly FindAssembly(string fullName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var one in assemblies)
            {
                if (one.FullName.Equals(fullName))
                {
                    return one;
                }
            }
            return null;
        }

        public static Assembly[] FindAllAssembly()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies;
        }

        public static Type[] FindTypesInAssembly(string[] assemblynames, Type isType)
        {
            Assembly[] assemblies = FindAllAssembly();
            List<Assembly> filteredAssemblies = new List<Assembly>();
            if (assemblynames != null && assemblynames.Length > 0)
            {
                // filterdassemblies = from a in assemblies from n in fullnames where a.FullName == n select a;
                foreach (var a in assemblies)
                {
                    foreach (var n in assemblynames)
                    {
                        if (a.FullName.Contains(n))
                        {
                            filteredAssemblies.Add(a);
                            break;
                        }
                    }
                }
            }
            else
            {
                filteredAssemblies.AddRange(assemblies);
            }
            List<Type> retList = new List<Type>();
            foreach (var assembly in filteredAssemblies)
            {
                var exportedTypes = assembly.GetExportedTypes();
                foreach (var t in exportedTypes)
                {
#if NETFX_CORE
                    if (t.GetTypeInfo().IsSubclassOf(isType))
#else
                    if (isType.IsAssignableFrom(t))
#endif
                    {
                        retList.Add(t);
                    }
                }
            }

            return retList.ToArray();
        }

    }
}
