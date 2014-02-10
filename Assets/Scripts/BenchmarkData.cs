using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BenchmarkData  {

	public string AgentType { get; set; }
	public int BeliefMemoryUsed { get; set; }
	public double ScrambleRate { get; set; }
	public double IncosistencyRate { get; set; }
	public string MapFile { get; set; }

	public List<SingleRunData> RunsData { get; private set; }

	public BenchmarkData() {
		RunsData = new List<SingleRunData>();
	}

	public void PrintToFile() {
		// Example #3: Write only some strings in an array to a file. 
		// The using statement automatically closes the stream and calls  
		// IDisposable.Dispose on the stream object. 
		string filename = string.Format("{4}-{0}-{1}-{2}-{3}.csv",
		                                AgentType,
		                                BeliefMemoryUsed,
		                                ScrambleRate,
		                                IncosistencyRate,
		                                MapFile);
		using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
		{
			file.WriteLine("#A,Explored,MaxMem,Lenght,PathTicks,UpdateTicks,PathFound");
			foreach (SingleRunData srd in RunsData) {
				file.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",
				                             srd.NumberOfAttempts,
				                             srd.ExploredNodes,
				                             srd.MaxMemoryUsage,
				                             srd.PathLenght.ToString("0.00"),
				                             srd.PathfindingTicks,
				                             srd.UpdateTicks,
				                             srd.PathFound));
			}
		}
	}

}

public class SingleRunData {
	public string StartingPoint {get; set; }
	public string TargetPoint {get; set;}
	public int NumberOfAttempts { get; set; }
	public int ExploredNodes { get; set;}
	public int MaxMemoryUsage { get; set;}
	public double PathLenght {get; set;}
	public long PathfindingTicks {get; set;}
	public long UpdateTicks {get; set;}
	public bool PathFound {get; set;}

	public SingleRunData ()
	{
	}
	
}