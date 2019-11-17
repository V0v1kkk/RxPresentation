using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RxWebPageParseDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var observable =
                ObservableFromDynamicInterval(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            var consoleWriteLock = new object();
            var subscriber = observable
                .GetWebPageText(new Uri("https://stackexchange.com/questions?tab=realtime"))
                .SelectTopics()
                .FilterOnlyNew()
                //.Select(list => list.Where(topic => topic.Link.Contains("stackoverflow")))
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




        static IObservable<Unit> ObservableFromDynamicInterval(TimeSpan period, TimeSpan notLess, TimeSpan maxIncrement)
        {
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
