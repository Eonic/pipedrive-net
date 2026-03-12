using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipedriveNet.Dto
{
    public class OrganizationDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public int owner_id { get; set; }
        public int? org_id { get; set; }
        public string add_time { get; set; }
        public string update_time { get; set; }
        public bool is_deleted { get; set; }
        public int visible_to { get; set; }
        public List<string> custom_fields { get; set; }
        public int people_count { get; set; }
        public string cc_email { get; set; }
        public AddressDto address { get; set; }

    }

    public class AddressDto
    {
        public string value { get; set; }
        public string country { get; set; }
        public string admin_area_level_1 { get; set; }
        public string admin_area_level_2 { get; set; }
        public string locality { get; set; }
        public string sublocality { get; set; }
        public string route { get; set; }
        public string street_number { get; set; }
        public string subpremise { get; set; }
        public string postal_code { get; set; }

    }
}
