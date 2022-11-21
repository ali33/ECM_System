using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using System.Drawing;
using Ecm.Workflow.Activities.Contract;
using Ecm.Workflow.Activities.NotifyConfiguration;
using System.Net.Mail;
using Ecm.CaptureDomain;
using Ecm.CaptureCore;
using System.Configuration;
using System.Net;
using Ecm.Workflow.Activities.CustomActivityDomain;
using log4net;
using System.Windows.Interop;
using Ecm.Workflow.WorkflowExtension;

[assembly: log4net.Config.XmlConfigurator(Watch = true, ConfigFile = @"log4net.xml")]

namespace Ecm.Workflow.Activities.Notifier
{
    [Designer(typeof(NotifyDesigner))]
    [ToolboxBitmap(typeof(Notify), "bell.png")]
    public class Notify : StoppableActivityContract
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(Notify));

        [Editor(typeof(NotifyConfigurationDesigner), typeof(DialogPropertyValueEditor))]
        public Guid NofitySetting { get; set; }

        //[RequiredArgument]
        //public InArgument<Batch> BatchInfo { get; set; }

        public InArgument<string> ProductUrl { get; set; }

        //[RequiredArgument]
        //public InArgument<string> ProductUrl
        //{
        //    get { return BrowserInteropHelper.Source.ToString(); }
        //    set { _productUrl = value; }
        //}
        private readonly SecurityManager _securityManager = new SecurityManager();

        protected override void ExecutionBody(NativeActivityContext context)
        {
            try
            {
                var wfSystem = _securityManager.Authorize("WorkflowSystem", "TzmdoMVgNmQ5QMXJDuLBKgKg6CYfx73S/8dPX8Ytva+Eu3hlFNVoAg==");
                wfSystem.ClientHost = string.Empty;

                WorkflowRuntimeData runtimeData = GetWorkflowRuntimeData(context);
                Batch batch = new BatchManager(wfSystem).GetBatch(Guid.Parse(runtimeData.ObjectID.ToString()));
                ActionLogManager actionLog = new ActionLogManager(wfSystem);

                actionLog.AddLog("Begin notify process on batch Id: " + batch.Id, wfSystem, ActionName.SentNotify, null, null);

                User user = wfSystem;
                CustomActivitySetting setting = GetSetting(batch.WorkflowDefinitionId, user);
                NotifySettings notifySetting = null;

                if (setting != null)
                {
                    notifySetting = Utility.UtilsSerializer.Deserialize<NotifySettings>(setting.Value);

                    if (notifySetting != null)
                    {
                        if (notifySetting.NotifyType == (int)NotifyTypeEnum.Mail)//Send mail
                        {
                            SendMail(batch, notifySetting, context, user);
                            actionLog.AddLog("Send mail on batch Id: " + batch.Id + " completed successfully", wfSystem, ActionName.SentNotify, null, null);
                        }
                        else if (notifySetting.NotifyType == (int)NotifyTypeEnum.SMS)//Send SMS
                        {
                            //TODO: Send Nofify by SMS here
                            actionLog.AddLog("Send SMS message on batch Id: " + batch.Id + " completed successfully", wfSystem, ActionName.SentNotify, null, null);
                        }
                        else // Send Mail and SMS
                        {
                            //TODO: Send Nofify by SMS here
                            SendMail(batch, notifySetting, context, user);
                            actionLog.AddLog("Send mail and SMS message on batch Id: " + batch.Id + " completed successfully", wfSystem, ActionName.SentNotify, null, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void SendMail(Batch batch, NotifySettings notifySetting, NativeActivityContext context, User user)
        {
            foreach (string mailTo in notifySetting.MailInfo.MailTos.Split(';'))
            {
                string newLine = "<BR/>";
                User workItemUser = GetUserByEmail(mailTo, user);

                string link = GetSendLink(ProductUrl.Get(context), batch.Id, workItemUser);
                string mailBody = string.Format(notifySetting.MailInfo.Body, batch.BatchName + newLine, batch.BatchType.Name + newLine,
                    batch.CreatedDate.ToString() + newLine, batch.CreatedBy + newLine, batch.LastAccessedDate == null ? string.Empty : batch.LastAccessedDate.ToString() + newLine,
                    batch.LastAccessedBy == null ? string.Empty : batch.LastAccessedBy.ToString() + newLine, link + newLine);
                string subject = string.Format(notifySetting.MailInfo.Subject, batch.BatchName);

                SendMail(subject, notifySetting.MailInfo.MailFrom, mailTo, "",
                    mailBody, notifySetting.MailInfo.SmtpHostName, Convert.ToInt32(notifySetting.MailInfo.SmtpPortNumber),
                    notifySetting.MailInfo.SmtpUserName, notifySetting.MailInfo.SmtpPassword, context);
            }
        }

        private CustomActivitySetting GetSetting(Guid wfDefinitionId, User user)
        {
            Guid activityId = this.UniqueID;
            return new CustomActivitySettingManager(user).GetCustomActivitySetting(wfDefinitionId, activityId);
        }

        private void SendMail(string subject, string from, string to, string cc, string body, string hostname,
            int portNumber, string username, string password, NativeActivityContext context)
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
            msg.BodyEncoding = Encoding.Unicode;
            msg.IsBodyHtml = true;
            SmtpClient client = new SmtpClient(hostname, portNumber)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };
            client.Send(msg);
        }

        private string GetSendLink(string url, Guid batchId, User user)
        {
            const string queryTemplate = "mode=workitem&username={0}&workitemid={1}";
            //string password = Utility.UrlKeywordReplace.GenerateUrlData(user.Password);

            var encodedUri = new Uri("http://localhost/index.html?" + string.Format(queryTemplate, user == null ? string.Empty : user.UserName, batchId));
            string link = url + encodedUri.Query;
            return link;
        }

        private User GetUserByEmail(string mail, User loginUser)
        {
            UserManager userManager = new UserManager(loginUser);
            return userManager.GetUser(mail);
        }
    }
}
