using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
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
            _proxy = new PassThroughProxy(ProxyPort);
            _server = new HttpStubServer(_serverBaseAddress);
            _client = new HttpClient(_handler = new HttpClientHandler
            {
                Proxy = new WebProxy(_proxyBaseAddress, false)
            });
        }

        [TearDown]
        public void TearDown()
        {
            _response?.Dispose();
            _client?.Dispose();
            _handler?.Dispose();
            _server?.Dispose();
            _proxy?.Dispose();
        }

        private static readonly string MachineName = Environment.MachineName;

        private const int ServerPort = 9000;
        private const int ProxyPort = 8889;

        private readonly Uri _serverBaseAddress = new Uri($"http://{MachineName}:{ServerPort}");
        private readonly Uri _proxyBaseAddress = new Uri($"http://{MachineName}:{ProxyPort}");

        private PassThroughProxy _proxy;
        private HttpStubServer _server;
        private HttpClientHandler _handler;
        private HttpClient _client;
        private HttpResponseMessage _response;

        [Test]
        public void it_talks_to_server()
        {
            _server
                .When(message => message.Method == HttpMethod.Post)
                .Return(() => new HttpResponseMessage(HttpStatusCode.Created));

            _response = _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, _serverBaseAddress)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new {key = "value"}), Encoding.UTF8, "application/json")
            }).GetAwaiter().GetResult();

            Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }
    }
}