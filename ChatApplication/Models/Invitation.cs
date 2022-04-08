using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApplication.Models
{
    public class Invitation
    {    [Key]
        public int InvitationId { get; set; }
        public string FromId { get; set; }
        public String ToId { get; set; }

        public bool Status { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
