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
  /// Simplified highlight logic that listens for interactable events and lets me directly assign
	/// renderers to be colored (VRTK over-designed)
  /// </summary>
	public class InteractableHighlight : InteractableEvents 
	{
		#region Config
		[Header("Config")]
		public HighlightMode onTouch = HighlightMode.ON;
		public Color touchHighlightColor = Color.clear;
		public HighlightMode onGrab = HighlightMode.NONE;
		public Color grabHighlightColor = Color.clear;
		public HighlightMode onUse = HighlightMode.NONE;
		public Color useHighlightColor = Color.clear;
		public Renderer[] highlightedRenderers;
		#endregion

		#region State
		private int interactableState = 0; // 0 is idle, 1 is touched, 2 is grabbed
		public enum HighlightMode {NONE, ON};
		protected Dictionary<string, Color> originalRendererColors = new Dictionary<string, Color>();
		[SerializeField] [ReadOnly] protected bool isTouched = false;
		[SerializeField] [ReadOnly] protected bool isGrabbed = false;
		[SerializeField] [ReadOnly] protected bool isUsed = false;
		#endregion

		#region Delegates
		/* 
		public override void OnInteractableTouched(object sender, InteractableEventArgs e) 
		{
			isTouched = true;
			switch (onTouch)
			{
				case HighlightMode.ON:
					foreach (Renderer renderer in highlightedRenderers)
					{
						ChangeToColor(renderer, touchHighlightColor);
					}
					break;
				case HighlightMode.NONE:
					// Do Nothing
					break;
			}
		}
		public override void OnInteractableUnTouched(object sender, InteractableEventArgs e) 
		{
			isTouched = false;
			switch (onTouch)
			{
				case HighlightMode.ON:
					foreach (Renderer renderer in highlightedRenderers)
					{
						ChangeToOriginalColor(renderer);
					}
					break;
				case HighlightMode.NONE:
					// Do Nothing
					break;
			}
		}
		public override void OnInteractableGrabbed(object sender, InteractableEventArgs e) 
		{
			isGrabbed = true;
			switch (onGrab)
			{
				case HighlightMode.ON:
					foreach (Renderer renderer in highlightedRenderers)
					{
						ChangeToColor(renderer, grabHighlightColor);
					}
					break;
				case HighlightMode.NONE:
					// Do Nothing
					break;
			}
		}
		public override void OnInteractableUnGrabbed(object sender, InteractableEventArgs e) 
		{
			isGrabbed = false;
			switch (onGrab)
			{
				case HighlightMode.ON:
					foreach (Renderer renderer in highlightedRenderers)
					{
						ChangeToOriginalColor(renderer);
					}
					break;
				case HighlightMode.NONE:
					// Do Nothing
					break;
			}
		}
		public override void OnInteractableUsed(object sender, InteractableEventArgs e) 
		{
			isUsed = true;
			switch (onUse)
			{
				case HighlightMode.ON:
					foreach (Renderer renderer in highlightedRenderers)
					{
						ChangeToColor(renderer, useHighlightColor);
					}
					break;
				case HighlightMode.NONE:
					// Do Nothing
					break;
			}
		}
		public override void OnInteractableUnUsed(object sender, InteractableEventArgs e) 
		{
			isUsed = false;
			switch (onUse)
			{
				case HighlightMode.ON:
					foreach (Renderer renderer in highlightedRenderers)
					{
						ChangeToOriginalColor(renderer);
					}
					break;
				case HighlightMode.NONE:
					// Do Nothing
					break;
			}
		}
		*/
		#endregion

		#region Initialization
    // Get References and Set Initial Values
		protected override void Awake() 
		{
			base.Awake();
			originalRendererColors = new Dictionary<string, Color>();
      StoreOriginalColors();
		}
		#endregion

		#region Core
		protected virtual void Update()
		{
			if (interactable.IsGrabbed())
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
