using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Aliencube.AzureFunctions.Extensions.OpenApi;
using Aliencube.AzureFunctions.Extensions.OpenApi.Abstractions;
using Aliencube.AzureFunctions.Extensions.OpenApi.Attributes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

using Newtonsoft.Json;

namespace KeyVault.FunctionApp
{
    public static class OpenApiHttpTrigger
    {
        private static AppSettings settings = new AppSettings();
        private static IDocument doc = new Document(new DocumentHelper());
        private static ISwaggerUI swaggerUI = new SwaggerUI();

        [FunctionName(nameof(RenderSwaggerDocument))]
        [OpenApiIgnore]
        public static async Task<IActionResult> RenderSwaggerDocument(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger.json")] HttpRequest req,
            ILogger log)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var result = await doc.InitialiseDocument()
                                  .AddMetadata(settings.OpenApiInfo)
                                  .AddServer(req, settings.HttpSettings.RoutePrefix)
                                  .Build(assembly)
                                  .RenderAsync(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json)
                                  .ConfigureAwait(false);
            var content = new ContentResult()
                              {
                                  Content = result,
                                  ContentType = "application/json",
                                  StatusCode = (int)HttpStatusCode.OK
                               };

            return content;
        }

        [FunctionName(nameof(RenderSwaggerUI))]
        [OpenApiIgnore]
        public static async Task<IActionResult> RenderSwaggerUI(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/ui")] HttpRequest req,
            ILogger log)
        {
            var result = await swaggerUI.AddMetadata(settings.OpenApiInfo)
                                        .AddServer(req, settings.HttpSettings.RoutePrefix)
                                        .BuildAsync()
                                        .RenderAsync("swagger.json", settings.SwaggerAuthKey)
                                        .ConfigureAwait(false);
            var content = new ContentResult()
                              {
                                  Content = result,
                                  ContentType = "text/html",
                                  StatusCode = (int)HttpStatusCode.OK
                              };

            return content;
        }
    }
}
