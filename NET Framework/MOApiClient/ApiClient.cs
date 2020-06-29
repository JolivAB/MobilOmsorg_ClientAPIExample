using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace MOApiClient
{
	public class ApiClient
	{
		/// <summary>
		/// The base URI of the API to be called. Should include "/api" root path element.
		/// </summary>
		public Uri BaseUri { get; private set; }

		/// <summary>
		/// The API consumer key that is associated with the application.
		/// </summary>
		public string ApiKey { get; set; }

		/// <summary>
		/// The API consumer secret that is associated with the application.
		/// </summary>
		public string ApiSecret { get; set; }

		/// <summary>
		/// The user's access token for authorization. Overrides API consumer key and secret, if used.
		/// </summary>
		public string AccessToken { get; set; }

		/// <summary>
		/// The device's token for authentication (optional).
		/// </summary>
		public string DeviceToken { get; set; }

		/// <summary>
		/// The company code to which we make the calls (optional).
		/// </summary>
		public string CompanyCode { get; set; }

		/// <summary>
		/// An impersonated user, meaning the end user performing the operations (optional).
		/// </summary>
		public int ImpersonatedUserId { get; set; }

		/// <summary>
		/// The language/culture that the application uses. Used for error messages etc.
		/// </summary>
		public string AcceptLanguage { get; set; }

		/// <summary>
		/// The end user's IP address. Used for blacklisting and whitelisting devices.
		/// </summary>
		public string UserIPAddress { get; set; }

		/// <summary>
		/// The user-agent string to pass to the API.
		/// </summary>
		public string UserAgent { get; set; } = "MOApiClient/1.x RestSharp";

		/// <summary>
		/// Timeout for requests to the API, in milliseconds
		/// </summary>
		public int Timeout { get; set; } = 30000;

		private const string JsonMimeType = "application/json";

		public ApiClient(Uri baseUri)
		{
			BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
		}

		/// <summary>
		/// Send a GET request to the MO API.
		/// </summary>
		/// <typeparam name="T">Return object type</typeparam>
		/// <param name="uri">Relative URL for API resource</param>
		/// <param name="parameters">Dynamic object with parameters</param>
		/// <returns>Object returned from API</returns>
		public async Task<T> Get<T>(string uri, dynamic parameters = null)
		{
			return await Request<T>("GET", uri, parameters);
		}

		/// <summary>
		/// Send a GET request to the MO API.
		/// </summary>
		/// <param name="uri">Relative URL for API resource</param>
		/// <param name="parameters">Dynamic object with parameters</param>
		/// <returns>String returned from API</returns>
		public async Task<string> Get(string uri, dynamic parameters = null)
		{
			return await Request("GET", uri, parameters);
		}

		/// <summary>
		/// Send a PUT request to the MO API, and return data.
		/// </summary>
		/// <typeparam name="T">Return object type</typeparam>
		/// <param name="uri">Relative URL for API resource</param>
		/// <param name="parameters">Dynamic object with parameters</param>
		/// <param name="body">Object that is sent as request body</param>
		/// <param name="contentType">The content type of the body (optional, default JSON)</param>
		/// <returns>Object returned from API</returns>
		public async Task<T> Put<T>(string uri, dynamic parameters = null, object body = null, string contentType = JsonMimeType)
		{
			return await Request<T>("PUT", uri, parameters, body, contentType);
		}

		/// <summary>
		/// Send a PUT request to the MO API.
		/// </summary>
		/// <param name="uri">Relative URL for API resource</param>
		/// <param name="parameters">Dynamic object with parameters</param>
		/// <param name="body">Object that is sent as request body</param>
		/// <param name="contentType">The content type of the body (optional, default JSON)</param>
		public async Task Put(string uri, dynamic parameters = null, object body = null, string contentType = JsonMimeType)
		{
			await Request("PUT", uri, parameters, body, contentType);
		}

		/// <summary>
		/// Send a POST request to the MO API, and return data.
		/// </summary>
		/// <typeparam name="T">Return object type</typeparam>
		/// <param name="uri">Relative URL for API resource</param>
		/// <param name="parameters">Dynamic object with parameters</param>
		/// <param name="body">Object that is sent as request body</param>
		/// <param name="contentType">The content type of the body (optional, default JSON)</param>
		/// <returns>Object returned from API</returns>
		public async Task<T> Post<T>(string uri, dynamic parameters = null, object body = null, string contentType = JsonMimeType)
		{
			return await Request<T>("POST", uri, parameters, body, contentType);
		}

		/// <summary>
		/// Send a POST request to the MO API.
		/// </summary>
		/// <param name="uri">Relative URL for API resource</param>
		/// <param name="parameters">Dynamic object with parameters</param>
		/// <param name="body">Object that is sent as request body</param>
		/// <param name="contentType">The content type of the body (optional, default JSON)</param>
		public async Task Post(string uri, dynamic parameters = null, object body = null, string contentType = JsonMimeType)
		{
			await Request("POST", uri, parameters, body, contentType);
		}

		/// <summary>
		/// Call API with no deserialization, return response as string.
		/// </summary>
		/// <param name="method">The method to use</param>
		/// <param name="uri">Relative URL for API resource</param>
		/// <param name="parameters">Dynamic object with parameters</param>
		/// <param name="body">Object that is sent as request body</param>
		/// <param name="contentType">The content type of the body (optional, default JSON)</param>
		/// <returns>String returned from API</returns>
		public async Task<string> Request(string method, string uri, dynamic parameters = null, object body = null, string contentType = JsonMimeType)
		{
			return await Execute((IRestRequest)MakeRequestObject(MethodHelper.FromString(method), uri, parameters, body, contentType));
		}

		public async Task<T> Request<T>(string method, string uri, dynamic parameters = null, object body = null, string contentType = JsonMimeType)
		{
			return await Execute<T>(MakeRequestObject(MethodHelper.FromString(method), uri, parameters, body, contentType));
		}

		public string RequestSync(string method, string url, dynamic parameters = null, object body = null, string contentType = JsonMimeType)
		{
			return ExecuteSync(MakeRequestObject(MethodHelper.FromString(method), url, parameters, body, contentType));
		}

		public T RequestSync<T>(string method, string url, dynamic parameters = null, object body = null, string contentType = JsonMimeType)
		{
			return ExecuteSync<T>(MakeRequestObject(MethodHelper.FromString(method), url, parameters, body, contentType));
		}

		private RestRequest MakeRequestObject(Method method, string requestUri, dynamic parameters = null, object body = null, string contentType = JsonMimeType)
		{
			if (parameters != null)
			{
				var parameterPairs = new List<string>();
				foreach (var prop in parameters.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
				{
					var value = prop.GetValue(parameters, null);
					if (value is IEnumerable && !(value is string))
					{
						// "Arrays" are represented in query strings as repeated parameters,
						// for example "resources=1&resources=2&resources=3"
						foreach (var each in value)
						{
							parameterPairs.Add(string.Format("{0}={1}", prop.Name, each));
						}
					}
					else
					{
						parameterPairs.Add(string.Format("{0}={1}", prop.Name, value));
					}
				}
				requestUri += "?" + string.Join("&", parameterPairs);
			}

			var request = new RestRequest(requestUri, method);

			if (body != null)
			{
				if (body is string)
				{
					request.AddParameter(contentType, body, ParameterType.RequestBody);
				}
				else if (contentType == "application/x-www-form-urlencoded" && body is IDictionary<string, string>)
				{
					foreach (var each in (IDictionary<string, string>)body)
					{
						request.AddParameter(each.Key, each.Value, ParameterType.GetOrPost);
					}
				}
				else
				{
					if (contentType != JsonMimeType)
						throw new ArgumentOutOfRangeException(nameof(contentType));

					request.RequestFormat = DataFormat.Json;
					request.JsonSerializer = CustomJsonSerializer.Default;
					request.AddBody(body);
				}
			}

			if (!string.IsNullOrEmpty(AcceptLanguage))
				request.AddHeader("Accept-Language", AcceptLanguage);

			if (!string.IsNullOrEmpty(UserIPAddress))
				request.AddHeader("X-Forwarded-For", UserIPAddress);

			return request;
		}

		/// <summary>
		/// The last response from the API
		/// </summary>
		public IRestResponse LastResponse { get; private set; }

		private async Task<T> Execute<T>(IRestRequest request)
		{
			var client = MakeClient();
			LastResponse = await GetResponse(client, request);
			VerifyResponse(LastResponse);
			return DeserializedResponse<T>(LastResponse);
		}

		private async Task<string> Execute(IRestRequest request)
		{
			var client = MakeClient();
			LastResponse = await GetResponse(client, request);
			VerifyResponse(LastResponse);
			return LastResponse.Content;
		}

		private string ExecuteSync(IRestRequest request)
		{
			var client = MakeClient();
			LastResponse = GetResponseSync(client, request);
			VerifyResponse(LastResponse);
			return LastResponse.Content;
		}

		private T ExecuteSync<T>(IRestRequest request)
		{
			var client = MakeClient();
			LastResponse = GetResponseSync(client, request);
			VerifyResponse(LastResponse);
			return DeserializedResponse<T>(LastResponse);
		}

		private IRestClient MakeClient()
		{
			return new RestClient(BaseUri)
			{
				UserAgent = UserAgent,
				Timeout = Timeout,
				Authenticator = new CustomAuthenticator()
				{
					ApiKey = ApiKey,
					ApiSecret = ApiSecret,
					AccessToken = AccessToken,
					DeviceToken = DeviceToken,
					CompanyCode = CompanyCode,
					ImpersonatedUserId = ImpersonatedUserId
				}
			};
		}

		private async Task<IRestResponse> GetResponse(IRestClient client, IRestRequest request)
		{
			try
			{
				return await client.ExecuteTaskAsync(request);
			}
			catch (Exception exception)
			{
				return FakeExceptionResponse(exception);
			}
		}

		private IRestResponse GetResponseSync(IRestClient client, IRestRequest request)
		{
			try
			{
				return client.Execute(request);
			}
			catch (Exception exception)
			{
				return FakeExceptionResponse(exception);
			}
		}

		private static readonly IEnumerable<HttpStatusCode> SuccessStatusCodes = new HttpStatusCode[] {
			HttpStatusCode.OK,
			HttpStatusCode.Created,
			HttpStatusCode.Accepted,
			HttpStatusCode.NoContent
		};

		private static void VerifyResponse(IRestResponse response)
		{
			if (response.StatusCode > 0 && !SuccessStatusCodes.Contains(response.StatusCode))
			{
				var message = $"Error retrieving response. Request returned status {response.StatusCode}.";
				throw new MOApiHttpException(message, (int)response.StatusCode);
			}
			else if (response.ErrorException != null)
			{
				var exception = response.ErrorException.GetBaseException();
				var message = $"Error retrieving response: {exception.Message}.";

				if (exception is WebException || exception is System.Net.Sockets.SocketException)
				{
					throw new MOApiNetworkException(message, exception);
				}
				else
				{
					throw new MOApiException(message, exception);
				}
			}
			else if (!string.IsNullOrEmpty(response.Content) && !response.ContentType.StartsWith(JsonMimeType))
			{
				var message = $"Error retrieving response. Request returned with unknown encoding {response.ContentEncoding}.";
				throw new MOApiException(message);
			}
		}

		/// <summary>
		/// Convert an exception to a response error instead
		/// </summary>
		/// <param name="exception">The exception thrown</param>
		/// <returns></returns>
		private static IRestResponse FakeExceptionResponse(Exception exception)
		{
			return new RestResponse()
			{
				ResponseStatus = ResponseStatus.Error,
				ErrorMessage = exception.GetBaseException().Message,
				ErrorException = exception.GetBaseException()
			};
		}

		/// <summary>
		/// Deserialize JSON content of response
		/// </summary>
		/// <typeparam name="T">Type, usually a DTO class</typeparam>
		/// <param name="response">The API response object</param>
		/// <returns>Object returned from API</returns>
		private static T DeserializedResponse<T>(IRestResponse response)
		{
			if (string.IsNullOrEmpty(response.Content))
			{
				return default(T);
			}
			else
			{
				try
				{
					return CustomJsonSerializer.Default.Deserialize<T>(response);
				}
				catch (JsonSerializationException e)
				{
					var exception = e.GetBaseException();
					var message = $"Error parsing response: {exception.Message}.";
					throw new MOApiParsingException(message, exception);
				}
			}
		}
	}
}
