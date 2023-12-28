using System.Collections;
using UnityEngine.Events;

namespace UnityEngine.SceneManagement
{
	/// <summary> Component that animates an effect meant to decorate the change between scenes. </summary>
	public abstract class SceneTransitionEffect : MonoBehaviour
	{
		/// <summary> Duration in seconds of the transition animation. </summary>
		[Space][SerializeField][Min(0)] public float Length = 1;

		/// <summary> True while animating the effect. </summary>
		public bool Animating => _animating;
		bool _animating = false;
		/// <summary> Currently active transition corroutine. </summary>	
		Coroutine _transitionRoutine = null;


		// -----------------------------------------------------------------

		/// <summary> Play this transition effect. </summary>
		/// <param name="targetValue"> If true the effect appears, if false the effect disappears. </param>
		public void AnimateTransitionTo(bool targetValue) => AnimateTransitionTo(targetValue, null);

		/// <summary> Play this transition effect. </summary>
		/// <param name="onCompleted"> Callback invoked when the animation is completed. </param>
		public void AnimateTransitionTo(bool targetValue, UnityAction onCompleted)
		{
			if (this.gameObject.activeInHierarchy == false)
			{
				SetTo(targetValue);
				onCompleted?.Invoke();
				return;
			}

			if (_transitionRoutine != null) { StopCoroutine(_transitionRoutine); }
			_transitionRoutine = StartCoroutine(TransitionRoutine(targetValue, onCompleted));
		}

		/// Corroutine that animates the transition value.
		private IEnumerator TransitionRoutine(bool value, UnityAction onCompleted)
		{
			float start = value == true ? 0 : 1;
			float end = value == true ? 1 : 0;
			float progress = 0;

			_animating = true;

			while(progress < 1)
			{
				progress += Time.deltaTime / Length;
				TransitionValue = Mathf.Lerp(start, end, progress);
				yield return null;
			}

			_animating = false;
			TransitionValue = end;

			yield return null;

			onCompleted?.Invoke();
		}

		// ---------

		/// <summary> Set the transition effect without animation. </summary>
		/// <param name="targetValue"> If true the effect appears, if false the effect disappears. </param>
		public void SetTo(bool targetValue)
		{
			if (_transitionRoutine != null) { StopCoroutine(_transitionRoutine); }

			TransitionValue = targetValue == true ? 1 : 0;
		}


		// -----------------------------------------------------------------

		/// <summary> Transition value that changes with the animation in the range of 0 to 1. 
		/// <br/> At 1 the effect is visible, at 0 is invisible. </summary>
		public abstract float TransitionValue { get; protected set; }
	}
}
