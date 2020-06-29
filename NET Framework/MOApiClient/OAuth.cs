using System.Collections.Generic;
using System.Threading.Tasks;

namespace MOApiClient
{
	/// <summary>
	/// Helper class for OAuth operations
	/// </summary>
	public static class OAuth
	{
		public static async Task<TokenResponse> AuthenticatePassword(ApiClient client, string username, string password, string companycode, string mfa_token = null, string device_token = null)
		{
			var fields = new Dictionary<string, string>
			{
				["grant_type"] = "password",
				["username"] = $"{username}@{companycode}",
				["password"] = password,
				["mfa_token"] = mfa_token,
				["device_token"] = device_token
			};
			return await RequestToken(client, fields);
		}

		public static async Task<TokenResponse> AuthenticateCode(ApiClient client, string grant_type, string code, string data = "")
		{
			var fields = new Dictionary<string, string>
			{
				["grant_type"] = grant_type,
				["code"] = code,
				["data"] = data
			};
			return await RequestToken(client, fields);
		}

		public static async Task<TokenResponse> AuthenticateSiths(ApiClient client, string company_code, string collect_uri, string device_token = null)
		{
			var fields = new Dictionary<string, string>
			{
				["grant_type"] = "siths",
				["company_code"] = company_code,
				["collect_uri"] = collect_uri,
				["device_token"] = device_token
			};
			return await RequestToken(client, fields);
		}

		private static async Task<TokenResponse> RequestToken(ApiClient client, IDictionary<string, string> fields)
		{
			return await client.Post<TokenResponse>("OAuth2/Token", body: fields, contentType: "application/x-www-form-urlencoded");
		}

		/// <summary>
		/// DTO for responses from the token endpoint
		/// </summary>
		public class TokenResponse
		{
			/// <summary>
			/// In case an error occured
			/// </summary>
			public string error { get; set; }

			/// <summary>
			/// In case an error occured
			/// </summary>
			public string error_description { get; set; }

			/// <summary>
			/// Access/bearer token, used in subsequent requests
			/// </summary>
			public string access_token { get; set; }

			/// <summary>
			/// Refresh token, used to get new access tokens
			/// </summary>
			public string refresh_token { get; set; }

			/// <summary>
			/// The type of token. Should be "bearer".
			/// </summary>
			public string token_type { get; set; }

			/// <summary>
			/// How many seconds until the access token expires
			/// </summary>
			public int expires_in { get; set; }

			/// <summary>
			/// Whether MFA is required
			/// </summary>
			public bool? mfa_required { get; set; }

			/// <summary>
			/// Whether device authentication is required
			/// </summary>
			public bool? device_auth_required { get; set; }

			/// <summary>
			/// MFA token, if applicable
			/// </summary>
			public string mfa_token { get; set; }

			/// <summary>
			/// Device token, if applicable
			/// </summary>
			public string device_token { get; set; }

			/// <summary>
			/// If user authentication is complete,
			/// which means that the user is 
			/// - authenticated by MFA (if required), and
			/// - device is authenticated (if required).
			/// If false, the user has only authenticated
			/// him or herself partially (during the login process).
			/// </summary>
			public bool auth_complete { get; set; }
		}
	}
}
