#if BFUN_INSTALLED_TRUE
using System;
using Bfun.LitMotion.Adapters;
using Unity.Collections;
using UnityEngine;

namespace Bfun.LitMotion.Animation
{
    public abstract class PropertyAnimationComponent<TObject, TValue, TOptions, TAdapter> : LitMotionAnimationComponent
        where TObject : UnityEngine.Object
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
        [SerializeField] protected TObject target;
        public SerializableMotionSettings<TValue, TOptions> settings;
        [SerializeField] bool relative;

        protected TValue startValue;
        protected TValue originalValue;
        protected bool hasCapturedOriginal;

        public void OnDisable() { hasCapturedOriginal = false; }
        public void OnValidate() { hasCapturedOriginal = false; }

        // 1. RECORD: Chỉ lưu nếu chưa lưu (An toàn)
        public override void RecordState()
        {
            if (!hasCapturedOriginal && target != null)
            {
                originalValue = GetValue(target);
                hasCapturedOriginal = true;
            }
        }

        // 2. RESTORE: Trả về vị trí gốc
        public override void RestoreState()
        {
            if (target != null && hasCapturedOriginal)
            {
                SetValue(target, originalValue);
#if UNITY_EDITOR
                if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(target);
#endif
            }
        }

        // 3. CLEAR: Xóa bộ nhớ
        public override void ClearCache()
        {
            hasCapturedOriginal = false;
        }

        public override void OnStop()
        {
            RestoreState();
            // Không xóa cache ở đây, để Panel quản lý việc xóa
        }

        public override MotionHandle Play()
        {
            // Fallback: Nếu chưa Record thì Record ngay
            if (!hasCapturedOriginal && target != null)
            {
                originalValue = GetValue(target);
                hasCapturedOriginal = true;
            }

            if (Application.isPlaying) OnInitialize();

            // Lấy vị trí hiện tại làm điểm bắt đầu (để Sequence nối tiếp nhau mượt mà)
            startValue = GetValue(target);

            onPlay?.Invoke();

            MotionHandle handle;
            if (relative)
            {
                handle = LMotion.Create<TValue, TOptions, TAdapter>(settings)
                    .WithOnComplete(() => onComplete?.Invoke()) // <--- Thêm dòng này
                    .Bind(this, (x, state) => state.SetValue(target, state.GetRelativeValue(state.startValue, x)));
            }
            else
            {
                handle = LMotion.Create<TValue, TOptions, TAdapter>(settings)
                    .WithOnComplete(() => onComplete?.Invoke()) // <--- Thêm dòng này
                    .Bind(this, (x, state) => state.SetValue(target, x));
            }
            return handle;
        }

