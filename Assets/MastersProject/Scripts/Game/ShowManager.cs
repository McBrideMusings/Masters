//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters 
{
	/// <summary>
  /// Show Manager shows/hides puppets, framed as ActObjects. ActObjects are cataloged at runtime
  /// </summary>
	public class ShowManager : MonoBehaviour
	{
		#region Config
		public GameObject[] actParents;
		public GameObject sandboxParent;
		#endregion

		#region State
		[ReadOnly] public bool initilized { get; private set; } 
		private int currentActIndex;
		private ActObject[] sandboxDB;
		private Dictionary<GameObject, ActObject[]> levelDB;
		private StateMachine<State> stateMachine;
		private enum State { GAME, SANDBOX }
		private State currentState
		{
			get { return stateMachine.CurrentState; }
			set { stateMachine.CurrentState = value; }
		}
		#endregion

		#region Public
		public void ManagedUpdate()
		{
			stateMachine.Update();
		}
		public void SwitchModes()
		{
			if (currentState == State.GAME)
			{
				currentState = State.SANDBOX;
			}
			else
			{
				currentState = State.GAME;
			}
		}
		public void NextAct()
		{
			ResetAct(currentActIndex);
			SetActActive(currentActIndex, false);
			
			currentActIndex++;
			if (currentActIndex >= actParents.Length) currentActIndex = 0;

			SetActActive(currentActIndex, true);
		}
		public void ResetAct()
		{
			if (currentState == State.GAME)
			{
				ResetAct(levelDB[actParents[currentActIndex]]);
			}
			else
			{
				currentState = State.SANDBOX;
			}
		}
		#endregion

		#region Initilization
		//On Awake
		protected void Awake()
		{
			initilized = false;
			levelDB = new Dictionary<GameObject, ActObject[]>();
			for (int i = 0; i < actParents.Length; i++)
			{
				levelDB.Add(actParents[i],CatalogLevel(actParents[i]));
				SetActActive(i, false);
			}
			sandboxDB = CatalogLevel(sandboxParent);
			SetActActive(sandboxDB, false);
			CreateStateMachine();
			initilized = true;
		}
		private void CreateStateMachine()
		{
			stateMachine = new StateMachine<State>();

			stateMachine.AddState(
				State.GAME, 		// Label
				StartGame, 			// onStart
				UpdateGame,		 	// onEnter
				StopGame				// onStop
			);	
			stateMachine.AddState(
				State.SANDBOX, 		// Label
				StartSandbox, 		// onStart
				UpdateSandbox,		// onEnter
				StopSandbox				// onStop
			);	

			stateMachine.CurrentState = State.GAME;
		}
		#endregion


		#region Game State
		public void StartGame() 
		{
			SetActActive(currentActIndex, true);
		}

		public void UpdateGame(){}

		public void StopGame() 
		{
			ResetAct(currentActIndex);
			SetActActive(currentActIndex, false);
			currentActIndex = 0;
		}
		#endregion

		#region Sandbox State
		public void StartSandbox() 
		{
			SetActActive(sandboxDB, true);
		}

		public void UpdateSandbox(){}

		public void StopSandbox() 
		{
			ResetAct(sandboxDB);
			SetActActive(sandboxDB, false);
		}
		#endregion

    #region Helper
		private ActObject[] CatalogLevel(GameObject levelParent)
		{
			Transform[] childTransforms = levelParent.GetComponentsInChildren<Transform>(true);
			ActObject[] levelObjArray = new ActObject[childTransforms.Length];
			for (int i = 0; i < childTransforms.Length; i++)
			{
				Transform child = childTransforms[i];
				levelObjArray[i] = new ActObject(child.gameObject);
			}
			return levelObjArray;
		}
		private void SetActActive(int actIndex, bool active)
		{
			SetActActive(levelDB[actParents[actIndex]], active);
		}
		private void SetActActive(ActObject[] actObjects, bool active)
		{
			foreach (var actObj in actObjects)
			{
				actObj.obj.SetActive(active);
			}
		}
		private void ResetAct(int actIndex)
		{
			ResetAct(levelDB[actParents[actIndex]]);
		}
		private void ResetAct(ActObject[] actObjects)
		{
			foreach (var actObj in actObjects)
			{
				// Reset Object Physics
				if (actObj.rigidbody)
				{
					actObj.rigidbody.velocity = Vector3.zero;
					actObj.rigidbody.angularVelocity = Vector3.zero;
				}

				// Reset Object Transform
				actObj.transform.position = actObj.orgPosition;
				actObj.transform.rotation = actObj.orgRotation;
			}
		}
		#endregion
	}
	// This should probably be a scritable object
	// Time spent building this out is unecessary at runtime. It's something that's easy to edit as a dev
	// But once a level/act is done it doesn't need to change.
	public class ActObject 
	{
		public GameObject obj;
		public Transform transform;
		public Rigidbody rigidbody;
		public Vector3 orgPosition;
		public Quaternion orgRotation;

		public ActObject(GameObject o)
		{
			obj = o;
			transform = obj.GetComponent<Transform>();
			rigidbody = obj.GetComponent<Rigidbody>();
			orgPosition = o.transform.position;
			orgRotation = o.transform.rotation;
		}
	}
}