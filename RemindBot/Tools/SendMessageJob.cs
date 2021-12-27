using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Sessions.Http.Managers;
using Quartz;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemindBot
{
    class SendMessageJob : IJob
    {
        public string Id { get; set; }
        public string Msg { get; set; }
        public string Type { get; set; }
        public string Sender { get; set; }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                if (Type == "群")
                {
                    MessageManager.SendGroupMessageAsync(Id, new MessageBase[] { new AtMessage(Sender), new PlainMessage("时间到了，该去" + Msg + "了") });
                }
                else
                {
                    MessageManager.SendFriendMessageAsync(Id, new PlainMessage("时间到了，该去" + Msg + "了"));
                }
            });
        }
    }
}
