using UnityEngine;
using System;

namespace PlayByPierce
{
	[Flags] public enum MonsterState
	{
		IsHungry = 1,
		IsThirsty = 2,
		IsAngry = 4,
		IsTired = 8
	}

	[Serializable] public class MonsterData
	{
		public string name;
		public string nickName;
		public Color color;
	}

	//This class would work exactly the same in the inspector
	//if it was extended from MonoBehaviour instead, except
	//for the InspectorButton
	public class PropertyDrawerExample : MonoBehaviour
	{
		[ReadOnly] public string readonlyString = "Cannot change in inspector";

		[Space]
		[Comment("Cannot be negative")]
		[NonNegative] public int nonNegativeInt = 0;

		[NonNegative] public float nonNegativeFloat = 0f;

		[Highlight] public int highligtedInt;

		[Space]
		[Comment("Can only be positive")]
		[Positive] public int positiveInt = 1;

		[Positive] public float positiveFloat = 0.1f;

		[InspectorFlags] public MonsterState monsterState = MonsterState.IsAngry | MonsterState.IsHungry;
	}
}
