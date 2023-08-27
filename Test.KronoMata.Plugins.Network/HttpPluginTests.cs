using KronoMata.Plugins.Network;
using KronoMata.Public;
using Newtonsoft.Json;

namespace Test.KronoMata.Plugins.Network
{
    public class HttpPluginTests
    {
        private const string HTTPBIN_ROOT_HTTP = "http://localhost:88/anything";
        private const string HTTPS_ROOT = "https://www.google.com/";

        [SetUp]
        public void Setup()
        {
        }

        private List<PluginResult> ExecutePlugin(Dictionary<string, string> parameters)
        {
            var plugin = new HttpPlugin();
            var log = new List<PluginResult>();

            try
            {
                return plugin.Execute(new Dictionary<string, string>(), parameters);
            }
            catch (Exception ex)
            {
                log.Add(new PluginResult()
                {
                    IsError = true,
                    Message = ex.Message,
                    Detail = ex.StackTrace ?? String.Empty
                });
            }

            return log;
        }

        [Test]
        public void Does_Validate_Required_Parameters()
        {
            var emptyConfiguration = new Dictionary<string, string>();
            var log = ExecutePlugin(emptyConfiguration);

            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.True);
            Assert.That(log[0].Message, Is.EqualTo("Missing required parameter(s)."));
        }

        [Test]
        public void Fails_On_Malformed_Uri()
        {
            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", "I am not a Uri" }
            };

