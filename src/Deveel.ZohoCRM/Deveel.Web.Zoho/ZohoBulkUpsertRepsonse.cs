using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Deveel.Web.Zoho
{
    public static class ZohoBulkUpsertResponseCodes
    {
        public const int RecordAddedSuccessfully = 2000;
        public const int RecordUpdatedSuccessfully = 2001;
        public const int RecordAlreadyExists = 2002;
    }


    public class ZohoBulkUpsertRepsonse<T> where T : ZohoEntity
    {
        public ZohoBulkUpsertRepsonse(string rawResponse, List<T> requestItems)
        {
            var xDocument = XDocument.Parse(rawResponse);
            ThrowIfError(xDocument.Root);

            Results = new List<ZohoBulkUpsertResponseItem<T>>();
            var resultNode = xDocument.Descendants("result").First();

            if (resultNode == null)
                throw new InvalidOperationException("Unexpected response for ZohoBulkUpsertRepsonse");

            foreach (var element in resultNode.Elements("row"))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var rowNumber = int.Parse(element.Attribute("no")?.Value);
                var requestItem = requestItems.SingleOrDefault(x => x.RowNumber == rowNumber);
                var responseItem = GetResponseItem(element.Elements().First());
                Results.Add(new ZohoBulkUpsertResponseItem<T>(rowNumber, requestItem, responseItem));
            }
        }

        private static void ThrowIfError(XElement parent)
        {
			if (parent.Name != "response")
				return;

			var firstChild = parent.Elements().First();
			if (firstChild.Name != "error")
                return;

			var code = firstChild.Descendants("code").FirstOrDefault()?.Value ?? "0000";
			var message = firstChild.Descendants("message").FirstOrDefault()?.Value ?? "Unknown error: " + firstChild;

            throw new ZohoResponseException(code, message);
        }

        private ZohoResponseItem GetResponseItem(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "success":
                    var details = element.Element("details");
                    return new ZohoDetailsResponseItem()
                    {
                        Code = int.Parse(element.Element("code").Value),
                        CreatedBy = GetDetailsValue(details, "Created By"),
                        Id = GetDetailsValue(details, "Id"),
                        ModifiedDateTime = DateTime.Parse(GetDetailsValue(details, "Modified Time")),
                        CreatedDateTime = DateTime.Parse(GetDetailsValue(details, "Created Time")),
                        ModifiedBy = GetDetailsValue(details, "Modified By")
                    };

                case "error":
                    return new ZohoErrorRepsonseItem
                    {
                        Code = int.Parse(element.Element("code").Value),
                        Error = element.Element("details").Value
                    };

                default:
                    throw new InvalidOperationException(string.Format("Unable to hanlde Zoho response {0}", element.Name.LocalName));
            }
        }

        private string GetDetailsValue(XElement element, string valName)
        {
            var xpath = string.Format(@"FL[@val=""{0}""]", valName);
            return element.XPathSelectElement(xpath).Value;
        }

        public ZohoBulkUpsertRepsonse(List<ZohoBulkUpsertResponseItem<T>> responseItems)
        {
            Results = responseItems;
        }
        public List<ZohoBulkUpsertResponseItem<T>> Results { get; }
    }

    public class ZohoBulkUpsertResponseItem<T> where T : ZohoEntity
    {
        public ZohoBulkUpsertResponseItem(int sequence, ZohoResponseItem repsonseItem)
        {
            ResponseItem = repsonseItem;
            Sequence = sequence;
        }

        public ZohoBulkUpsertResponseItem(int sequence, T item, ZohoResponseItem repsonseItem)
            : this(sequence, repsonseItem)
        {
            Record = item;
        }

        public int Sequence { get; }
        public T Record { get;}
        public ZohoResponseItem ResponseItem { get; set; }
    }

    public abstract class ZohoResponseItem
    {
        public int Code { get; set; }
    }

    public class ZohoDetailsResponseItem : ZohoResponseItem
    {
        public string Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
    public class ZohoErrorRepsonseItem : ZohoResponseItem
    {
        public string Error { get; set; }   
    }


}
