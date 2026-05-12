#if BFUN_INSTALLED_TRUE
using UnityEditor;

namespace Bfun.LitMotion.Editor
{
    internal sealed class EditorUpdateMotionScheduler : IMotionScheduler
    {
        public double Time => EditorApplication.timeSinceStartup;

        public MotionHandle Schedule<TValue, TOptions, TAdapter>(ref MotionBuilder<TValue, TOptions, TAdapter> builder)
            where TValue : unmanaged
            where TOptions : unmanaged, IMotionOptions
            where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
        {
            return EditorMotionDispatcher.Schedule(ref builder);
        }
    }
}
#endif