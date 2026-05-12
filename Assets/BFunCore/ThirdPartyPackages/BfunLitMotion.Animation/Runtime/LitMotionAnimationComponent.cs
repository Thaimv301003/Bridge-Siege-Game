#if BFUN_INSTALLED_TRUE
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Bfun.LitMotion.Animation
{
    public enum AnimationJoinType { Parallel, Sequence }

    [Serializable]
    public abstract class LitMotionAnimationComponent
    {
        public LitMotionAnimationComponent()
        {
#if UNITY_EDITOR
            var type = GetType();
            var attribute = type.GetCustomAttribute<LitMotionAnimationComponentMenuAttribute>();
            displayName = attribute != null ? attribute.MenuName.Split('/').Last() : type.Name;
#endif
        }
        [SerializeField]
        string soundName = "";
        public string SoundName => soundName;


        [SerializeField] string displayName;
        [SerializeField] bool enabled = true;
        [SerializeField] AnimationJoinType joinType = AnimationJoinType.Parallel;
        [SerializeField] float joinDelay = 0f;

        // --- MỚI: EVENT START & END ---
        [Space(10)]
        [SerializeField] protected UnityEvent onPlay;     // Chạy khi bắt đầu
        [SerializeField] protected UnityEvent onComplete; // Chạy khi kết thúc
        // ------------------------------

        public AnimationJoinType JoinType => joinType;
        public float JoinDelay => joinDelay;
        public bool Enabled => enabled;
        public string DisplayName => displayName;

        public abstract MotionHandle Play();
        public virtual bool OnInitialize() { return false; }
        public virtual void OnResume() { }
        public virtual void OnPause() { }

        // Stop: Gọi Restore để trả về vị trí gốc
        public virtual void OnStop() { RestoreState(); }

        // --- HỆ THỐNG SNAPSHOT ---
        public virtual void RecordState() { }
        public virtual void RestoreState() { }
        public virtual void ClearCache() { }
        // -------------------------

        public MotionHandle TrackedHandle { get; set; }
    }
}
#endif