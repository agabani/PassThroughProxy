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
            _client = new HttpClient(_handler = new HttpClientHandler
            {
                Proxy = new WebProxy(_proxyAddress, false)
            });
        }

        [TearDown]
        public void TearDown()
        {
            _response?.Dispose();
            _client?.Dispose();
            _handler?.Dispose();
            _server?.Dispose();
        }

        private readonly Uri _proxyAddress = new Uri($"http://{Environment.MachineName}:8888");
        private readonly Uri _baseAddress = new Uri($"http://{Environment.MachineName}:9000");

        private HttpStubServer _server;
        private HttpClientHandler _handler;
        private HttpClient _client;
        private HttpResponseMessage _response;

        [Test]
        public void it_talks_to_server()
        {
            _server
                .When(message => true)
                .Return(() => new HttpResponseMessage(HttpStatusCode.Created));

            _response = _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, _baseAddress)).GetAwaiter().GetResult();

            Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }
    }
}