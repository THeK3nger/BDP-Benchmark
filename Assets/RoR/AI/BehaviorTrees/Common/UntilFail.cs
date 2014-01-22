using UnityEngine;
using System.Collections;

using RoomOfRequirement.AI.Data;

namespace RoomOfRequirement.AI.BehaviorTree {

	/// <summary>
	/// Implements the until fail decorator. It executes its child until it fails.
	/// </summary>
	public class UntilFail : Decorator {
		
		public UntilFail(Task task) : base(task) {}
		
		public override TaskReturnValue Run (HierarchicalBlackboard blackboard) {
			while (true) {
				TaskReturnValue result = Child.Run(blackboard);
				if (result != TaskReturnValue.SUCCESS) break;
			}
			return TaskReturnValue.SUCCESS;
		}
	}
}
