using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using RuneOptim.swar;

namespace RuneApp.InternalServer {
    public partial class Master : PageRenderer {
        [PageAddressRender("api")]
        public class ApiRenderer : PageRenderer {
            public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                var resp = this.Recurse(req, uri);
                if (resp != null)
                    return resp;

                if (req.AcceptTypes?.Contains("application/json") ?? false) {
                    return new SwaggerJsRenderer().Render(req, uri);
                }
                // yaml?

                return returnHtml(
                    new ServedResult[] {
                        new ServedResult("link") { contentDic = { { "type", "\"text/css\"" }, { "rel", "\"stylesheet\"" }, { "href", "\"/css/reset.css\"" } } },
                        new ServedResult("link") { contentDic = { { "type", "\"text/css\"" }, { "rel", "\"stylesheet\"" }, { "href", "\"https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/2.2.4/css/screen.css\"" } } },
                        //new ServedResult("link") { contentDic = { { "type", "\"text/css\"" }, { "rel", "\"stylesheet\"" }, { "href", "\"/css/swagger.css\"" } } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"object-assign-pollyfill.js\"" } } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"http://code.jquery.com/jquery-1.8.0.js\"" } } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"jquery.slideto.min.js\"" } } },
                        //new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"jquery.wiggle.min.js\"" } } },  // Do we need this?
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"jquery.ba-bbq.min.js\"" } } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/handlebars.js/4.0.5/handlebars.js\"" } } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/lodash-compat/3.10.1/lodash.min.js\"" } } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/backbone.js/1.1.2/backbone-min.js\"" } } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" } }, contentList = { @"
// From http://stackoverflow.com/a/19431552
// Compatibility override - Backbone 1.1 got rid of the 'options' binding
// automatically to views in the constructor - we need to keep that.
Backbone.View = (function(View) {
    return View.extend({
        constructor: function(options) {
            this.options = options || {};
            View.apply(this, arguments);
        }
    });
})(Backbone.View);
" } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/2.2.4/swagger-ui.js\"" } } },
                        //new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"highlight.9.1.0.pack.js\"" } } },
                        //new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"highlight.9.1.0.pack_extended.js\"" } } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/json-editor/0.7.22/jsoneditor.js\"" } } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/marked/0.3.6/marked.js\"" } } },
                        //new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"swagger-oauth.js\"" } } },
                        new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" } }, contentList = { @"
