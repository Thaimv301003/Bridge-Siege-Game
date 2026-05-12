#if BFUN_INSTALLED_TRUE
namespace Bfun.LitMotion.Editor
{
    /// <summary>
    /// Schedulers available in Editor.
    /// </summary>
    public static class EditorMotionScheduler
    {
        /// <summary>
        /// Scheduler that updates motion at EditorApplication.update.
        /// </summary>
        public static readonly IMotionScheduler Update = new EditorUpdateMotionScheduler();
    }
}
#endif