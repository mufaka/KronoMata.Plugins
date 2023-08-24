using KronoMata.Plugins.Network;
using KronoMata.Public;

namespace Test.KronoMata.Plugins.Network
{
    public class HttpPluginTests
    {
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
    }
}