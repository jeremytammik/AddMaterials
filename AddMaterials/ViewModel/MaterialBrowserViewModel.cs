using System;
using System.Collections.Generic;
using System.Windows.Input;
using AddMaterials.View.Commands;
using AddMaterials.ViewModel.Events;

namespace AddMaterials.ViewModel
{
    public class MaterialBrowserViewModel
    {
        public event EventHandler<DialogCloseEventArgs> RequestClose;

        protected void OnRequestClose(DialogCloseEventArgs e)
        {
            EventHandler<DialogCloseEventArgs> handler = RequestClose;
            if (handler != null) handler(this, e);
        }

        protected void Close(bool? result)
        {
            OnRequestClose(new DialogCloseEventArgs(result));
        }

        public IEnumerable<MaterialViewModel> Materials { get; set; }

        private ICommand okCommand;
        public ICommand OkCommand
        {
            get { return okCommand ?? (okCommand = new RelayCommand(o => Close(true))); }
        }

        private ICommand cancelCommand;
        public ICommand CancelCommand
        {
            get { return cancelCommand ?? (cancelCommand = new RelayCommand(o => Close(false))); }
        }
    }
}