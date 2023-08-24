﻿using KronoMata.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KronoMata.Plugins.Network
{
    public class HttpPlugin : IPlugin
    {
        public string Name { get { return "Http Plugin"; } }

        public string Description { get { return "Sends a request to the configured endpoint."; } }

        public string Version { get { return "1.0"; } }

        public List<PluginParameter> Parameters
        {
            get
            {
                var parameters = new List<PluginParameter>();

                parameters.Add(new PluginParameter()
                {
                    Name = "Method",
                    Description = "The HTTP method to use.",
                    DataType = ConfigurationDataType.Select,
                    SelectValues = "GET,PUT,POST,PATCH,DELETE",
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Uri",
                    Description = "The endpoint URL for the request.",
                    DataType = ConfigurationDataType.String,
                    IsRequired = true
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Headers",
                    Description = "HTTP Headers for the request. 1 per line in the form of {Name}={Value}",
                    DataType = ConfigurationDataType.Text,
                    IsRequired = false
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Queries",
                    Description = "Query string parameters for the request. 1 per line in the form of {Name}={Value}",
                    DataType = ConfigurationDataType.Text,
                    IsRequired = false
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "Body",
                    Description = "The content body for the request.",
                    DataType = ConfigurationDataType.Text,
                    IsRequired = false
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "AuthUser",
                    Description = "Basic authentication username.",
                    DataType = ConfigurationDataType.String,
                    IsRequired = false
                });

                parameters.Add(new PluginParameter()
                {
                    Name = "AuthPassword",
                    Description = "Basic authentication password",
                    DataType = ConfigurationDataType.Password,
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
                var invalidConfigurationResult = ValidateRequiredParameters(pluginConfig);

                if (invalidConfigurationResult != null)
                {
                    log.Add(invalidConfigurationResult);
                }
                else
                {
                    // WebClient or HttpClient? SSL Cert?
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
    }
}