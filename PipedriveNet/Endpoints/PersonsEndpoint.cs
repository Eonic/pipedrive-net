using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PipedriveNet.Endpoints
{
	public class PersonsEndpoint<TPerson>
	{
		private readonly ApiClient _client;

		internal PersonsEndpoint(ApiClient client)
		{
			_client = client;
		}

	    public Task<List<TPerson>> All
	    {
	        get { return _client.Get<List<TPerson>>("v2/persons"); }
	    }

		JObject PreparePersonData(string name, string email, string phone, Dictionary<Expression<Func<TPerson, object>>, object> extras = null)
		{
			var req = new JObject();
			req["name"] = name;
			if (email != null)
			{
				var emailsArray = new JArray();
				var emailObj = new JObject();
				emailObj["value"] = email;
				emailObj["primary"] = true;
				emailObj["label"] = "work";
				emailsArray.Add(emailObj);
				req["emails"] = emailsArray;
			}
			if (phone != null)
			{
				var phonesArray = new JArray();
				var phoneObj = new JObject();
				phoneObj["value"] = phone;
				phoneObj["primary"] = true;
				phoneObj["label"] = "work";
				phonesArray.Add(phoneObj);
				req["phones"] = phonesArray;
			}
			if (extras != null && extras.Count > 0)
			{
				var customFields = new JObject();
				foreach (var extra in extras)
				{
					customFields[_client.ResolveProperty(extra.Key)] = JToken.FromObject(extra.Value, _client.Serializer);
				}
				req["custom_fields"] = customFields;
			}
			return req;
		}

	    public Task<TPerson> Create(string name, string email, string phone, Dictionary<Expression<Func<TPerson,object>>, object> extras = null)
	    {

	        return _client.Post<TPerson>("v2/persons", PreparePersonData(name, email, phone, extras));
	    }

		public Task<TPerson> Update(int id, string name, string email, string phone, Dictionary<Expression<Func<TPerson, object>>, object> extras = null)
		{
			return _client.Patch<TPerson>("v2/persons/" + id, PreparePersonData(name, email, phone, extras));
		}

	    public Task Delete(int id)
	    {
	        return _client.Delete("v2/persons/" + id);
	    }
	}
}
