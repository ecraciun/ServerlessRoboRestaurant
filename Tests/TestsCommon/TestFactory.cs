using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace TestsCommon
{
    public class TestFactory
    {
        private static Dictionary<string, StringValues> CreateQueryStringDictionary(Dictionary<string, string> initialDictionary)
        {
            var queryStringDictionary = new Dictionary<string, StringValues>();
            if (initialDictionary != null)
            {
                foreach (var keyValuePair in initialDictionary)
                {
                    queryStringDictionary.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return queryStringDictionary;
        }

        public static DefaultHttpRequest CreateHttpRequest(
            string method = "get",
            Dictionary<string, string> query = null,
            Dictionary<string, string> headers = null)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = method
            };

            if (query != null)
            {
                request.Query = new QueryCollection(CreateQueryStringDictionary(query));
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            return request;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;

            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }

        public static void SetupEnvironmentVariables(Dictionary<string, string> environmentVariables)
        {
            if (environmentVariables != null)
            {
                foreach (var environmentVariable in environmentVariables)
                {
                    Environment.SetEnvironmentVariable(environmentVariable.Key, environmentVariable.Value);
                }
            }
        }

        public static void SetupDefaultEnvironmentVariables()
        {
            var defaultEnvironmentVariables = new Dictionary<string, string>
            {
                { Constants.CosmosDbEndpointKeyName, "test" },
                { Constants.CosmosDbKeyKeyName, "test" }
            };

            SetupEnvironmentVariables(defaultEnvironmentVariables);
        }
    }
}