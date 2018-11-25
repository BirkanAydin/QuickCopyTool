using GongSolutions.Wpf.DragDrop;
using OpenXml.Excel.Data; //https://github.com/gSerP1983/OpenXml.Excel.Data
using QuickCopyTool.Models;
using QuickCopyTool.Root;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace QuickCopyTool.ViewModels
{
    public class WindowDropHandler : DefaultDropHandler
    {
        public override void DragOver(IDropInfo dropInfo)
        {
            dropInfo.NotHandled = true;
        }

        public override void Drop(IDropInfo dropInfo)
        {
            dropInfo.NotHandled = true;
        }
    }

    public class QuickCopyViewModel : IDropTarget
    {
        XmlDocument doc;
        public ICommand StartButton { get; set; }
        public QuickCopy QuickCopy { get; set; }

        public QuickCopyViewModel()
        {
            QuickCopy = new QuickCopy();
            StartButton = new RelayCommand(o => Start());

        }

        private void Start()
        {
            if (QuickCopy.DataTableContextView != null && QuickCopy.DataTableContextView.Count > 0 && QuickCopy.Source != string.Empty)
            {
                string outputText = "";
               
                DataTable dataTable = new DataTable();
                dataTable=QuickCopy.DataTableContextView.ToTable();
                for(int i =0; i< dataTable.Rows.Count;i++)
                {
                    string tempVal = QuickCopy.Source;
                    for (int s =0; s < dataTable.Columns.Count;s++)
                    {
                        tempVal = tempVal.Replace("#"+dataTable.Columns[s].Caption+"#", dataTable.Rows[i][s].ToString());
                    }
                    outputText += tempVal + "\r\n";
                }
                File.WriteAllText(@"output.txt", outputText);
                Process.Start(@"output.txt");
            }
            else
            {
                MessageBox.Show("Something is empty");
            }
           
        }

        public void DragOver(IDropInfo dropInfo)
        {
            DragOver_Event(dropInfo);
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.VisualTarget.Uid == "lstContext")
            {
                ContextDrop_Event(dropInfo);

            }
            else if (dropInfo.VisualTarget.Uid == "txtSource")
            {
                SourceDrop_Event(dropInfo);
            }

        }

        private void SourceDrop_Event(IDropInfo e)
        {

            if (((DataObject)e.Data).GetDataPresent(DataFormats.FileDrop))
            {
                string[] docPath = (string[])((DataObject)e.Data).GetData(DataFormats.FileDrop);
                var dataFormat = DataFormats.Text;
                if (System.IO.File.Exists(docPath[0]))
                {
                    try
                    {
                        QuickCopy.Source = System.IO.File.ReadAllText(docPath[0], Encoding.GetEncoding("Windows-1254"));
                    }
                    catch (System.Exception)
                    {
                        MessageBox.Show("File could not be opened. Make sure the file is a text file.");
                    }
                }
            }
        }

        public void DragOver_Event(IDropInfo e)
        {
            if (((DataObject)e.Data).GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

        }

        private void ContextDrop_Event(IDropInfo e)
        {
            if (((DataObject)e.Data).GetDataPresent(DataFormats.FileDrop))
            {
                string[] docPath = (string[])((DataObject)e.Data).GetData(DataFormats.FileDrop);
                var dataFormat = DataFormats.Text;
                if (System.IO.File.Exists(docPath[0]))
                {
                    try
                    {
                        string docExt = System.IO.Path.GetExtension(docPath[0]);
                        if (docExt.ToLower() == GeneralVariable.FileTypes.Xaml)
                        {
                            XamlProcess(docPath[0]);
                        }
                        if (docExt.ToLower() == (GeneralVariable.FileTypes.Xls) || docExt.ToLower() == (GeneralVariable.FileTypes.Xlsx))
                        {
                            ExcelProcess(docPath[0]);
                        }

                    }
                    catch (System.Exception exc)
                    {
                        MessageBox.Show(exc.StackTrace);
                    }
                }
            }
        }

        private void XamlProcess(string docPath)
        {
            doc = new XmlDocument();
            doc.Load(docPath);
            QuickCopy.DataTableContext.Columns.Add("Var1");
            QuickCopy.DataTableContext.Columns.Add("Var2");
            QuickCopy.DataTableContext.Columns.Add("Var3");
            TraverseNodes(doc.ChildNodes);
            QuickCopy.DataTableContextView = QuickCopy.DataTableContext.DefaultView;

            void TraverseNodes(XmlNodeList nodes)
            {
                try
                {
                    foreach (XmlNode nodeElement in nodes)
                    {
                        if (nodeElement.Attributes != null)
                        {
                            foreach (XmlAttribute aItem in nodeElement.Attributes)
                            {
                                if (aItem.Value.Contains("Binding"))
                                {
                                    string[] split = aItem.Value.Split(',');
                                    foreach (string sItem in split)
                                    {
                                        if (sItem.Contains("Binding"))
                                        {

                                            string BindingName = sItem.Replace("Binding", "").Replace("{", "").Replace("}", "").Trim();
                                            string tt = "";
                                            if (aItem.OwnerElement.LocalName == "TextBox")
                                                tt = "String";
                                            else if (aItem.OwnerElement.LocalName == "ComboBox")
                                                tt = "List<>";
                                            else if (aItem.OwnerElement.LocalName == "CheckBox")
                                                tt = "Bool";
                                            else if (aItem.OwnerElement.LocalName == "DatePicker")
                                                tt = "DateTime";
                                            else if (aItem.OwnerElement.LocalName == "DataGrid")
                                                tt = "List<>";
                                            else if (aItem.OwnerElement.LocalName == "Label")
                                                tt = "String";
                                            if (BindingName.Trim() != string.Empty)
                                            {
                                                QuickCopy.ContextList.Add(aItem.OwnerElement.LocalName + "," + BindingName + "," + tt);
                                                DataRow dr = QuickCopy.DataTableContext.NewRow();
                                                dr[0] = aItem.OwnerElement.LocalName;
                                                dr[1] = BindingName;
                                                dr[2] = tt;
                                                QuickCopy.DataTableContext.Rows.Add(dr);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        TraverseNodes(nodeElement.ChildNodes);
                    }

                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.StackTrace);
                }
            }
        }

        private void ExcelProcess(string filePath)
        {
          DataTable  dt = new DataTable();
            using (var reader = new ExcelDataReader(filePath, 0, true))
                dt.Load(reader);
            QuickCopy.DataTableContextView = dt.DefaultView;
        }
    }
}
