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

        private List<PluginResult> ExecutePlugin(IPlugin plugin, Dictionary<string, string> parameters)
        {
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
            var log = ExecutePlugin(new HttpPlugin(), emptyConfiguration);

            Assert.That(log.Count, Is.EqualTo(1));
            Assert.That(log[0].IsError, Is.True);
            Assert.That(log[0].Message, Is.EqualTo("Missing required parameter(s)."));

            Assert.Pass();
        }
    }
}