#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
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
    const string _not_available = "N/A";

    const string _input_file_name = "C:/RevitAPI/MaterialList.xlsx";

    //const string _input_file_name = "Z:/a/doc/revit/blog/zip/MaterialList.xlsx";

    static string PluralSuffix( int i )
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

        int nRows = 0;
        List<string> materials_added = new List<string>();

        int iRow = 5;

        using( Transaction tx = new Transaction( doc ) )
        {
          tx.Start( "Add Materials" );

          while( null != range.Cells[iRow, 1].Value2 )
          {
            string matName = (string) range.Cells[iRow, 1].Value2;
            matName += " " + (string) range.Cells[iRow, 2].Value2;
            matName += " " + (string) range.Cells[iRow, 3].Value2;

            if( matName != null )
            {
              double red = (double) range.Cells[iRow, 4].Value2;
              double green = (double) range.Cells[iRow, 5].Value2;
              double blue = (double) range.Cells[iRow, 6].Value2;
              double transparency = (double) range.Cells[iRow, 8].Value2;
              string surPattern = (string) range.Cells[iRow, 9].Value2;
              string cutPattern = (string) range.Cells[iRow, 10].Value2;

              // Identity data of material class to duplicate.

              string CSI = (string) range.Cells[iRow, 11].Value2;

              if( materials.ContainsKey( CSI ) )
              {
                Material materialCSI = materials[CSI];

                Material myMaterial
                  = materialCSI.Duplicate( matName );

                Color matColor = new Color(
                  Byte.Parse( red.ToString() ),
                  Byte.Parse( green.ToString() ),
                  Byte.Parse( blue.ToString() ) );

                myMaterial.Color = matColor;

                myMaterial.Transparency
                  = (int) transparency;

                if( 0 < surPattern.Length
                  && !surPattern.Equals( _not_available ) )
                {
                  myMaterial.SurfacePatternId
                    = fillPatterns[surPattern].Id;
                }

                if( 0 < cutPattern.Length
                  && !cutPattern.Equals( _not_available ) )
                {
                  myMaterial.CutPatternId
                    = fillPatterns[cutPattern].Id;
                }
                materials_added.Add( matName );
              }
            }
            ++nRows;
            ++iRow;
          }
          tx.Commit();
        }

        workbook.Close( true, null, null );
        excel.Quit();

        int n = materials_added.Count;

        string msg = string.Format(
          "{0} row{1} successfully parsed and "
          + "{2} material{3} added:",
          nRows, PluralSuffix( nRows ),
          n, PluralSuffix( n ) );

        TaskDialog dlg = new TaskDialog( 
          "Revit AddMaterials" );
        
        dlg.MainInstruction = msg;

        dlg.MainContent = string.Join( ", ",
          materials_added ) + ".";

        dlg.Show();

        return Result.Succeeded;
      }
      catch( Exception ex )
      {
        message = "Revit AddMaterials Exception:\n"
          + ex.ToString();

        return Result.Failed;
      }
    }
  }
}

// Z:\a\rvt\add_material_csi_03.rvt
