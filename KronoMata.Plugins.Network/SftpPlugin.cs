using KronoMata.Public;
using System.Net;

namespace KronoMata.Plugins.Network
{
    public class SftpPlugin : IPlugin
    {
        public string Name { get { return "Sftp Plugin"; } }

        public string Description { get { return "Sends or receives files using the SFTP protocol"; } }

        public string Version { get { return "1.0"; } }

        public List<PluginParameter> Parameters
        {
            get
            {
                var parameters = new List<PluginParameter>();

                parameters.Add(new PluginParameter()
                {
                    Name = "Server",
                    Description = "The IP Address or Domain Name of the SFTP server.",
                    DataType = ConfigurationDataType.String,
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Port",
                    Description = "The Port of the SFTP server. 22 will be used if blank.",
                    DataType = ConfigurationDataType.String,
                    IsRequired = false
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "User",
                    Description = "The user name to use when connecting to the server.",
                    DataType = ConfigurationDataType.String,
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Password",
                    Description = "The password to use when connecting to the server.",
                    DataType = ConfigurationDataType.Password,
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Direction",
                    Description = "The direction of the file transfer.",
                    DataType = ConfigurationDataType.Select,
                    SelectValues = "SEND,RECEIVE",
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Local Directory",
                    Description = "The local directory where files will be sent from or received to.",
                    DataType = ConfigurationDataType.String,
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Remote Directory",
                    Description = "The remote directory where files will be sent to or received from.",
                    DataType = ConfigurationDataType.String,
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Delete",
                    Description = "Delete source files after transfer?",
                    DataType = ConfigurationDataType.Boolean,
                    IsRequired = false
                });

                return parameters;
            }
        }

        private PluginResult? ValidateRequiredParameters(Dictionary<string, string> pluginConfig)
        {
            PluginResult? missingRequiredParameterResult = null;

            foreach (PluginParameter parameter in Parameters)
            {
                if (parameter.IsRequired && !pluginConfig.ContainsKey(parameter.Name))
                {
                    missingRequiredParameterResult ??= new PluginResult()
                    {
                        IsError = true,
                        Message = "Missing required parameter(s).",
                        Detail = "The plugin configuration is missing the following parameters:"
                    };

                    missingRequiredParameterResult.Detail = missingRequiredParameterResult.Detail + Environment.NewLine + parameter.Name;
                }
            }

            return missingRequiredParameterResult;
        }

        public List<PluginResult> Execute(Dictionary<string, string> systemConfig, Dictionary<string, string> pluginConfig)
        {
            var log = new List<PluginResult>();

            try
            {
                if (!Directory.Exists(pluginConfig["Local Directory"]))
                {
                    log.Add(new PluginResult()
                    {
                        IsError = true,
                        Message = "Directory not found.",
                        Detail = $"{pluginConfig["Local Directory"]} was not found."
                    });
                }

                var invalidConfigurationResult = ValidateRequiredParameters(pluginConfig);

                if (invalidConfigurationResult != null)
                {
                    log.Add(invalidConfigurationResult);
                }
                else
                {
                    switch (pluginConfig["Direction"])
                    {
                        case "SEND":
                            SendFiles(log, pluginConfig); break;
                        case "RECEIVE":
                            ReceiveFiles(log, pluginConfig); break;
                        default:
                            log.Add(new PluginResult()
                            {
                                IsError = true,
                                Message = "Invalid Direction value.",
                                Detail = $"{pluginConfig["Direction"]} is not a valid direction. Expecting either SEND or RECEIVE."
                            });
                            break;
                    }
                }
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

        private void SendFiles(List<PluginResult> log, Dictionary<string, string> pluginConfig)
        {
            var localDirectory = pluginConfig["Local Directory"];

            // should have a glob pattern parameter.
            var files = Directory.GetFiles(localDirectory, "*", SearchOption.TopDirectoryOnly);
        }
 
        private void ReceiveFiles(List<PluginResult> log, Dictionary<string, string> pluginConfig)
        {

        }
    }
}
