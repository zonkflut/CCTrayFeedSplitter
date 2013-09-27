using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace CCTrayFeedSplitter
{
    public class XmlActionResult : ActionResult
    {
        public XmlActionResult(XDocument xml)
        {
            Xml = xml;
            ContentType = "text/xml";
            Encoding = Encoding.UTF8;
        }

        public XDocument Xml { get; private set; }
        
        public string ContentType { get; set; }
        
        public Encoding Encoding { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = ContentType;
            context.HttpContext.Response.HeaderEncoding = Encoding;
            var writer = new XmlTextWriter(context.HttpContext.Response.OutputStream, Encoding.UTF8);
            Xml.WriteTo(writer);
            writer.Close();
        }
    }
}