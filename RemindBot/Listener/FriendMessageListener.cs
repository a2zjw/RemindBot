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
	public class FriendMessageListener
	{
		private IScheduler scheduler;
		public FriendMessageListener(IScheduler scheduler)
		{
			this.scheduler = scheduler;
		}
		public void Business(FriendMessageReceiver messenger)
		{
			if (messenger.Sender.Id == MiraiBot.Instance.QQ)
			{
				return;//忽略自己给自己发信息 重要 不要删
			}
			string text = string.Join("", messenger.MessageChain.OfType<PlainMessage>().Select(a => a.Text));//获取发送的文本
			if (!text.Contains("提醒"))
			{
				MessageManager.SendFriendMessageAsync(messenger.Sender.Id, new PlainMessage("无法理解您说了什么"));
				return;
			}
			string[] array = text.Split("提醒");
			DateTime? dateTime = BotHelper.GetDateTimeByText(array[0]);//设置提醒时间
			if (dateTime == null)
			{
				MessageManager.SendFriendMessageAsync(messenger.Sender.Id, new PlainMessage("时间设置错误"));
				return;
			}
			string dosth = array[1].Replace("我", "");

			JobKey jobKey = new JobKey(messenger.Sender.Id, "friend");//把申请者的qq当作key加入friend分组中
			var jobDetail = JobBuilder.Create<SendMessageJob>()
						.WithIdentity(jobKey)
						.SetJobData(new JobDataMap() {
								new KeyValuePair<string, object>("Id", messenger.Sender.Id),
								new KeyValuePair<string, object>("Msg",dosth)
						}).Build();

			bool IsExist = scheduler.GetJobDetail(jobKey).Result != null; //判断调度器中有无该人的任务
			if (IsExist)
			{
				IsExist = scheduler.DeleteJob(jobKey).Result;//如果有，删除旧任务
			}

			ITrigger trigger = TriggerBuilder.Create().StartAt(new DateTimeOffset(dateTime.Value)).Build();//创建触发器
			scheduler.ScheduleJob(jobDetail, trigger);//加入任务

			PlainMessage plainMessage;
			if (IsExist)
			{
				plainMessage = new PlainMessage("队列中的上一个提醒已取消，将会在" + dateTime.Value.ToString("HH:mm:ss") + "提醒您" + dosth);
			}
			else
			{
				plainMessage = new PlainMessage("好的，将会在" + dateTime.Value.ToString("HH:mm:ss") + "提醒您" + dosth);
			}
			MessageManager.SendFriendMessageAsync(messenger.Sender.Id, plainMessage);
			Console.WriteLine("好友：" + messenger.Sender.Id + "已加入队列");
		}

	}
}
