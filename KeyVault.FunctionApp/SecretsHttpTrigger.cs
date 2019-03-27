using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Aliencube.AzureFunctions.Extensions.OpenApi.Attributes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Enums;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;

namespace KeyVault.FunctionApp
{
    public static class SecretsHttpTrigger
    {
        private static string keyVaultBaseUrl = GetKeyVaultBaseUrl();
        private static IKeyVaultClient kv = GetKeyVaultClient();

        [FunctionName(nameof(GetSecrets))]
        [OpenApiOperation("list", "secret", Summary = "Gets the list of secrets", Description = "This gets the list of secrets.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseBody(HttpStatusCode.OK, "application/json", typeof(SecretCollectionResponseModel), Summary = "Secret collection response.")]
        public static async Task<IActionResult> GetSecrets(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "secrets")] HttpRequest req,
            ILogger log)
        {
            IActionResult result;
            try
            {
                var response = await kv.GetSecretsAsync(keyVaultBaseUrl)
                                       .ConfigureAwait(false);
                var models = response.OfType<SecretItem>()
                                     .Select(p => new SecretResponseModel() { Name = p.Identifier.Name })
                                     .ToList();

                result = new OkObjectResult(new SecretCollectionResponseModel() { Items = models });
            }
            catch (KeyVaultErrorException ex)
            {
                var statusCode = (int)ex.Response.StatusCode;
                var value = new { StatusCode = statusCode, Message = ex.Body?.Error?.Message };

                result = new ObjectResult(value) { StatusCode = statusCode };
            }
            catch (Exception ex)
            {
                var statusCode = (int)HttpStatusCode.InternalServerError;
                var value = new { StatusCode = statusCode, Message = ex.Message };

                result = new ObjectResult(value) { StatusCode = statusCode };
            }

            return result;
        }

        [FunctionName(nameof(GetSecret))]
        [OpenApiOperation("view", "secret", Summary = "Gets the secret", Description = "This gets the secret.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("name", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The secret name.")]
        [OpenApiResponseBody(HttpStatusCode.OK, "application/json", typeof(SecretResponseModel), Summary = "Secret response.")]
        public static async Task<IActionResult> GetSecret(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "secrets/{name}")] HttpRequest req,
            string name,
            ILogger log)
        {
            IActionResult result;
            try
            {
                var response = await kv.GetSecretAsync(keyVaultBaseUrl, name)
                                       .ConfigureAwait(false);
                var model = new SecretResponseModel() { Name = response.SecretIdentifier.Name, Value = response.Value };

                result = new OkObjectResult(model);
            }
            catch (KeyVaultErrorException ex)
            {
                var statusCode = (int)ex.Response.StatusCode;
                var value = new { StatusCode = statusCode, Message = ex.Body?.Error?.Message };

                result = new ObjectResult(value) { StatusCode = statusCode };
            }
            catch (Exception ex)
            {
                var statusCode = (int)HttpStatusCode.InternalServerError;
                var value = new { StatusCode = statusCode, Message = ex.Message };

                result = new ObjectResult(value) { StatusCode = statusCode };
            }

            return result;
        }

        private static string GetKeyVaultBaseUrl()
        {
            var name = Environment.GetEnvironmentVariable("KeyVaultName");

            return $"https://{name}.vault.azure.net/";
        }

        private static IKeyVaultClient GetKeyVaultClient()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            return kv;
        }
    }
}
