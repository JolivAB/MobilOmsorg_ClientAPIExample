using System;
using System.Security.Cryptography;
using System.Text;

namespace MOApiClient
{
	static class SecurityHelper
	{
		/// <summary>
		/// Generates a random alphanumeric string
		/// </summary>
		/// <param name="length">Number of random characters to return</param>
		public static string GetRandomAlphanumericString(int length)
		{
			return GetRandomString(length, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890");
		}

		/// <summary>
		/// Generates a random string from given character pool
		/// </summary>
		/// <param name="length">Number of random characters to return</param>
		/// <param name="charPool">Characters to choose from</param>
		public static string GetRandomString(int length, string charPool)
		{
			var res = new StringBuilder();
			using (var rng = new RNGCryptoServiceProvider())
			{
				var uintBuffer = new byte[sizeof(uint)];

				while (length-- > 0)
				{
					rng.GetBytes(uintBuffer);
					uint num = BitConverter.ToUInt32(uintBuffer, 0);
					res.Append(charPool[(int)(num % (uint)charPool.Length)]);
				}
			}
			return res.ToString();
		}

		/// <summary>
		/// Returns a base64-encoded hash-based message authentication code (HMAC),
		/// using the SHA-256 hash algorithm and a secret key.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string HMACSHA256Hash(string message, string key)
		{
			if (message == null || key == null)
				return null;

			using (var hash = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
			{
				return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(message)));
			}
		}

		/// <summary>
		/// Returns a base64-encoded MD5 hash sum
		/// </summary>
		public static string MD5Hash(string message)
		{
			if (message == null)
				return null;

			using (var hash = MD5.Create())
			{
				return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(message)));
			}
		}
	}
}
