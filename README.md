# SmugSharp
C# async library for using SmugMug's v2 API

## Getting Started
Eventually, this will be a nuget package available everywhere nuget is. But for now, clone the repo and add a reference to the project.

To run tests, create the following file in `/SmugSharpTest/Configuration/Authentication.cs` and fill in your own values:
```csharp
namespace SmugSharpTest.Configuration
{
    public static class Authentication
    {
        // Fill these in with your own values.
        public static readonly string ApiKey = "";
        public static readonly string ApiSecret = "";
        public static readonly string CallbackUrl = "";
        public static readonly string AccessToken = "";
        public static readonly string AccessTokenSecret = "";
    }
}
```
