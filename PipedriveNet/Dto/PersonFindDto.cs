using System.Collections.Generic;

namespace PipedriveNet.Dto
{
	public class PersonFindDto
	{
		// v2 API properties (lowercase/snake_case)
		public int id { get; set; }
		public string type { get; set; }
		public string name { get; set; }
        //  public List<PhoneDto> phones { get; set; }
        //  public List<EmailDto> emails { get; set; }
        public List<string> phones { get; set; }
        public List<string> emails { get; set; }

        public string primary_email { get; set; }
		public int visible_to { get; set; }
		public OwnerDto owner { get; set; }
		public OrganizationDto organization { get; set; }
		public List<string> custom_fields { get; set; }
		public List<string> notes { get; set; }
		public string update_time { get; set; }

	}

	/* TODO: Nothing */

}
