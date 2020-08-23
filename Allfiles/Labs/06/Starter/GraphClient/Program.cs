using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphClient
{
    public class Program
    {
        private const string _clientId = "";
        private const string _tenantId = "";

        public static async Task Main(string[] args)
        {
            IPublicClientApplication app = PublicClientApplicationBuilder
                .Create(_clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
                .WithRedirectUri("http://localhost")
                .Build();

            List<string> scopes = new List<string>
            {
                "user.read"
            };

            // AuthenticationResult result = await app
            //     .AcquireTokenInteractive(scopes)
            //     .ExecuteAsync();

            // Console.WriteLine($"Token:\t{result.AccessToken}");

            DeviceCodeProvider provider = new DeviceCodeProvider(app, scopes);
            GraphServiceClient client = new GraphServiceClient(provider);

            User myProfile = await client.Me.Request().GetAsync();
            Console.WriteLine($"Name:\t{myProfile.DisplayName}");
            Console.WriteLine($"AAD Id:\t{myProfile.Id}");

            var myApps = client.Applications.Request().GetAsync();
        }
    }
}
