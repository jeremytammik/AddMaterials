using System.Linq;
using System.Windows.Input;
using AddMaterials.View;
using AddMaterials.ViewModel;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AddMaterials
{
  [Transaction( TransactionMode.ReadOnly )]
  public class FillPatternBenchmarkCommand 
    : IExternalCommand
  {
    public Result Execute( 
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      var doc = commandData.Application
        .ActiveUIDocument.Document;

      var fillPatternElements 
        = new FilteredElementCollector( doc )
          .OfClass( typeof( FillPatternElement ) )
          .OfType<FillPatternElement>()
          .OrderBy( fp => fp.Name )
          .ToList();

      var fillPatterns 
        = fillPatternElements.Select( 
          fpe => fpe.GetFillPattern() );

      FillPatternsViewModel fillPatternsViewModel 
        = new FillPatternsViewModel( fillPatterns
          .Select( x => new FillPatternViewModel( 
            x ) ) );

      FillPatternsView fillPatternsView 
        = new FillPatternsView()
      {
        DataContext = fillPatternsViewModel
      };

      fillPatternsView.ShowDialog();

      return Result.Succeeded;
    }
  }
}
