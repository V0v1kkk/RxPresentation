using System;
using System.Collections.Generic;
using System.Linq;

namespace RxWebPageParseDemo
{
    public class StackExchangeTopic : IEquatable<StackExchangeTopic>
    {
        public string Title { get; }
        public string Link { get; }
        public List<string> Tags { get; }

        public StackExchangeTopic(string title, string link, List<string> tags)
        {
            Title = title.Trim();
            Link = link.Trim();
            Tags = tags ?? new List<string>(0);
        }

        public override string ToString()
        {
            return $"Title: {Title}{Environment.NewLine}Tags: {Tags.Aggregate((a, b) => a + ", " + b)}{Environment.NewLine}Link: {Link}{Environment.NewLine}----";

        }

        public bool Equals(StackExchangeTopic other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Title, other.Title, StringComparison.InvariantCultureIgnoreCase) && string.Equals(Link, other.Link, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StackExchangeTopic) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Title != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Title) : 0) * 397) ^ (Link != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Link) : 0);
            }
        }

        public static bool operator ==(StackExchangeTopic left, StackExchangeTopic right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StackExchangeTopic left, StackExchangeTopic right)
        {
            return !Equals(left, right);
        }
    }
}