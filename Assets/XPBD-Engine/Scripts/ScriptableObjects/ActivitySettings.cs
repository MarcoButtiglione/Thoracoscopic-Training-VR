using UnityEngine;
using System.Collections.Generic;

namespace XPBD_Engine.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ActivitySettings", menuName = "XPBD_Engine/ActivitySettings", order = 0)]
    public class ActivitySettings : ScriptableObject
    {
        public List<int> vertexIndices;
        public float grabbingDistance = 0.1f;
    }
}