using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deveel.Web.Zoho;
using NUnit.Framework;

namespace Deveel.Web.Deveel.Web.Zoho
{
    [TestFixture(Category = "Integration")]
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
    }

}
