using System;
using System.Net;
using System.Net.Http;
using NUnit.Framework;
using StubServer.Http;

namespace Proxy.Tests
{
    [TestFixture]
    internal class CharacterisationTests
    {
        [SetUp]
        public void SetUp()
        {
            _server = new HttpStubServer(_baseAddress);
            _client = new HttpClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _server.Dispose();
        }

        private HttpStubServer _server;
        private HttpClient _client;
        private readonly Uri _baseAddress = new Uri("http://localhost:9000");

        [Test]
        public void it_talks_to_server()
        {
            _server
                .When(message => true)
                .Return(() => new HttpResponseMessage(HttpStatusCode.OK));

            var response = _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, _baseAddress)).GetAwaiter().GetResult();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}