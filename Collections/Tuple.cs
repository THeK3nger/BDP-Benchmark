using System.Collections;
using System;
using System.Runtime.Serialization;

using UnityEngine;

/// <summary>
/// Personal implementation for the Tuple collection in .NET 4.0.
/// </summary>
[Serializable]
public class Tuple<T1, T2> : ISerializable
{
	/// <summary>
	/// Gets the first element.
	/// </summary>
	/// <value>The first element.</value>
	public T1 First { get; private set; }

	/// <summary>
	/// Gets the second element.
	/// </summary>
	/// <value>The second element.</value>
	public T2 Second { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Tuple`2"/> class.
	/// </summary>
	/// <param name="first">The first element.</param>
	/// <param name="second">The second element.</param>
	internal Tuple (T1 first, T2 second)
	{
		First = first;
		Second = second;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Tuple`1"/> class.
	/// </summary>
	/// <param name="info">Serialization Info.</param>
	/// <param name="context">Context.</param>
	public Tuple(SerializationInfo info, StreamingContext context) {
		if (info == null)
			throw new ArgumentNullException("info");
		First = (T1) info.GetValue("First",typeof(T1));
		Second = (T2) info.GetValue("Second",typeof(T2));
		//Debug.Log(string.Format("Tuple {0} {1}",First,Second));
	}

	/// <summary>
	/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Tuple`2"/>.
	/// </summary>
	/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Tuple`2"/>.</param>
	/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="Tuple`2"/>; otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj)
	{
		if (obj == null  || GetType() != obj.GetType()) return false;
		var other = (Tuple<T1,T2>) obj;
		return (First.Equals(other.First)) && (Second.Equals(other.Second));
	}
	
	/// <summary>
	/// Serves as a hash function for a <see cref="Tuple`2"/> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
	public override int GetHashCode()
	{
		return First.GetHashCode() + 11*Second.GetHashCode();
	}

	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="Tuple`2"/>.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="Tuple`2"/>.</returns>
	public override string ToString ()
	{
		return string.Format ("<{0}, {1}>", First, Second);
	}

	#region SertializationMethods
	/// <summary>
	/// Implement ISerializable interface.
	/// </summary>
	/// <param name="info">Serialization Info.</param>
	/// <param name="context">Streaming Context.</param>
	public void GetObjectData (SerializationInfo info, StreamingContext context) {
		if (info == null)
			throw new ArgumentNullException("info");
		info.AddValue("First",First);
		info.AddValue("Second",Second);
		//Debug.Log(string.Format("Tuple {0} {1}",First,Second));
	}
	#endregion
}
