using System.Collections.Generic;

using RoomOfRequirement.AI.Data;

namespace RoomOfRequirement.AI.BehaviorTree
{
	/// <summary>
	/// Collection of possible return values for a BHT task.
	/// </summary>
	public enum TaskReturnValue
	{
		SUCCESS,
		FAILURE,
		UNDEFINED
	} // And eventually more...

	/// <summary>
	/// Represents the basic element of a Behavior Tree.
	/// </summary>
	public abstract class Task
	{
		/// <summary>
		/// The list of children (sub-trees).
		/// </summary>
		protected List<Task> Children;

		/// <summary>
		/// Initializes a new instance of the <see cref="RoomOfRequirement.AI.BehaviorTree.Task"/> class.
		/// </summary>
		public Task() {
			Children = new List<Task>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RoomOfRequirement.AI.BehaviorTree.Task"/> class.
		/// </summary>
		/// <param name="children">The list of task's children.</param>
		public Task(params Task[] children) {
			Children = new List<Task>(children);
		}

		/// <summary>
		/// Run the task with the specified blackboard.
		/// </summary>
		/// <param name="blackboard">Blackboard.</param>
		public abstract TaskReturnValue Run (HierarchicalBlackboard blackboard);

		/// <summary>
		/// Adds a child to the task.
		/// </summary>
		/// <param name="child">Child.</param>
		public virtual void AddChild(Task child) {
			Children.Add(child);
		}
	}
}