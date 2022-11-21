using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace Ecm.Utility
{
    public class UtilsMapi
    {
        private enum HowTo
        {
            MapiTo
        };

        public void AddAttachment(string strAttachmentFileName)
        {
            _attachments.Add(strAttachmentFileName);
        }

        public bool AddRecipientBcc(string email)
        {
            return AddRecipient(email, HowTo.MapiTo);
        }

        public bool AddRecipientCc(string email)
        {
            return AddRecipient(email, HowTo.MapiTo);
        }

        public bool AddRecipientTo(string email)
        {
            return AddRecipient(email, HowTo.MapiTo);
        }

        public string GetLastError()
        {
            if (_lastError <= 26)
            {
                return _errors[_lastError];
            }
            return "MAPI error [" + _lastError.ToString() + "]";
        }

        public int SendMailDirect(string strSubject, string strBody)
        {
            return SendMail(strSubject, strBody, MAPI_LOGON_UI);
        }

        public int SendMailPopup(string strSubject, string strBody)
        {
            return SendMail(strSubject, strBody, MAPI_LOGON_UI | MAPI_DIALOG);
        }

        [DllImport("MAPI32.DLL")]
        private static extern int MAPISendMail(IntPtr sess, IntPtr hwnd, MapiMessage message, int flg, int rsv);

        private bool AddRecipient(string email, HowTo howTo)
        {
            var recipient = new MapiRecipDesc { recipClass = (int)howTo, name = email };
            _recipients.Add(recipient);
            return true;
        }

        private void Cleanup(ref MapiMessage msg)
        {
            int size = Marshal.SizeOf(typeof(MapiRecipDesc));
            int ptr;

            if (msg.recips != IntPtr.Zero)
            {
                ptr = (int)msg.recips;
                for (int i = 0; i < msg.recipCount; i++)
                {
                    Marshal.DestroyStructure((IntPtr)ptr, typeof(MapiRecipDesc));
                    ptr += size;
                }

                Marshal.FreeHGlobal(msg.recips);
            }

            if (msg.files != IntPtr.Zero)
            {
                size = Marshal.SizeOf(typeof(MapiFileDesc));

                ptr = (int)msg.files;
                for (int i = 0; i < msg.fileCount; i++)
                {
                    Marshal.DestroyStructure((IntPtr)ptr, typeof(MapiFileDesc));
                    ptr += size;
                }
                Marshal.FreeHGlobal(msg.files);
            }

            _recipients.Clear();
            _attachments.Clear();
            _lastError = 0;
        }

        private IntPtr GetAttachments(out int fileCount)
        {
            fileCount = 0;
            if (_attachments == null)
            {
                return IntPtr.Zero;
            }

            if ((_attachments.Count <= 0) || (_attachments.Count > maxAttachments))
            {
                return IntPtr.Zero;
            }

            int size = Marshal.SizeOf(typeof(MapiFileDesc));
            IntPtr intPtr = Marshal.AllocHGlobal(_attachments.Count * size);

            var mapiFileDesc = new MapiFileDesc { position = -1 };
            var ptr = (int)intPtr;

            foreach (string strAttachment in _attachments)
            {
                mapiFileDesc.name = Path.GetFileName(strAttachment);
                mapiFileDesc.path = strAttachment;
                Marshal.StructureToPtr(mapiFileDesc, (IntPtr)ptr, false);
                ptr += size;
            }

            fileCount = _attachments.Count;
            return intPtr;
        }

        private IntPtr GetRecipients(out int recipCount)
        {
            recipCount = 0;
            if (_recipients.Count == 0)
            {
                return IntPtr.Zero;
            }

            int size = Marshal.SizeOf(typeof(MapiRecipDesc));
            IntPtr intPtr = Marshal.AllocHGlobal(_recipients.Count * size);

            var ptr = (int)intPtr;
            foreach (MapiRecipDesc mapiDesc in _recipients)
            {
                Marshal.StructureToPtr(mapiDesc, (IntPtr)ptr, false);
                ptr += size;
            }

            recipCount = _recipients.Count;
            return intPtr;
        }

        private int SendMail(string strSubject, string strBody, int how)
        {
            var msg = new MapiMessage { subject = strSubject, noteText = strBody };
            msg.recips = GetRecipients(out msg.recipCount);
            msg.files = GetAttachments(out msg.fileCount);
            _lastError = MAPISendMail(new IntPtr(0), new IntPtr(0), msg, how, 0);
            if (_lastError > 1)
            {
                throw new Exception("Send mail failed! " + GetLastError());
            }

            Cleanup(ref msg);
            return _lastError;
        }

        private const int MAPI_DIALOG = 0x00000008;

        private const int MAPI_LOGON_UI = 0x00000001;

        private const int maxAttachments = 20;

        private readonly List<string> _attachments = new List<string>();

        private readonly string[] _errors = new[]
            {
                "OK [0]", "User abort [1]", "General MAPI failure [2]", "MAPI login failure [3]", "Disk full [4]",
                "Insufficient memory [5]", "Access denied [6]", "-unknown- [7]", "Too many sessions [8]",
                "Too many files were specified [9]", "Too many recipients were specified [10]",
                "A specified attachment was not found [11]", "Attachment open failure [12]",
                "Attachment write failure [13]", "Unknown recipient [14]", "Bad recipient type [15]", "No messages [16]",
                "Invalid message [17]", "Text too large [18]", "Invalid session [19]", "Type not supported [20]",
                "A recipient was specified ambiguously [21]", "Message in use [22]", "Network failure [23]",
                "Invalid edit fields [24]", "Invalid recipients [25]", "Not supported [26]"
            };

        private readonly List<MapiRecipDesc> _recipients = new List<MapiRecipDesc>();

        private int _lastError;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiMessage
    {
        public int reserved;

        public string subject;

        public string noteText;

        public string messageType;

        public string dateReceived;

        public string conversationID;

        public int flags;

        public IntPtr originator;

        public int recipCount;

        public IntPtr recips;

        public int fileCount;

        public IntPtr files;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiFileDesc
    {
        public int reserved;

        public int flags;

        public int position;

        public string path;

        public string name;

        public IntPtr type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiRecipDesc
    {
        public int reserved;

        public int recipClass;

        public string name;

        public string address;

        public int eIDSize;

        public IntPtr entryID;
    }
}