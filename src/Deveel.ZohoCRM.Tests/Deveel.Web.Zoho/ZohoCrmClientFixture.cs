using System.Collections.Generic;
using Deveel.Web.Zoho;
using NUnit.Framework;

namespace Deveel.Web.Deveel.Web.Zoho
{
    public class ZohoCrmClientFixture
    {
        [Test]
        public void should_allow_users_to_intercept_request_and_response()
        {
            string interceptedRequest = null;
            string interceptedResponse = null;

            var zohoCrmClient = new ZohoCrmClient("doesntmatter")
            {
                OnPostDataRawRequest = xml => { interceptedRequest = xml; },
                OnPostDataRawResponse = xml => { interceptedResponse = xml; }
            };

            List<ZohoContact> contacts = new List<ZohoContact>();
            contacts.Add(new ZohoContact("email@email.com", "Firstname", "lastname"));

            var exception = Assert.Throws<ZohoResponseException>(() => zohoCrmClient.BulkUpsertRecords(contacts));

            Assert.That(interceptedRequest, Does.Contain("email@email.com"));
            Assert.That(interceptedResponse, Does.Contain("Invalid Authtoken"));
        }        
    }
}