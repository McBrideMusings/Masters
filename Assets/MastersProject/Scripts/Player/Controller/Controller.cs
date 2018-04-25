//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Mostly VRTK
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace PlayByPierce.Masters
{
	public enum ControllerHand { Left, Right };
	/// <summary>
  /// A master class for all controller-based logic
	/// (Not written out right now)
  /// </summary>
	public class Controller : MonoBehaviour 
	{
		#region Public Fields
		public ControllerHand hand = ControllerHand.Left;
		public ControllerEvents controllerEvents;
		public ControllerTouch controllerTouch;
		public ControllerGrab controllerGrab;
		public ControllerUse controllerUse;
		#endregion

		#region Private Fields
		#endregion
	}
}
