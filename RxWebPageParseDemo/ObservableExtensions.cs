using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using HtmlAgilityPack;

namespace RxWebPageParseDemo
{
    public static class ObservableExtensions
    {
        public static IObservable<string> GetWebPageText(this IObservable<Unit> observable, Uri pageUri)
        {
            return observable.SelectMany(_ => Observable.FromAsync(async () =>
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(pageUri);
                    var pageContents = await response.Content.ReadAsStringAsync();
                    return pageContents;
                }
            }));
        }

        public static IObservable<IList<StackExchangeTopic>> SelectTopics(this IObservable<string> pageContentObservable)
        {
            return pageContentObservable
                .Select(content =>
                {
                    var document = new HtmlDocument();
                    document.LoadHtml(content);
                    var summaryNodes = document.DocumentNode
                        .Descendants()
                        .Where(d => d.HasClass("question-hot"))
                        .ToList();

                    var topics = new List<StackExchangeTopic>(summaryNodes.Count);

                    foreach (var summaryNode in summaryNodes)
                    {
                        var linkNode = summaryNode
                            .Descendants()
                            .FirstOrDefault(d => d.HasClass("realtime-question-url"));
                        
                        var href = linkNode?.Attributes["href"]?.Value;
                        var title = linkNode?.InnerText;

                        if (linkNode == null 
                            || string.IsNullOrWhiteSpace(href) 
                            || string.IsNullOrWhiteSpace(title)
                            || title.Trim() == "template")
                        {
                            continue;
                        }

                        var tags = summaryNode
                            .Descendants()
                            .Where(d => d.HasClass("post-tag"))
                            .Select(node => node.InnerText)
                            .ToList();
                        
                        var topic = new StackExchangeTopic(title, href, tags);
                        topics.Add(topic);
                    }

                    return topics;
                });

            
        }

        /// <summary>
        /// Returns only new stackexchange topics
        /// </summary>
        public static IObservable<IList<StackExchangeTopic>> FilterOnlyNew(
            this IObservable<IList<StackExchangeTopic>> pageContentObservable)
        {
            var buffer = new CircularBuffer<StackExchangeTopic>(300);

            return pageContentObservable
                .Select(exchangeTopics =>
                {
                    var newTopics = new List<StackExchangeTopic>(5);
                    foreach (var exchangeTopic in exchangeTopics)
                    {
                        if (buffer.Contains(exchangeTopic))
                            continue;

                        buffer.PushBack(exchangeTopic);
                        newTopics.Add(exchangeTopic);
                    }

                    return newTopics;
                });
        }

    }
}