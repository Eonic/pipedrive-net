using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PipedriveNet.Dto
{
    public class DealDto
    {
        // v2 API properties (lowercase/snake_case)
        public int id { get; set; }
        public string title { get; set; }
        public int creator_user_id { get; set; }
        public int owner_id { get; set; }
        public decimal value { get; set; }
        public int? person_id { get; set; }
        public int? org_id { get; set; }
        public int stage_id { get; set; }
        public int pipeline_id { get; set; }
        public string currency { get; set; }
        public string archive_time { get; set; }
        public string add_time { get; set; }
        public string update_time { get; set; }
        public string stage_change_time { get; set; }
        public string status { get; set; }
        public bool is_archived { get; set; }
        public bool is_deleted { get; set; }
        public int? probability { get; set; }
        public string lost_reason { get; set; }
        public int visible_to { get; set; }
        public string close_time { get; set; }
        public string won_time { get; set; }
        public string lost_time { get; set; }
        public string local_won_date { get; set; }
        public string local_lost_date { get; set; }
        public string local_close_date { get; set; }
        public string expected_close_date { get; set; }
        public List<int> label_ids { get; set; }
        public string origin { get; set; }
        public string origin_id { get; set; }
        public int? channel { get; set; }
        public string channel_id { get; set; }
        public string source_lead_id { get; set; }
        public decimal? acv { get; set; }
        public decimal? arr { get; set; }
        public decimal? mrr { get; set; }
        public Dictionary<string, object> custom_fields { get; set; }

        // Legacy v1 API properties for backward compatibility

        [JsonIgnore]
        public PersonIdDto PersonId
        { 
            get => person_id.HasValue ? new PersonIdDto { Value = person_id.Value } : null; 
            set => person_id = value?.Value; 
        }
        [JsonIgnore]
        public DealStatus Status
        { 
            get 
            {
                if (string.IsNullOrEmpty(status))
                    return DealStatus.Open;

                switch (status.ToLower())
                {
                    case "won":
                        return DealStatus.Won;
                    case "lost":
                        return DealStatus.Lost;
                    case "deleted":
                        return DealStatus.Deleted;
                    default:
                        return DealStatus.Open;
                }
            }
            set => status = value.ToString().ToLower();
        }
        [JsonIgnore]
        public int Value { get => (int)value; set => this.value = value; }
        [JsonIgnore]
        public DateTime AddTime
        { 
            get => DateTime.TryParse(add_time, out var dt) ? dt : DateTime.MinValue;
            set => add_time = value.ToString("yyyy-MM-dd HH:mm:ss");
        }
        [JsonIgnore]
        public DateTime? WonTime
        { 
            get => DateTime.TryParse(won_time, out var dt) ? dt : (DateTime?)null;
            set => won_time = value?.ToString("yyyy-MM-dd HH:mm:ss");
        }
        [JsonIgnore]
        public DateTime? LostTime
        { 
            get => DateTime.TryParse(lost_time, out var dt) ? dt : (DateTime?)null;
            set => lost_time = value?.ToString("yyyy-MM-dd HH:mm:ss");
        }
        [JsonIgnore]
        public DateTime? CloseTime
        { 
            get => DateTime.TryParse(close_time, out var dt) ? dt : (DateTime?)null;
            set => close_time = value?.ToString("yyyy-MM-dd HH:mm:ss");
        }
        [JsonIgnore]
        public DateTime? ExpectedCloseTime
        { 
            get => DateTime.TryParse(expected_close_date, out var dt) ? dt : (DateTime?)null;
            set => expected_close_date = value?.ToString("yyyy-MM-dd");
        }
    }

    public enum DealStatus
    {
        Open, Won, Lost, Deleted
    }

    public class PersonIdDto
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public List<PipedriveStringListItemDto> Email { get; set; }
        public List<PipedriveStringListItemDto> Phone { get; set; }
    }

    /*
     {
        "id": 1,
        "user_id": {
            "id": 487920,
            "name": "Anton Aksentyuk",
            "email": "support@promarket.pro",
            "has_pic": false,
            "value": 487920
        },
        "person_id": {
            "name": "Николай",
            "email": null,
            "phone": "",
            "value": 2
        },
        "org_id": {
            "name": "Петербуржкский Мельничный Комбинат",
            "people_count": 1,
            "cc_email": "promarket@pipedrivemail.com",
            "value": 1
        },
        "stage_id": 3,
        "title": "покупка профиля",
        "value": 4000,
        "currency": "RUB",
        "add_time": "2015-02-10 13:41:27",
        "update_time": "2015-02-10 17:09:02",
        "stage_change_time": "2015-02-10 17:09:02",
        "active": true,
        "deleted": false,
        "status": "open",
        "next_activity_date": "2015-02-10",
        "next_activity_time": null,
        "next_activity_id": 1,
        "last_activity_id": null,
        "last_activity_date": null,
        "lost_reason": null,
        "visible_to": "3",
        "close_time": null,
        "pipeline_id": 1,
        "won_time": null,
        "lost_time": null,
        "products_count": null,
        "files_count": null,
        "notes_count": 0,
        "followers_count": 1,
        "email_messages_count": null,
        "activities_count": 1,
        "done_activities_count": 0,
        "undone_activities_count": 1,
        "participants_count": 1,
        "expected_close_date": null,
        "4f56e05c527a8d4d7e584f310316d8e105eb34ac": null,
        "stage_order_nr": 4,
        "person_name": "Николай",
        "org_name": "Петербуржкский Мельничный Комбинат",
        "next_activity_subject": "Позвонить второй раз и уточнить покупку",
        "next_activity_type": "call",
        "next_activity_duration": "00:00:00",
        "next_activity_note": "",
        "formatted_value": "4 000 руб.",
        "weighted_value": 4000,
        "formatted_weighted_value": "4 000 руб.",
        "rotten_time": "2015-03-02 17:09:02",
        "owner_name": "Anton Aksentyuk",
        "cc_email": "promarket+deal1@pipedrivemail.com",
        "org_hidden": false,
        "person_hidden": false,
        "average_time_to_won": {
            "y": 0,
            "m": 0,
            "d": 0,
            "h": 0,
            "i": 0,
            "s": 0,
            "total_seconds": 0
        },
        "average_stage_progress": 0,
        "age": {
            "y": 0,
            "m": 0,
            "d": 0,
            "h": 6,
            "i": 39,
            "s": 27,
            "total_seconds": 23967
        },
        "stay_in_pipeline_stages": {
            "times_in_stages": {
                "2": 2068,
                "3": 21899
            },
            "order_of_stages": [
                3,
                2
            ]
        },
        "last_activity": null,
        "next_activity": {
            "id": 1,
            "company_id": 340542,
            "user_id": 487920,
            "done": false,
            "type": "call",
            "due_date": "2015-02-10",
            "due_time": "",
            "duration": "",
            "add_time": "2015-02-10 16:23:22",
            "marked_as_done_time": "",
            "subject": "Позвонить второй раз и уточнить покупку",
            "deal_id": 1,
            "org_id": 1,
            "person_id": 2,
            "active_flag": true,
            "update_time": "2015-02-10 16:23:22",
            "gcal_event_id": null,
            "google_calendar_id": null,
            "google_calendar_etag": null,
            "note": "",
            "person_name": "Николай",
            "org_name": "Петербуржкский Мельничный Комбинат",
            "deal_title": "покупка профиля",
            "public_id": 1,
            "assigned_to_user_id": 487920,
            "created_by_user_id": 487920,
            "owner_name": "Anton Aksentyuk",
            "person_dropbox_bcc": "promarket@pipedrivemail.com",
            "deal_dropbox_bcc": "promarket+deal1@pipedrivemail.com"
        }
    },
    "additional_data": {
        "dropbox_email": "promarket+deal1@pipedrivemail.com"
    },
    "related_objects": {
        "person": {
            "2": {
                "id": 2,
                "name": "Николай",
                "email": null,
                "phone": ""
            }
        },
        "organization": {
            "1": {
                "id": 1,
                "name": "Петербуржкский Мельничный Комбинат",
                "people_count": 1,
                "cc_email": "promarket@pipedrivemail.com"
            }
        },
        "user": {
            "487920": {
                "id": 487920,
                "name": "Anton Aksentyuk",
                "email": "support@promarket.pro",
                "has_pic": false
            }
        }
     */
}
