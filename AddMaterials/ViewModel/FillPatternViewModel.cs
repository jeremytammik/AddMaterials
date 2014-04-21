using Autodesk.Revit.DB;

namespace AddMaterials.ViewModel
{
    public class FillPatternViewModel
    {
        private readonly FillPattern _fillPattern;

        public FillPatternViewModel(FillPattern fillPattern)
        {
            _fillPattern = fillPattern;
        }

        public FillPattern FillPattern
        {
            get { return _fillPattern; }
        }

        public string Name
        {
            get { return _fillPattern.Name; }
        }
    }
}