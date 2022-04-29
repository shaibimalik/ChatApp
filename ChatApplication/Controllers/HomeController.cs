using ChatApplication.Data;
using ChatApplication.Hubs;
using ChatApplication.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

    
    
        public HomeController(DataContext dataContext, UserManager<IdentityUser> userManager, IHubContext<ChatHub> hubContext)
        {
            db = dataContext;
            _userManager = userManager;
            _hubContext = hubContext;
        }

       
        public async Task<IActionResult> Index()
     {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _userManager.GetUserAsync(HttpContext.User);

            GeoInfoProvider.GeoInfoProvider geoInfoProvider = new GeoInfoProvider.GeoInfoProvider();
            var response = await geoInfoProvider.GetGeoInfo();

            var UserLocationExist = db.userCurrentLocations.Where(x => x.UserId == user.Id && x.Status == true && x.Ended == null).Select(x => x.UserId).Count();

            if (UserLocationExist > 0)
            {

                var userlocation = await db.userCurrentLocations.Where(x => x.UserId == user.Id && x.Status == true && x.Ended == null).FirstOrDefaultAsync();
                userlocation.Status = false;
                userlocation.Updated = DateTime.Now;
                db.Update(userlocation);
                await db.SaveChangesAsync();

                UserCurrentLocation currentLocation = new UserCurrentLocation();
                {

                    currentLocation = JsonConvert.DeserializeObject<UserCurrentLocation>(response);
                    currentLocation.UserId = user.Id;
                    currentLocation.Status = true;
                    currentLocation.Created = DateTime.Now;
                };

                await db.userCurrentLocations.AddAsync(currentLocation);
                await db.SaveChangesAsync();


            }
            else
            {
                UserCurrentLocation currentLocation = new UserCurrentLocation();
                {

                    currentLocation = JsonConvert.DeserializeObject<UserCurrentLocation>(response);
                    currentLocation.UserId = user.Id;
                    currentLocation.Status = true;
                    currentLocation.Created = DateTime.Now;
                };

                await db.userCurrentLocations.AddAsync(currentLocation);
                await db.SaveChangesAsync();


            }








          

            var allMessages = await db.Messages.Where(x =>
                x.FromUserId == user.Id ||
                x.ToUserId == user.Id)
                .ToListAsync();
            var Inviteduser = await db.Invitations.Where(x => x.FromId == user.Id || x.ToId==user.Id && x.Status == true).Select(x => new { Toid =x.ToId,Fromid=x.FromId}).ToListAsync();
            var count =  db.Invitations.Where(x => x.ToId == user.Id && x.Status == false).Select(x=>x.ToId).Count();
            var chats = new List<ChatModel>();
           
            foreach (var i in await db.Users.Where(x => Inviteduser.Select(y => y.Toid).Contains(x.Id)|| Inviteduser.Select(y => y.Fromid).Contains(x.Id)).ToListAsync()) 
            {
                if (i == user) continue;

                var chat = new ChatModel()
                {
                    MyMessages = allMessages.Where(x => x.FromUserId == user.Id && x.ToUserId == i.Id).ToList(),
                    OtherMessages = allMessages.Where(x => x.FromUserId == i.Id && x.ToUserId == user.Id).ToList(),
                    RecipientName = i.UserName
                };

                var chatMessages = new List<Message>();
                chatMessages.AddRange(chat.MyMessages);
                chatMessages.AddRange(chat.OtherMessages);

                chat.LastMessage = chatMessages.OrderByDescending(x => x.Timestamp).FirstOrDefault();
                //chat.Count = count;
                chats.Add(chat);
            }
            NotifiUsers notifiUsers = new NotifiUsers();
            notifiUsers.Count = count;

            ChatViewModel chatViewModel = new ChatViewModel();
            chatViewModel.chatmodel = chats;
            chatViewModel.notification = notifiUsers;




            return View(chatViewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

      
        [HttpPost]
        public async Task<IActionResult> GetInviteUser(string Search)
     {

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var InvitedUser = await db.Invitations.Where(x => x.FromId == user.Id).Select(x => x.ToId).ToListAsync();
            var recipient = await db.Users.Where(x => x.UserName.Contains( Search) && !InvitedUser.Contains(x.Id) && x.Id !=user.Id).ToListAsync();
            //ChatViewModel inviteView = new ChatViewModel()
            //{

            //    FromUserId = recipient.Id,
            //    FromUserName = recipient.UserName


            //};

            return Json(recipient);
            //  return View(inviteView);
        }

        [HttpPost]

        public async Task<IActionResult> SendInvitation(string Toid) {

            var user = await _userManager.GetUserAsync(HttpContext.User);
            Invitation invitation = new Invitation()
            {

                ToId = Toid,
                FromId= user.Id,
                Status=false,
                CreatedDate=DateTime.Now
            };
            await db.Invitations.AddAsync(invitation);
            await db.SaveChangesAsync();
            var count = db.Invitations.Where(x => x.FromId == user.Id && x.Status == false).Select(x => x.ToId).Count();
            var recipient = await db.Users.SingleOrDefaultAsync(x => x.Id == Toid);
            try
            {
                string connectionId = ChatHub.UsernameConnectionId[recipient.UserName];

                await _hubContext.Clients.Client(connectionId).SendAsync("Notification", count);
                return Json("OK");
            }
            catch (Exception ex) {

                string msg = ex.Message;
                return Json("OK");
            }
           

            // return Json("OK");
        }


        public async Task<IActionResult> GetListInvitation()
        {


            var user = await _userManager.GetUserAsync(HttpContext.User);
            var InvitedUser = await db.Invitations.Where(x => x.ToId == user.Id && x.Status==false).Select(x => x.FromId).ToListAsync();
            var recipient = await db.Users.Where(x => InvitedUser.Contains(x.Id)).ToListAsync();
            //ChatViewModel inviteView = new ChatViewModel()
            //{

            //    FromUserId = recipient.Id,
            //    FromUserName = recipient.UserName


            //};

            return Json(recipient);
            //  return View(inviteView);
        }
        [HttpPost]
        public async Task<IActionResult> HandleInvitation(string Toid, bool Status)
        {

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var invitedUser = await db.Invitations.Where(x => x.FromId == Toid && x.ToId == user.Id).FirstOrDefaultAsync();
            invitedUser.Status = Status;
            invitedUser.ModifiedDate = DateTime.Now;
            db.Update(invitedUser);
            await db.SaveChangesAsync();

            return Json("OK");
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult UserLocation()
        {
            return View();
        }


    }
}
