using Ecm.CaptureCustomAddIn.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using Office = Microsoft.Office.Core;

// TODO:  Follow these steps to enable the Ribbon (XML) item:

// 1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new Ribbon();
//  }

// 2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
//    actions, such as clicking a button. Note: if you have exported this Ribbon from the Ribbon designer,
//    move your code from the event handlers to the callback methods and modify the code to work with the
//    Ribbon extensibility (RibbonX) programming model.

// 3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.  

// For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.


namespace ExcelCapture
{
    [ComVisible(true)]
    public class Ribbon : Office.IRibbonExtensibility
    {
        private ResourceManager _resource = new ResourceManager("ExcelCapture.Resources", Assembly.GetExecutingAssembly());
        private Office.IRibbonUI ribbon;

        public Ribbon(Action<CaptureAddinAction> buttonClick)
        {
            ButtonClick = buttonClick;
        }

        public Action<CaptureAddinAction> ButtonClick { get; set; }

        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("ExcelCapture.Ribbon.xml");
        }

        public void Invalidate()
        {
            ribbon.Invalidate();
        }
        #endregion

        #region Ribbon Callbacks
        //Create callback methods here. For more information about adding callback methods, select the Ribbon XML item in Solution Explorer and then press F1

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
        }

        public Bitmap GetImage(Microsoft.Office.Core.IRibbonControl image)
        {
            if (image.Tag == "SendToCapture")
            {
                return new Bitmap((Bitmap)_resource.GetObject("Logo"));
            }
            if (image.Tag == "AssignedTask")
            {
                return new Bitmap((Bitmap)_resource.GetObject("task"));
            }
            if (image.Tag == "SaveWorkitem")
            {
                return new Bitmap((Bitmap)_resource.GetObject("save"));
            }
            if (image.Tag == "Approve")
            {
                return new Bitmap((Bitmap)_resource.GetObject("approve"));
            }
            if (image.Tag == "Reject")
            {
                return new Bitmap((Bitmap)_resource.GetObject("reject"));
            }
            if (image.Tag == "CaptureInfo")
            {
                return new Bitmap((Bitmap)_resource.GetObject("Dashboard"));
            }
            if (image.Tag == "Logout")
            {
                return new Bitmap((Bitmap)_resource.GetObject("exit"));
            }
            return null;

        }

        public void btnSendToCapture_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(CaptureAddinAction.SendToCapture);
            }
        }

        public void btnOpenAssignedTask_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(CaptureAddinAction.Open);
            }
        }

        public void btnSaveWorkitem_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(CaptureAddinAction.Save);
            }
        }

        public void btnApprove_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(CaptureAddinAction.Approver);
            }
        }

        public void btnReject_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(CaptureAddinAction.Rejected);
            }
        }

        public void btnCaptureInfo_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(CaptureAddinAction.Dashboard);
            }
        }

        public void btnLogout_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(CaptureAddinAction.Logout);
            }
        }

        public bool SaveWorkitemEnable(Microsoft.Office.Core.IRibbonControl control)
        {
            return Globals.ThisAddIn.CanSave;
        }

        public bool ApproveEnable(Microsoft.Office.Core.IRibbonControl control)
        {
            return Globals.ThisAddIn.CanSave;
        }

        public bool RejectEnable(Microsoft.Office.Core.IRibbonControl control)
        {
            return Globals.ThisAddIn.CanSave;
        }

        public bool CaptureDashboardEnable(Microsoft.Office.Core.IRibbonControl control)
        {
            return Globals.ThisAddIn.CanSave;
        }

        public bool LogoutEnable(Microsoft.Office.Core.IRibbonControl control)
        {
            return LoginViewModel.LoginUser != null;
        }

        public bool CaptureVisible(Microsoft.Office.Core.IRibbonControl control)
        {
            return true;
        }
        #endregion

        #region Helpers
        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }

    public enum CaptureAddinAction
    {
        SendToCapture,
        Open,
        Save,
        Approver,
        Rejected,
        Dashboard,
        Logout
    }
}
