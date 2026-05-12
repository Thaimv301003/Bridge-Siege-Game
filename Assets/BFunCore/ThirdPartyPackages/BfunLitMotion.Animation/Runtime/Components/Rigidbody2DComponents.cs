#if BFUN_INSTALLED_TRUE
#if LITMOTION_ANIMATION_PHYSICS_2D

using System;
using Bfun.LitMotion.Adapters;
using UnityEngine;

namespace Bfun.LitMotion.Animation.Components
{
    public abstract class Rigidbody2DPositionAnimationBase<TOptions, TAdapter> : PropertyAnimationComponent<Rigidbody2D, Vector2, TOptions, TAdapter>
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<Vector2, TOptions>
    {
        [SerializeField] bool useMovePosition = true;
        // INIT VALUE
        [Space(5)][SerializeField] protected bool useInitValue;
        [SerializeField] protected Vector2 initValue;

        public override bool OnInitialize()
        {
            if (useInitValue && target != null)
            {
                SetValue(target, initValue);
                return true;
            }
            return false;
        }

        protected override Vector2 GetValue(Rigidbody2D target) => target.position;
        protected override void SetValue(Rigidbody2D target, in Vector2 value)
        {
            if (useMovePosition) target.MovePosition(value);
            else target.position = value;
        }
        protected override Vector2 GetRelativeValue(in Vector2 startValue, in Vector2 relativeValue) => startValue + relativeValue;
    }
    [Serializable][LitMotionAnimationComponentMenu("Rigidbody2D/Position")] public sealed class Rigidbody2DPositionAnimation : Rigidbody2DPositionAnimationBase<NoOptions, Vector2MotionAdapter> { }
    [Serializable][LitMotionAnimationComponentMenu("Rigidbody2D/Position (Punch)")] public sealed class Rigidbody2DPositionPunchAnimation : Rigidbody2DPositionAnimationBase<PunchOptions, Vector2PunchMotionAdapter> { }
    [Serializable][LitMotionAnimationComponentMenu("Rigidbody2D/Position (Shake)")] public sealed class Rigidbody2DPositionShakeAnimation : Rigidbody2DPositionAnimationBase<ShakeOptions, Vector2ShakeMotionAdapter> { }


    public abstract class Rigidbody2DRotationAnimationBase<TOptions, TAdapter> : PropertyAnimationComponent<Rigidbody2D, float, TOptions, TAdapter>
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<float, TOptions>
    {
        [SerializeField] bool useMoveRotation;
        // INIT VALUE
        [Space(5)][SerializeField] protected bool useInitValue;
        [SerializeField] protected float initValue;

        public override bool OnInitialize()
        {
            if (useInitValue && target != null)
            {
                SetValue(target, initValue);
                return true;
            }
            return false;
        }

        protected override float GetValue(Rigidbody2D target) => target.rotation;
        protected override void SetValue(Rigidbody2D target, in float value)
        {
            if (useMoveRotation) target.MoveRotation(value);
            else target.rotation = value;
        }
        protected override float GetRelativeValue(in float startValue, in float relativeValue) => startValue + relativeValue;
    }
    [Serializable][LitMotionAnimationComponentMenu("Rigidbody2D/Rotation")] public sealed class Rigidbody2DRotationAnimation : Rigidbody2DRotationAnimationBase<NoOptions, FloatMotionAdapter> { }
    [Serializable][LitMotionAnimationComponentMenu("Rigidbody2D/Rotation (Punch)")] public sealed class Rigidbody2DRotationPunchAnimation : Rigidbody2DRotationAnimationBase<PunchOptions, FloatPunchMotionAdapter> { }
    [Serializable][LitMotionAnimationComponentMenu("Rigidbody2D/Rotation (Shake)")] public sealed class Rigidbody2DRotationShakeAnimation : Rigidbody2DRotationAnimationBase<ShakeOptions, FloatShakeMotionAdapter> { }
}
#endif
#endif