//setTimeout(function() {
$(function () {
window.swaggerUi = new SwaggerUi({
    url: 'http://" + req.UserHostName + @"/api/swagger.json',
    dom_id: 'swagger-ui-container',
    //booleanValues: ['true', 'false'],
    supportedSubmitMethods: ['get', 'post', 'put', 'delete', 'patch'],
    useJQuery: true,
    onComplete: function(swaggerApi, swaggerUi) {
        console.log('Swaggered');
    },
    onFailure: function(data) {
        console.log('Unable to Load SwaggerUI');
    },
    docExpansion: 'list',
    jsonEditor: false,
    defaultModelRendering: 'schema',
    showRequestHeaders: false,
    //oauth2RedirectUrl: window.location.href.replace('index', 'o2c-html').split('#')[0],
    //sorter : 'alpha'
    highlightSizeThreshold: 1
});
//window.swaggerUi.options.validatorUrl = '';
window.swaggerUi.load();
//},10);
function log() {
    if ('console' in window) {
        console.log.apply(console, arguments);
    }
}
});
" } },
                    },
                    new ServedResult("div") {
                        contentDic = {
                            { "class", "\"swagger-section\"" }
                        },
                        contentList = {
                            new ServedResult("div") { contentDic = { { "id", "\"message-bar\"" }, { "class", "\"swagger-ui-wrap\"" } }, contentList = { "&nbsp;" }},
                            new ServedResult("div") { contentDic = { { "id", "\"swagger-ui-container\"" }, { "class", "\"swagger-ui-wrap\"" } }, contentList = { "" }}
                        }
                    }
                );
            }

            [PageAddressRender("swagger.json")]
            public class SwaggerJsRenderer : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    return new HttpResponseMessage(HttpStatusCode.OK) {
                        Content = new StringContent(new ServedResult() {
                            contentDic =
                        {
                            { "swagger", "2.0" },
                            { "info", new ServedResult()
                                {
                                    contentDic =
                                    {
                                        { "description", "RuneManager API Swagger documentation" },
                                        { "version", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion },
                                        { "title", "RuneManager API" },
                                        { "contact", new ServedResult() {
                                            contentDic = {
                                                { "url", "https://github.com/Skibisky/RuneManager" },
                                                { "email", "skibisky@outlook.com.au" }
                                            }
                                        } }
                                    }
                                }
                            },
                            { "host", req.UserHostName },
                            { "basePath", "/" },
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
                                                                { "summary", "Add a new pet" },
                                                                // TODO
                                                            }
                                                        }
                                                    }
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
            public class MonstersRenderer : PageRenderer {
                [HttpMethod("POST")]
                public HttpResponseMessage PostMethod(HttpListenerRequest req, string[] uri) {
                    Monster mon = null;
                    ulong id = 0;
                    if (uri.Length > 0) {
                        ulong.TryParse(uri[0], out id);
                    }
                    if (req.QueryString.Count == 0) {
                        using (var str = new StreamReader(req.InputStream)) {
                            var raw = str.ReadToEnd();
                            mon = JsonConvert.DeserializeObject<Monster>(raw);
                        }
                        if (id == 0)
                            id = mon.Id;

                        if (uri.Length == 0 || Program.data.GetMonster(id) == null) {
                            Program.AddMonster(mon);
                            return new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent(new ServedResult("POST") { contentDic = { { "created", id.ToString() } } }.ToJson()) };
                        }
                        else {
                            Program.UpdateMonster(mon);
                            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new ServedResult("POST") { contentDic = { { "updated", id.ToString() } } }.ToJson()) };
                        }
                    }
                    else if (req.QueryString.GetValues("action").Length > 0) {
                        mon = Program.data.GetMonster(id);
                        if (mon == null) {
                            return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(new ServedResult("POST") { contentDic = { { "nonexistant", id.ToString() } } }.ToJson()) };
                        }
                        switch (req.QueryString.GetValues("action").First()) {
                            case "reserve":
                                if (!Program.goals.ReservedIds.Contains(id)) {
                                    Program.goals.ReservedIds.Add(id);
                                    Program.SaveGoals();
                                }
                                break;
                            case "unreserve":
                                Program.goals.ReservedIds.Remove(id);
                                Program.SaveGoals();
                                break;
                            case "lock":
                                mon.Locked = true;
                                Program.data.LockedUnits.Add(mon.Id);
                                Program.data.isModified = true;
                                break;
                            case "unlock":
                                mon.Locked = false;
                                Program.data.LockedUnits.Remove(mon.Id);
                                Program.data.isModified = true;
                                break;
                            case "doskill":
                                Program.goals.NoSkillIds.Remove(id);
                                Program.SaveGoals();
                                break;
                            case "noskill":
                                Program.goals.NoSkillIds.Add(id);
                                Program.SaveGoals();
                                break;
                        }
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new ServedResult("POST") { contentDic = { { "POST", id.ToString() } } }.ToJson()) };
                }

                [HttpMethod("PUT")]
                public HttpResponseMessage PutMethod(HttpListenerRequest req, string[] uri) {
                    Monster mon = null;
                    ulong id = 0;
                    if (uri.Length > 0) {
                        ulong.TryParse(uri[0], out id);
                    }
                    using (var str = new StreamReader(req.InputStream)) {
                        var raw = str.ReadToEnd();
                        mon = JsonConvert.DeserializeObject<Monster>(raw);
                    }
                    if (id == 0)
                        id = mon.Id;

                    Program.UpdateMonster(mon);

                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new ServedResult("PUT") { contentDic = { { "PUT", id.ToString() } } }.ToJson()) };
                }

                [HttpMethod("DELETE")]
                public HttpResponseMessage DeleteMethod(HttpListenerRequest req, string[] uri) {
                    Monster mon = null;
                    ulong id = 0;
                    if (uri.Length > 0) {
                        ulong.TryParse(uri[0], out id);
                    }

                    mon = Program.data.GetMonster(id);
                    if (mon != null) {
                        Program.data.Monsters.Remove(mon);
                        Program.data.isModified = true;
                        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new ServedResult("DELETE") { contentDic = { { "deleted", id.ToString() } } }.ToJson()) };
                    }
                    return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(new ServedResult("DELETE") { contentDic = { { "failed", id.ToString() } } }.ToJson()) };
                }

                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    switch (req.HttpMethod) {
                        case "PUT":
                            return PutMethod(req, uri);

                        case "POST":
                            return PostMethod(req, uri);

                        case "DELETE":
                            return DeleteMethod(req, uri);

                        case "GET":
                            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{GET}") };
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{FAIL}") };
                }
            }

            [PageAddressRender("runes")]
            public class RunesRenderer : PageRenderer {
                [HttpMethod("POST")]
                public HttpResponseMessage PostMethod(HttpListenerRequest req, string[] uri) {
                    if (Program.data == null) {
                        return return404();
                    }
                    Rune rune = null;
                    ulong id = 0;
                    if (uri.Length > 0) {
                        ulong.TryParse(uri[0], out id);
                    }
                    if (req.QueryString.Count == 0) {
                        using (var str = new StreamReader(req.InputStream)) {
                            var raw = str.ReadToEnd();
                            rune = JsonConvert.DeserializeObject<Rune>(raw);
                        }
                        if (id == 0)
                            id = rune.Id;

                        if (uri.Length == 0 || Program.data.GetRune(id) == null) {
                            Program.AddRune(rune);
                            return new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent(new ServedResult("POST") { contentDic = { { "created", id.ToString() } } }.ToJson()) };
                        }
                        else {
                            Program.UpdateRune(rune);
                            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new ServedResult("POST") { contentDic = { { "updated", id.ToString() } } }.ToJson()) };
                        }
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new ServedResult("POST") { contentDic = { { "POST", id.ToString() } } }.ToJson()) };
                }

                [HttpMethod("PUT")]
                public HttpResponseMessage PutMethod(HttpListenerRequest req, string[] uri) {
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{PUT}") };
                }

                [HttpMethod("DELETE")]
                public HttpResponseMessage DeleteMethod(HttpListenerRequest req, string[] uri) {
                    Rune rune = null;
                    ulong id = 0;
                    if (uri.Length > 0) {
                        ulong.TryParse(uri[0], out id);
                    }
                    rune = Program.data?.GetRune(id);
                    if (rune != null) {
                        Program.data.Runes.Remove(rune);
                        Program.data.isModified = true;
                        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new ServedResult("DELETE") { contentDic = { { "deleted", id.ToString() } } }.ToJson()) };
                    }
                    return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(new ServedResult("DELETE") { contentDic = { { "failed", id.ToString() } } }.ToJson()) };
                }

                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    switch (req.HttpMethod) {
                        case "PUT":
                            return PutMethod(req, uri);
                        case "POST":
                            return PostMethod(req, uri);
                        case "DELETE":
                            return DeleteMethod(req, uri);
                        case "GET":
                            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{GET}") };
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{FAIL}") };
                }
            }
        }

    }
}
