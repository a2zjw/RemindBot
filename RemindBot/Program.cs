using System;
using System.Threading.Tasks;
using System.Linq;
using Mirai.Net.Sessions;
using Mirai.Net.Data.Messages.Receivers;
using RemindBot.Listener;
using System.Reactive.Linq;
using Quartz.Impl;
using Quartz;
using Mirai.Net.Data.Events.Concretes.Request;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Data.Events.Concretes.Bot;
using Mirai.Net.Data.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemindBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //启用log4net记录日志
            LogHelper.Configure();
            Console.WriteLine("日志已启用");

            //读取配置文件
            JObject config;
            using (System.IO.StreamReader file = System.IO.File.OpenText("appSettings.json"))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    config = (JObject)JToken.ReadFrom(reader);
                }
            }

            //初始化机器人
            using var bot = new MiraiBot
            {
                Address = config["Address"].ToString(),
                QQ = config["QQ"].ToString(),
                VerifyKey = config["Key"].ToString()
            };
            Console.WriteLine("当前QQ:" + bot.QQ);

            //创建调度器
            var schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();
            await scheduler.Start();
            Console.WriteLine($"任务调度器已启动");


            

            //监听群消息
            var GroupMsgListener = new GroupMessageListener(scheduler);
            bot.MessageReceived
            .OfType<GroupMessageReceiver>()
            //.Where(it=>it.Sender.Group.Id=="111")//可以用linq只监听群号是111群的信息
            .Subscribe(async receiver =>
            {
                try
                {
                    GroupMsgListener.Business(receiver);
                }
                catch (Exception ex)
                {
                    var messageInfo = string.Join("", receiver.MessageChain.OfType<Mirai.Net.Data.Messages.Concretes.PlainMessage>().Select(a => a.Text));
                    LogHelper.Error("[群聊]QQ:" + receiver.Sender.Id + "文本:" + messageInfo + ex.ToString());
                }

            });

            //监听好友消息
            var FriendMsgListener = new FriendMessageListener(scheduler);
            bot.MessageReceived
                .OfType<FriendMessageReceiver>()
                .Subscribe(async receiver =>
                {
                    try
                    {
                        FriendMsgListener.Business(receiver);
                    }
                    catch (Exception ex)
                    {
                        var messageInfo = string.Join("", receiver.MessageChain.OfType<Mirai.Net.Data.Messages.Concretes.PlainMessage>().Select(a => a.Text));
                        LogHelper.Error("[私信]QQ:" + receiver.Sender.Id + "文本:" + messageInfo + ex.ToString());
                    }

                });

            //监听好友申请
            bot.EventReceived
                .OfType<NewFriendRequestedEvent>()
                .Subscribe(async receiver =>
                {
                    if (receiver.Message.Contains("1020"))//当申请信息的验证消息中存在1020则同意，否则拒绝
                    {
                        await RequestManager.HandleNewFriendRequestedAsync(receiver, NewFriendRequestHandlers.Approve);
                    }
                    else
                    {
                        await RequestManager.HandleNewFriendRequestedAsync(receiver, NewFriendRequestHandlers.Reject, "请在备注中输入正确的密码");
                    }
                });

            //监听群申请
            bot.EventReceived
               .OfType<NewInvitationRequestedEvent>()
               .Subscribe(async receiver =>
               {
                   //直接拒绝
                   await RequestManager.HandleNewInvitationRequestedAsync(receiver, NewInvitationRequestHandlers.Reject, " ");
               });

            //掉线重连事件
            bot.EventReceived
                .OfType<DroppedEvent>()
                .Subscribe(async receiver =>
                {
                    bot.Dispose();
                    Console.WriteLine("重新连接mcl，请稍后");
                    await bot.LaunchAsync();
                });

           

            try
            {
                Console.WriteLine("正在连接mcl，请稍后");
                await bot.LaunchAsync();
            }
            catch (Exception e)
            {
                LogHelper.Error("连接异常" +  e.ToString());
                return;
            }
            Console.WriteLine("启动成功");

            while (true)
            {
                if (await Console.In.ReadLineAsync() == "exit")
                {
                    var d = scheduler.GetJobKeys(Quartz.Impl.Matchers.GroupMatcher<JobKey>.GroupEquals("friend")).Result;
                    foreach (var item in d)
                    {
                        try
                        {
                            await MessageManager.SendFriendMessageAsync(item.Name, new Mirai.Net.Data.Messages.Concretes.PlainMessage("系统维护,您的任务已丢失，请于维护完成后重新设置"));

                        }
                        catch (Exception)
                        {
                            Console.WriteLine(item.Name + "提醒失败");
                        }
                    }
                    return;
                }
            }
        }
    }
}
