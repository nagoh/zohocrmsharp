using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deveel.Web.Zoho;
using NUnit.Framework;

namespace Deveel.Web.Deveel.Web.Zoho
{
    [TestFixture]
    public class PotentialTest : ZohoCrmTestBase
    {
        private List<ZohoPotential> _potentials;
        private List<string> _createdIds;
        private ZohoCrmClient _client;

        [SetUp]
        public void Setup()
        {
            _client = CreateClient();
            _createdIds = new List<string>();
            _potentials = new List<ZohoPotential>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_createdIds.Any())
            {
                _createdIds.ForEach(x => _client.DeleteRecordById<ZohoPotential>(x));
            }
        }

        [Test]
        public void should_send_multiple()
        {
            for (int i = 0; i < 2; i++)
            {
                _potentials.Add(new ZohoPotential(Guid.NewGuid().ToString()) { Amount = 100, Stage = "Created" });
            }
           
            var client = CreateClient();
            
            var records = client.BulkUpsertRecords(_potentials);
            _createdIds.AddRange(records.Results.Select(x => x.ResponseItem).OfType<ZohoDetailsResponseItem>().Select(x => x.Id));

            foreach (var potential in _potentials)
            {
                //var returnedRecord = records.Results.SingleOrDefault(x => x.Record.Name == potential.Name);
                //potential.ZohoId = ((ZohoDetailsResponseItem) returnedRecord.ResponseItem).Id;

                potential.Amount++;
            }

            records = client.BulkUpsertRecords(_potentials);
        }

        [Test]
        public void should_send_data_in_cdata_block()
        {
            // Arrange
            var zohoOrder = new ZohoPotential(Guid.NewGuid().ToString());
            zohoOrder.SetValue("test", "---some-data---");

            // Act
            var collection = new ZohoEntityCollection<ZohoPotential>{ zohoOrder };
            
            // Assert
            string xml = null;
            Assert.DoesNotThrow(() => xml = collection.ToXmlString());
            
            Assert.That(xml, Is.Not.Null);
            Assert.That(xml.Contains("<FL val=\"test\"><![CDATA[---some-data---]]></FL>"));
        }

        [Test]
        public void should_not_include_null_value()
        {
            // Arrange
            var zohoOrder = new ZohoPotential(Guid.NewGuid().ToString());
            zohoOrder.SetValue("test", null);

            // Act
            var collection = new ZohoEntityCollection<ZohoPotential>{ zohoOrder };
            
            // Assert
            string xml = null;
            Assert.DoesNotThrow(() => xml = collection.ToXmlString());
            
            Assert.That(xml, Is.Not.Null);
            Assert.That(xml.Contains("<FL val=\"test\">"), Is.False);
        }

        [Test]
        public void should_create_empty_tag_with_empty_string()
        {
            // Arrange
            var zohoOrder = new ZohoPotential(Guid.NewGuid().ToString());
            zohoOrder.SetValue("test", "");

            // Act
            var collection = new ZohoEntityCollection<ZohoPotential>{ zohoOrder };
            
            // Assert
            string xml = null;
            Assert.DoesNotThrow(() => xml = collection.ToXmlString());
            
            Assert.That(xml, Is.Not.Null);
            Assert.That(xml.Contains("<FL val=\"test\" />"));
        }

        [Test]
        public void should_handle_invalid_cdata_string()
        {
            // Arrange
            var zohoOrder = new ZohoPotential(Guid.NewGuid().ToString());
            zohoOrder.SetValue("test", "---]]>---");  // Cannot have ']]>' inside an XML CDATA block

            // Act
            var collection = new ZohoEntityCollection<ZohoPotential>{ zohoOrder };
            
            // Assert
            string xml = null;
            Assert.DoesNotThrow(() => xml = collection.ToXmlString());
            
            Assert.That(xml, Is.Not.Null);
            Assert.That(xml.Contains("<FL val=\"test\"><![CDATA[---]]>]]&gt;<![CDATA[---]]></FL>"), "Should have separate CData blocks with escaped ']]>'");
        }

        [Test]
        public void should_handle_multiple_invalid_cdata_string()
        {
            // Arrange
            var zohoOrder = new ZohoPotential(Guid.NewGuid().ToString());
            zohoOrder.SetValue("test", "]]>---]]>---]]>");  // Cannot have ']]>' inside an XML CDATA block

            // Act
            var collection = new ZohoEntityCollection<ZohoPotential>{ zohoOrder };
            
            // Assert
            string xml = null;
            Assert.DoesNotThrow(() => xml = collection.ToXmlString());
            
            Assert.That(xml, Is.Not.Null);
            Assert.That(xml.Contains("<FL val=\"test\">]]&gt;<![CDATA[---]]>]]&gt;<![CDATA[---]]>]]&gt;</FL>"), "Should have separate CData blocks with escaped ']]>'");
        }
    }
}
