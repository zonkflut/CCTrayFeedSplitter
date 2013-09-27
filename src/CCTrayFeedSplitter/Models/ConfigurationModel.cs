using System;

namespace CCTrayFeedSplitter.Models
{
    public class ConfigurationModel
    {
        public int PartitionCount { get; set; }

        public Uri FeedUrl { get; set; }
    }
}