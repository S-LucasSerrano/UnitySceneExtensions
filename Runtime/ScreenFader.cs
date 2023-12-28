using UnityEngine.UI;

namespace UnityEngine.SceneManagement
{
	/// <summary> Scene transition effect that fades the alpha of an UI image. </summary>
	
	[AddComponentMenu("Scene Management/Screen Fader")]
	public class ScreenFader : SceneTransitionEffect
	{
		[SerializeField] public Image Image = null;


		private void Reset()
		{
			Image = GetComponent<Image>();
		}

		public override float TransitionValue
		{ 
			get
			{
				return Image.color.a;
			}

			protected set
			{
				bool active = value > 0;
				if (Image.enabled != active)
					Image.enabled = active;

				var color = Image.color;
				color.a = value;
				Image.color = color;
			}
		}
	}
}
