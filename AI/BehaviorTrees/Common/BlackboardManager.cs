using UnityEngine;
using System.Collections;

using RoomOfRequirement.AI.Data;

namespace RoomOfRequirement.AI.BehaviorTree {

	/// <summary>
	/// Implements a Blackboard manager decorator. This decorator create
	/// a new child blackboard before to execute the child sub-tree.
	/// </summary>
	public class BlackboardManager : Decorator {
		
		public BlackboardManager(Task task) : base(task) {}
		
		public override TaskReturnValue Run (HierarchicalBlackboard blackboard) {
			var newBB = new HierarchicalBlackboard(blackboard);
			return Child.Run(newBB);
		}
	}
}
