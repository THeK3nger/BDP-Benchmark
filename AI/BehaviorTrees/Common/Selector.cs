using UnityEngine;
using System.Collections;

using RoomOfRequirement.AI.Data;

namespace RoomOfRequirement.AI.BehaviorTree {

	/// <summary>
	/// Implements a Selector. A selector returns success when it executes
	/// the first child which returns succesfully.
	/// </summary>
	public class Selector : Task {

		public Selector(params Task[] children) : base(children) {}

		public override TaskReturnValue Run (HierarchicalBlackboard blackboard) {
			foreach (Task t in Children) {
				if (t.Run(blackboard) == TaskReturnValue.SUCCESS) {
					return TaskReturnValue.SUCCESS;
				}
			}
			return TaskReturnValue.FAILURE;
		}

	}

	/// <summary>
	/// Implements a Random Selector. Random selectors explores its children
	/// in random order and returns success when it executes
	/// the first child which returns succesfully
	/// </summary>
	public class RandomSelector : Selector {

		public RandomSelector(params Task[] children) : base(children) {}
		
		public override TaskReturnValue Run (HierarchicalBlackboard blackboard) {
			RandomCollection.Shuffle(Children);
			return base.Run(blackboard);
		}
		
	}
}