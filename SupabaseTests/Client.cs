using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Postgrest;
using SupabaseTests.Stubs;

namespace SupabaseTests
{
    [TestClass]
    public class Client
    {
        private Supabase.Client _instance;

        private static readonly string? IntegrationUrl = Environment.GetEnvironmentVariable("SUPABASE_TEST_URL");
        private static readonly string? IntegrationServiceRoleKey =
            Environment.GetEnvironmentVariable("SUPABASE_TEST_SERVICE_ROLE_KEY");
        private const string MissingCredentialsMessage =
            "Integration tests require SUPABASE_TEST_URL and SUPABASE_TEST_SERVICE_ROLE_KEY environment variables.";

        private static bool HasIntegrationConfig =>
            !string.IsNullOrWhiteSpace(IntegrationUrl) && !string.IsNullOrWhiteSpace(IntegrationServiceRoleKey);

        private async Task<Supabase.Client> EnsureClientAsync()
        {
            if (!HasIntegrationConfig)
                Assert.Inconclusive(MissingCredentialsMessage);

            if (_instance != null)
                return _instance;

            _instance = new Supabase.Client(IntegrationUrl!, IntegrationServiceRoleKey!, new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = false
            });
            await _instance.InitializeAsync();
            return _instance;
        }

        [TestMethod("Client: Initializes.")]
        public async Task ClientInitializes()
        {
            var client = await EnsureClientAsync();
            Assert.IsNotNull(client.Realtime);
            Assert.IsNotNull(client.Auth);
        }

        [TestMethod("SupabaseModel: Successfully Updates")]
        public async Task SupabaseModelUpdates()
        {
            var client = await EnsureClientAsync();
            var model = new Models.Channel { Slug = Guid.NewGuid().ToString() };
            var insertResult = await client.From<Models.Channel>().Insert(model);
            var newChannel = insertResult.Models.FirstOrDefault();

            var newSlug = $"Updated Slug @ {DateTime.Now.ToLocalTime()}";
            newChannel.Slug = newSlug;

            var updatedResult = await newChannel.Update<Models.Channel>();

            Assert.AreEqual(newSlug, updatedResult.Models.First().Slug);
        }

        [TestMethod("SupabaseModel: Successfully Deletes")]
        public async Task SupabaseModelDeletes()
        {
            var client = await EnsureClientAsync();
            var slug = Guid.NewGuid().ToString();
            var model = new Models.Channel { Slug = slug };

            var insertResult = await client.From<Models.Channel>().Insert(model);
            var newChannel = insertResult.Models.FirstOrDefault();

            await newChannel.Delete<Models.Channel>();

            var result = await client.From<Models.Channel>()
                .Filter("slug", Constants.Operator.Equals, slug).Get();

            Assert.AreEqual(0, result.Models.Count);
        }

        [TestMethod("Supports Dependency Injection for clients via property")]
        public void SupportsDIForClientsViaProperty()
        {
            var client = new Supabase.Client(
                new FakeAuthClient(),
                new FakeRealtimeClient(),
                new FakeFunctionsClient(),
                new FakeRestClient(),
                new FakeStorageClient(),
                new Supabase.SupabaseOptions());

            Assert.ThrowsExceptionAsync<NotImplementedException>(() => client.Auth.GetUser(""));
            Assert.ThrowsExceptionAsync<NotImplementedException>(() => client.Functions.Invoke(""));
            Assert.ThrowsExceptionAsync<NotImplementedException>(() => client.Realtime.ConnectAsync());
            Assert.ThrowsExceptionAsync<NotImplementedException>(() => client.Postgrest.Rpc("", null));
            Assert.ThrowsExceptionAsync<NotImplementedException>(() => client.Storage.ListBuckets());
        }
    }
}
