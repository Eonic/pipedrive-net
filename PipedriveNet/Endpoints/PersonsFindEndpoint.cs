using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PipedriveNet.Endpoints
{
	public class PersonsFindEndpoint<TPersonFind>
	{
		private readonly ApiClient _client;

		internal PersonsFindEndpoint(ApiClient client)
		{
			_client = client;
		}

        public Task<List<TPersonFind>> Find(string email)
        {
            return _client.Get<List<TPersonFind>>("v1/persons/search?term=" + Uri.EscapeDataString(email) + "&start=0&search_by_email=1");
        }

        public async Task<List<TPersonFind>> Search(string email)
        {
            var result = await _client.Get<ApiClient.SearchResultContainer<TPersonFind>>("v2/persons/search?term=" + Uri.EscapeDataString(email) + "&fields=email&exact_match=0&limit=100");
            return result?.Items?.Select(i => i.item).ToList() ?? new List<TPersonFind>();
        }

    }
}
