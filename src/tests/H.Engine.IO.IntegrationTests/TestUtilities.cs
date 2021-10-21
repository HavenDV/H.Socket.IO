namespace H.Engine.IO.IntegrationTests;

public static class TestUtilities
{
    public static void CheckDeveloper()
    {
        var _ =
            Environment.GetEnvironmentVariable("DEVELOPER") ??
            throw new AssertInconclusiveException("DEVELOPER environment variable is not found.");
    }
}
