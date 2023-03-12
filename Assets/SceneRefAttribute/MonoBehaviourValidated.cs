using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneRefAttributes
{
    public class MonoBehaviourValidated : MonoBehaviour
    {
        private void OnValidate()
        {
            this.ValidateRefs();
        }
    }
}
