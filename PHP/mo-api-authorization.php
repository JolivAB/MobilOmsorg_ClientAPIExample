<?php

/*
	This code is an example of how to generate an ApiKey authorization string for Mobil Omsorg's API,
	as a complement to the written documentation and the C# API client.

	The intention is to aid during development and troubleshooting, not as a production-ready client.

	Copyright 2020 Joliv AB

	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

		http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.
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
