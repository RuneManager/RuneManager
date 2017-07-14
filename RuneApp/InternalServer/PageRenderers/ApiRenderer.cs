using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web;

namespace RuneApp.InternalServer
{
	public partial class Master : PageRenderer
	{
		[PageAddressRender("api")]
		public class ApiRenderer : PageRenderer
		{
			public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
			{
				var resp = this.Recurse(req, uri);
				if (resp != null)
					return resp;

				if (req.AcceptTypes?.Contains("application/json") ?? false)
				{
					return new HttpResponseMessage() { Content = new StringContent(
						"{\"endpoints\":[\"runes\",\"monsters\"], \"version\":\"0.4.2.0\",\"baseurl\":\"" + req.UserHostName + "\"}"
						) };
				}

				return returnHtml(new ServedResult[] {
						new ServedResult("link") { contentDic = { { "type", "\"text/css\"" }, { "rel", "\"stylesheet\"" }, { "href", "\"/css/swagger.css\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"http://code.jquery.com/jquery-1.8.0.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/underscore.js/1.3.3/underscore-min.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/handlebars.js/1.0.0/handlebars.min.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/backbone.js/0.9.2/backbone-min.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"/scripts/swagger.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"/scripts/swagger-ui.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"/scripts/swagger-client.js\"" } } },
						/*
	spec: JSON.parse('" + "{ \"swagger\": \"2.0\", \"info\": { \"description\": \"This is a sample server Petstore server.  You can find out more about Swagger at [http://swagger.io](http://swagger.io) or on [irc.freenode.net, #swagger](http://swagger.io/irc/).  For this sample, you can use the api key `special-key` to test the authorization filters.\", \"version\": \"1.0.0\", \"title\": \"Swagger Petstore\", \"termsOfService\": \"http://swagger.io/terms/\", \"contact\": { \"email\": \"apiteam@swagger.io\" }, \"license\": { \"name\": \"Apache 2.0\", \"url\": \"http://www.apache.org/licenses/LICENSE-2.0.html\" } }, \"host\": \"petstore.swagger.io\", \"basePath\": \"/v2\", \"tags\": [{ \"name\": \"pet\", \"description\": \"Everything about your Pets\", \"externalDocs\": { \"description\": \"Find out more\", \"url\": \"http://swagger.io\" } }, { \"name\": \"store\", \"description\": \"Access to Petstore orders\" }, { \"name\": \"user\", \"description\": \"Operations about user\", \"externalDocs\": { \"description\": \"Find out more about our store\", \"url\": \"http://swagger.io\" } }], \"schemes\": [\"http\"], \"paths\": { \"/pet\": { \"post\": { \"tags\": [\"pet\"], \"summary\": \"Add a new pet to the store\", \"description\": \"\", \"operationId\": \"addPet\", \"consumes\": [\"application/json\", \"application/xml\"], \"produces\": [\"application/xml\", \"application/json\"], \"parameters\": [{ \"in\": \"body\", \"name\": \"body\", \"description\": \"Pet object that needs to be added to the store\", \"required\": true, \"schema\": { \"$ref\": \"#/definitions/Pet\" } }], \"responses\": { \"405\": { \"description\": \"Invalid input\" } }, \"security\": [{ \"petstore_auth\": [\"write:pets\", \"read:pets\"] }] }, \"put\": { \"tags\": [\"pet\"], \"summary\": \"Update an existing pet\", \"description\": \"\", \"operationId\": \"updatePet\", \"consumes\": [\"application/json\", \"application/xml\"], \"produces\": [\"application/xml\", \"application/json\"], \"parameters\": [{ \"in\": \"body\", \"name\": \"body\", \"description\": \"Pet object that needs to be added to the store\", \"required\": true, \"schema\": { \"$ref\": \"#/definitions/Pet\" } }], \"responses\": { \"400\": { \"description\": \"Invalid ID supplied\" }, \"404\": { \"description\": \"Pet not found\" }, \"405\": { \"description\": \"Validation exception\" } }, \"security\": [{ \"petstore_auth\": [\"write:pets\", \"read:pets\"] }] } }, \"/user/{username}\": { \"put\": { \"tags\": [\"user\"], \"summary\": \"Updated user\", \"description\": \"This can only be done by the logged in user.\", \"operationId\": \"updateUser\", \"produces\": [\"application/xml\", \"application/json\"], \"parameters\": [{ \"name\": \"username\", \"in\": \"path\", \"description\": \"name that need to be updated\", \"required\": true, \"type\": \"string\" }, { \"in\": \"body\", \"name\": \"body\", \"description\": \"Updated user object\", \"required\": true, \"schema\": { \"$ref\": \"#/definitions/User\" } }], \"responses\": { \"400\": { \"description\": \"Invalid user supplied\" }, \"404\": { \"description\": \"User not found\" } } }, \"delete\": { \"tags\": [\"user\"], \"summary\": \"Delete user\", \"description\": \"This can only be done by the logged in user.\", \"operationId\": \"deleteUser\", \"produces\": [\"application/xml\", \"application/json\"], \"parameters\": [{ \"name\": \"username\", \"in\": \"path\", \"description\": \"The name that needs to be deleted\", \"required\": true, \"type\": \"string\" }], \"responses\": { \"400\": { \"description\": \"Invalid username supplied\" }, \"404\": { \"description\": \"User not found\" } } } } }, \"securityDefinitions\": { \"petstore_auth\": { \"type\": \"oauth2\", \"authorizationUrl\": \"http://petstore.swagger.io/oauth/dialog\", \"flow\": \"implicit\", \"scopes\": { \"write:pets\": \"modify pets in your account\", \"read:pets\": \"read your pets\" } }, \"api_key\": { \"type\": \"apiKey\", \"name\": \"api_key\", \"in\": \"header\" } }, \"definitions\": { \"Pet\": { \"type\": \"object\", \"required\": [\"name\", \"photoUrls\"], \"properties\": { \"id\": { \"type\": \"integer\", \"format\": \"int64\" }, \"category\": { \"$ref\": \"#/definitions/Category\" }, \"name\": { \"type\": \"string\", \"example\": \"doggie\" }, \"photoUrls\": { \"type\": \"array\", \"xml\": { \"name\": \"photoUrl\", \"wrapped\": true }, \"items\": { \"type\": \"string\" } }, \"tags\": { \"type\": \"array\", \"xml\": { \"name\": \"tag\", \"wrapped\": true }, \"items\": { \"$ref\": \"#/definitions/Tag\" } }, \"status\": { \"type\": \"string\", \"description\": \"pet status in the store\", \"enum\": [\"available\", \"pending\", \"sold\"] } }, \"xml\": { \"name\": \"Pet\" } } }, \"externalDocs\": { \"description\": \"Find out more about Swagger\", \"url\": \"http://swagger.io\" } }" + @"'),
						 * */
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" } }, contentList = { @"setTimeout(function() {
window.swaggerUi = new SwaggerUi({
	url: ""http://" + req.UserHostName + @"/api/swagger.json"",
	dom_id: ""swagger-ui-container"",
	supportedSubmitMethods: [""get"", ""post"", ""put"", ""delete""],
	useJQuery: true,
	onComplete: function(swaggerApi, swaggerUi) {
	},
	onFailure: function(data) {
		console.log(""Unable to Load SwaggerUI"");
	},
	docExpansion: ""list"",
	sorter : ""alpha""
});

window.swaggerUi.load();
document.getElementById(""out"").className = ""swagger-section"";
document.getElementById(""out"").innerHTML = ""<div class=\""swagger-ui-wrap\"" id=\""swagger-ui-container\"">"" + document.getElementById(""out"").innerHTML + ""</div>"";
},10);" } },
					},
					new ServedResult("div") { contentDic = { { "id", "\"out\"" } }, contentList = { " " } });
			}

			[PageAddressRender("swagger.json")]
			public class SwaggerJsRenderer : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StringContent(new ServedResult()
						{
							contentDic =
						{
							{ "swagger", "2.0" },
							{ "info", new ServedResult()
								{
									contentDic =
									{
										{ "description", "RuneManager sample Swag" },
										{ "version", Assembly.GetExecutingAssembly().ImageRuntimeVersion },
										{ "title", "RuneManager API" },
										{ "contact", "skibisky@outlook.com.au" }
									}
								}
							},
							{ "host", req.UserHostName },
							{ "schemes", new ServedResult(true) { contentList = { "http" } } },
							{ "paths", new ServedResult()
								{
									contentDic =
									{
										{ "/pet", new ServedResult()
											{
											contentDic =
												{ { "post", new ServedResult()
												{
												contentDic =
													{
														{"summary", "Add a new pet" }
													}
												} }
												}
											}
										}
									}
								}
							}
						}
						}.ToJson())
					};
				}
			}


			[PageAddressRender("monsters")]
			public class MonstersRenderer : PageRenderer
			{
				[HttpMethod("POST")]
				public HttpResponseMessage PostMethod(HttpListenerRequest req)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{POST}") };
				}

				[HttpMethod("PUT")]
				public HttpResponseMessage PutMethod(HttpListenerRequest req)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{POST}") };
				}

				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					switch (req.HttpMethod)
					{
						case "PUT":
							return PutMethod(req);

						case "POST":
							return PostMethod(req);

						case "GET":
							return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{GET}") };
					}
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{FAIL}") };
				}
			}

			[PageAddressRender("runes")]
			public class RunesRenderer : PageRenderer
			{
				[HttpMethod("POST")]
				public HttpResponseMessage PostMethod(HttpListenerRequest req)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{POST}") };
				}

				[HttpMethod("PUT")]
				public HttpResponseMessage PutMethod(HttpListenerRequest req)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{POST}") };
				}

				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					switch (req.HttpMethod)
					{
						case "PUT":
							return PutMethod(req);

						case "POST":
							return PostMethod(req);
						case "GET":
							return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{GET}") };
					}
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{FAIL}") };
				}
			}
		}

	}
}
