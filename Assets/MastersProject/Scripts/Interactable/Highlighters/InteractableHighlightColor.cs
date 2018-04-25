//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Mostly VRTK
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	/// <summary>
  /// VRTK-based script that allows me to override the object to hightlight 
  /// </summary>
	public class InteractableHighlightColor : InteractableHighlightBase 
	{
		#region Config
		[Header("Config")]
		[Tooltip("Optional. Assign to parent object of Renderers to highlight if the InteractableObject isn't the parent of the Renderers you wish to highlight")]
		public GameObject overrideHighlightObject;
		#endregion

		#region State
		[Space(10f)]
		protected Dictionary<string, Color> originalRendererColors = new Dictionary<string, Color>();
    protected GameObject rendererParent;
		#endregion

		#region Delegates
		#endregion

		#region Properties
		#endregion

		#region Public
    /// <summary>
    /// The Initialise method sets up the highlighter for use.
    /// </summary>
    /// <param name="color">Not used.</param>
    public override void Initialise(Color? color = null)
    {
      originalRendererColors = new Dictionary<string, Color>();
      StoreOriginalColors();
    }
    /// <summary>
    /// The Highlight method initiates the change of colour on the object and will fade to that colour (from a base white colour) for the given duration.
    /// </summary>
    /// <param name="color">The colour to highlight to.</param>
    /// <param name="duration">The time taken to fade to the highlighted colour.</param>
    public override void Highlight(Color? color, float duration = 0f)
    {
      if (color == null) return;
			foreach (Renderer renderer in rendererParent.GetComponentsInChildren<Renderer>(true))
			{
				ChangeToHighlightColor(renderer, (Color)color);
			}
    }
		/// <summary>
    /// The Unhighlight method returns the object back to it's original colour.
    /// </summary>
    /// <param name="color">Not used.</param>
    /// <param name="duration">Not used.</param>
    public override void Unhighlight(Color? color = null, float duration = 0f)
    {
      if (originalRendererColors == null || rendererParent == null) return;
			foreach (Renderer renderer in rendererParent.GetComponentsInChildren<Renderer>(true))
			{
        ChangeToOriginalColor(renderer);
			}
    }
    /// <summary>
    /// The ResetHighlighter method stores the object's materials and shared materials prior to highlighting.
    /// </summary>
    public override void ResetHighlighter()
    {
      StoreOriginalColors();
    }
		#endregion

		#region Core
    protected virtual void StoreOriginalColors()
    {
      originalRendererColors.Clear();
			rendererParent = overrideHighlightObject != null ? overrideHighlightObject : gameObject;
			foreach (Renderer renderer in rendererParent.GetComponentsInChildren<Renderer>(true))
			{
				var objectReference = renderer.gameObject.GetInstanceID().ToString();
				originalRendererColors[objectReference] = renderer.material.color;
			}
    }
    protected virtual void ChangeToHighlightColor(Renderer renderer, Color color)
    {
      var swapCustomMaterials = new Material[renderer.materials.Length];

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
      renderer.material.color = originalRendererColors[objectReference];
    }
		#endregion
	}
}
