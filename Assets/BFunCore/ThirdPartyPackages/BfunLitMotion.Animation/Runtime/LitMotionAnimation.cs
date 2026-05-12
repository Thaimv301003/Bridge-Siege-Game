#if BFUN_INSTALLED_TRUE
using System;
using System.Collections;
using System.Collections.Generic;
using Bfun.LitMotion.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Bfun.LitMotion.Animation
{
    [AddComponentMenu("LitMotion Animation")]
    public sealed class LitMotionAnimation : MonoBehaviour
    {
        [SerializeField] bool playOnEnable = false;

        [SerializeReference] LitMotionAnimationComponent[] components = Array.Empty<LitMotionAnimationComponent>();
        FastListCore<LitMotionAnimationComponent> playingComponents;
        private Coroutine runtimeCoroutine;

        public static System.Action<string> OnRequestPlaySound;

#if UNITY_EDITOR
        private List<IEnumerator> editorCoroutines = new List<IEnumerator>();
#endif
        public IReadOnlyList<LitMotionAnimationComponent> Components => components;

        public bool PlayOnEnable { get => playOnEnable; set => playOnEnable = value; }

        // --- CLASS WRAPPER ---
        private class HandleWrapper
        {
            public MotionHandle Handle;
            public bool IsDelaying;

            public bool IsRunning
            {
                get
                {
                    if (IsDelaying) return true;
                    if (!Handle.IsActive()) return false;
                    // Check xem còn thời gian chạy không (trừ sai số nhỏ)
                    return Handle.Time < (Handle.TotalDuration - 0.001f);
                }
            }
        }
        // ---------------------

        void Start() { }

        void OnEnable()
        {
            if (Application.isPlaying && playOnEnable)
            {
                Play();
            }
        }

#if UNITY_EDITOR
        public void SetComponents(LitMotionAnimationComponent[] newComponents) { components = newComponents; }
#endif
        // Hàm này đặt các giá trị về Start Value (VD: Alpha 0, Scale 0)
        public void Initialize() { if (components == null) return; foreach (var c in components) if (c != null && c.Enabled) c.OnInitialize(); }

        public void RecordAllStates() { if (components == null) return; foreach (var c in components) if (c != null) c.RecordState(); }
        public void ClearCache() { if (components == null) return; foreach (var c in components) if (c != null) c.ClearCache(); }

        // --- CẬP NHẬT HÀM PLAY CHO GIỐNG PANEL ---
        public void Play()
        {
            if (components == null || components.Length == 0) return;

            // 1. Stop các anim đang chạy dở (nhưng không reset về mặc định hệ thống, để bước Initialize lo)
            Stop(resetValues: false);

            // 2. [QUAN TRỌNG] Initialize: Đưa object về trạng thái Bắt đầu (Start Value) ngay lập tức
            // Giúp animation chạy mượt từ 0 -> 1 thay vì bị kẹt ở 1.
            Initialize();

            // 3. [QUAN TRỌNG] Update UI: Tính toán lại Layout Group để tránh bị vỡ khung hình khi bắt đầu
            UnityEngine.Canvas.ForceUpdateCanvases();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                editorCoroutines.Clear();
                editorCoroutines.Add(PlayRoutine());
                EditorApplication.update += EditorUpdate;
            }
            else
#endif
            {
                if (gameObject.activeInHierarchy) runtimeCoroutine = StartCoroutine(PlayRoutine());
            }
        }
        // ------------------------------------------

        IEnumerator PlayRoutine()
        {
            HandleWrapper previousWrapper = new HandleWrapper();
            bool isFirstActiveItem = true;

            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component == null || !component.Enabled) continue;

                // LOGIC SEQUENCE
                if (!isFirstActiveItem && component.JoinType == AnimationJoinType.Sequence)
                {
                    if (previousWrapper != null)
                    {
                        while (previousWrapper.IsRunning)
                        {
#if UNITY_EDITOR
                            if (!Application.isPlaying) EditorApplication.QueuePlayerLoopUpdate();
#endif
                            yield return null;
                        }
                    }
                }

                HandleWrapper currentWrapper = new HandleWrapper();

                if (component.JoinType == AnimationJoinType.Sequence)
                {
                    if (component.JoinDelay > 0)
                    {
                        var delayRoutine = HandleDelay(component.JoinDelay);
                        if (Application.isPlaying) yield return delayRoutine;
                        else while (delayRoutine.MoveNext()) yield return null;
                    }
                    PlayComponentInternal(component, currentWrapper);
                    previousWrapper = currentWrapper;
                }
                else // Parallel
                {
                    if (component.JoinDelay > 0)
                    {
                        currentWrapper.IsDelaying = true;
                        previousWrapper = currentWrapper;

                        IEnumerator parallelRoutine = ParallelPlayRoutine(component, currentWrapper);
                        if (Application.isPlaying) StartCoroutine(parallelRoutine);
#if UNITY_EDITOR
                        else editorCoroutines.Add(parallelRoutine);
#endif
                    }
                    else
                    {
                        PlayComponentInternal(component, currentWrapper);
                        previousWrapper = currentWrapper;
                    }
                }

                isFirstActiveItem = false;
            }

            runtimeCoroutine = null;
        }

        IEnumerator ParallelPlayRoutine(LitMotionAnimationComponent component, HandleWrapper wrapper)
        {
            var delayRoutine = HandleDelay(component.JoinDelay);
            if (Application.isPlaying) yield return delayRoutine;
            else while (delayRoutine.MoveNext()) yield return null;

            wrapper.IsDelaying = false;
            PlayComponentInternal(component, wrapper);
        }

        void PlayComponentInternal(LitMotionAnimationComponent component, HandleWrapper wrapper)
        {
            try
            {
                if (Application.isPlaying && !string.IsNullOrEmpty(component.SoundName))
                {
                    OnRequestPlaySound?.Invoke(component.SoundName);
                }

                var handle = component.Play();
                if (handle.IsActive())
                {
                    handle.Preserve();
                    component.TrackedHandle = handle;
                    playingComponents.Add(component);

                    if (wrapper != null) wrapper.Handle = handle;
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        IEnumerator HandleDelay(float delay)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                double s = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - s < delay) yield return null;
            }
            else
#endif
            {
                if (Time.timeScale == 0f) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
            }
        }

