﻿using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;

public class MethodProfiler {

	static public long LastMethodTime;

	static public long ProfileMethod<T1,T2>(Action<T1,T2> method, T1 param1, T2 param2) {
		Stopwatch st = Stopwatch.StartNew();
		method(param1,param2);
		st.Stop();
		LastMethodTime = st.ElapsedTicks;
		return LastMethodTime;
	}

}
