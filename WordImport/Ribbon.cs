using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Office = Microsoft.Office.Core;
using System.Drawing;
using Ecm.WordImport;
using System.Resources;
using Microsoft.Office.Tools.Ribbon;
using WordImport;

// TODO:  Follow these steps to enable the Ribbon (XML) item:

// 1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new Ribbon1();
//  }

// 2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
//    actions, such as clicking a button. Note: if you have exported this Ribbon from the Ribbon designer,
//    move your code from the event handlers to the callback methods and modify the code to work with the
//    Ribbon extensibility (RibbonX) programming model.

// 3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.  

// For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.


namespace Ecm.WordImport
{
    [ComVisible(true)]
    public class Ribbon : Office.IRibbonExtensibility
    {
        private ResourceManager _resource = new ResourceManager("WordImport.Resources", Assembly.GetExecutingAssembly());
        private Office.IRibbonUI ribbon;

        public Ribbon(Action<AddinAction> buttonClick)
        {
            ButtonClick = buttonClick;
        }

        public Action<AddinAction> ButtonClick { get; set; }

        #region IRibbonExtensibility Members
        
        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("WordImport.Ribbon.xml");
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

        public void btnSendToArchive_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(AddinAction.SendToAcrchive);
            }
        }

        public void btnOpen_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(AddinAction.Open);
            }
        }

        public void btnSave_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(AddinAction.Save);
            }
        }

        public void btnSaveAs_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(AddinAction.SaveAs);
            }
        }

        public void btnDashboard_Click(Microsoft.Office.Core.IRibbonControl control)
        {
            if (ButtonClick != null)
            {
                ButtonClick(AddinAction.Dashboard);
            }
        }

        public bool SaveEnable(Microsoft.Office.Core.IRibbonControl control)
        {
            return Globals.ThisAddIn.CanSave;
        }

        public bool SaveAsEnable(Microsoft.Office.Core.IRibbonControl control)
        {
            return Globals.ThisAddIn.CanSave;
        }

        public bool DashboardEnable(Microsoft.Office.Core.IRibbonControl control)
        {
            return Globals.ThisAddIn.CanSave;
        }

        public Bitmap GetImage(Microsoft.Office.Core.IRibbonControl image)
        {
            if (image.Tag == "SendToArchive")
            {
                return new Bitmap((Bitmap)_resource.GetObject("Logo1"));
            }
            if (image.Tag == "Open")
            {
                return new Bitmap((Bitmap)_resource.GetObject("open-file"));
            }
            if (image.Tag == "Save")
            {
                return new Bitmap((Bitmap)_resource.GetObject("save"));
            }
            if (image.Tag == "SaveAs")
            {
                return new Bitmap((Bitmap)_resource.GetObject("save_as"));
            }
            if (image.Tag == "info")
            {
                return new Bitmap((Bitmap)_resource.GetObject("Dashboard"));
            }
            return null;

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

    public enum AddinAction
    {
        SendToAcrchive,
        Open,
        Save,
        SaveAs,
        Dashboard
    }
}
