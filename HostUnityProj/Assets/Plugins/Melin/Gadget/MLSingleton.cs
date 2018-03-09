using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if DEBUG_ML
using System.Threading;
#endif

namespace ML
{
    public abstract class MLSingleton<T> : MonoBehaviour where T : MLSingleton<T>
    {
        static T _ins;
        static readonly object locker = new object();
        public static T Self
        {
            get
            {
                lock(locker)
#if DEBUG_ML
                if(IsCheckOnMainThread)
                {
                    int t1 = MSystem.Instance.MainThreadId;
                    int t2 = Thread.CurrentThread.ManagedThreadId;
                    if (t1 != t2)
                    {
                        Log.ML.PrintWarning("Singleton Instance {0} use on thread: {1}, main: {2}", typeof(T).Name, t2, t1); ;
                    }
                }
#endif
                if (_ins == null)
                {
                    if (IsMono)
                    {
                        _ins = MSystem.Instance.gameObject.AddComponent<T>();
                        MSystem.Container.Inject(_ins);
                        _ins.onInitialize();
                    }
                    else
                    {
                        _ins = (T)MSystem.Container.CreateInstance(typeof(T));
                        _ins.onInitialize();
                    }
                }

                return _ins;
            }
        }
#if DEBUG_ML
        public static bool IsCheckOnMainThread
        {
            get
            {
                return true;
            }
        }
#endif
        public static bool IsMono
        {
            get
            {
                return true;
            }
        }

        protected abstract void onInitialize();
    }
}