using System.ComponentModel.Design.Serialization;

using UnityEngine;
using System.Collections;

using RoomOfRequirement.Architectural;

public class StepCounter : MonoSingleton<StepCounter>
{

    public static long Steps = 0;

	public static void Increase()
	{
	    StepCounter.Steps++;
	}

}
