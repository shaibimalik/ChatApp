using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApplication.Models
{
    public class InviteView:ChatViewModel
    {
        public string FromUserId { get; set; }
        public String FromUserName { get; set; }
    }
}
