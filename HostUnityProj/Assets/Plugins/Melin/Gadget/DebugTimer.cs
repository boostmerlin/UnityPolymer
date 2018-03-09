using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace ML
{
    /// <summary>
    /// 测试指定代码运行时间 
    /// </summary>
    public class DebugTimer
    {
#if DEBUG_TIMER
        Dictionary<string, Stopwatch> watchers;

        Queue<Stopwatch> pools;
        static DebugTimer s_debugTimer;

        DebugTimer()
        {
            watchers = new Dictionary<string, Stopwatch>();
            pools = new Queue<Stopwatch>();
            watchers.Clear();
        }

        Stopwatch get()
        {
            if (pools.Count <= 0)
            {
                Stopwatch w = new Stopwatch();
                return w;
            }
            else
            {
                Stopwatch w = pools.Dequeue();
                return w;
            }
        }


        void release(Stopwatch w)
        {
            if (w.IsRunning)
            {
                w.Stop();
            }
            w.Reset();
            pools.Enqueue(w);
        }
#endif

        public static void BEGIN()
        {
#if DEBUG_TIMER
        BEGIN("unnamed");
#endif
        }

        public static void BEGIN(string name)
        {
#if DEBUG_TIMER
        if (s_debugTimer == null)
        {
            s_debugTimer = new DebugTimer();
        }

        Stopwatch commonWatch;
        if (!s_debugTimer.watchers.ContainsKey(name))
        {
            s_debugTimer.watchers.Add(name, s_debugTimer.get());
        }
        else
        {
            Log.TimeCost.PrintWarning("Watch has already in watchers: " + name);
        }
        commonWatch = s_debugTimer.watchers[name];
        commonWatch.Start();
#endif
        }

        public static void END()
        {
#if DEBUG_TIMER
        END("unnamed");
#endif
        }
        public static void END(string name)
        {
#if DEBUG_TIMER
        Stopwatch commonWatch;

        if (s_debugTimer.watchers.TryGetValue(name, out commonWatch))
        {
            commonWatch.Stop();
            Log.TimeCost.Print(string.Format("<color=green>$$$$$$$$$$ {0} takes: {1} ms </color>", name, commonWatch.ElapsedMilliseconds));
            s_debugTimer.release(commonWatch);
            s_debugTimer.watchers.Remove(name);
        }
        else
        {
            Log.TimeCost.PrintWarning("END not Find watch: " + name);
        }
#endif
        }
    }
}