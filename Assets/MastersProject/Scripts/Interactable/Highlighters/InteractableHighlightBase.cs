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
  /// VRTK-based abstract class for object highlighting
  /// </summary>
	public abstract class InteractableHighlightBase : MonoBehaviour 
	{
		#region Public Fields
    [Tooltip("Determines if this highlighter is the active highlighter for the object the component is attached to. Only 1 active highlighter can be applied to a game object.")]
    public bool active = true;
    [Tooltip("Determines if the highlighted object should be unhighlighted when it is disabled.")]
    public bool unhighlightOnDisable = true;
		#endregion

		#region Private Fields
		#endregion

		#region Bookkeeping 
		#endregion

		#region Initilization 
    /// <summary>
    /// The Initalise method is used to set up the state of the highlighter.
    /// </summary>
    /// <param name="color">An optional colour may be passed through at point of initialisation in case the highlighter requires it.</param>
    /// <param name="options">An optional dictionary of highlighter specific options that may be differ with highlighter implementations.</param>
    public abstract void Initialise(Color? color = null);
		#endregion


    #region Initilization 
    /// <summary>
    /// The ResetHighlighter method is used to reset the highlighter if anything on the object has changed. It should be called by any scripts changing object materials or colours.
    /// </summary>
    public abstract void ResetHighlighter();
    #endregion
		

		#region Core 
    /// <summary>
    /// The Highlight method is used to initiate the highlighting logic to apply to an object.
    /// </summary>
    /// <param name="color">An optional colour to highlight the game object to. The highlight colour may already have been set in the `Initialise` method so may not be required here.</param>
    /// <param name="duration">An optional duration of how long before the highlight has occured. It can be used by highlighters to fade the colour if possible.</param>
    public abstract void Highlight(Color? color = null, float duration = 0f);

    /// <summary>
    /// The Unhighlight method is used to initiate the logic that returns an object back to it's original appearance.
    /// </summary>
    /// <param name="color">An optional colour that could be used during the unhighlight phase. Usually will be left as null.</param>
    /// <param name="duration">An optional duration of how long before the unhighlight has occured.</param>
    public abstract void Unhighlight(Color? color = null, float duration = 0f);

    /// <summary>
    /// The GetOption method is used to return a value from the options array if the given key exists.
    /// </summary>
    /// <typeparam name="T">The system type that is expected to be returned.</typeparam>
    /// <param name="options">The dictionary of options to check in.</param>
    /// <param name="key">The identifier key to look for.</param>
    /// <returns>The value in the options at the given key returned in the provided system type.</returns>
    public virtual T GetOption<T>(Dictionary<string, object> options, string key)
    {
      if (options != null && options.ContainsKey(key) && options[key] != null)
      {
        return (T)options[key];
      }
      return default(T);
    }

    /// <summary>
    /// The GetActiveHighlighter method checks the given game object for a valid and active highlighter.
    /// </summary>
    /// <param name="obj">The game object to check for a highlighter on.</param>
    /// <returns>A valid and active highlighter.</returns>
    public static InteractableHighlightBase GetActiveHighlighter(GameObject obj)
    {
      InteractableHighlightBase objectHighlighter = null;
      foreach (var tmpHighlighter in obj.GetComponents<InteractableHighlightBase>())
      {
        if (tmpHighlighter.active)
        {
          objectHighlighter = tmpHighlighter;
          break;
        }
      }

      return objectHighlighter;
    }

    protected virtual void OnDisable()
    {
      if (unhighlightOnDisable)
      {
        Unhighlight();
      }
    }
		#endregion

		#region Helper 
		#endregion
	}
}
