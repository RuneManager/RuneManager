using System.IO;
using System.Net;
using System.Net.Http;

namespace RuneApp.InternalServer {
    public partial class Master : PageRenderer {
        [PageAddressRender("scripts")]
        public class ScriptRenderer : PageRenderer {
            public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                var resp = this.Recurse(req, uri);
                if (resp != null)// && resp.StatusCode != HttpStatusCode.NotFound)
                    return resp;

                // allows downloading files
                if (uri.Length > 0 && File.Exists(uri[0])) {
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(File.ReadAllText(uri[0])) };
                }

                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(@"function nextProgress() {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function() {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
            document.getElementById('button1').setAttribute('style', 'width:' + (Number(xmlHttp.responseText) + 50) + 'px;');
            console.log(xmlHttp.responseText);
            if (xmlHttp.responseText < 100) {
                setTimeout(nextProgress, 20);
            }
        }
    }
    xmlHttp.open('GET', '/api/getProgress?id=1', true); // true for asynchronous 
    xmlHttp.send(null);
}")
                };
            }

            [PageAddressRender("swagger.js")]
            public class SwaggerRenderer : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Swagger.Swagger.swagger_js) };
                }
            }

            [PageAddressRender("swagger-ui.js")]
            public class SwaggerUiRenderer : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Swagger.Swagger.swagger_ui) };
                }
            }

            [PageAddressRender("swagger-client.js")]
            public class SwaggerClientRenderer : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Swagger.Swagger.swagger_client) }; ;
                }
            }

        }

    }
}
