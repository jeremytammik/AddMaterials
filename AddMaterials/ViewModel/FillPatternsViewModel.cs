using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AddMaterials.ViewModel
{
    public class FillPatternsViewModel
    {
        private ReadOnlyCollection<FillPatternViewModel> _fillPatterns;

        public FillPatternsViewModel(IEnumerable<FillPatternViewModel> fillPatterns)
        {
            _fillPatterns = new ReadOnlyCollection<FillPatternViewModel>(new List<FillPatternViewModel>(fillPatterns));
        }

        public ReadOnlyCollection<FillPatternViewModel> FillPatterns
        {
            get { return _fillPatterns; }
        }
    }
}