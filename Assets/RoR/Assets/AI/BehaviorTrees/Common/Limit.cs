using UnityEngine;
using System.Collections;

using RoomOfRequirement.AI.Data;

namespace RoomOfRequirement.AI.BehaviorTree {

	/// <summary>
	/// Implements the Limit decorator. It execute its child a fixed amount
	/// of times or until it fails.
	/// </summary>
	public class Limit : Decorator {

		public int RunLimit {get; private set;}

		int runSoFar = 0; 

		public Limit(Task task, int runLimit) : base(task) {
			if (runLimit<0) {
				// TODO: EXCEPTION
			}
			RunLimit = runLimit;
		}

		public override TaskReturnValue Run (HierarchicalBlackboard blackboard) {
			if (runSoFar >= RunLimit) {
				return TaskReturnValue.FAILURE;
			}
			runSoFar++;
			return Child.Run(blackboard);
		}
	}
}
