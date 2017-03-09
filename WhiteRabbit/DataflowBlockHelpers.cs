namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    internal static class DataflowBlockHelpers
    {
        private static ExecutionDataflowBlockOptions ExecutionOptions { get; } = new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 100000,
        };

        public static IPropagatorBlock<T, T> Id<T>()
        {
            return new TransformBlock<T, T>(element => element, ExecutionOptions);
        }

        public static void WriteToTargetBlock<T>(this IEnumerable<T> enumerable, ITargetBlock<T> target)
        {
            var block = new TransformBlock<T, T>(line => line, ExecutionOptions);
            block.LinkForever(target);

            WriteToTargetBlockAsync(enumerable, block).Wait();
            block.Complete();
        }

        public static IPropagatorBlock<TInput, TOutput> PipeMany<TInput, TIntermediate, TOutput>(this IPropagatorBlock<TInput, TIntermediate> source, Func<TIntermediate, IEnumerable<TOutput>> mapper)
        {
            return source.Pipe(new TransformManyBlock<TIntermediate, TOutput>(mapper, ExecutionOptions));
        }

        public static IPropagatorBlock<TInput, TOutput> Pipe<TInput, TIntermediate, TOutput>(this IPropagatorBlock<TInput, TIntermediate> source, Func<TIntermediate, TOutput> mapper)
        {
            return source.Pipe(new TransformBlock<TIntermediate, TOutput>(mapper, ExecutionOptions));
        }

        public static IPropagatorBlock<TInput, TOutput> Pipe<TInput, TIntermediate, TOutput>(this IPropagatorBlock<TInput, TIntermediate> source, IPropagatorBlock<TIntermediate, TOutput> target)
        {
            source.LinkForever(target);
            return DataflowBlock.Encapsulate(source, target);
        }

        public static ISourceBlock<TOutput> Pipe<TInput, TOutput>(this ISourceBlock<TInput> source, Func<TInput, TOutput> mapper)
        {
            return source.Pipe(new TransformBlock<TInput, TOutput>(mapper, ExecutionOptions));
        }

        public static ISourceBlock<TOutput> Pipe<TInput, TOutput>(this ISourceBlock<TInput> source, IPropagatorBlock<TInput, TOutput> target)
        {
            source.LinkForever(target);
            return target;
        }

        public static Task LinkForever<TOutput>(this ISourceBlock<TOutput> source, Action<TOutput> action)
        {
            return source.LinkForever(new ActionBlock<TOutput>(action, ExecutionOptions));
        }

        public static Task LinkForever<TOutput>(this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target)
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

            return target.Completion;
        }

        private static async Task WriteToTargetBlockAsync<T>(IEnumerable<T> enumerable, ITargetBlock<T> target)
        {
            foreach (var element in enumerable)
            {
                await target.SendAsync(element);
            }
        }
    }
}
