using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace KeyVault.FunctionApp
{
    public static class SecretsHttpTrigger
    {
        private static string keyVaultBaseUrl = GetKeyVaultBaseUrl();
        private static IKeyVaultClient kv = GetKeyVaultClient();

        [FunctionName(nameof(GetSecrets))]
        public static async Task<IActionResult> GetSecrets(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "secrets")] HttpRequest req,
            ILogger log)
        {
            IActionResult result;
            try
            {
                var value = await kv.GetSecretsAsync(keyVaultBaseUrl)
                                    .ConfigureAwait(false);

                result = new OkObjectResult(value);
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
        public static async Task<IActionResult> GetSecret(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "secrets/{name}")] HttpRequest req,
            string name,
            ILogger log)
        {
            IActionResult result;
            try
            {
                var value = await kv.GetSecretAsync(keyVaultBaseUrl, name)
                                    .ConfigureAwait(false);

                result = new OkObjectResult(value);
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
