using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Linq;
using System.Text;

namespace MOApiClient
{
	/// <summary>
	/// A variant of HttpBasicAuthenticator, but
	/// we cannot inherit due to _authHeader being private.
	/// </summary>
	class CustomAuthenticator : IAuthenticator
	{
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
		/// The company code to which we make the calls (optional).
		/// </summary>
		public string CompanyCode { get; set; }

		/// <summary>
		/// The device's token for authentication (optional).
		/// </summary>
		public string DeviceToken { get; set; }

		/// <summary>
		/// An impersonated user, meaning the end user performing the operations (optional).
		/// </summary>
		public int ImpersonatedUserId { get; set; }

		public void Authenticate(IRestClient client, IRestRequest request)
		{
			// Is there already an Authorization header in the request?
			if (request.Parameters.Any(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
				return;

			if (!string.IsNullOrEmpty(AccessToken))
			{
				if (AccessToken.StartsWith("Basic ") || AccessToken.StartsWith("Bearer "))
					// Token type is included in the string, don't add a prefix
					AddAuthorizationHeader(request, AccessToken);
				else
					// Use OAuth bearer tokens by default
					AddAuthorizationHeader(request, $"Bearer {AccessToken}");
			}
			else if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiSecret))
			{
				AddAuthorizationHeader(request, MakeAuthString(client, request));
			}
		}

		private void AddAuthorizationHeader(IRestRequest request, string value)
		{
			request.AddParameter("Authorization", value, ParameterType.HttpHeader);
		}

		private string MakeAuthString(IRestClient client, IRestRequest request)
		{
			var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
			var nonce = SecurityHelper.GetRandomAlphanumericString(12);
			var signature = SignRequest(client, request, ApiSecret, timestamp, nonce);
			var tokens = new string[] { ApiKey, nonce, timestamp.ToString(), CompanyCode, signature, DeviceToken, ImpersonatedUserId.ToString() };
			return "ApiKey " + Base64Encode(string.Join(":", tokens));
		}

		private static string SignRequest(IRestClient client, IRestRequest request, string secret, long timestamp, string nonce)
		{
			var method = request.Method.ToString();
			var path = client.BuildUri(request).AbsolutePath;
			var contentType = "";
			var contentHash = "";

			var body = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
			if (body != null)
			{
				contentType = body.Name;
				contentHash = SecurityHelper.MD5Hash(body.Value as string);
				request.AddHeader("Content-MD5", contentHash);
			}

			var message = $"{method.ToUpperInvariant()} {path.ToLowerInvariant()} {contentType.ToLowerInvariant()} {contentHash} {timestamp} {nonce}";
			return SecurityHelper.HMACSHA256Hash(message, secret);
		}

		private static string Base64Encode(string plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
	}
}
