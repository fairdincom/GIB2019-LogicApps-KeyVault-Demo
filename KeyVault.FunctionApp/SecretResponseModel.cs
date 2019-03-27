using System.Collections.Generic;

namespace KeyVault.FunctionApp
{
    public class SecretResponseModel
    {
        public virtual string Name { get; set; }
        public virtual string Value { get; set; }
    }
}
