using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace LucasSerrano.SceneManagement
{
	/// <summary> Abstract SceneTransition effect that lerps a value between 0 and 1 over time. <br/>
	/// Tha value can be used by concrete implementations to animate any effect. </summary>
	public abstract class LerpedSceneTransition : SceneTransition
	{
		/// <summary> Duration in seconds of the transition animation. </summary>
		[Space][SerializeField][Min(0)] public float Length = 1;


		// -----------------------------------------------------------------

		protected override IEnumerator ConcreteTransitionRoutine(bool targetValue)
		{ 
			float start = (targetValue == true) ? 0 : 1;
			float end = (targetValue == true) ? 1 : 0;
			float progress = 0;

			while(progress < 1)
			{
				progress += Time.deltaTime / Length;
				TransitionValue = Mathf.Lerp(start, end, progress);
				yield return null;
			}

			TransitionValue = end;
		}

		protected override void ConcreteSetDirectly(bool targetValue)
		{
			TransitionValue = targetValue == true ? 1 : 0;
		}


		// -----------------------------------------------------------------

		/// <summary> Transition value that changes with the animation in the range of 0 to 1. 
		/// <br/> At 1 the effect is visible, at 0 is invisible. </summary>
		public abstract float TransitionValue { get; protected set; }
	}
}
