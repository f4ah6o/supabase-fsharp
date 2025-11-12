using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using SupabaseTests.Models;
using static Supabase.Gotrue.Constants;
using static Supabase.StatelessClient;

namespace SupabaseTests
{
    [TestClass]
    public class StatelessClient
    {

        private static readonly string? IntegrationUrl = Environment.GetEnvironmentVariable("SUPABASE_TEST_URL");
        private static readonly string? IntegrationServiceRoleKey =
            Environment.GetEnvironmentVariable("SUPABASE_TEST_SERVICE_ROLE_KEY");
        private const string MissingCredentialsMessage =
            "Integration tests require SUPABASE_TEST_URL and SUPABASE_TEST_SERVICE_ROLE_KEY environment variables.";

        private static bool HasIntegrationConfig =>
            !string.IsNullOrWhiteSpace(IntegrationUrl) && !string.IsNullOrWhiteSpace(IntegrationServiceRoleKey);

        private Supabase.SupabaseOptions options = new()
        {
            AuthUrlFormat = "{0}/auth/v1",
            RealtimeUrlFormat = "{0}/realtime/v1",
            RestUrlFormat = "{0}/rest/v1",
        };

        private static void EnsureIntegrationConfig()
        {
            if (!HasIntegrationConfig)
                Assert.Inconclusive(MissingCredentialsMessage);
        }

        [TestMethod("Can access Stateless REST")]
        public async Task CanAccessStatelessRest()
        {
            EnsureIntegrationConfig();

            var restOptions = GetRestOptions(IntegrationServiceRoleKey!, options);
            var restEndpoint = string.Format(options.RestUrlFormat, IntegrationUrl);
            var result1 = await new Supabase.Postgrest.Client(restEndpoint, restOptions).Table<Channel>().Get();

            var result2 = await From<Channel>(IntegrationUrl!, IntegrationServiceRoleKey!, options).Get();

            Assert.AreEqual(result1.Models.Count, result2.Models.Count);
        }

        [TestMethod("Can access Stateless GoTrue")]
        public void CanAccessStatelessGotrue()
        {
            var baseUrl = IntegrationUrl ?? "https://project.supabase.co";
            var gotrueOptions = GetAuthOptions(baseUrl, null, options);

            var client = new Supabase.Gotrue.Client(gotrueOptions);

            var url = client.SignIn(Provider.Spotify);

            Assert.IsNotNull(url);
        }

        [TestMethod("User defined Headers will override internal headers")]
        public void CanOverrideInternalHeaders()
        {
            Supabase.SupabaseOptions options = new Supabase.SupabaseOptions
            {
                AuthUrlFormat = "{0}:9999",
                RealtimeUrlFormat = "{0}:4000/socket",
                RestUrlFormat = "{0}:3000",
                Headers = new Dictionary<string, string> {
                    { "Authorization", "Bearer 123" }
                }
            };

            var baseUrl = IntegrationUrl ?? "https://project.supabase.co";
            var gotrueOptions = GetAuthOptions(baseUrl, "456", options);

            Assert.AreEqual("Bearer 123", gotrueOptions.Headers["Authorization"]);
        }
    }
}
