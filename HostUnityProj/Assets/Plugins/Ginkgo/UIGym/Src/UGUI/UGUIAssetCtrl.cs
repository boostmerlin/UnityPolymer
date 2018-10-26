using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ginkgo.UI
{
    /// <summary>
    /// Manage UI Asset load and Cache.
    /// </summary>
    public class UGUIAssetsCtrl : IUIAssetCtrl
    {
        Dictionary<string, Dictionary<string, GameObject>> m_loadedObjects = new Dictionary<string, Dictionary<string, GameObject>>();


        GameObject findObject(string pkgName, string componentName)
        {
            Dictionary<string, GameObject> coms;
            if (m_loadedObjects.TryGetValue(pkgName, out coms)){
                GameObject go;
                if(coms.TryGetValue(componentName,  out go))
                {
                    return go;
                }
            }
            return null;
        }
        public object CreateObject(string pkgName, string componentName)
        {
            GameObject go = findObject(pkgName, componentName);
            if (!go)
            {
                
            }
            else
            {
                return GameObject.Instantiate(go);
            }

            return go;
        }

        bool loadLocalComponent(string pkgName, string componentName)
        {
            string key = pkgName + "/" + componentName;
            GameObject go = Resources.Load<GameObject>(key);
            if(go == null)
            {
                return false;
            }
            if (!m_loadedObjects.ContainsKey(pkgName))
            {
                m_loadedObjects.Add(pkgName, new Dictionary<string, GameObject>());
            }
            m_loadedObjects[pkgName].Add(componentName, go);
            return true;
        }
        void remoteLoad(string pkgName, Action<int> action)
        {

        }

        public bool LoadLocalPackage(string pkgName)
        {
            GameObject[] gos = Resources.LoadAll<GameObject>(pkgName);
            if(gos.Length > 0)
            {
                if (!m_loadedObjects.ContainsKey(pkgName))
                {
                    m_loadedObjects.Add(pkgName, new Dictionary<string, GameObject>());
                }
                var pkg = m_loadedObjects[pkgName];
                foreach (var go in gos)
                {
                    pkg.Add(go.name, go);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LoadRemotePackge(string[] assetPath)
        {
           foreach(var pkg in assetPath)
            {
                remoteLoad(pkg, (flag) =>
                {
                    Debug.Log("Load over: " + pkg + " " + flag);
                });
            }
        }

        public void PrepareAssets(string pkgName, string componentName, Action callBack)
        {
            var go = findObject(pkgName, componentName);
            if (go != null)
            {
                callBack();
            }
            else
            {
                if (GUIConfig.Selfie.checkRemoteAssetFirst)
                {
                    remoteLoad(pkgName, (i) =>
                    {

                    });
                }
                else
                {
                    if(!loadLocalComponent(pkgName, componentName))
                    {
                        remoteLoad(pkgName, (i)=>
                        {

                        });
                    }
                }
            }
        }

        public void RemoveComponent(string pkgName, string componentName)
        {
            GameObject go;
            go = findObject(pkgName, componentName);
            if(go != null)
            {
                Resources.UnloadAsset(go);
            }
        }

        public void RemovePackage(string pkgName)
        {
            Dictionary<string, GameObject> coms;
            if (m_loadedObjects.TryGetValue(pkgName, out coms))
            {
                foreach(var de in coms)
                {
                    Resources.UnloadAsset(de.Value);
                }
            }
        }
    }
}