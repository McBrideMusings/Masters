//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace PlayByPierce.Masters 
{
	public delegate void VRPlatformDelegate(GameObject headset, GameObject leftController, GameObject rightController);
	/// <summary>
  /// Singleton manager of the game state. Catches input and controls which puppets are active
  /// </summary>
	public class GameManager : Singleton<GameManager>  
	{
		#region Config
		public const float ExpectedDeltaTime = 0.0111f;
		
		[Header("Debug Controls")]
		public string restartGame = "RestartGame";
		public string switchModes = "SwitchModes";
		public string nextAct = "NextAct";
		public string quit = "Quit";
		public string resetAct = "ResetAct";
		#endregion


		#region State
		public VRTK_SDKManager sdkManager { get; private set; } 
		public GameObject headset { get; private set; } 
		public Controller leftController { get; private set; } 
		public Controller rightController { get; private set; } 
		public SoundManager sound { get; private set; } 
		public ShowManager show { get; private set; } 
		private StateMachine<State> stateMachine;
		private enum State { INIT, GAME }
		private State currentState
		{
			get { return stateMachine.CurrentState; }
			set { stateMachine.CurrentState = value; }
		}
		#endregion


		#region Bookkeeping Fields
		public bool initilized { get; private set; } 
		public bool vrLoaded { get; private set; } 
		#endregion

    #region Delegates
		// I need a smarter way to talk to VRTK's low-level hardware abstractions. These are really useful
		// But I need a better way to filter between different changes in setup
		// and data about a setup (size of play area, for example)
		public void OnLoadedSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
			if (e.currentSetup != e.previousSetup) {
				GetVRReferences(e);
			}
    }
		#endregion

    #region Initilization 
		public override void Awake() {
			base.Awake();
			sound = gameObject.GetComponent<SoundManager>();
			show = gameObject.GetComponent<ShowManager>();
			Time.fixedDeltaTime = ExpectedDeltaTime; // Taken from NewtonVR. Do not know what it's for

			sdkManager = VRTK_SDKManager.instance;
			sdkManager.LoadedSetupChanged += OnLoadedSetupChanged;
			CreateStateMachine();
		}
		// See above. Need a dedicated bridge between VRTK and my code. I want to use VRTK
		// low-level code but not the high-level stuff. I want to not care WHICH controller I'm talking to but
		// Want to handle the logic that happens after myself.
		public void GetVRReferences(VRTK_SDKManager.LoadedSetupChangeEventArgs e) {
			if (e.currentSetup != null) {
				headset = e.currentSetup.actualHeadset;
				leftController = e.currentSetup.actualLeftController.GetComponent<Controller>();
				rightController = e.currentSetup.actualRightController.GetComponent<Controller>();
				vrLoaded = true;
			}
			else {
				vrLoaded = false;
			}
		}
		private void CreateStateMachine()
		{
			stateMachine = new StateMachine<State>();

			stateMachine.AddState(
				State.INIT, 		// Label
				StartInit, 			// onStart
				UpdateInit,		 	// onEnter
				StopInit				// onStop
			);	

			stateMachine.AddState(
				State.GAME, 		// Label
				StartGame, 			// onStart
				UpdateGame,		 	// onEnter
				StopGame				// onStop
			);	

			stateMachine.CurrentState = State.INIT;
		}
		#endregion


    #region Core 
		/// <summary>
		/// Update is called
		/// </summary>
		public void Update()
		{
			stateMachine.Update();
		}
		#endregion


		#region Init State
		public void StartInit() {}

		public void UpdateInit()
		{
			if (show.initilized)
      {
				stateMachine.CurrentState = State.GAME;
			}	
		}

		public void StopInit() {}
		#endregion


		#region Game State
		public void StartGame() {}

		public void UpdateGame()
		{
			show.ManagedUpdate();

			// This is bad. Should have a real dedicated InputManager
			if (Input.GetButtonDown(resetAct)) show.ResetAct();
			else if (Input.GetButtonDown(nextAct)) show.NextAct();
			else if (Input.GetButtonDown(switchModes)) show.SwitchModes();
			else if (Input.GetButtonDown(quit)) QuitGame();
		}

		public void StopGame() {}
		#endregion


    #region Helper
		public void QuitGame()
		{
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#endif
			Application.Quit();
		}
		#endregion 
	}
}
