#!/usr/bin/env python
# -*- coding: utf-8 -*-

"""
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
"""

import hmac
import hashlib
import base64
import time
import random
import string

class Request:
	"""
	Contains information about the API request to be made.

	Attributes
	----------
	method : str
		The HTTP method for the request (e.g. GET, POST or PUT)
	path : str
		The path for the API request (starting with "/api/")
	contentType : str
		The content-type of the body of the request (empty if no body)
	contentHash : str
		The MD5 hash of the body of the request (empty if no body)
	"""

	method = ""
	path = ""
	contentType = ""
	contentHash = ""

class Credentials:
	"""
	Contains information to generate an authorization string.

	Attributes
	----------
	apiKey : str
		The API key you have been given to use against Mobil Omsorg
	apiSecret : str
		The API secret you have been given to use against Mobil Omsorg
	nonce : str
		A random string of at least 4 characters, unique for every request
	timestamp : int
		Unix time when request is created
	companyCode : str
		The company code for the customer
	signature : str
		Generated HMAC
	"""

	apiKey = ""
	apiSecret = ""
	nonce = ""
	timestamp = 0
	companyCode = ""
	signature = ""

	def sign(self, request: Request) -> str:
		message = " ".join([request.method.upper(), request.path.lower(), request.contentType.lower(), request.contentHash, str(self.timestamp), self.nonce])
		print("HMAC message to sign: " + message)
		self.signature = to_base64(
			hmac.new(
				bytes(self.apiSecret, "utf-8"),
				bytes(message, "utf-8"),
				digestmod=hashlib.sha256
			).digest()
		)

	def __str__(self) -> str:
		return ":".join([self.apiKey, self.nonce, str(self.timestamp), self.companyCode, self.signature])

def to_base64(s) -> str:
	"""Converts str or bytes to Base64 string"""
	if isinstance(s, bytes):
		return base64.b64encode(s).decode("utf-8")
	else:
		return base64.b64encode(bytes(str(s), "utf-8")).decode("utf-8")

def random_string() -> str:
	alphabet = string.ascii_letters + string.digits
	return "".join(random.SystemRandom().choice(alphabet) for _ in range(8))

def unix_time() -> int:
	return int(time.time())

request = Request()
request.method = "GET"
request.path = "/api/BusinessIntelligence/GetActiveGroups"
request.contentType = ""
request.contentHash = ""

credentials = Credentials()
# Please note that this API key and secret only works in Joliv's internal environment
credentials.apiKey = "a87a466d-52e4-4dde-8f1d-a6ff3a68209e"
credentials.apiSecret = "eJFf7vJ4c2C7euY294PCxuSV5jVYK"
credentials.nonce = random_string()
credentials.timestamp = unix_time()
credentials.companyCode = "dev"
credentials.sign(request)

# This section prints the signature and authorization string
print("HMAC signature:       " + credentials.signature)
print("Digest string:        " + str(credentials))
print("Authorization header: ApiKey " + to_base64(str(credentials)))
