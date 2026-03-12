using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using System.Net.Http;

namespace PipedriveNet.Endpoints
{
	public class FilesEndpoint<TFile>
	{
		private readonly ApiClient _client;

		internal FilesEndpoint(ApiClient client)
		{
			_client = client;
		}

		public Task<List<TFile>> All
		{
			get 
			{ 
				try
				{
					return _client.Get<List<TFile>>("v1/files");
				}
				catch (PipedriveException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new PipedriveException($"Failed to retrieve files: {ex.Message}");
				}
			}
		}

		public Task<TFile> Create(String FileName, ByteArrayContent filedata, int dealId = 0, int personId = 0, int orgId = 0)
		{
			if (string.IsNullOrWhiteSpace(FileName))
				throw new ArgumentException("File name cannot be null or empty.", nameof(FileName));

			if (filedata == null)
				throw new ArgumentNullException(nameof(filedata), "File data cannot be null.");

			try
			{
				MultipartFormDataContent form = new MultipartFormDataContent();
				if (dealId != 0)
				{
					form.Add(new StringContent(dealId.ToString()), "deal_id");
				}
				if (personId != 0)
				{
					form.Add(new StringContent(personId.ToString()), "person_id");
				}
				if (orgId != 0)
				{
					form.Add(new StringContent(orgId.ToString()), "org_id");
				}

				filedata.Headers.Add("Content-Type", "application/octet-stream");
				form.Add(filedata,"file", FileName);
				return _client.PostMultipart<TFile>("v1/files", form);
			}
			catch (PipedriveException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new PipedriveException($"Failed to create file '{FileName}': {ex.Message}");
			}
		}

		public Task Delete(int id)
		{
			if (id <= 0)
				throw new ArgumentException("File ID must be greater than zero.", nameof(id));

			try
			{
				return _client.Delete("v1/files/" + id);
			}
			catch (PipedriveException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new PipedriveException($"Failed to delete file with ID {id}: {ex.Message}");
			}
		}
	}
}
