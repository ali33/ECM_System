using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;

namespace ArchiveMVC.Utility
{
    public delegate void MailProgress();
    public class MailHelper
    {
        #region Mail member
        private MailAddress from;
        private MailAddress to;
        private MailMessage Message = new MailMessage();
        private SmtpClient smtp;
        private static string HostGmail = "smtp.gmail.com";
        private static string HostYahoo = "smtp.mail.yahoo.com";
        private static string HostLive = "smtp.live.com";
        public string MailAddress_From
        {
            set { from = new MailAddress(value); Message.From = from; }
            get { return from.Address; } 
        }
        public string DisplayName_From 
        {
            set { from = new MailAddress(from.Address, value); Message.From = from; }
            get { return from.DisplayName; }
        }
        public string MailAddress_To 
        {
            set { to = new MailAddress(value); Message.To.Add(to); }
            get { return to.Address; }
        }
        public string Subject 
        { 
            set { Message.Subject = value; }
            get { return Message.Subject; }
        }
        public string Body
        {
            set { Message.Body = value; }
            get { return Message.Body; }
        }
        public SmtpClient SMTP
        {
            set { smtp = value; }
            get { return smtp; }
        }
        #endregion
        public MailHelper(string _username, string _password)
        {
            ConfigDefaultSMTP(_username,_password);
        }
        public MailHelper()
        {
            
        }
        private void ConfigDefaultSMTP(string user,string pass)
        {
            try
            {
                smtp = new SmtpClient
                {
                    Host = GetHost(user),
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(user, pass),
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Send()
        {
            try
            {
                smtp.Send(Message);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static string GetHost(string address)
        {
            string domain = address.Substring(address.IndexOf('@') + 1);
            if (domain.StartsWith("gmail"))
                return HostGmail;
            else if (domain.StartsWith("yahoo"))
                return HostYahoo;
            else if (domain.StartsWith("hotmail") || domain.StartsWith("outlook"))
                return HostLive;
            return HostGmail;
        }
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Attachment(MailMessage message, string attachmentFilename)
        {
            Attachment attachment = new Attachment(attachmentFilename, MediaTypeNames.Application.Octet);
            ContentDisposition disposition = attachment.ContentDisposition;
            disposition.CreationDate = File.GetCreationTime(attachmentFilename);
            disposition.ModificationDate = File.GetLastWriteTime(attachmentFilename);
            disposition.ReadDate = File.GetLastAccessTime(attachmentFilename);
            disposition.FileName = Path.GetFileName(attachmentFilename);
            disposition.Size = new FileInfo(attachmentFilename).Length;
            disposition.DispositionType = DispositionTypeNames.Attachment;
            message.Attachments.Add(attachment);
        }
        private void Attachment(MailMessage message, Stream attachmentStream)
        {
            Attachment attachment = new Attachment(attachmentStream, MediaTypeNames.Application.Octet);
            message.Attachments.Add(attachment);
        }
        public void Attach(Stream stream)
        {
            Attachment attachment = new Attachment(stream, MediaTypeNames.Application.Octet);
            Message.Attachments.Add(attachment);
        }
        public void Attach(string fileName)
        {
            Attachment attachment = new Attachment(fileName, MediaTypeNames.Application.Octet);
            Message.Attachments.Add(attachment);
        }
    }
}
