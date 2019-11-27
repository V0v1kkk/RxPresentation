using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
                    using (var request = new HttpRequestMessage(HttpMethod.Get, pageUri))
                    {
                        request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
                        request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                        request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                        request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                        using (var response = await client.SendAsync(request).ConfigureAwait(false))
                        {
                            response.EnsureSuccessStatusCode();
                            using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                            using (var streamReader = new StreamReader(decompressedStream))
                            {
                                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
                            }
                        }
                    }
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
                            .Where(tagText => tagText != "spacer")
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

        public static IObservable<IList<StackExchangeTopic>> SelectStackOverflow(this IObservable<IList<StackExchangeTopic>> observable)
        {
            return observable
                .Select(list => list.Where(topic => topic.Link.Contains("stackoverflow")).ToList());
        }

    }
}