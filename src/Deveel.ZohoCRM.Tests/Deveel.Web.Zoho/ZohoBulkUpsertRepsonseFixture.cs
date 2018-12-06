using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using Deveel.Web.Zoho;
using NUnit.Framework;

namespace Deveel.Web.Deveel.Web.Zoho
{
    [TestFixture]
    public class ZohoBulkUpsertRepsonseFixture
    {
        private ZohoBulkUpsertResponseItem<ZohoPotential> ZohoPotentialError =>
            new ZohoBulkUpsertResponseItem<ZohoPotential>(1, new ZohoPotential(""),
                new ZohoErrorRepsonseItem() {Code = 2012, Error = "Error"});

        private ZohoBulkUpsertResponseItem<ZohoPotential> ZohoPotentialDetail=>
            new ZohoBulkUpsertResponseItem<ZohoPotential>(0, new ZohoPotential(""),
                new ZohoDetailsResponseItem() { Code = 2001, Id= "120394239483" });

        [Test]
        public void should_create_errors_and_successes_in_same_response_list()
        {
            var responseItems = new List<ZohoBulkUpsertResponseItem<ZohoPotential>>();
            responseItems.Add(ZohoPotentialError);
            responseItems.Add(ZohoPotentialDetail);

            var repsonse = new ZohoBulkUpsertRepsonse<ZohoPotential>(responseItems);

            Assert.That(repsonse.Results.Select(x => x.ResponseItem).OfType<ZohoDetailsResponseItem>().Count(), Is.EqualTo(1));
            Assert.That(repsonse.Results.Select(x => x.ResponseItem).OfType<ZohoErrorRepsonseItem>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void should_serialize_single_record()
        {
            var response =
                @"<response uri=""/crm/private/xml/Leads/insertRecords"">
                    <result>
                        <row no=""1"">
                            <success>
                                <code>2000</code>
                                <details>
                                    <FL val=""Id"">2000000120006</FL>
                                    <FL val=""Created Time"">2013-02-11 17:55:04</FL>
                                    <FL val=""Modified Time"">2013-02-11 17:55:04</FL>
                                    <FL val=""Created By"">
                                    <![CDATA[aghil123]]>
                                    </FL>
                                    <FL val=""Modified By"">
                                    <![CDATA[aghil123]]>
                                    </FL>
                               </details>
                            </success>
                        </row>
                    </result>
                </response>";

            var item = new ZohoBulkUpsertRepsonse<ZohoPotential>(response, new List<ZohoPotential>());


            Assert.That(item.Results.Count, Is.EqualTo(1));
            Assert.That(item.Results.First().ResponseItem, Is.TypeOf<ZohoDetailsResponseItem>());

            var details = item.Results.First().ResponseItem as ZohoDetailsResponseItem;

            Assert.That(details.CreatedBy, Is.EqualTo("aghil123"));
            Assert.That(details.Id, Is.EqualTo("2000000120006"));
            Assert.That(details.Code, Is.EqualTo(2000));
        }

        [Test]
        public void should_serialize_multiple_records_of_different_types()
        {
            var response = @"<response uri=""/crm/private/xml/Leads/insertRecords"">
                <result>
                    <row no=""1"">
                        <success>
                            <code>2001</code>
                            <details>
                                <FL val=""Id"">2000000120006</FL>
                                <FL val=""Created Time"">2013-02-11 17:55:04</FL>
                                <FL val=""Modified Time"">2013-02-11 17:55:04</FL>
                                <FL val=""Created By"">
                                <![CDATA[ aghil123 ]]>
                                </FL>
                                <FL val=""Modified By"">
                                <![CDATA[ aghil123 ]]>
                                </FL>
                            </details>
                        </success>
                    </row>
                    <row no=""2"">
                <error>
                     <code>4832</code>
                    <details>You have given a wrong value for the field : Annual Revenue</details>
                        </error>
                    </row>
                </result>
            </response>";
            var item = new ZohoBulkUpsertRepsonse<ZohoPotential>(response, new List<ZohoPotential>());

            Assert.AreEqual(2, item.Results.Count);
            var details = item.Results.Select(x => x.ResponseItem).OfType<ZohoDetailsResponseItem>().First();
            Assert.NotNull(details);
            Assert.AreEqual(details.Id, "2000000120006");


            var error = item.Results.Select(x => x.ResponseItem).OfType<ZohoErrorRepsonseItem>().First();

            Assert.NotNull(error);
            Assert.AreEqual(4832, error.Code);
        }


        [Test]
        public void should_return_responseItem_with_requestItem()
        {
            var response = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
                                <response uri=""/crm/private/xml/Potentials/insertRecords"">
                                    <result>
                                        <row no=""1"">
                                            <success><code>2001</code><details><FL val=""Id"">2829458000000286010</FL><FL val=""Created Time"">2017-11-23 20:29:23</FL><FL val=""Modified Time"">2017-11-23 20:31:38</FL><FL val=""Created By""><![CDATA[Sam McGoldrick]]></FL><FL val=""Modified By""><![CDATA[Sam McGoldrick]]></FL></details></success>
                                        </row>
                                        <row no=""2"">
                                            <success><code>2001</code><details><FL val=""Id"">2829458000000286011</FL><FL val=""Created Time"">2017-11-23 20:29:23</FL><FL val=""Modified Time"">2017-11-23 20:31:38</FL><FL val=""Created By""><![CDATA[Sam McGoldrick]]></FL><FL val=""Modified By""><![CDATA[Sam McGoldrick]]></FL></details></success>
                                        </row>
                                    </result>
                                </response>";

            var potential1 = new ZohoPotential("") { RowNumber = 1 };
            var potential2 = new ZohoPotential("") { RowNumber = 2 };

            var upsertResponse = new ZohoBulkUpsertRepsonse<ZohoPotential>(response, new List<ZohoPotential>{ potential2, potential1 });
            var resultItems = upsertResponse.Results.Select(x => x.ResponseItem).Cast<ZohoDetailsResponseItem>().ToList();

            Assert.AreNotEqual(resultItems[0].Id, resultItems[1].Id);
        }

        [Test]
        public void should_throw_when_error_returned()
        {
            const string code = "4401";
            const string message = "Unable to populate data, please check if mandatory value is entered correctly.";

            var response =
                $@"<response uri=""/crm/private/xml/Potentials/insertRecords"">
                    <error>
                        <code>{code}</code>
                        <message>{message}</message>
                    </error>
                </response>";

            var exception = Assert.Throws<ZohoResponseException>(() => new ZohoBulkUpsertRepsonse<ZohoPotential>(response, new List<ZohoPotential>()));
            Assert.That(exception.ErrorCode, Is.EqualTo(code));
            Assert.That(exception.Message, Is.EqualTo(message));
        }

        [Test]
        public void should_throw_when_error_returned2()
        {
            var response =
                $@"<response uri=""/crm/private/xml/Potentials/insertRecords"">
                    <error>
                        <unexpected></unexpected>
                    </error>
                </response>";

            var exception = Assert.Throws<ZohoResponseException>(() => new ZohoBulkUpsertRepsonse<ZohoPotential>(response, new List<ZohoPotential>()));
            Assert.That(exception.ErrorCode, Is.EqualTo("0000"));
            StringAssert.StartsWith("Unknown error:", exception.Message);
        }
    }
}
