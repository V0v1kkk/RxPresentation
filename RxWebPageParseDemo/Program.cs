using System;
using System.Reactive;
using System.Reactive.Linq;

namespace RxWebPageParseDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }


        IObservable<Unit> ObservableFromFlowInterval(TimeSpan period, TimeSpan notSmaller, TimeSpan maxOtklon)
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
                var narmalizeTicks = lastTicks + notSmaller.Ticks;
                var maxPlus = LongRandom(narmalizeTicks, maxOtklon.Ticks, random) - lastTicks;
                return TimeSpan.FromTicks(maxPlus);
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
