using System;

namespace MOApiClient
{
	[Serializable]
	public class MOApiException : Exception
	{
		public MOApiException() : base() { }

		public MOApiException(string message) : base(message) { }

		public MOApiException(string message, Exception inner) : base(message, inner) { }

		protected MOApiException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class MOApiHttpException : MOApiException
	{
		public int StatusCode { get; private set; }

		public MOApiHttpException() : base() { }

		public MOApiHttpException(string message, int statusCode = 0) 
			: base(message)
		{
			StatusCode = statusCode;
		}

		public MOApiHttpException(string message, int statusCode, Exception inner) 
			: base(message, inner)
		{
			StatusCode = statusCode;
		}

		protected MOApiHttpException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class MOApiNetworkException : MOApiException
	{
		public MOApiNetworkException() : base() { }

		public MOApiNetworkException(string message) : base(message) { }

		public MOApiNetworkException(string message, Exception inner) : base(message, inner) { }

		protected MOApiNetworkException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class MOApiParsingException : MOApiException
	{
		public MOApiParsingException() : base() { }

		public MOApiParsingException(string message) : base(message) { }

		public MOApiParsingException(string message, Exception inner) : base(message, inner) { }

		protected MOApiParsingException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
