using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApplication.Models
{
    public class UserCurrentLocation
    {
        [Key]
        public int User_Location_Id { get; set; }
        public string UserId { get; set; }
        
        [JsonProperty("ip")]

        public string IpAddress { get; set; }

        [JsonProperty("longitude")]

        public string Longitude { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        public bool Status { get; set; }


        public DateTime Created { get; set; }

        public DateTime? Updated { get; set; }

        public DateTime? Ended { get; set; }

    }
}
