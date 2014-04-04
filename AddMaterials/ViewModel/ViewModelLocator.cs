using System;

namespace AddMaterials.ViewModel
{
    public class ViewModelLocator
    {
        private static readonly Lazy<ViewModelLocator> InstanceObj =
            new Lazy<ViewModelLocator>(() => new ViewModelLocator());

        public static ViewModelLocator Instance
        {
            get { return InstanceObj.Value; }
        }

        private ViewModelLocator()
        {
            
        }
    }
}