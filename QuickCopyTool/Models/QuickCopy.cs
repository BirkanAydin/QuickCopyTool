using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickCopyTool.Models
{
    public class QuickCopy:INotifyPropertyChanged
    {
        private ObservableCollection<string> _ContextList = new ObservableCollection<string>();
        public ObservableCollection<string> ContextList
        {
            get { return _ContextList; }
            set
            {
                _ContextList = value;
                NotifyPropertyChanged();
            }
        }
        private DataTable _DataTableContext = new DataTable();

        public DataTable DataTableContext
        {
            get { return _DataTableContext; }
            set
            {
                _DataTableContext = value;
                NotifyPropertyChanged();
            }
        }

        private String _Source;
        public String Source
        {
            get { return _Source; }
            set
            {
                _Source = value;
                NotifyPropertyChanged();
            }
        }

        private DataView _DataTableContextView;

        public DataView DataTableContextView
        {
            get { return _DataTableContextView; }
            set
            {
                _DataTableContextView = value;
                NotifyPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
