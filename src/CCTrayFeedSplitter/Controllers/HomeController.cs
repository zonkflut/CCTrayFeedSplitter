using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using CCTrayFeedSplitter.Models;
using CCTrayFeedSplitter.Properties;

namespace CCTrayFeedSplitter.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var partitionCount = Settings.Default.PartitionCount;
            var host = Request.Url != null 
                ? Request.Url.Scheme + "://" + Request.Url.Authority 
                : string.Empty;
            var feedList = Enumerable
                .Range(0, partitionCount)
                .Select(x => new Feed { Url = host + Url.Action("Feed", "Home", new { id = x }) })
                .ToArray();
            return View(feedList);
        }

        public ActionResult Configure()
        {
            var configurationModel = new ConfigurationModel
            {
                FeedUrl = Settings.Default.FeedUrl,
                PartitionCount = Settings.Default.PartitionCount,
            };
            return View(configurationModel);
        }

        [HttpPost]
        public ActionResult Configure(ConfigurationModel configuration)
        {
            Settings.Default.FeedUrl = configuration.FeedUrl;
            Settings.Default.PartitionCount = configuration.PartitionCount;
            Settings.Default.Save();
            return RedirectToAction("Index", "Home");
        }

        public XmlActionResult Feed(int id)
        {
            var feedUrl = Settings.Default.FeedUrl;
            var partitionCount = Settings.Default.PartitionCount;
            
            if (id < 0 || id >= partitionCount)
            {
                Response.StatusCode = 404; 
                Response.TrySkipIisCustomErrors = true; 
                return new XmlActionResult(new XDocument(new XElement("Projects"))
                {
                    Declaration = new XDeclaration("1.0", "utf-8", null)
                });
            }
            
            var rawFeed = XDocument.Load(feedUrl.AbsoluteUri);
            var feedPartitions = new XDocument[partitionCount];
            var projectCount = rawFeed.Descendants("Project").Count();
            var itemsPerFeed = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(projectCount) / partitionCount));

            for (var i = 0; i < feedPartitions.Count(); i++)
            {
                var newDocs = rawFeed.Descendants("Project")
                    .Skip(i * itemsPerFeed)
                    .Take(itemsPerFeed);

                feedPartitions[i] = new XDocument(new XElement("Projects", newDocs))
                {
                    Declaration = new XDeclaration("1.0", "utf-8", null)
                };
            }

            return new XmlActionResult(feedPartitions[id]);
        }
    }
}
