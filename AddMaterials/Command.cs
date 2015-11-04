#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using AddMaterials.View;
using AddMaterials.ViewModel;
using AddMaterials.ViewModel.Enum;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Excel = Microsoft.Office.Interop.Excel;
#endregion

namespace AddMaterials
{
  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    private const string _not_available = "N/A";

    //const string _input_file_name = "C:/RevitAPI/MaterialList.xlsx";
    //private const string _input_file_name = "C:/tmp/MaterialList.xlsx";
    //const string _input_file_name = "Z:/a/doc/revit/blog/zip/MaterialList.xlsx";
    
    const string _input_file_name = "Z:/a/doc/revit/tbc/zip/MaterialList.xlsx";

    private static string PluralSuffix( int i )
    {
      return 1 == i ? "" : "s";
    }

    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Document doc = uidoc.Document;

      // Create dictionary of existing 
      // materials keyed by their name.

      Dictionary<string, Material> materials
        = new FilteredElementCollector( doc )
          .OfClass( typeof( Material ) )
          .Cast<Material>()
          .ToDictionary<Material, string>(
            e => e.Name );

      // Ditto for fill patterns.

      Dictionary<string, FillPatternElement> fillPatterns
        = new FilteredElementCollector( doc )
          .OfClass( typeof( FillPatternElement ) )
          .Cast<FillPatternElement>()
          .ToDictionary<FillPatternElement, string>(
            e => e.Name );

      try
      {
        var materialsToImport = ReadMaterialsFromXlsx( materials, fillPatterns ).ToList();
        var browser = new MaterialBrowserViewModel { Materials = materialsToImport };
        var window = new MaterialsView { DataContext = browser };
        var res = window.ShowDialog();
        if( !res.HasValue || !res.Value )
          return Result.Cancelled;

        var materialsToAdd = browser.Materials.Where( x => x.AddToProject ).ToList();
        if( !materialsToAdd.Any() )
          return Result.Cancelled;

        using( var tx = new Transaction( doc ) )
        {
          tx.Start( "Add Materials" );
          foreach( var materialViewModel in materialsToAdd )
          {
            var materialCSI = materials[materialViewModel.BaseMaterialClass];
            var myMaterial = materialCSI.Duplicate( materialViewModel.Name );
            myMaterial.Color = materialViewModel.Color;
            myMaterial.Transparency = (int) materialViewModel.Transparency;
            if( materialViewModel.SurfacePattern != null )
              myMaterial.SurfacePatternId 
                = fillPatterns[materialViewModel.SurfacePattern.Name].Id;
            if( materialViewModel.CutPattern != null )
              myMaterial.CutPatternId 
                = fillPatterns[materialViewModel.CutPattern.Name].Id;
          }
          tx.Commit();
        }

        string msg = string.Format(
          "{0} row{1} successfully parsed and "
          + "{2} material{3} added:",
          materialsToImport.Count, 
          PluralSuffix( materialsToImport.Count ),
          materialsToAdd.Count, 
          PluralSuffix( materialsToAdd.Count ) );

        TaskDialog dlg = new TaskDialog(
            "Revit AddMaterials" );

        dlg.MainInstruction = msg;

        dlg.MainContent = string.Join( ", ", 
          materialsToAdd.Select( x => x.Name ) ) + ".";

        dlg.Show();
      }
      catch( Exception ex )
      {
        message 
          = "Revit AddMaterials Exception:\n" + ex;

        return Result.Failed;
      }
      return Result.Succeeded;
    }

    private IEnumerable<MaterialViewModel> ReadMaterialsFromXlsx( 
      Dictionary<string, Material> materials,
      Dictionary<string, 
      FillPatternElement> fillPatterns )
    {
      Excel.Application excel
        = new Excel.Application();

      excel.Visible = false;

      Excel.Workbook workbook = excel.Workbooks.Open(
        _input_file_name, 0, true, 5, "", "", true,
        Excel.XlPlatform.xlWindows, "\t", false,
        false, 0, true, 1, 0 );

      Excel.Worksheet worksheet = (Excel.Worksheet)
                                  workbook.Worksheets.get_Item( 1 );

      Excel.Range range = worksheet.UsedRange;

      int iRow = 5;

      while( null != range.Cells[iRow, 1].Value2 )
      {
        string matName = (string) range.Cells[iRow, 1].Value2;
        matName += " " + (string) range.Cells[iRow, 2].Value2;
        matName += " " + (string) range.Cells[iRow, 3].Value2;
        if( !string.IsNullOrEmpty( matName ) )
        {
          double red = (double) range.Cells[iRow, 4].Value2;
          double green = (double) range.Cells[iRow, 5].Value2;
          double blue = (double) range.Cells[iRow, 6].Value2;
          double transparency = (double) range.Cells[iRow, 8].Value2;
          string surPatternName = (string) range.Cells[iRow, 9].Value2;
          string cutPatternName = (string) range.Cells[iRow, 10].Value2;
          string csi = (string) range.Cells[iRow, 11].Value2;

          FillPatternElement fillPatternElement;
          FillPattern surfacePattern = null, cutPattern = null;
          if( fillPatterns.TryGetValue( surPatternName, out fillPatternElement ) )
            surfacePattern = fillPatternElement.GetFillPattern();

          if( fillPatterns.TryGetValue( cutPatternName, out fillPatternElement ) )
            cutPattern = fillPatternElement.GetFillPattern();

          var status = Status.Normal;
          if( !materials.ContainsKey( csi ) )
            status = Status.BaseMaterialClassNotFound;
          if( materials.ContainsKey( matName ) )
            status = Status.ProjectAlreadyContainsMaterialWithTheSameName;

          yield return new MaterialViewModel
          {
            Name = matName,
            BaseMaterialClass = csi,
            Color = new Color(
              Byte.Parse( red.ToString() ),
              Byte.Parse( green.ToString() ),
              Byte.Parse( blue.ToString() ) ),
            Transparency = transparency,
            SurfacePattern = surfacePattern,
            CutPattern = cutPattern,
            Status = status,
            AddToProject = status == Status.Normal
          };
        }
        ++iRow;
      }
      workbook.Close( true, null, null );
      excel.Quit();
    }
  }
}

// Z:\a\rvt\add_material_csi_03.rvt
// Z:\a\rvt\fill_pattern_viewer.rvt

