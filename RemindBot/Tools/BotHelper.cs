using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemindBot
{
	public class BotHelper
    {
		public static DateTime? GetDateTimeByText(string text)
		{
			int num = 0;
			int num2 = 0;
			if (text.Contains("小时"))
			{
				int num3 = 0;
				int num4 = text.IndexOf("小时") - 1;
				while (num4 >= 0 && text[num4] >= '0' && text[num4] < ':')
				{
					num += (int)(text[num4] - '0') * (int)Math.Pow(10.0, (double)num3++);
					num4--;
				}
			}
			if (text.Contains("分"))
			{
				int num3 = 0;
				int num5 = text.IndexOf("分") - 1;
				while (num5 >= 0 && text[num5] >= '0' && text[num5] < ':')
				{
					num2 += (int)(text[num5] - '0') * (int)Math.Pow(10.0, (double)num3++);
					num5--;
				}
			}
			if (num == 0 && num2 == 0)
			{
				return null;
			}
			return new DateTime?(DateTime.Now.AddHours((double)num).AddMinutes((double)num2));
		}
	}
}
