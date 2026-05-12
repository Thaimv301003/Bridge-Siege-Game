#if BFUN_INSTALLED_TRUE
namespace Bfun.LitMotion
{
    public static class LSequence
    {
        public static MotionSequenceBuilder Create()
        {
            var source = MotionSequenceBuilderSource.Rent();
            return new MotionSequenceBuilder(source);
        }
    }
}
#endif