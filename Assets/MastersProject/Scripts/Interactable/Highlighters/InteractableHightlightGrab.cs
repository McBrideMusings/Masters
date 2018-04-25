//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	/// <summary>
  /// class description
  /// </summary>
	public class InteractableHightlightGrab : InteractableEvents 
	{
		#region Config
		[Header("Config")]
		public PhysGrabLimb grabLimb;
		public Color touchHighlightColor = Color.clear;
		public Color grabHighlightColor = Color.clear;
		public Color grabbingHighlightColor = Color.clear;
		public Renderer[] highlightedRenderers;
		#endregion

		#region State
		private Color currentColor = Color.clear; // If clear, means original color
		protected Dictionary<string, Color> originalRendererColors = new Dictionary<string, Color>();
		private bool isTouched = false; // Is this object being touchged by a controller?
		private bool isGrabbed = false; // Is this object being grabbed by a controller?
		private bool isGrabbing = false; // Is this object grabbing another object?
		private AvatarGrabbables grabbables;
		#endregion

		#region Delegates
		public void OnGrabbing()
		{
			isGrabbing = true;
		}
		public void OnNotGrabbing()
		{
			isGrabbing = false;
		}
		public override void OnInteractableTouched(object sender, InteractableEventArgs e) 
		{
			isTouched = true;
		}
		public override void OnInteractableUnTouched(object sender, InteractableEventArgs e) 
		{
			isTouched = false;
		}
		public override void OnInteractableGrabbed(object sender, InteractableEventArgs e) 
		{
			isGrabbed = true;
		}
		public override void OnInteractableUnGrabbed(object sender, InteractableEventArgs e) 
		{
			isGrabbed = false;
		}
		#endregion

		#region Initialization
    // Get References and Set Initial Values
		protected override void Awake() 
		{
			base.Awake();
			originalRendererColors = new Dictionary<string, Color>();
      StoreOriginalColors();
			grabLimb.Grabbing += OnGrabbing;
			grabLimb.NotGrabbing += OnNotGrabbing;
		}
		#endregion

		#region Core
		protected virtual void Update()
		{
			if (isGrabbing)
			{
				foreach (Renderer renderer in highlightedRenderers)
				{
					ChangeToColor(renderer, grabbingHighlightColor);
				}
			}
			else if (interactable.IsGrabbed())
			{
				foreach (Renderer renderer in highlightedRenderers)
				{
					ChangeToColor(renderer, grabHighlightColor);
				}
			}
			else if (interactable.IsTouched())
			{
				foreach (Renderer renderer in highlightedRenderers)
				{
					ChangeToColor(renderer, touchHighlightColor);
				}
			}
			else
			{
				foreach (Renderer renderer in highlightedRenderers)
				{
					ChangeToOriginalColor(renderer);
				}
			}
		}
		protected void DecideColor()
		{
			if (isGrabbing)
			{
				foreach (Renderer renderer in highlightedRenderers)
				{
					ChangeToColor(renderer, grabbingHighlightColor);
				}
			}
			else if (isGrabbed)
			{
				foreach (Renderer renderer in highlightedRenderers)
				{
					ChangeToColor(renderer, grabHighlightColor);
				}
			}
			else if (isTouched)
			{
				foreach (Renderer renderer in highlightedRenderers)
				{
					ChangeToColor(renderer, touchHighlightColor);
				}
			}
			else
			{
				foreach (Renderer renderer in highlightedRenderers)
				{
					ChangeToOriginalColor(renderer);
				}
			}
		}
    protected virtual void StoreOriginalColors()
    {
      originalRendererColors.Clear();
			foreach (Renderer renderer in highlightedRenderers)
			{
				var objectReference = renderer.gameObject.GetInstanceID().ToString();
				originalRendererColors[objectReference] = renderer.material.color;
			}
    }
    protected virtual void ChangeToColor(Renderer renderer, Color color)
    {
      for (int i = 0; i < renderer.materials.Length; i++)
      {
        var material = renderer.materials[i];
        material.color = color;
      }
    }
    protected virtual void ChangeToOriginalColor(Renderer renderer)
    {
      var objectReference = renderer.gameObject.GetInstanceID().ToString();
      if (!originalRendererColors.ContainsKey(objectReference))
      {
        return;
      }
			ChangeToColor(renderer, originalRendererColors[objectReference]);
    }
		#endregion

		#region Listeners
		#endregion
	}
}
