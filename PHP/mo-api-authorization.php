<?php

/*
	This code is an example of how to generate an ApiKey authorization string for Mobil Omsorg's API,
	as a complement to the written documentation and the C# API client.

	The intention is to aid during development and troubleshooting, not as a production-ready client.

	Copyright 2020 Joliv AB

	Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
	associated documentation files (the "Software"), to deal in the Software without restriction,
	including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
	subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all copies or substantial
	portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
	NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
	NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES
	OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
	CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

class Request {
	public $method = "";
	public $path = "";
	public $contentType = "";
	public $contentHash = "";
}

class Credentials {
	public $apiKey = "";
	public $apiSecret = "";
	public $nonce = "";
	public $timestamp = 0;
	public $companyCode = "";
	public $signature = "";

	function sign($request) {
		$message = join(" ", [strtoupper($request->method), strtolower($request->path), strtolower($request->contentType), $request->contentHash, $this->timestamp, $this->nonce]);
		print("HMAC message to sign: $message\n");
		$this->signature = base64_encode(hash_hmac("sha256", $message, $this->apiSecret, true));
	}

	function __toString() {
		return join(":", [$this->apiKey, $this->nonce, $this->timestamp, $this->companyCode, $this->signature]);
	}
}

function random_string() {
	return base64_encode(random_bytes(5));
}

function unix_time() {
	return time();
}

$request = new Request();
$request->method = "GET";
$request->path = "/api/BusinessIntelligence/GetActiveGroups";
$request->contentType = "";
$request->contentHash = "";

$credentials = new Credentials();
// Please note that this API key and secret only works in Joliv's internal environment
$credentials->apiKey = "a87a466d-52e4-4dde-8f1d-a6ff3a68209e";
$credentials->apiSecret = "eJFf7vJ4c2C7euY294PCxuSV5jVYK";
$credentials->nonce = random_string();
$credentials->timestamp = unix_time();
$credentials->companyCode = "dev";
$credentials->sign($request);

# This section prints the signature and authorization string
print("HMAC signature:	   $credentials->signature\n");
print("Digest string:		$credentials\n");
print("Authorization header: ApiKey " . base64_encode($credentials) . "\n");
