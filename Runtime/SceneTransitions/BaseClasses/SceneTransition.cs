using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace LucasSerrano.SceneManagement
{
	/// <summary> Base class for all scene transition effects to inherit from. </summary>
	public abstract class SceneTransition : MonoBehaviour
	{
		/// <summary> True while animating the effect. </summary>
		public bool Animating { get; private set; }

		/// <summary> Currently active transition corroutine. </summary>
		private Coroutine transitionRoutine = null;


		// -----------------------------------------------------------------

		protected virtual void Start()
		{
			// Start with the animation to false.
			// But avoid canceling the animation if it has already started.
			if (Animating == false)
			{
				SetDirectlyTo(false);
			}
		}


		// -----------------------------------------------------------------

		/// <summary> Play this transition effect. </summary>
		/// <param name="targetValue"> If true the effect appears, if false the effect disappears. </param>
		public void AnimateTo(bool targetValue)
		{
			AnimateTo(targetValue, null);
		}

		/// <summary> Play this transition effect. </summary>
		/// <param name="onCompleted"> Callback invoked when the animation is completed. </param>
		public void AnimateTo(bool targetValue, UnityAction onCompleted)
		{
			if (this.gameObject.activeInHierarchy == false)
			{
				SetDirectlyTo(targetValue);
				onCompleted?.Invoke();
				return;
			}

			if (transitionRoutine != null) { StopCoroutine(transitionRoutine); }
			transitionRoutine = StartCoroutine(TransitionRoutine(targetValue, onCompleted));
		}

		/// Corroutine that calls the concrete implementation of this effect.
		private IEnumerator TransitionRoutine(bool targetValue, UnityAction onCompleted)
		{
			Animating = true;
			yield return ConcreteTransitionRoutine(targetValue);
			Animating = false;

			yield return null;
			onCompleted?.Invoke();
		}

		/// <summary>
		/// Concrete implementation of the transtion coroutine. It needs to animate the effect and wait for it to end. </summary>
		protected abstract IEnumerator ConcreteTransitionRoutine(bool targetValue);

		// ---------

		/// <summary> Set the transition effect without animation. </summary>
		public void SetDirectlyTo(bool targetValue)
		{
			if (transitionRoutine != null) { StopCoroutine(transitionRoutine); }
			ConcreteSetDirectly(targetValue);
		}

		/// <summary> Concrete implementation that skips the transition animation. </summary>
		protected abstract void ConcreteSetDirectly(bool targetValue);
	}
}
