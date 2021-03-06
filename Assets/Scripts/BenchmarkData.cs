// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BenchmarkData.cs" company="">
//   
// </copyright>
// <summary>
//   Store and write to file benchmark data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;

/// <summary>
/// Store and write to file benchmark data.
/// </summary>
public class BenchmarkData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BenchmarkData"/> class. 
    /// Constructor for the benchmark data.
    /// </summary>
    /// <param name="test">
    /// A reference to the actual testing class instance.
    /// </param>
    public BenchmarkData(PathfindTester test)
    {
        this.RunsData = new List<SingleRunData>();
        this.AgentType = test.ThePathfinder.AgentBelief.ToString();
        this.BeliefMemoryUsed = test.ThePathfinder.AgentBelief.MemoryByteUsed();
        this.ScrambleRate = test.ScrambleRate;
        this.IncosistencyRate = test.RandomStrategy.GetRandomAmount();
        this.MapFile = test.CurrentMapName;
		this.ParamTag = test.CurrentParam;
    }

    /// <summary>
    /// Gets or sets the agent type.
    /// </summary>
    public string AgentType { get; set; }

    /// <summary>
    /// Gets or sets the amount of memory used by the agent.
    /// </summary>
    public int BeliefMemoryUsed { get; set; }

    /// <summary>
    /// Gets or sets the frequency of map changes.
    /// </summary>
    public double ScrambleRate { get; set; }

    /// <summary>
    /// Gets or sets the map average entropy of the map expressed as a floating point 
    /// quantity.
    /// </summary>
    public double IncosistencyRate { get; set; }

    /// <summary>
    /// Gets or sets the map file used in the benchmark.
    /// </summary>
    public string MapFile { get; set; }

    /// <summary>
    /// Gets a list of SingleRunData for the benchmark.
    /// </summary>
    public List<SingleRunData> RunsData { get; private set; }

    public float TMin { get; set; }

	public string ParamTag {get; set; }

    /// <summary>
    /// Print all the collected data in to a file in CSV format.
    /// </summary>
    public void PrintToFile()
    {
        var filename = string.Format(
			"{4}-{0}-{1}-{2}-{3}-{5}-{6}.csv", 
            AgentType, 
            BeliefMemoryUsed, 
            ScrambleRate, 
            IncosistencyRate, 
			MapFile,TMin,ParamTag);
        using (var file = new System.IO.StreamWriter(filename))
        {
            file.WriteLine("#A,Explored,MaxMem,Lenght,PathTicks,UpdateTicks,PathFound,RealPath,RealEffort");
            foreach (var srd in RunsData)
            {
                file.WriteLine(
					"{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
                    srd.NumberOfAttempts, 
                    srd.ExploredNodes, 
                    srd.MaxMemoryUsage, 
                    srd.PathLenght.ToString("0.00"), 
                    srd.PathfindingTicks, 
                    srd.UpdateTicks, 
                    srd.PathFound,
                    srd.RealPathExists,
					srd.OmniscientEffort);
            }
        }
    }
}

/// <summary>
/// A SingleRunData store information for a single pathfinding action
/// in the particular benchmark. A full benchmark is usually composed by 200
/// SRD.
/// </summary>
public class SingleRunData
{
    /// <summary>
    /// The starting point.
    /// </summary>
    public string StartingPoint { get; set; }

    /// <summary>
    /// The target point.
    /// </summary>
    public string TargetPoint { get; set; }

    /// <summary>
    /// The number of attempts for reaching the target point (or declare it
    /// as unreachable).
    /// </summary>
    public int NumberOfAttempts { get; set; }

    /// <summary>
    /// The number of explored nodes for reaching the target point.
    /// </summary>
    public int ExploredNodes { get; set; }

    /// <summary>
    /// The maximum amount of memory used for the computation.
    /// </summary>
    public int MaxMemoryUsage { get; set; }

    /// <summary>
    /// The real path lenght.
    /// </summary>
    public double PathLenght { get; set; }

    /// <summary>
    /// The number of ticks for the pathfinding computation.
    /// </summary>
    public long PathfindingTicks { get; set; }

    /// <summary>
    /// The number of ticks for the belief update phase.
    /// </summary>
    public long UpdateTicks { get; set; }

    /// <summary>
    /// True if the agent reached the target tile during the execution of 
    /// this step.
    /// </summary>
    public bool PathFound { get; set; }

    public bool RealPathExists { get; set; }

	public int OmniscientEffort { get; set; }
}
