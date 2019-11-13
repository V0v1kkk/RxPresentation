using System;

namespace RxMulticastingDemo
{
    public static class ObservableExtensions
    {
        private static readonly object Lock = new object();

        public static IDisposable SubscribeConsole<T>(this IObservable<T> observable, int number, ConsoleColor color)
        {
            return observable.Subscribe(obj =>
            {
                lock (Lock)
                {
                    var originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    Console.WriteLine($"Subscriber {number} - {obj}");
                    Console.ForegroundColor = originalColor;
                }
            });
        }
    }
}