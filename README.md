# MobilOmsorg_ClientAPIExample
Example projects for connecting to Mobil Omsorg API.

# Background and Scope

This document concerns authentication of an external system to Mobil Omsorg's API, using API keys.

The external system is here called "Acme", and Mobil Omsorg is called "MO".

# Key Concepts

## API Keys

Authentication relies on two pieces of data that is shared between MO and Acme,

1. an API key that _identifies_ Acme, and
2. an API secret that _authenticates_ Acme.

These strings must be stored in a responsible way, and if there is any risk of them
getting into the wrong hands, Acme must notify Joliv, who will then generate a new set of
strings.

Joliv generates and sends these strings to Acme, along with the URL for the testing and
production environments.

## REST

MO's API is RESTful and stateless, so each request must be authenticated.

## JSON

MO accepts data in the request body as JSON, with the content-type "application/json".

MO returns data as JSON.

## Company Code

In MO, each customer database is identified by a "company code". A company code usually
identifies a customer organisation or site office.

The company code is a required parameter for practically all API requests.

# Authenticating an API Request

Each request to MO must contain an Authorization header with the "ApiKey" type, and possibly a Content-MD5 header, like so,

```http
GET /api/something
Authorization: ApiKey <credentials>
Content-MD5: <digest>
```

"Credentials" is a Base64-encoded string containing these fields in UTF-8, joined by colons (`:`):

1. API key
2. Random nonce (short, random string)
3. Unix time of request
4. Company code
5. Signature of the request

The signature (#5) is in turn a Base64-encoded SHA-256 HMAC hash of these fields joined
with a space, and signed with the API secret:

1. Method in uppercase (e.g. "GET" or "POST")
2. Request path in lowercase (e.g. "/api/groups/get")
3. Content type in lowercase (e.g. "application/json") -- use empty string if
   the request body is empty
4. Base64-encoded MD5 hash of the request body in UTF-8 -- use empty string if
   the request body is empty
5. Timestamp (same as above)
6. Nonce (same as above)

Also, if the request contains a body (e.g. POST or PUT data), the request must
include a `Content-MD5` header, with the same MD5 hash as #4 above.

## Implementation in C#

This is a basic example implementation in C#:

```csharp
string md5Digest = SignContent(body);

string timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
string nonce = GetRandomNonce();

string signature = SignRequest(method, uri, contentType, md5Digest, apiSecret, timestamp, nonce);

string[] tokens = { apiKey, nonce, timestamp, companyCode, signature };
string credentials = string.Join(":", tokens);
string encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
string authorization = $"ApiKey {encodedCredentials}";

// TODO: Add headers to request
// Content-Type: {contentType}
// Content-MD5: {md5Digest}
// Authorization: {authorization}
```

The two methods used above:

```csharp
private static string SignContent(string content)
{
  if (string.IsNullOrEmpty(content))
    return null;

  using (MD5 hash = MD5.Create())
  {
    return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(content)));
  }
}

private static string SignRequest(string method, Uri uri, string contentType, string contentHash, string secret, string timestamp, string nonce)
{
  string path = uri.AbsolutePath;
  string[] tokens = { method.ToUpperInvariant(), path.ToLowerInvariant(), contentType.ToLowerInvariant(), contentHash, timestamp, nonce };
  string message = string.Join(" ", tokens);

  using (HMACSHA256 hash = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
  {
    return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(message)));
  }
}
```

The implementation of `GetRandomNonce()` is not included.
