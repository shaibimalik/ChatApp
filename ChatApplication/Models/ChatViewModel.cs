using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApplication.Models
{
    public class ChatModel
    {
        public string RecipientName { get; set; }
        public List<Message> MyMessages { get; set; }
        public List<Message> OtherMessages { get; set; }
        public Message LastMessage { get; set; }
        public string FromUserId { get; set; }
        public string FromUserName { get; set; }


        //public NotifiUsers notifiUsers {
        //    get { return this.notifiUsers; }
        //    set { notifiUsers = value; }
        
        //}

        //public int InvitationNumber { get; set; }
        //public int Count { get; set; }

      
    }
    public class ChatViewModel
    {
        public List<ChatModel> chatmodel { get; set; }
        public NotifiUsers notification { get; set; }
    }
}
