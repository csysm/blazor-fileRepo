using MimeKit;
using MailKit.Net.Smtp;
namespace FileRepoSys.Api.Util
{
    public class Mailhelper
    {
        public static void SendMail(string userMail, string userName, string activeLink)//收件方邮箱，收件方名称，激活连接
        {
            //编辑邮件信息
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("xqqqq的文件管理服务", "593676339@qq.com"));//发件方
            message.To.Add(new MailboxAddress(userName, userMail));//收件方
            message.Subject = "Activates your account ";//邮件标题
            message.Body = new TextPart("plain")
            {
                Text = @"Please click the link to activate your account:" + activeLink//邮件内容
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.qq.com", 465, true);//这里填写QQ邮箱stmp服务地址，端口465，是否开启SSL为true
                                                         // Note: only needed if the SMTP server requires authentication
                client.Authenticate("593676339@qq.com", "mhjbjdkixnicbcfi");//填写发件方的邮箱,获取的授权码
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
