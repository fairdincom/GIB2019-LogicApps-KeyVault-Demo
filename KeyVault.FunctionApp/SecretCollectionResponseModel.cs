using System.Collections.Generic;

namespace KeyVault.FunctionApp
{
    public class SecretCollectionResponseModel
    {
        public SecretCollectionResponseModel()
        {
            this.Items = new List<SecretResponseModel>();
        }

        public virtual List<SecretResponseModel> Items { get; set; }
    }
}