#if UNITY_EDITOR
        void EditorUpdate()
        {
            if (editorCoroutines.Count > 0)
            {
                for (int i = editorCoroutines.Count - 1; i >= 0; i--)
                {
                    bool hasNext = false;
                    try { if (editorCoroutines[i] != null) hasNext = editorCoroutines[i].MoveNext(); }
                    catch (Exception e) { Debug.LogException(e); hasNext = false; }
                    if (!hasNext) editorCoroutines.RemoveAt(i);
                }
            }

            bool isAnyAnimRunning = false;
            foreach (var c in playingComponents.AsSpan())
            {
                if (c.TrackedHandle.IsActive() && c.TrackedHandle.IsPlaying())
                {
                    isAnyAnimRunning = true;
                    break;
                }
            }

            if (editorCoroutines.Count > 0 || isAnyAnimRunning)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                UnityEngine.Canvas.ForceUpdateCanvases();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            else
            {
                EditorApplication.update -= EditorUpdate;
            }
        }
#endif
        public void Pause() { foreach (var c in playingComponents.AsSpan()) { var h = c.TrackedHandle; if (h.IsActive()) { h.PlaybackSpeed = 0f; c.OnPause(); } } }

        public void Stop(bool resetValues = true)
        {
            if (runtimeCoroutine != null) StopCoroutine(runtimeCoroutine);
            runtimeCoroutine = null;
            StopAllCoroutines();
#if UNITY_EDITOR
            if (editorCoroutines.Count > 0) { EditorApplication.update -= EditorUpdate; editorCoroutines.Clear(); }
#endif
            var span = playingComponents.AsSpan();
            foreach (var c in span) { var h = c.TrackedHandle; if (h.IsActive()) h.TryCancel(); c.TrackedHandle = default; }
            playingComponents.Clear();
            if (resetValues && components != null) { for (int i = components.Length - 1; i >= 0; i--) { var c = components[i]; if (c != null) c.OnStop(); } }
#if UNITY_EDITOR
            if (!Application.isPlaying) { UnityEngine.Canvas.ForceUpdateCanvases(); EditorApplication.QueuePlayerLoopUpdate(); UnityEditorInternal.InternalEditorUtility.RepaintAllViews(); }
#endif
        }
        public void Restart() { Stop(resetValues: false); Play(); }
        public bool IsActive
        {
            get
            {
                if (runtimeCoroutine != null) return true;
#if UNITY_EDITOR
                if (editorCoroutines.Count > 0) return true;
#endif
                if (playingComponents.AsSpan().Length == 0) return false; foreach (var c in playingComponents.AsSpan()) if (c.TrackedHandle.IsActive()) return true; return false;
            }
        }
        public bool IsPlaying
        {
            get
            {
                if (runtimeCoroutine != null) return true;
#if UNITY_EDITOR
                if (editorCoroutines.Count > 0) return true;
#endif
                if (playingComponents.AsSpan().Length == 0) return false; foreach (var c in playingComponents.AsSpan()) if (c.TrackedHandle.IsPlaying()) return true; return false;
            }
        }
        void OnDestroy() { Stop(); }
    }
}
#endif