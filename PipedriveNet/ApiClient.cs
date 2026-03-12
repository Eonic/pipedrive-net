using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace PipedriveNet
{
	class ApiClient
	{
		private readonly string _apiKey;
	    private readonly ContractResolver _resolver;
	    internal readonly JsonSerializer Serializer = new JsonSerializer();

	    internal readonly HttpClient HttpClient = new HttpClient();
	    private const string ApiBase = "https://api.pipedrive.com/api/";
        public ApiClient(string apiKey, ContractResolver resolver)
		{
			_apiKey = apiKey;
		    _resolver = resolver;
		    Serializer.ContractResolver = resolver;
		    Serializer.Converters.Add(new StringEnumConverter {CamelCaseText = true});
		    HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

	    public string ResolveProperty<T>(Expression<Func<T, object>> prop)
	    {
	        return _resolver.ResolveCustomName(prop.ExtractProperty());
	    }


	    Uri GetUri(string endpoint)
	    {
	        return new Uri(ApiBase + endpoint + (endpoint.Contains("?") ? "&" : "?") + "api_token=" + _apiKey);
	    }

		sealed class ResponseContainer<T>
		{
			public bool Success { get; set; }
			public string Error { get; set; }
			public T Data { get; set; }
		}

		internal sealed class SearchResultContainer<T>
		{
			public List<SearchResultItem<T>> Items { get; set; }
		}

		internal sealed class SearchResultItem<T>
		{
			public double result_score { get; set; }
			public T item { get; set; }
		}

        async Task<T> Deserialize<T>(Task<HttpResponseMessage> resp)
        {
            var response = await resp;
            string rawJson = null;

            try
            {
                // Read the raw response content first for debugging
                rawJson = await response.Content.ReadAsStringAsync();

                // Check HTTP status code
                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = $"HTTP {(int)response.StatusCode} {response.StatusCode}: {rawJson}";
                    throw new PipedriveException(errorMsg);
                }

                using (var stringReader = new StringReader(rawJson))
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    var container = Serializer.Deserialize<ResponseContainer<T>>(jsonReader);
                    if (container == null)
                        throw new PipedriveException($"Failed to deserialize response. Raw JSON: {rawJson}");

                    if (!container.Success)
                        throw new PipedriveException($"API Error: {container.Error}. Raw JSON: {rawJson}");

                    if (container.Data == null && typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                        container.Data = Activator.CreateInstance<T>();

                    return container.Data;
                }
            }
            catch (ArgumentException argEx) when (argEx.Message.Contains("An item with the same key has already been added"))
            {
                // Duplicate key error - likely caused by custom_fields or dictionary deserialization
                var errorDetails = $"Duplicate Key Error: {argEx.Message}\n" +
                                 $"This error typically occurs when deserializing dictionaries with duplicate keys.\n" +
                                 $"Check if the ContractResolver is causing property name collisions.\n" +
                                 $"Type: {typeof(T).FullName}\n" +
                                 $"Raw Response: {rawJson}";
                throw new PipedriveException(errorDetails, argEx);
            }
            catch (JsonSerializationException jsonEx)
            {
                // JSON deserialization error - check if it's a custom_fields type mismatch
                if (jsonEx.Message.Contains("custom_fields") && jsonEx.Message.Contains("List`1"))
                {
                    var errorDetails = $"Custom Field Type Mismatch: A custom field is configured with a type that doesn't match the API response.\n" +
                                     $"Error: {jsonEx.Message}\n" +
                                     $"Hint: Check your Field() configurations for type mismatches. Ensure custom field types match the API response.\n" +
                                     $"Raw Response (first 500 chars): {(rawJson?.Length > 500 ? rawJson.Substring(0, 500) + "..." : rawJson)}";
                    throw new PipedriveException(errorDetails, jsonEx);
                }
                else
                {
                    var errorDetails = $"JSON Deserialization Error: {jsonEx.Message}\nRaw Response: {rawJson}";
                    throw new PipedriveException(errorDetails, jsonEx);
                }
            }
            catch (JsonException jsonEx)
            {
                // JSON deserialization error - store details for debugging
                var errorDetails = $"JSON Deserialization Error: {jsonEx.Message}\nRaw Response: {rawJson}";
                throw new PipedriveException(errorDetails, jsonEx);
            }
            catch (PipedriveException)
            {
                // Re-throw PipedriveException as-is
                throw;
            }
            catch (Exception e)
            {
                // General error - include all details for debugging
                var errorDetails = $"Unexpected Error: {e.Message}\nType: {e.GetType().Name}\nRaw Response: {rawJson}";
                throw new PipedriveException(errorDetails, e);
            }
        }
        async Task<T> DeserializeTest<T>(Task<HttpResponseMessage> resp)
        {
            using (var stream = await (await resp).Content.ReadAsStreamAsync())
            {

                //Char[] buffer;
                //using (var sr = new StreamReader(stream))
                //{
                //    buffer = new Char[(int)sr.BaseStream.Length];
                //    await sr.ReadAsync(buffer, 0, (int)sr.BaseStream.Length);
                //}
                //String response = new string(buffer);

                try
                {
                    var container = Serializer.Deserialize<ResponseContainer<T>>(new JsonTextReader(new StreamReader(stream)));
                    if (!container.Success)
                        throw new PipedriveException(container.Error);

                    //Replace null by empty list
                    if (container.Data == null && typeof(T).IsGenericType &&
                        typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                        container.Data = Activator.CreateInstance<T>();

                    return container.Data;

                }
                catch (System.Exception e)
                {

                    return default(T);
                }


            }
        }


        public Task<T> Get<T>(string endpoint)
	    {
	        return Deserialize<T>(HttpClient.GetAsync(GetUri(endpoint)));
	    }

        public Task<T> GetTest<T>(string endpoint)
        {
            return DeserializeTest<T>(HttpClient.GetAsync(GetUri(endpoint)));
        }


		async Task<T> Send<T>(string endpoint, HttpMethod method, object data)
		{
			HttpRequestMessage message = null;
			string serializedData = null;

			try {
				var ms = new MemoryStream();
				var jsonWriter = new JsonTextWriter(new StreamWriter(ms));
				Serializer.Serialize(jsonWriter, data);
				jsonWriter.Flush();
				ms.Seek(0, SeekOrigin.Begin);

				// Capture serialized data for debugging
				using (var sr = new StreamReader(ms, Encoding.UTF8, true, 1024, true))
				{
					serializedData = await sr.ReadToEndAsync();
				}
				ms.Seek(0, SeekOrigin.Begin);

				message = new HttpRequestMessage(method, GetUri(endpoint));
				if (data != null)
					message.Content = new StreamContent(ms)
					{
						Headers = {ContentType = new MediaTypeHeaderValue("application/json") {CharSet = "utf-8"}}
					};

				var httpResponse = await HttpClient.SendAsync(message);
				var result = await Deserialize<T>(Task.FromResult(httpResponse));

				return result; // Breakpoint here to inspect result
			}
			catch (PipedriveException pex)
			{
				// PipedriveException already has detailed error info, re-throw
				throw;
			}
			catch (System.Exception e)
			{
				var errorMsg = $"Send Error [{method} {endpoint}]:\nException: {e.Message}\nType: {e.GetType().Name}\nRequest Data: {serializedData}\nStack Trace: {e.StackTrace}";
				throw new PipedriveException(errorMsg);
			}
			finally
			{
				message?.Dispose();
			}
		}

        async Task<T> SendMultipart<T>(string endpoint, HttpMethod method, MultipartFormDataContent form)
        {
            HttpRequestMessage message = null;

            try
            {
                message = new HttpRequestMessage(method, GetUri(endpoint));
                var httpResponse = await HttpClient.PostAsync(message.RequestUri, form);
                var result = await Deserialize<T>(Task.FromResult(httpResponse));

                return result; // Breakpoint here to inspect result
            }
            catch (PipedriveException pex)
            {
                // PipedriveException already has detailed error info, re-throw
                throw;
            }
            catch (System.Exception e)
            {
                var errorMsg = $"SendMultipart Error [{method} {endpoint}]:\nException: {e.Message}\nType: {e.GetType().Name}\nStack Trace: {e.StackTrace}";
                throw new PipedriveException(errorMsg);
            }
            finally
            {
                message?.Dispose();
            }
        }

        public Task Delete(string endpoint)
	    {
	        return Send<object>(endpoint, HttpMethod.Delete, null);
	    }

	    public Task<T> Post<T>(string endpoint, object data)
	    {
            return Send<T>(endpoint, HttpMethod.Post, data);

	    }

        public Task<T> PostMultipart<T>(string endpoint, MultipartFormDataContent data)
        {
            return SendMultipart<T>(endpoint, HttpMethod.Post, data);

        }

		public Task<T> Put<T>(string endpoint, object data)
		{
			return Send<T>(endpoint, HttpMethod.Put, data);
		}

		public async Task<T> Patch<T>(string endpoint, object data)
		{
			var result = await Send<T>(endpoint, new HttpMethod("PATCH"), data);
			return result; // Breakpoint can be set here to inspect result before returning
		}
	}
}
