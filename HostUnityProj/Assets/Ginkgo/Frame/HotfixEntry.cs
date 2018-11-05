using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using Ginkgo;

/// <summary>
/// 热更工程的加载入口
/// </summary>
public class HotfixEntry
{
#if ILRUNTIME
	private Runtime.Enviorment.AppDomain appDomain;
#else
    private Assembly assembly;
#endif

    public Action Update { get; set; }
    public Action LateUpdate { get; set; }
    public Action OnApplicationQuit { get; set; }

    public void LoadHotfixAssembly()
    {
#if ILRUNTIME
			Log.Debug($"当前使用的是ILRUNTIME模式");
			this.appDomain = new ILRUNTIME.Runtime.Enviorment.AppDomain();
			
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] pdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;

			using (MemoryStream fs = new MemoryStream(assBytes))
			using (MemoryStream p = new MemoryStream(pdbBytes))
			{
				this.appDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
			}

			this.start = new ILStaticMethod(this.appDomain, "ETHotfix.Init", "Start", 0);
#else
        Log.Common.Print("Load Assembly using Mono.");
        byte[] pdbBytes = null;
        byte[] assBytes = null;
#if DEBUG
#endif
        this.assembly = Assembly.Load(assBytes, pdbBytes);
#endif
    }
}

