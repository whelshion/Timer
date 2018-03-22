//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.SignalR;

//namespace Timer.Web.Core.LiveLog
//{
//    public class LiveLogHub : Hub
//    {
//        public async Task Send(string message)
//        {
//            await this.Clients.All.SendAsync("Send", DateTimeOffset.Now,"消息发送","admin");
//        }
//    }
//}
