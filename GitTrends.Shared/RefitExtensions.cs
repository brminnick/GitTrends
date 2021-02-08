using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;

namespace GitTrends.Shared
{
    public static class RefitExtensions
    {
        public static T For<T>(HttpClient client) => RestService.For<T>(client, GetRefitSettings());

        static RefitSettings GetRefitSettings() => new(new NewtonsoftJsonContentSerializer(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
    }
}
