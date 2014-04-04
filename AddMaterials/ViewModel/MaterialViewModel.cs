using AddMaterials.ViewModel.Enum;
using Autodesk.Revit.DB;

namespace AddMaterials.ViewModel
{
    public class MaterialViewModel
    {
        public string Name { get; set; }

        public string BaseMaterialClass { get; set; }

        public FillPattern SurfacePattern { get; set; }

        public FillPattern CutPattern { get; set; }

        public Color Color { get; set; }

        public double Transparency { get; set; }

        public Status Status { get; set; }

        public bool AddToProject { get; set; }
    }
}