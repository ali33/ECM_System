using Ecm.Core;
using Ecm.CustomControl;
using Ecm.Domain;
using Ecm.ImportData.Model;
using Ecm.ImportData.ViewModel;
using Ecm.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace Ecm.ImportData
{
    /// <summary>
    /// Interaction logic for ShowDetailDocType.xaml
    /// </summary>
    public partial class ShowDetailDocType : Window
    {
        public ShowDetailDocType()
        {
            InitializeComponent();
        }
        DataTable tbl = new DataTable();
        string indexini = "";
        public ShowDetailDocType(ItemDocType item,User user):this()
        {
            InitializeComponent();
            Item = item;            
           
            
            tbl.Columns.Add(Contents.ColumnDocName).ReadOnly = true ;
            foreach (FieldMetaData field in item.documentType.FieldMetaDatas.Where(p=>!p.IsSystemField))
            {
                tbl.Columns.Add(field.Name);
            }
            tbl.Columns.Add(Contents.ColumnPageName).ReadOnly = true;


            FileInfo[] fileIndex = item.directoryInfo.GetFiles(Contents.FileIniName);
            if (fileIndex == null || fileIndex.Count() <= 0)
            {
                indexini = System.IO.Path.Combine(Item.directoryInfo.FullName, Contents.FileIniName);
                File.WriteAllText(indexini, "", Encoding.Unicode);
            }
            else
            {
                indexini = fileIndex[0].FullName;
            }
            foreach (DirectoryInfo dir in item.directoryInfo.GetDirectories())
            {
                ReadIniFile iniFIle = new ReadIniFile(indexini);
                var row1 = tbl.NewRow();
                row1[Contents.ColumnDocName] = dir.Name;
                row1[Contents.ColumnPageName] = string.Join(",", dir.GetFiles().Select(p => String.Format(Contents.FormatFileName, p.Name)).ToList());
                foreach (FieldMetaData field in item.documentType.FieldMetaDatas.Where(p => !p.IsSystemField))
                {
                    string d = iniFIle.IniReadValue(dir.Name, field.Name);

                    row1[field.Name] = d;

                }
                tbl.Rows.Add(row1);
            }


            /*
            var row1 = tbl.NewRow();
            row1["Test"] = "dsjfks";
            tbl.Rows.Add(row1);
            row1 = tbl.NewRow();

            row1["Test"] = "dsjfks";
            tbl.Rows.Add(row1);
            */



           
            Table.ItemsSource =tbl.DefaultView;
        }

        private ItemDocType itemDocType;
        public ItemDocType Item
        {
            get
            {
                return itemDocType;
            }
            set
            {
                itemDocType = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteOldDataInIndexFile.IsChecked ?? false)
            {
                File.WriteAllText(indexini, "", Encoding.Unicode);
            }

            ReadIniFile readIni = new ReadIniFile(indexini);
            foreach (DataRow row in tbl.Rows)
            {
                if (row.RowState == DataRowState.Modified)
                {
                    foreach (FieldMetaData field in Item.documentType.FieldMetaDatas.Where(p => !p.IsSystemField))
                    {
                        readIni.IniWriteValue(row[Contents.ColumnDocName].ToString(), field.Name, row[field.Name].ToString());
                    }
                }
                else
                {
                    if (row.RowState == DataRowState.Added && (AddNewDoc.IsChecked ?? false))
                    {
                        Directory.CreateDirectory(System.IO.Path.Combine(Item.directoryInfo.FullName, row[Contents.ColumnDocName].ToString()));
                        foreach (FieldMetaData field in Item.documentType.FieldMetaDatas.Where(p => !p.IsSystemField))
                        {
                            readIni.IniWriteValue(row[Contents.ColumnDocName].ToString(), field.Name, row[field.Name].ToString());
                        }
                    }
                }
            }
            if (MessageBox.Show(Contents.MessageProcessSuccess, Contents.MessageProcessSuccessTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(Contents.ProcessNameShowFolder, Item.directoryInfo.FullName);
                
            }

        }

        private void AddNewDoc_Checked(object sender, RoutedEventArgs e)
        {
            if (AddNewDoc.IsChecked ?? false)
            {
                tbl.Columns[Contents.ColumnDocName].ReadOnly = false;
                
            }
            else
            {
                tbl.Columns[Contents.ColumnDocName].ReadOnly = true;            
            }
            Table.ItemsSource = null;
            Table.ItemsSource = tbl.DefaultView;
        }

        
        
    }
}