            var log = ExecutePlugin(configuration);

            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.True);
            Assert.That(log[0].Message, Is.EqualTo("Invalid url provided. It must be well formed and absolute. eg: http://www.domain.com"));
        }

        [Test]
        public void Fails_On_Invalid_Scheme()
        {
            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", "file://somefile.txt" }
            };

            var log = ExecutePlugin(configuration);

            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.True);
            Assert.That(log[0].Message, Is.EqualTo("The Uri scheme must be either http or https."));
        }

        [Test]
        public void Fails_On_Invalid_Method()
        {
            var configuration = new Dictionary<string, string>
            {
                { "Method", "GETSOME" },
                { "Uri", "http://www.domain.com" }
            };

            var log = ExecutePlugin(configuration);

            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.True);
            Assert.That(log[0].Message, Is.EqualTo("Unexpected Method provided. Expecting one of GET,PUT,POST,PATCH,DELETE but received GETSOME."));
        }

        [Test]
        public void Can_Get_Http_With_No_Parameters()
        {
            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", HTTPBIN_ROOT_HTTP }
            };

            var log = ExecutePlugin(configuration);
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.False);
        }

        [Test]
        public void Can_Get_Https_With_No_Parameters()
        {
            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", HTTPS_ROOT }
            };

            var log = ExecutePlugin(configuration);

            Assert.That(log.Count, Is.EqualTo(1));

            if (log[0].IsError)
            {
                Console.WriteLine(log[0].Message);
                Console.WriteLine(log[0].Detail);
            }

            Assert.That(log[0].IsError, Is.False);
        }


        [Test]
        public void Cannot_Get_Http_With_Incorrect_Url()
        {
            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", HTTPBIN_ROOT_HTTP + "butthis" }
            };

            var log = ExecutePlugin(configuration);
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.True);
            Assert.That(log[0].Message, Is.EqualTo("Received HTTP Status Code NotFound"));
        }

        [Test]
        public void Can_Send_Http_With_Headers()
        {
            // header values should be Hyphenated-Pascal-Case
            var headerValue = @"Kronomata-Header-1=Header 1 Value
Kronomata-Header-2=Header 2 Value";

            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", HTTPBIN_ROOT_HTTP },
                { "Headers", headerValue }
            };

            var log = ExecutePlugin(configuration);
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.False);

            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(log[0].Detail);

            var header1Value = httpBinResponse.headers["Kronomata-Header-1"];
            var header2Value = httpBinResponse.headers["Kronomata-Header-2"];

            Assert.That(header1Value, Is.EqualTo("Header 1 Value"));
            Assert.That(header2Value, Is.EqualTo("Header 2 Value"));
        }

        [Test]
        public void Can_Get_Http_With_Query()
        {
            var queryValue = @"foo=bar
biz=baz";

            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", HTTPBIN_ROOT_HTTP },
                { "Query Parameters", queryValue }
            };

            var log = ExecutePlugin(configuration);
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.False);

            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(log[0].Detail);

            var arg1 = httpBinResponse.args["foo"];
            var arg2 = httpBinResponse.args["biz"];

            Assert.That(arg1, Is.EqualTo("bar"));
            Assert.That(arg2, Is.EqualTo("baz"));
        }

        [Test]
        public void Can_Get_Http_With_Query_With_Spaces()
        {
            var queryValue = @"foo 1=bar 1
biz 1=baz 1";

            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", HTTPBIN_ROOT_HTTP },
                { "Query Parameters", queryValue }
            };

            var log = ExecutePlugin(configuration);
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.False);

            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(log[0].Detail);

            var arg1 = httpBinResponse.args["foo 1"];
            var arg2 = httpBinResponse.args["biz 1"];

            Assert.That(arg1, Is.EqualTo("bar 1"));
            Assert.That(arg2, Is.EqualTo("baz 1"));
        }

        [Test]
        public void Can_Get_Http_With_Query_And_Existing_Params()
        {
            var queryValue = @"foo 1=bar 1
biz 1=baz 1";

            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", HTTPBIN_ROOT_HTTP + "?existing=hello" },
                { "Query Parameters", queryValue }
            };

            var log = ExecutePlugin(configuration);
            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.False);

            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(log[0].Detail);

            var arg1 = httpBinResponse.args["foo 1"];
            var arg2 = httpBinResponse.args["biz 1"];
            var arg3 = httpBinResponse.args["existing"];

            Assert.That(arg1, Is.EqualTo("bar 1"));
            Assert.That(arg2, Is.EqualTo("baz 1"));
            Assert.That(arg3, Is.EqualTo("hello"));
        }

        [Test]
        public void Can_Get_Http_With_Body()
        {
            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", HTTPBIN_ROOT_HTTP },
                { "Body", "I am a body" }
            };

            var log = ExecutePlugin(configuration);
            Assert.That(log.Count, Is.EqualTo(1));

            if (log[0].IsError )
            {
                Console.WriteLine(log[0].Detail);
            }

            Assert.That(log[0].IsError, Is.False);

            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(log[0].Detail);

            Assert.That(httpBinResponse.data, Is.EqualTo("I am a body"));
        }

        [Test]
        public void Can_Put_Http_With_Body()
        {
            var configuration = new Dictionary<string, string>
            {
                { "Method", "PUT" },
                { "Uri", HTTPBIN_ROOT_HTTP },
                { "Body", "I am a body" }
            };

            var log = ExecutePlugin(configuration);
            Assert.That(log.Count, Is.EqualTo(1));

            if (log[0].IsError )
            {
                Console.WriteLine(log[0].Detail);
            }

            Assert.That(log[0].IsError, Is.False);

            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(log[0].Detail);

            Assert.That(httpBinResponse.data, Is.EqualTo("I am a body"));
        }



        [Test]
        public void Can_Get_Http_With_JsonBody()
        {
            var jsonBody = "{ 'message': 'I am a message.' }";

            var configuration = new Dictionary<string, string>
            {
                { "Method", "GET" },
                { "Uri", HTTPBIN_ROOT_HTTP },
                { "Body", jsonBody },
                { "Body Is Json", "True" }
            };

            var log = ExecutePlugin(configuration);
            Assert.That(log.Count, Is.EqualTo(1));

            if (log[0].IsError)
            {
                Console.WriteLine(log[0].Detail);
            }

            Assert.That(log[0].IsError, Is.False);

            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(log[0].Detail);

            Assert.That(httpBinResponse.json, Is.EqualTo(jsonBody));
        }



        private class HttpBinResponse
        {
            public Dictionary<string, string> args { get; set; }
            public Dictionary<string, string> headers { get; set; }
            public string data { get; set; }
            public string method { get; set; }
            public string origin { get; set; }
            public string url { get; set; }
            public string json { get; set; }
            public Dictionary<string, string> forms { get; set; }
        }
    }
}