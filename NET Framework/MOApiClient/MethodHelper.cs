using RestSharp;
using System;

namespace MOApiClient
{
	static class MethodHelper
	{
		public static Method FromString(string method)
		{
			switch (method.ToUpperInvariant())
			{
				case "GET":
					return Method.GET;
				case "POST":
					return Method.POST;
				case "PUT":
					return Method.PUT;
				case "DELETE":
					return Method.DELETE;
				default:
					throw new ArgumentOutOfRangeException(nameof(method));
			}
		}
	}
}
