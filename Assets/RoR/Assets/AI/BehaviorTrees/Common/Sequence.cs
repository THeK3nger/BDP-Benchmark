using UnityEngine;
using System.Collections;

using RoomOfRequirement.AI.Data;

namespace RoomOfRequirement.AI.BehaviorTree {

	/// <summary>
	/// Implements a sequence task. A sequence returns faliure when it finds
	/// a child which returns a failure.
	/// </summary>
	public class Sequence : Task {

		public Sequence(params Task[] children) : base(children) {}

		public override TaskReturnValue Run (HierarchicalBlackboard blackboard) {
			foreach (Task t in Children) {
				if (t.Run(blackboard) != TaskReturnValue.SUCCESS) {
					return TaskReturnValue.FAILURE;
				}
			}
			return TaskReturnValue.SUCCESS;
		}
		
	}

	/// <summary>
	/// Implements a random sequence. It explores its children in ranom order and
	/// returns faliure when it finds a child which returns a failure.
	/// </summary>
	public class RandomSequence : Sequence {

		public RandomSequence(params Task[] children) : base(children) {}

		public override TaskReturnValue Run (HierarchicalBlackboard blackboard) {
			RandomCollection.Shuffle(Children);
			return base.Run(blackboard);
		}

	}
}
