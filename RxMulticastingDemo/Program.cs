using System;
using System.Reactive.Linq;
using System.Threading;

namespace RxMulticastingDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White;

            var random = new Random();


            var observable = Observable.Interval(TimeSpan.FromSeconds(2))
                .Select(_ => random.Next(1, 10))
                .Take(5);

            observable.SubscribeConsole(1, ConsoleColor.DarkCyan); // subscriber 1
            observable.SubscribeConsole(2, ConsoleColor.DarkRed);  // subscriber 2

            Console.ReadKey();

            var connectableObservable = observable.Publish();

            connectableObservable.SubscribeConsole(3, ConsoleColor.Black); // subscriber 3
            connectableObservable.SubscribeConsole(4, ConsoleColor.Blue);  // subscriber 4

            connectableObservable.Connect();


            observable.Wait();
        }
    }
}
