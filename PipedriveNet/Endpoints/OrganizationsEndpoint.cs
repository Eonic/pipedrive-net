using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PipedriveNet.Endpoints
{
    public class OrganizationsEndpoint<TOrganization>
    {
        private readonly ApiClient _client;

        internal OrganizationsEndpoint(ApiClient client)
        {
            _client = client;
        }
        public Task<List<TOrganization>> All
        {
            get { return _client.Get<List<TOrganization>>("v2/organizations"); }
        }

        public async Task<List<TOrganization>> Search(string name)
        {
           // var result = await _client.Get<ApiClient.SearchResultContainer<TOrganization>>("v2/organizations/search?term=" + name);
            var result = await _client.Get<ApiClient.SearchResultContainer<TOrganization>>("v2/organizations/search?term=" + Uri.EscapeDataString(name) + "&fields=name&exact_match=0&limit=100");
            return result?.Items?.Select(i => i.item).ToList() ?? new List<TOrganization>();
        }

        JObject PrepareOrgData(string name, Dictionary<Expression<Func<TOrganization, object>>, object> extras = null)
        {
            var req = new JObject();
            req["name"] = name;
            req["visible_to"] = 3;
            return req;
        }

        public Task<TOrganization> Create(string name, Dictionary<Expression<Func<TOrganization, object>>, object> extras = null)
        {

            return _client.Post<TOrganization>("v2/organizations", PrepareOrgData(name, extras));
        }

        public Task<TOrganization> Update(int id, string name, Dictionary<Expression<Func<TOrganization, object>>, object> extras = null)
        {
            return _client.Patch<TOrganization>("v2/organizations/" + id, PrepareOrgData(name, extras));
        }

        public Task Delete(int id)
        {
            return _client.Delete("v2/organizations/" + id);
        }
    }
}

