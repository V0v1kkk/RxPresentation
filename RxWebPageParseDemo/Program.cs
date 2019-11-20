using System;
using System.Reactive;
using System.Reactive.Linq;

namespace RxWebPageParseDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var observable =
                ObservableFromRandomInterval(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            var consoleWriteLock = new object();

            observable
                .GetWebPageText(new Uri("https://stackexchange.com/questions?tab=realtime"))
                .SelectTopics()
                .FilterOnlyNew()
                .SelectStackOverflow()
                .Subscribe(newTopics =>
                {
                    lock (consoleWriteLock)
                    {
                        Console.WriteLine(DateTimeOffset.Now);
                        foreach (var stackExchangeTopic in newTopics)
                        {
                            Console.WriteLine(stackExchangeTopic);
                        }
                        Console.WriteLine();
                    }
                });

            
            observable.Wait();
        }


        private static IObservable<Unit> ObservableFromRandomInterval(TimeSpan notLess, TimeSpan maxIncrement)
        {
            if (notLess > maxIncrement)
                throw new ArgumentException("maxIncrement must be greater then notLess");

            var random = new Random();
            long LongRandom(long min, long max, Random rand)
            {
                byte[] buf = new byte[8];
                rand.NextBytes(buf);
                long longRand = BitConverter.ToInt64(buf, 0);
                return (Math.Abs(longRand % (max - min)) + min);
            }

            TimeSpan GetNextInterval(long lastTicks)
            {
                var notLessTicks = lastTicks + notLess.Ticks;
                var notMoreTicks = notLessTicks + maxIncrement.Ticks;
                var randomDelta = LongRandom(notLessTicks, notMoreTicks, random) - lastTicks;
                return TimeSpan.FromTicks(randomDelta);
            }

            return Observable.Generate(DateTimeOffset.Now.Ticks,
                s => true,
                s => s,
                s => s,
                GetNextInterval)
                .Select(_ => Unit.Default);
        }

    }
}
