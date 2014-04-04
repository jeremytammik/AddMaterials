using System;

namespace AddMaterials.ViewModel.Events
{
    public class DialogCloseEventArgs : EventArgs
    {
        private readonly bool? result;

        public DialogCloseEventArgs(bool? result)
        {
            this.result = result;
        }

        public bool? Result
        {
            get { return result; }
        }
    }
}