        protected abstract TValue GetValue(TObject target);
        protected abstract void SetValue(TObject target, in TValue value);
        protected abstract TValue GetRelativeValue(in TValue startValue, in TValue relativeValue);
    }

    // --- CÁC CLASS HELPER (Giữ nguyên) ---
    public abstract class FloatPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, float, NoOptions, FloatMotionAdapter> where TObject : UnityEngine.Object { [Space(5)][SerializeField] protected bool useInitValue; [SerializeField] protected float initValue; public override bool OnInitialize() { if (useInitValue && target != null) { SetValue(target, initValue); return true; } return false; } protected sealed override float GetRelativeValue(in float startValue, in float relativeValue) => startValue + relativeValue; }
    public abstract class DoublePropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, double, NoOptions, DoubleMotionAdapter> where TObject : UnityEngine.Object { [Space(5)][SerializeField] protected bool useInitValue; [SerializeField] protected double initValue; public override bool OnInitialize() { if (useInitValue && target != null) { SetValue(target, initValue); return true; } return false; } protected sealed override double GetRelativeValue(in double startValue, in double relativeValue) => startValue + relativeValue; }
    public abstract class IntPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, int, IntegerOptions, IntMotionAdapter> where TObject : UnityEngine.Object { [Space(5)][SerializeField] protected bool useInitValue; [SerializeField] protected int initValue; public override bool OnInitialize() { if (useInitValue && target != null) { SetValue(target, initValue); return true; } return false; } protected sealed override int GetRelativeValue(in int startValue, in int relativeValue) => startValue + relativeValue; }
    public abstract class LongPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, long, IntegerOptions, LongMotionAdapter> where TObject : UnityEngine.Object { [Space(5)][SerializeField] protected bool useInitValue; [SerializeField] protected long initValue; public override bool OnInitialize() { if (useInitValue && target != null) { SetValue(target, initValue); return true; } return false; } protected sealed override long GetRelativeValue(in long startValue, in long relativeValue) => startValue + relativeValue; }
    public abstract class Vector2PropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, Vector2, NoOptions, Vector2MotionAdapter> where TObject : UnityEngine.Object { [Space(5)][SerializeField] protected bool useInitValue; [SerializeField] protected Vector2 initValue; public override bool OnInitialize() { if (useInitValue && target != null) { SetValue(target, initValue); return true; } return false; } protected sealed override Vector2 GetRelativeValue(in Vector2 startValue, in Vector2 relativeValue) => startValue + relativeValue; }
    public abstract class Vector3PropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, Vector3, NoOptions, Vector3MotionAdapter> where TObject : UnityEngine.Object { [Space(5)][SerializeField] protected bool useInitValue; [SerializeField] protected Vector3 initValue; public override bool OnInitialize() { if (useInitValue && target != null) { SetValue(target, initValue); return true; } return false; } protected sealed override Vector3 GetRelativeValue(in Vector3 startValue, in Vector3 relativeValue) => startValue + relativeValue; }
    public abstract class Vector4PropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, Vector4, NoOptions, Vector4MotionAdapter> where TObject : UnityEngine.Object { [Space(5)][SerializeField] protected bool useInitValue; [SerializeField] protected Vector4 initValue; public override bool OnInitialize() { if (useInitValue && target != null) { SetValue(target, initValue); return true; } return false; } protected sealed override Vector4 GetRelativeValue(in Vector4 startValue, in Vector4 relativeValue) => startValue + relativeValue; }
    public abstract class ColorPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, Color, NoOptions, ColorMotionAdapter> where TObject : UnityEngine.Object { [Space(5)][SerializeField] protected bool useInitValue; [SerializeField] protected Color initValue = Color.white; public override bool OnInitialize() { if (useInitValue && target != null) { SetValue(target, initValue); return true; } return false; } protected sealed override Color GetRelativeValue(in Color startValue, in Color relativeValue) => startValue + relativeValue; }
    public abstract class RectPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, Rect, NoOptions, RectMotionAdapter> where TObject : UnityEngine.Object { [Space(5)][SerializeField] protected bool useInitValue; [SerializeField] protected Rect initValue; public override bool OnInitialize() { if (useInitValue && target != null) { SetValue(target, initValue); return true; } return false; } protected sealed override Rect GetRelativeValue(in Rect startValue, in Rect relativeValue) => new Rect(startValue.position + relativeValue.position, startValue.size + relativeValue.size); }
    public abstract class FixedString512BytesPropertyAnimationComponent<TObject> : PropertyAnimationComponent<TObject, FixedString512Bytes, StringOptions, FixedString512BytesMotionAdapter> where TObject : UnityEngine.Object { [Space(5)][SerializeField] protected bool useInitValue; [SerializeField] protected string initValueString; public override bool OnInitialize() { if (useInitValue && target != null) { SetValue(target, new FixedString512Bytes(initValueString)); return true; } return false; } protected sealed override FixedString512Bytes GetRelativeValue(in FixedString512Bytes startValue, in FixedString512Bytes relativeValue) { var value = startValue; value.Append(relativeValue); return value; } }
}
#endif