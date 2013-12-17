using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Represent a generic Path along a graph.
/// </summary>
public class Path<TNode> : IEnumerable<TNode> {

#region PublicProperties
	/// <summary>
	/// Gets the last step.
	/// </summary>
	/// <value>The last step.</value>
	public TNode LastStep { get; private set;}

	/// <summary>
	/// Gets the previous steps.
	/// </summary>
	/// <value>The previous steps.</value>
	public Path<TNode> PreviousSteps { get; private set;}

	/// <summary>
	/// Gets the total cost.
	/// </summary>
	/// <value>The total cost.</value>
	public double TotalCost { get; private set; }
#endregion

#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="Path`1"/> class.
	/// </summary>
	/// <param name="lastStep">Last step.</param>
	/// <param name="previousSteps">Previous steps.</param>
	/// <param name="totalCost">Total cost.</param>
	Path(TNode lastStep, Path<TNode> previousSteps, double totalCost) {
		LastStep = lastStep;
		PreviousSteps = previousSteps;
		TotalCost = totalCost;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Path`1"/> class.
	/// </summary>
	/// <param name="start">Start.</param>
	public Path(TNode start) : this(start,null,0) { }
#endregion

	/// <summary>
	/// Adds a step to the path.
	/// </summary>
	/// <returns>The new path.</returns>
	/// <param name="step">The step.</param>
	/// <param name="stepCost">The step cost.</param>
	public Path<TNode> AddStep(TNode step, double stepCost) {
		return new Path<TNode>(step,this,TotalCost+stepCost);
	}

#region	EnumerableImplementation
	/// <summary>
	/// Gets the enumerator.
	/// </summary>
	/// <returns>The enumerator.</returns>
	public IEnumerator<TNode> GetEnumerator()
	{
		for (Path<TNode> p = this; p != null; p = p.PreviousSteps)
			yield return p.LastStep;
	}

	/// <summary>
	/// Gets the enumerator.
	/// </summary>
	/// <returns>The enumerator.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}
#endregion
	
}
