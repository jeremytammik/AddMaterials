using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Image = System.Drawing.Image;
using Matrix = System.Drawing.Drawing2D.Matrix;
using Rectangle = System.Drawing.Rectangle;

namespace AddMaterials.View.Controls
{
  /// <summary>
  /// Interaction logic for FillPatternViewerControlWpf.xaml
  /// </summary>
  public partial class FillPatternViewerControlWpf
    : INotifyPropertyChanged
  {
    private const float Scale = 50;
    private Bitmap fillPatternImg;

    public static readonly DependencyProperty
      FillPatternProperty = DependencyProperty
        .RegisterAttached( "FillPattern",
          typeof( FillPattern ),
          typeof( FillPatternViewerControlWpf ),
          new UIPropertyMetadata( null,
            OnFillPatternChanged ) );

    private static void OnFillPatternChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e )
    {
      var fillPatternViewerControl
        = d as FillPatternViewerControlWpf;

      if( fillPatternViewerControl == null ) return;

      fillPatternViewerControl.OnPropertyChanged(
        "FillPattern" );

      fillPatternViewerControl.CreateFillPatternImage();
    }

    public FillPattern FillPattern
    {
      get
      {
        return (FillPattern) GetValue(
          FillPatternProperty );
      }
      set
      {
        SetValue( FillPatternProperty, value );
      }
    }

    public FillPattern GetFillPattern(
      DependencyObject obj )
    {
      return (FillPattern) obj.GetValue(
        FillPatternProperty );
    }

    public void SetFillPattern(
      DependencyObject obj,
      FillPattern value )
    {
      obj.SetValue( FillPatternProperty, value );
    }

    public FillPatternViewerControlWpf()
    {
      InitializeComponent();
    }

    public event PropertyChangedEventHandler
      PropertyChanged;

    //[NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged(
      string propertyName )
    {
      var handler = PropertyChanged;
      if( handler != null ) handler( this,
        new PropertyChangedEventArgs( propertyName ) );
    }

    public Image FillPatternImage
    {
      get
      {
        if( fillPatternImg == null )
          CreateFillPatternImage();
        return fillPatternImg;
      }
    }

    private void CreateFillPatternImage()
    {
      if( FillPattern == null )
        return;

      var width =
        ( ActualWidth == 0 ? Width : ActualWidth ) == 0
          ? 100
          : ( ActualWidth == 0 ? Width : ActualWidth );

      if( double.IsNaN( width ) )
        width = 100;

      var height =
          ( ActualHeight == 0 ? Height : ActualHeight ) == 0
            ? 30
            : ( ActualHeight == 0 ? Height : ActualHeight );

      if( double.IsNaN( height ) )
        height = 30;

      fillPatternImg = new Bitmap(
        (int) width, (int) height );

      using( var g = Graphics.FromImage(
        fillPatternImg ) )
      {
        DrawFillPattern( g );
      }

      OnPropertyChanged( "FillPatternImage" );
    }

    private void DrawFillPattern( Graphics g )
    {
      float matrixScale;

      var fillPattern = FillPattern;

      if( fillPattern == null )
        return;

      if( fillPattern.Target == FillPatternTarget.Model )
        matrixScale = Scale;
      else
        matrixScale = Scale * 10;

      try
      {
        var width =
        ( ActualWidth == 0 ? Width : ActualWidth ) == 0
          ? 100
          : ( ActualWidth == 0 ? Width : ActualWidth );

        if( double.IsNaN( width ) )
          width = 100;

        var height =
            ( ActualHeight == 0 ? Height : ActualHeight ) == 0
              ? 30
              : ( ActualHeight == 0 ? Height : ActualHeight );

        if( double.IsNaN( height ) )
          height = 30;

        var rect = new Rectangle( 0, 0,
          (int) width, (int) height );

        var centerX = ( rect.Left + rect.Left
          + rect.Width ) / 2;

        var centerY = ( rect.Top + rect.Top
          + rect.Height ) / 2;

        g.TranslateTransform( centerX, centerY );

        var rectF = new Rectangle( -1, -1, 2, 2 );

        g.FillRectangle( Brushes.Blue, rectF );

        g.ResetTransform();

        var fillGrids = fillPattern.GetFillGrids();

        Debug.Print( "FilPattern name: {0}",
          fillPattern.Name );

        Debug.Print( "Grids count: {0}",
          fillGrids.Count );

        foreach( var fillGrid in fillGrids )
        {
          var degreeAngle = (float) RadianToGradus(
            fillGrid.Angle );

          Debug.Print( new string( '-', 100 ) );

          Debug.Print( "\tOrigin: U: {0} V:{1}",
            fillGrid.Origin.U, fillGrid.Origin.V );

          Debug.Print( "\tOffset: {0}", fillGrid.Offset );
          Debug.Print( "\tAngle: {0}", degreeAngle );
          Debug.Print( "\tShift: {0}", fillGrid.Shift );

          var pen = new Pen( System.Drawing.Color.Black )
          {
            Width = 1f / matrixScale
          };

          float dashLength = 1;

          var segments = fillGrid.GetSegments();

          if( segments.Count > 0 )
          {
            pen.DashPattern = segments
                .Select( Convert.ToSingle )
                .ToArray();

            Debug.Print( "\tSegments:" );

            foreach( var segment in segments )
            {
              Debug.Print( "\t\t{0}", segment );
            }

            dashLength = pen.DashPattern.Sum();
          }

          g.ResetTransform();

          var rotateMatrix = new Matrix();
          rotateMatrix.Rotate( degreeAngle );

          var matrix = new Matrix( 1, 0, 0, -1,
            centerX, centerY );

          matrix.Scale( matrixScale, matrixScale );

          matrix.Translate( (float) fillGrid.Origin.U,
            (float) fillGrid.Origin.V );

          var backMatrix = matrix.Clone();
          backMatrix.Multiply( rotateMatrix );
          matrix.Multiply( rotateMatrix );

          bool first = true;
          for( int i = 20; i > 0; i-- )
          {
            if( !first )
            {
              matrix.Translate( (float) fillGrid.Shift,
                (float) fillGrid.Offset );

              backMatrix.Translate( (float) fillGrid.Shift,
                -(float) fillGrid.Offset );
            }
            else
            {
              first = false;
            }

            var offset = ( -10 ) * dashLength;
            matrix.Translate( offset, 0 );
            backMatrix.Translate( offset, 0 );

            g.Transform = matrix;

            g.DrawLine( pen, new PointF( 0, 0 ),
              new PointF( 200, 0 ) );

            g.Transform = backMatrix;

            g.DrawLine( pen, new PointF( 0, 0 ),
              new PointF( 200, 0 ) );
          }
        }
      }
      catch( Exception ex )
      {
        Debug.Print( ex.Message );
      }
    }

    private double RadianToGradus( double radian )
    {
      return radian * 180 / Math.PI;
    }
  }
}
