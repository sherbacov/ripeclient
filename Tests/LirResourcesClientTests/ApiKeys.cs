using Microsoft.Extensions.Configuration;

namespace LirResourcesClientTests;

public static class UnitTestApiKeys {
   public static string? SecretKey { get; private set; }
  
   public static readonly IConfiguration _config;   
   
    static UnitTestApiKeys() {
        SecretKey = Environment.GetEnvironmentVariable("LIR_RESOURCES_API_KEY");
    }
}