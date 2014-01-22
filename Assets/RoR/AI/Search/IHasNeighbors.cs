using System.Collections.Generic;

namespace RoomOfRequirement
{

/// <summary>
/// Interface that rapresent data structures that has the ability to find node neighbours.
/// </summary>
	public interface IHasNeighbours<T>
	{
		/// <summary>
		/// Gets the neighbours of the instance.
		/// </summary>
		/// <value>The neighbours.</value>
		IEnumerable<T> Neighbours (T node);
	}
}