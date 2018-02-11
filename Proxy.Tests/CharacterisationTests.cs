using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using Proxy.Configurations;
using StubServer.Http;

namespace Proxy.Tests
{
    [TestFixture]
    internal class CharacterisationTests
    {
        public void SetUp(bool rejectHttpProxy = false)
        {
            Configuration.Settings = new Configuration(
                new Server(ProxyPort, rejectHttpProxy),
                new Authentication(false, null, null),
                new Firewall(false, new Rule[] { }));

            _proxy = new PassThroughProxy(ProxyPort, Configuration.Settings);
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

        private Uri _serverBaseAddress = new Uri($"http://{MachineName}:{ServerPort}");
        private Uri _proxyBaseAddress = new Uri($"http://{MachineName}:{ProxyPort}");

        private PassThroughProxy _proxy;
        private HttpStubServer _server;
        private HttpClientHandler _handler;
        private HttpClient _client;
        private HttpResponseMessage _response;

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void it_talks_to_server()
        {
            SetUp();

            _server
                .When(message => message.Method == HttpMethod.Post)
                .Return(() => new HttpResponseMessage(HttpStatusCode.Created));

            _response = _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, _serverBaseAddress)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new {key = "value"}), Encoding.UTF8, "application/json")
            }).GetAwaiter().GetResult();

            Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void it_allows_rejecting_http_proxy()
        {
            SetUp(rejectHttpProxy: true);
            
            _response = _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, _serverBaseAddress)).GetAwaiter().GetResult();

            Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.MethodNotAllowed));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void it_accepts_https_tunnel_when_rejecting_http_proxy()
        {
            SetUp(rejectHttpProxy: true);

            var content = Guid.NewGuid().ToString();
            _server
                .When(message => message.Method == HttpMethod.Get)
                .Return(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content),
                });

            using (var tcpClient = new TcpClient())
            {
                tcpClient.Connect(MachineName, ProxyPort);
                using (var networkStream = tcpClient.GetStream())
                using (var streamWriter = new StreamWriter(networkStream, Encoding.ASCII))
                using (var streamReader = new StreamReader(networkStream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, bufferSize: 1))
                {
                    // Connect
                    streamWriter.Write($"CONNECT {MachineName}:{ServerPort} HTTP/1.1\r\n");
                    streamWriter.Write($"Host: {MachineName}:{ServerPort}\r\n");
                    streamWriter.Write($"\r\n");
                    streamWriter.Flush();

                    // Wait for the connection to be established.
                    var connectionEstablished = streamReader.ReadLine();
                    var emptyLine = streamReader.ReadLine();

                    // Contact the origin server.
                    streamWriter.Write($"GET / HTTP/1.1\r\n");
                    streamWriter.Write($"Host: {MachineName}:{ServerPort}\r\n");
                    streamWriter.Write($"\r\n");
                    streamWriter.Flush();

                    // Read the response.
                    var output = new StringBuilder();
                    int read;
                    do
                    {
                        read = streamReader.Read();
                        output.Append((char)read);
                    }
                    while (read >= 0 && !output.ToString().Contains(content));

                    Assert.True(output.ToString().Contains(content));
                }
            }

        }
    }
}