using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Quartz;

namespace RemindBot.Listener
{
	public class GroupMessageListener
	{
		private IScheduler scheduler;
		public GroupMessageListener(IScheduler scheduler)
		{
			this.scheduler = scheduler;
		}
		public void Business(GroupMessageReceiver messenger)
		{
			if (messenger.MessageChain.OfType<AtMessage>().Where(it => it.Target == MiraiBot.Instance.QQ).Any())
			{
				string text = string.Join("", messenger.MessageChain.OfType<PlainMessage>().Select(a => a.Text));
				if (text.Contains("提醒"))
				{
					string[] array = text.Split("提醒");
					DateTime? dateTime = BotHelper.GetDateTimeByText(array[0]);
					if (dateTime == null)
					{
						MessageManager.SendGroupMessageAsync(messenger.Sender.Group.Id, new PlainMessage("时间设置错误"));
						return;
					}
					string dosth = array[1].Replace("我", "");

					JobKey jobKey = new JobKey(messenger.Sender.Id, "group");//把申请者的qq当作key加入group分组中
					var jobDetail = JobBuilder.Create<SendMessageJob>()
								.WithIdentity(jobKey)
								.SetJobData(new JobDataMap() {
								new KeyValuePair<string, object>("Id", messenger.Sender.Id),
								new KeyValuePair<string, object>("Msg",dosth)
								}).Build();

					bool IsExist = scheduler.GetJobDetail(jobKey).Result != null;
					if (IsExist)
					{
						IsExist = scheduler.DeleteJob(jobKey).Result;
					}
					ITrigger trigger = TriggerBuilder.Create().StartAt(new DateTimeOffset(dateTime.Value)).Build();
					this.scheduler.ScheduleJob(jobDetail, trigger, default(CancellationToken));
					PlainMessage plainMessage;
					if (IsExist)
					{
						plainMessage = new PlainMessage("队列中的上一个提醒已取消，将会在" + dateTime.Value.ToString("HH:mm:ss") + "提醒您" + dosth);
					}
					else
					{
						plainMessage = new PlainMessage("好的，将会在" + dateTime.Value.ToString("HH:mm:ss") + "提醒您" + dosth);
					}
					MessageManager.SendGroupMessageAsync(messenger.Sender.Group.Id, plainMessage);
					Console.WriteLine("群：" + messenger.Sender.Id + "已加入队列");
					return;
				}
				else
				{
					MessageManager.SendGroupMessageAsync(messenger.Sender.Group.Id, new MessageBase[]
					{
						new PlainMessage("无法理解您说了什么")
					});
				}
			}
		}
		
	}
}
