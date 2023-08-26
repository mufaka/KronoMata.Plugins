using KronoMata.Public;
using System.Net;
using System.Net.Http.Json;

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
                    Description = "The endpoint URL for the request. (http://xyz.somedomain.com)",
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
                    Name = "Query Parameters",
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
                    Name = "Body Is Json",
                    Description = "Whether or not the body is Json.",
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
                var invalidConfigurationResult = ValidateRequiredParameters(pluginConfig);

                if (invalidConfigurationResult != null)
                {
                    log.Add(invalidConfigurationResult);
                }
                else
                {
                    var url = pluginConfig["Uri"];

                    if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        throw new ArgumentException("Invalid url provided. It must be well formed and absolute. eg: http://www.domain.com");
                    }

                    var uri = new Uri(pluginConfig["Uri"]);

                    if (uri.Scheme.ToLower() != "http" && uri.Scheme.ToLower() != "https")
                    {
                        throw new ArgumentException("The Uri scheme must be either http or https.");
                    }

                    var method = pluginConfig["Method"];
                    url = GetUrlWithQueryStringParameters(url, pluginConfig);

                    switch (method)
                    {
                        case "GET":
                            return HttpGetResult(HttpMethod.Get, url, pluginConfig);
                        case "PUT":
                            return HttpGetResult(HttpMethod.Put, url, pluginConfig);
                        case "POST":
                            return HttpGetResult(HttpMethod.Post, url, pluginConfig);
                        case "PATCH":
                            return HttpGetResult(HttpMethod.Patch, url, pluginConfig);
                        case "DELETE":
                            return HttpGetResult(HttpMethod.Delete, url, pluginConfig);
                        default:
                            throw new ArgumentException($"Unexpected Method provided. Expecting one of GET,PUT,POST,PATCH,DELETE but received {method}.");
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

        private List<PluginResult> HttpGetResult(HttpMethod method, string url, Dictionary<string, string> parameters)
        {
            var log = new List<PluginResult>();

            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(method, url);

                AddHeaders(request, parameters);

                // Body in Get request is not specifically disallowed in the HTTP specification. A lot of server implementations will ignore this
                // but others, like ElasticSearch (https://www.elastic.co/guide/en/elasticsearch/reference/current/api-conventions.html), do allow for it.
                AddBody(request, parameters);

                var response = client.Send(request);

                if (StatusCodeMeansSuccess(response.StatusCode))
                {
                    using var reader = new StreamReader(response.Content.ReadAsStream());
                    var result = reader.ReadToEnd();

                    // TODO: The result should probably be trimmed
                    log.Add(new PluginResult()
                    {
                        IsError = false,
                        Message = $"Request Successful ({response.StatusCode})",
                        Detail = result
                    });
                }
                else
                {
                    log.Add(new PluginResult()
                    {
                        IsError = true,
                        Message = $"Received HTTP Status Code {response.StatusCode}",
                        Detail = "For a complete list of status codes and their meaning, visit https://learn.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view=net-6.0"
                    });
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

        private List<KeyValuePair<string, string>> ReadValues(string rawProperties)
        {
            if (String.IsNullOrWhiteSpace(rawProperties)) return new List<KeyValuePair<string, string>>();

            var reader = new StringReader(rawProperties);
            var keyValuePairs = new List<KeyValuePair<string, string>>();

            string line;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            while ((line = reader.ReadLine()) != null)
            {
                var tokens = line.Trim().Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length > 1)
                {
                    keyValuePairs.Add(new KeyValuePair<string, string>(tokens[0], tokens[1]));
                }
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            return keyValuePairs;
        }

        private string GetUrlWithQueryStringParameters(string url, Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey("Query Parameters"))
            {
                var uriBuilder = new UriBuilder(url);
                var keyValuePairs = ReadValues(parameters["Query Parameters"]);

                foreach (var keyValuePair in keyValuePairs)
                {
                    string query = $"{Uri.EscapeDataString(keyValuePair.Key)}={Uri.EscapeDataString(keyValuePair.Value)}";

                    if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
                    {
                        uriBuilder.Query = uriBuilder.Query + "&" + query;
                    }
                    else
                    {
                        // .NET Core version of UriBuilder automatically adds the ?
                        uriBuilder.Query = query;
                    }
                }

                return uriBuilder.Uri.AbsoluteUri;
            }
            else
            {
                return url;
            }
        }

        private void AddHeaders(HttpRequestMessage message, Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey("Headers"))
            {
                var keyValuePairs = ReadValues(parameters["Headers"]);

                foreach (var keyValuePair in keyValuePairs)
                {
                    // NOTE: The value can be a comma separated list
                    message.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        private void AddBody(HttpRequestMessage message, Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey("Body"))
            {
                var body = parameters["Body"];
                var contentIsJson = false;

                if (parameters.ContainsKey("Body Is Json"))
                {
                    contentIsJson = parameters["Body Is Json"] == "True";
                }

                message.Content = contentIsJson ? JsonContent.Create(body) : new StringContent(body);
            }
        }

        private bool StatusCodeMeansSuccess(HttpStatusCode statusCode)
        {
            // https://www.rfc-editor.org/rfc/rfc9110.html#name-status-codes
            // https://datatracker.ietf.org/doc/html/rfc2616
            // https://learn.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view=net-6.0
            switch (statusCode)
            {
                // Successful 2xx
                case HttpStatusCode.OK:                             // 200
                case HttpStatusCode.Created:                        // 201
                case HttpStatusCode.Accepted:                       // 202
                case HttpStatusCode.NonAuthoritativeInformation:    // 203
                case HttpStatusCode.NoContent:                      // 204
                case HttpStatusCode.ResetContent:                   // 205
                case HttpStatusCode.PartialContent:                 // 206
                case HttpStatusCode.MultipleChoices:                // 207
                case HttpStatusCode.AlreadyReported:                // 208
                case HttpStatusCode.IMUsed:                         // 226
                    return true;
                default:
                    return false;
            }
        }

    }
}
