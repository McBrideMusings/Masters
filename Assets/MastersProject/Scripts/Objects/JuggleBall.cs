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
  /// class description
  /// </summary>
	public class JuggleBall : MonoBehaviour 
	{
		#region Config
		[Header("References")]
		public JuggleHand[] hands;

		[Header("Config")]
		public int index = 0;
		public float speedMultipier = 1f;
		public float distanceThreshold = 0.5f;
		public float radius = 1f;
		#endregion

		#region State
		[Space(10f)]
		[ReadOnly] [SerializeField] private JuggleHand currentHand;
		[ReadOnly] [SerializeField] private JuggleHand previousHand;
		[ReadOnly] [SerializeField] private bool held = true;
		private Vector3 tempPosition = Vector3.zero;
		#endregion

		#region Delegates
		#endregion

		#region Properties
		#endregion

		#region Public
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		public void Bounce() 
		{
			index++;
			if (index >= hands.Length) index = 0;
			previousHand = currentHand;
			currentHand = hands[index];
			currentHand.AddBall(this);
			held = false;
		}
		#endregion

		#region Initialization
    // Get References and Set Initial Values
		protected void Awake() 
		{
			currentHand = hands[index];
			previousHand = hands[index+1];
			currentHand.AddBall(this);
		}
		#endregion

		#region Core
		protected void FixedUpdate() 
		{			
			
			float t = Mathf.PingPong(Time.time * speedMultipier, 1f);
			tempPosition = Vector3.Lerp(currentHand.transform.position, previousHand.transform.position, t);
			tempPosition.y = Mathf.Sin(t*(Mathf.PI))*radius + tempPosition.y;
			transform.position = tempPosition;
			/* 
			if (held) 
			{
				transform.position = currentHand.transform.position;
			}
			else
			{
				float t = Time.time * speedMultipier;
				tempPosition = Vector3.Lerp(previousHand.transform.position, currentHand.transform.position, t);
				//tempPosition.y = Mathf.Sin(t * Mathf.PI) + tempPosition.y;
				transform.position = tempPosition;
				if (Vector3.Distance(currentHand.transform.position, previousHand.transform.position) < distanceThreshold)
				{
					held = true;
				}
			}
			*/
		}
		#endregion
	}
}

/* 

		#region Config
		[Header("References")]
		public Transform[] setupPoints;
		[HideInInspector] public Rigidbody body;
		[HideInInspector] public HoldPoint[] holdPoints;
		#endregion

		#region Initialization
		protected void Awake() 
		{
			body = GetComponent<Rigidbody>();
			holdPoints = new HoldPoint[setupPoints.Length];
			for (int i = 0; i < holdPoints.Length; i++)
			{
				holdPoints[i] = new HoldPoint(setupPoints[i]);
			}
		}
		#endregion
	
		#region Public
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		public GameObject GetBall() 
		{
			GameObject sentBall = null;
			for (int i = 0; i < holdPoints.Length; i++)
			{
				if (holdPoints[i].heldBall != null)
				{
					sentBall = holdPoints[i].heldBall;
					holdPoints[i].heldBall = null;
				}
			}
			return sentBall;
		}
		#endregion

		#region Data
		public struct HoldPoint
		{
			public Transform point;
			public GameObject heldBall;
			public HoldPoint(Transform p)
			{
				point = p;
				heldBall = null;
			}
		}
		#endregion


			#region Config
		[Header("Reference")]
		public GameObject[] balls;
		public JuggleHand[] juggleHands;

		[Header("Config")]
		public float velocityThreshold = 1f;
		public float speed = 1f;
		#endregion

		#region State
		[Space(10f)]
		private Vector3 temp = Vector3.zero;
		#endregion

		#region Properties
		#endregion

		#region Delegates
		#endregion

		#region Initialization
		protected void Start() 
		{
			for (int i = 0; i < balls.Length; i++)
			{
				balls[i].transform.position = juggleHands[0].holdPoints[i].point.position;
				juggleHands[0].holdPoints[i].heldBall = balls[i];
			}
		}
		#endregion

		#region Core
    /// Updates Once Per Physics Update
		protected void FixedUpdate() 
		{
			for (int i = 0; i < juggleHands.Length; i++)
			{
				if (juggleHands[i].body.velocity.y > velocityThreshold)
				{
					GameObject ball = juggleHands[i].GetBall();
					if (ball != null)
					{

					}
				}
			}
			if (ball.stateMachine.CurrentState == JuggleBall.State.HELD)
			{
				if (rigidbody.velocity.y > velocityThreshold)
				{
					ballIndex++;
					if (ballIndex > ballPoints.Length) ballIndex = 0;
					ball.target = ballPoints[ballIndex];
					ball.stateMachine.CurrentState = JuggleBall.State.MOVING;
				}
			}
			float t = Mathf.PingPong(Time.time * speed, 1f);
			temp = Vector3.Lerp(Hand1.position, Hand2.position, t);
			temp.y = Mathf.Sin(t*Mathf.PI);
			transform.position = temp;
*/
