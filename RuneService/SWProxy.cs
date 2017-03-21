using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace RuneService
{
	class SWProxy
	{
		ProxyServer proxyServer;

		public SWProxy()
		{
			proxyServer = new ProxyServer();
		}
		public void StartProxy()
		{
			proxyServer.BeforeRequest += OnRequest;
			proxyServer.BeforeResponse += OnResponse;

			// TODO: allow rebinding / more endpoints
			var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8080, false);
			proxyServer.AddEndPoint(explicitEndPoint);
			proxyServer.Start();
		}

		public void Stop()
		{
			proxyServer.BeforeRequest -= OnRequest;
			proxyServer.BeforeResponse -= OnResponse;

			proxyServer.Stop();
		}

		private async Task OnRequest(object sender, SessionEventArgs e)
		{
			var requestHeaders = e.WebSession.Request.RequestHeaders;
			
			var method = e.WebSession.Request.Method.ToUpper();
			if ((method == "POST" || method == "PUT" || method == "PATCH"))
			{
				//Get/Set request body bytes
				byte[] bodyBytes = await e.GetRequestBody();
				await e.SetRequestBody(bodyBytes);

				//Get/Set request body as string
				string bodyString = await e.GetRequestBodyAsString();
				await e.SetRequestBodyString(bodyString);

				// Typical requests endpoint:
				//http://summonerswar-gb.qpyou.cn/api/gateway_c2.php
				if (e.WebSession.Request.RequestUri.AbsoluteUri.Contains("summonerswar"))
				{
					Console.WriteLine("Request " + e.WebSession.Request.RequestUri.AbsoluteUri);

					var dec = decryptRequest(bodyString, e.WebSession.Request.RequestUri.AbsolutePath.Contains("_c2.php") ? 2 : 1);

					Console.WriteLine(dec);
				}
			}
		}

		private async Task OnResponse(object sender, SessionEventArgs e)
		{
			if (e.WebSession.Request.Method == "GET" || e.WebSession.Request.Method == "POST")
			{
				if (e.WebSession.Response.ResponseStatusCode == "200")
				{
					if (e.WebSession.Response.ContentType != null && e.WebSession.Response.ContentType.Trim().ToLower().Contains("text/html"))
					{
						byte[] bodyBytes = await e.GetResponseBody();
						await e.SetResponseBody(bodyBytes);

						string body = await e.GetResponseBodyAsString();
						await e.SetResponseBodyString(body);

						if (e.WebSession.Request.RequestUri.AbsoluteUri.Contains("summonerswar"))
						{
							Console.WriteLine("Response " + e.WebSession.Request.RequestUri.AbsoluteUri);

							var dec = decryptResponse(body, e.WebSession.Request.RequestUri.AbsolutePath.Contains("_c2.php") ? 2 : 1);

							Console.WriteLine(dec);
						}
					}
				}
			}
		}

		private static string decryptRequest(string bodyString, int version = 1)
		{
			var inMsg = Convert.FromBase64String(bodyString);
			return Encoding.Default.GetString(decryptMessage(inMsg, version));
		}

		private static string decryptResponse(string bodystring, int version = 1)
		{
			var inMsg = Convert.FromBase64String(bodystring);
			var outMsg = decryptMessage(inMsg, version);
			return zlibDecompressData(outMsg);
		}

		// Use inbuilts for zlib Decomp
		// http://stackoverflow.com/questions/17212964/net-zlib-inflate-with-net-4-5
		private static string zlibDecompressData(byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes, 2, bytes.Length - 2))
			using (var inflater = new DeflateStream(stream, CompressionMode.Decompress))
			using (var streamReader = new StreamReader(inflater))
			{
				return streamReader.ReadToEnd();
			}
		}

		private static byte[] decryptMessage(byte[] bodyString, int version = 1)
		{
			byte[] key;
			switch (version)
			{
				case 1:
					byte[] kek = new byte[] { 13, 122, 63, 6, 125, 13, 39, 120, 60, 124, 47, 36, 45, 113, 122, 18 };
					key = kek.Select(b => (byte)(b ^ 0x1248)).ToArray();
					break;
				case 2:
					byte[] kek2 = new byte[] { 15, 58, 124, 27, 122, 45, 33, 6, 36, 127, 50, 57, 125, 5, 58, 29 };
					key = kek2.Select(b => (byte)(b ^ 0x1248)).ToArray();
					break;
				default:
					throw new NotImplementedException($"Decrypting version {version} is unsupported.");
			}

			return Decrypt(bodyString, key, new byte[16]);
		}
		
		public static byte[] Decrypt(byte[] buff, byte[] key, byte[] iv)
		{
			using (RijndaelManaged rijndael = new RijndaelManaged())
			{
				rijndael.Padding = PaddingMode.PKCS7;
				rijndael.Mode = CipherMode.CBC;
				rijndael.KeySize = key.Length * 8;
				ICryptoTransform decryptor = rijndael.CreateDecryptor(key, iv);
				MemoryStream memoryStream = new MemoryStream(buff);
				CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
				byte[] output = new byte[buff.Length];
				int readBytes = cryptoStream.Read(output, 0, output.Length);
				return output.Take(readBytes).ToArray();
			}
		}
	}
}