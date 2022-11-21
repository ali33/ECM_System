using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace Ecm.Utility
{
    public class UtilsMail
    {
        public static void SendMail(string subject, string from, string to, string cc, string body, string hostname, int portNumber, string username, string password)
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(from);

            string[] tos = to.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string addr in tos)
            {
                string trimedAddr = addr.Trim();
                if (trimedAddr != string.Empty)
                {
                    msg.To.Add(trimedAddr);
                }
            }

            cc += ";" + from;
            string[] ccs = cc.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string addr in ccs)
            {
                string trimedAddr = addr.Trim();
                if (trimedAddr != string.Empty)
                {
                    msg.CC.Add(trimedAddr);
                }
            }

            msg.Subject = subject;
            msg.Body = body;
            msg.IsBodyHtml = true;
            SmtpClient client = new SmtpClient(hostname, portNumber)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };
            client.Send(msg);
        }

    }
}
