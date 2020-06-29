using Newtonsoft.Json;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System.IO;

namespace MOApiClient
{
	/// <summary>
	/// Custom JSON serializer and deserializer, using Newtonsoft (JSON.NET) instead of SimpleJson.
	/// 
	/// Copyright (c) Philipp Wagner. All rights reserved. Licensed under the MIT license.
	/// </summary>
	/// <see cref="https://bytefish.de/blog/restsharp_custom_json_serializer/"/>
	class CustomJsonSerializer : ISerializer, IDeserializer
	{
		public string ContentType { get { return "application/json"; } set { } }
		public string DateFormat { get; set; }
		public string Namespace { get; set; }
		public string RootElement { get; set; }

		private readonly Newtonsoft.Json.JsonSerializer serializer;

		public CustomJsonSerializer()
		{
			serializer = new Newtonsoft.Json.JsonSerializer
			{
				NullValueHandling = NullValueHandling.Ignore,
				DateTimeZoneHandling = DateTimeZoneHandling.Local,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			};
		}

		private static CustomJsonSerializer @default;

		public static CustomJsonSerializer Default
		{
			get
			{
				if (@default == null)
					@default = new CustomJsonSerializer();
				return @default;
			}
		}

		public CustomJsonSerializer(Newtonsoft.Json.JsonSerializer serializer)
		{
			this.serializer = serializer;
		}

		public string Serialize(object obj)
		{
			using (var stringWriter = new StringWriter())
			{
				using (var jsonTextWriter = new JsonTextWriter(stringWriter))
				{
					serializer.Serialize(jsonTextWriter, obj);

					return stringWriter.ToString();
				}
			}
		}

		public T Deserialize<T>(RestSharp.IRestResponse response)
		{
			var content = response.Content;

			using (var stringReader = new StringReader(content))
			using (var jsonTextReader = new JsonTextReader(stringReader))
			{
				return serializer.Deserialize<T>(jsonTextReader);
			}
		}
	}
}
