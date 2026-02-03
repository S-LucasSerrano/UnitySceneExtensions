using UnityEngine;

namespace LucasSerrano.SceneManagement.Sample
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}
