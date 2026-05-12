#if BFUN_INSTALLED_TRUE
using UnityEngine;

namespace Bfun.LitMotion.Animation
{
    [CreateAssetMenu(fileName = "LitMotionAnimationData", menuName = "LitMotion/Animation Data")]
    public sealed class LitMotionAnimationData : ScriptableObject
    {
        [SerializeReference]
        public LitMotionAnimationComponent[] components;
    }
}
#endif