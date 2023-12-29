using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneExtensions.Sample
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}
