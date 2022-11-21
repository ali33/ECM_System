using Ecm.Core;
using Ecm.Domain;
using Ecm.ImportData.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using log4net;
using Ecm.ImportData.Model;
using System.Windows.Forms;
using System.Windows.Forms;
using Ecm.ImportData.Model;
using log4net;


namespace Ecm.ImportData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();

            Login login = new Login();
            login.ShowDialog();

            while (!login.IsCancel)
            {
                if (login.IsGetValue)
                {
                    SecurityManager sec = new SecurityManager();
                    user = sec.Login(login.UserName, login.Password, "");
                    if (user != null)
                    {
                        DocTypeManager d = new DocTypeManager(user);
                        docTypes = d.GetDocumentTypes();
                        docTypes.Insert(0, new DocumentType()
                        {
                            Id = Guid.Empty,
                            Name = "--Tất cả--"
                        });
                        cmbDocType.ItemsSource = docTypes;
                        
                        break;
                    }
                    else
                    {
                        login = new Login();
                        login.ShowDialog();
                    }
                }
                else
                {
                    login = new Login();
                    login.ShowDialog();
                }
            }
            if (login.IsCancel)
                this.Close();
            
           
        }
        string str = "";
        ObservableCollection<ItemDocType> list = new ObservableCollection<ItemDocType>();
        ImportModel model = new ImportModel();
        ItemDocType currenItem = new ItemDocType();
        List<DocumentType> docTypes = new List<DocumentType>();
        User user = new User();
        
        void importDir_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
            //    str += sender.ToString() + Environment.NewLine; ;
            }
            else
            {
                
                //str += sender.ToString() + Environment.NewLine;
            }
           
            
            currenItem = list.Where(p => p.backgroundWorker.Equals(sender)).FirstOrDefault();
            currenItem.Progress = 100;
            currenItem.Status = Ecm.ImportData.ViewModel.Status.Stop;
        }

        void importDir_DoWork(object sender, DoWorkEventArgs e)
        {
            

            currenItem = list.Where(p => p.backgroundWorker.Equals(sender)).FirstOrDefault();
            /*
            string indexName = "[index].ini";
            System.IO.DirectoryInfo dir = (System.IO.DirectoryInfo)e.Argument;
            str += "[" + dir.FullName + "]" + Environment.NewLine;
            //   List1.Items.Add("[" + dir.FullName + "]" + Environment.NewLine);
            foreach (System.IO.FileInfo file in dir.GetFiles(indexName))
            {
                str += "[" + file.FullName + "]" + Environment.NewLine;
                //  List1.Items.Add("[" + file.FullName + "]" + Environment.NewLine);
            }
            FileInfo[] files = dir.GetFiles();
            int count = 0;
            foreach (System.IO.FileInfo file in files)
            {
                if (file.Name != indexName)
                {
                    count++;
                    currenItem.Progress = count * 100 / files.Count();
                    str += "[" + file.FullName + "]" + Environment.NewLine;
                    //  List1.Items.Add("[" + file.FullName + "]" + Environment.NewLine);
                }
            }
            */
            //xu ly file
            DocumentManager docMn = new DocumentManager(user);
            
          

            FileInfo[] fileIndex = currenItem.directoryInfo.GetFiles("[index].ini");
            string indexini = fileIndex[0].FullName;
            int i = 0;
            DirectoryInfo[]  dirs= currenItem.directoryInfo.GetDirectories();

            _log.Info(String.Format("Begin insert Document Type {0} ID: {{{1}}} - Path=[{2}]" + Environment.NewLine + "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", currenItem.documentType.Name, currenItem.documentType.Id, currenItem.directoryInfo.FullName));
            foreach (DirectoryInfo dir in dirs)
            {
                i++;
                _log.Info(String.Format("Begin insert Document {0}, Name:[{1}] - Doctype:[{2}] ID: {{{3}}}", i, dir.Name, currenItem.documentType.Name, currenItem.documentType.Id));

                FileInfo[] files = dir.GetFiles();
                if (files.Count() <= 0)
                    continue;
                
                Document doc = new Document();
                doc.DocumentType = currenItem.documentType;
                    doc.DocTypeId= currenItem.documentType.Id;
                doc.PageCount = files.Count();
                
                bool isNative = files.Where(p => Contents.StringExtension_Native.Contains(p.Extension.ToLower())).Any();
                bool isImage = files.Where(p => Contents.StringExtension_Image.Contains(p.Extension.ToLower())).Any();
                bool isMedia = files.Where(p => Contents.StringExtension_Media.Contains(p.Extension.ToLower())).Any();

                //Nếu có trên 2 kiểu dữ liệu khác nhau thì BinaryType="Compound"
                int count=0;
                count += isNative ? 1 : 0;
                count += isImage ? 1 : 0;
                count += isMedia ? 1 : 0;

                if (count > 1)
                    doc.BinaryType = Contents.Compound;
                else
                    doc.BinaryType = isNative ? Contents.Native : (isImage ? Contents.Image : isMedia ? Contents.Media : Contents.Text);
                                

                ReadIniFile iniFIle = new ReadIniFile(indexini);
               

                foreach (FieldMetaData field in currenItem.documentType.FieldMetaDatas.Where(p => !p.IsSystemField))
                {
                    FieldValue fieldValue=new FieldValue();
                    fieldValue.Value = iniFIle.IniReadValue(dir.Name, field.Name);
                    fieldValue.FieldId = field.Id;
                    fieldValue.FieldMetaData = field;
                    doc.FieldValues.Add(fieldValue);
                }
                int pageNumber = 0;
                foreach (FileInfo file in files)
                {
                    pageNumber++;

                    Ecm.Domain.Page page =new Domain.Page();
                    page.CreatedBy = user.UserName;
                    page.CreatedDate = DateTime.Now;
                    page.PageNumber = pageNumber;                    
                    page.DocTypeId = currenItem.documentType.Id;
                    page.FileBinary = File.ReadAllBytes(file.FullName);
                    page.FileExtension = file.Extension;
                    doc.Pages.Add(page);
                }
                Guid guid = docMn.InsertDocument(doc);
                
                //Note lại Doc dang xử lý
                iniFIle.IniWriteValue(dir.Name, Contents.SessionInsertSuccess, guid.ToString() + Contents.SessionInsertNotes);

                

                if (guid != null && guid != Guid.Empty)
                {
                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            _log.Info(String.Format("Delete file [{0}]", file.FullName));
                            File.Delete(file.FullName);
                        }
                        catch(Exception ex)
                        {
                            _log.Error(String.Format("Delete file [{0}] fault" + Environment.NewLine + "Exception:[{1}] ", file.FullName, ex.Message));
                        }
                    }
                }
                try
                {
                    _log.Info(String.Format("Delete folder {0} Document {1}, DocType:[{2}] ,ID: {{{3}}}", currenItem.documentType.Name, i, currenItem.documentType.Name, currenItem.documentType.Id));
                    dir.Delete(true);
                }
                catch (Exception ex)
                {
                    _log.Error(String.Format("Delete folder {0} Document {1}, DocType:[{1}] ,ID: {{{2}}} fault" + Environment.NewLine + "Exception:[{3}] " + Environment.NewLine + "Exception:[{3}] ", currenItem.documentType.Name, i, currenItem.documentType.Id, ex.Message));

                }

                _log.Info(String.Format("End insert Document {0} Name:[{1}], ID:{{{2}}} - DocType:[{3}], ID: {{{4}}}" + Environment.NewLine + "-------------------------------------------------", i, dir.Name, guid, currenItem.documentType.Name, currenItem.documentType.Id));

                //Note lại Doc dang xử lý
                iniFIle.IniWriteValue(dir.Name, Contents.SessionInsertSuccess, guid.ToString());
                currenItem.Progress = i * 100 / dirs.Count();
            }
            _log.Info(String.Format("End insert Document Type {0} ID: {{{1}}} - Path=[{2}]"+Environment.NewLine+"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", currenItem.documentType.Name, currenItem.documentType.Id, currenItem.directoryInfo.FullName));

            //

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            currenItem = ((FrameworkElement)sender).DataContext as ItemDocType;
            
            ShowDetailDocType d = new ShowDetailDocType(currenItem, user);
            d.ShowDialog();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {


            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.Description = Contents.FolderBrowserDialogDescription;            
            dialog.SelectedPath = @"C:\RootDir";

            DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
            {
                list = new ObservableCollection<ItemDocType>();
                txtPath.Text = dialog.SelectedPath;

                System.IO.DirectoryInfo dirs = new System.IO.DirectoryInfo(txtPath.Text);
                foreach (System.IO.DirectoryInfo dir in dirs.GetDirectories())
                {
                    bool co = false;
                    foreach (DocumentType doctype in docTypes.Where(p => p.Id != Guid.Empty))
                    {

                        if (doctype.Name.Equals(dir.Name))
                        {

                            var importDir = new BackgroundWorker();

                            importDir.DoWork += importDir_DoWork;
                            importDir.RunWorkerCompleted += importDir_RunWorkerCompleted;
                            importDir.WorkerSupportsCancellation = true;
                            ItemDocType item = new ItemDocType()
                            {
                                backgroundWorker = importDir,
                                directoryInfo = dir,
                                documentType = doctype,
                                Status = Ecm.ImportData.ViewModel.Status.Stop,
                                Progress = 0
                            };
                            list.Add(item);
                            co = true;

                        }
                    }

                    if (!co)
                    {
                        var importDir = new BackgroundWorker();

                        importDir.DoWork += importDir_DoWork;
                        importDir.RunWorkerCompleted += importDir_RunWorkerCompleted;
                        ItemDocType item = new ItemDocType()
                        {
                            backgroundWorker = importDir,
                            directoryInfo = dir,
                            documentType = new DocumentType() { Id = Guid.Empty, Name = Contents.Unmapping },
                            Status = Ecm.ImportData.ViewModel.Status.Stop,
                            Progress = 0
                        };
                        list.Add(item);
                    }
                }

                List1.ItemsSource = list;
            }


        }

        private void cmbDocType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] != null && e.AddedItems[0] is DocumentType)
            {
                DocumentType doctype = (DocumentType)e.AddedItems[0];
                if (doctype.Id == Guid.Empty)
                {
                    List1.ItemsSource = list;
                }
                else
                {
                    List1.ItemsSource = list.Where(p => p.documentType.Id == doctype.Id).ToList();
                }
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (currenItem != null &&
                currenItem.backgroundWorker != null &&
                currenItem.backgroundWorker.IsBusy)
            {
                currenItem.backgroundWorker.CancelAsync();
            }
        }

        private void List1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currenItem = (ItemDocType)List1.SelectedItem;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            currenItem = ((FrameworkElement)sender).DataContext as ItemDocType;
            if (currenItem != null &&
                currenItem.backgroundWorker != null &&
                !currenItem.backgroundWorker.IsBusy)
            {
                if (!Guid.Empty.Equals(currenItem.documentType.Id))
                {
                    currenItem.backgroundWorker.RunWorkerAsync(currenItem.directoryInfo);
                    currenItem.Status = ViewModel.Status.Running;
                    //((System.Windows.Controls.Button)sender).Content = "Stop";
                }
                else
                {
                    System.Windows.MessageBox.Show(Contents.MessageUmapping, Contents.MessageUmappingTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                

            }
            else
            {
                currenItem.Status = ViewModel.Status.Stop;
                //((System.Windows.Controls.Button)sender).Content = "Start";
                currenItem.backgroundWorker.CancelAsync();
            }

        }

        private void ShowLog_Click(object sender, RoutedEventArgs e)
        {
           // urrenItem = ((FrameworkElement)sender).DataContext as ItemDocType;

            ShowLog d = new ShowLog();//(currenItem, user);
            d.ShowDialog();
        }


    }
}
