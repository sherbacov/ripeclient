namespace LirResourcesClientTests;

public static class UnitTestApiKeys {
   public static string? SecretKey { get; private set; }
   
    static UnitTestApiKeys() {
        SecretKey = Environment.GetEnvironmentVariable("LIR_RESOURCES_API_KEY");
    }
}