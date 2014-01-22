using UnityEngine;
using System.Collections;

using RoomOfRequirement.AI.Data;

namespace RoomOfRequirement.AI.BehaviorTree {

	/// <summary>
	/// Implements a Decorator for a Task.
	/// </summary>
	/// <remarks>
	/// Decorators are a special type of task which can have only one child. 
	/// </remarks>
	public abstract class Decorator : Task {

		/// <summary>
		/// Gets the child.
		/// </summary>
		/// <value>The child.</value>
		public Task Child {
			get { return Children[0]; }
		}

		public Decorator(Task task) : base()  {
			Children.Add(task);
		}

		/// <summary>
		/// Adds a child to the task. Can add only one child.
		/// </summary>
		/// <param name="child">Child.</param>
		public sealed override void AddChild (Task child)
		{
			if (Children.Count >= 1) {
				// TODO: EXCEPTION
			}
			Children.Add(child);
		}

	}
}