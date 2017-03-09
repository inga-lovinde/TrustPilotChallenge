namespace WhiteRabbit
{
    using System.Threading.Tasks.Dataflow;

    internal static class Extensions
    {
        public static void LinkForever<TOutput>(this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target)
        {
            source.LinkTo(target);
            source.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    target.Fault(t.Exception);
                }
                else
                {
                    target.Complete();
                }
            });
        }
    }
}
