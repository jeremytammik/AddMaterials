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
    private const float LENGTH = 100;
    private Bitmap fillPatternImg;

    #region FillPattern DependencyProperty
    public static readonly 
      DependencyProperty FillPatternProperty 
        = DependencyProperty.RegisterAttached( 
          "FillPattern",
          typeof( FillPattern ),
          typeof( FillPatternViewerControlWpf ),
          new UIPropertyMetadata( null, 
            OnFillPatternChanged ) );

    private static void OnFillPatternChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e )
    {
      var FillPatternViewerControlWpfControl
        = d as FillPatternViewerControlWpf;

      if( FillPatternViewerControlWpfControl == null ) return;

      FillPatternViewerControlWpfControl.OnPropertyChanged(
        "FillPattern" );

      FillPatternViewerControlWpfControl.CreateFillPatternImage();
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
    #endregion

    public void Regenerate()
    {
      CreateFillPatternImage();
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
      if( handler != null )
      {
        handler( this,
          new PropertyChangedEventArgs( 
            propertyName ) );
      }
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
      var width 
        = ( ActualWidth == 0 ? Width : ActualWidth ) == 0
          ? 100
          : ( ActualWidth == 0 ? Width : ActualWidth );

      if( double.IsNaN( width ) )
        width = 100;

      var height 
        = ( ActualHeight == 0 ? Height : ActualHeight ) == 0
          ? 30
          : ( ActualHeight == 0 ? Height : ActualHeight );

      if( double.IsNaN( height ) )
        height = 30;

      fillPatternImg = new Bitmap(
        (int) width, (int) height );

      using( var g = Graphics.FromImage(
        fillPatternImg ) )
      {
        var rect = new Rectangle( 
          0, 0, (int) width, (int) height );
        g.FillRectangle( Brushes.White, rect );
        DrawFillPattern( g );
      }

      OnPropertyChanged( "FillPatternImage" );
    }

    private void DrawFillPattern( Graphics g )
    {
      Stopwatch sw = Stopwatch.StartNew();
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
        var width 
          = ( ActualWidth == 0 ? Width : ActualWidth ) == 0
            ? 100
            : ( ActualWidth == 0 ? Width : ActualWidth );

        if( double.IsNaN( width ) )
          width = 100;

        var height 
          = ( ActualHeight == 0 ? Height : ActualHeight ) == 0
            ? 30
            : ( ActualHeight == 0 ? Height : ActualHeight );

        if( double.IsNaN( height ) )
          height = 30;

        var viewRect = new Rectangle( 
          0, 0, (int) width, (int) height );

        var centerX = ( viewRect.Left 
          + viewRect.Left + viewRect.Width ) / 2;

        var centerY = ( viewRect.Top 
          + viewRect.Top + viewRect.Height ) / 2;

        g.TranslateTransform( centerX, centerY );

        var rectF = new Rectangle( -1, -1, 2, 2 );
        g.FillRectangle( Brushes.Blue, rectF ); //draw a small rectangle in the center of the image

        g.ResetTransform();

        var fillGrids = fillPattern.GetFillGrids();

        Debug.Print( new string( '-', 100 ) );
        Debug.Print( "FilPattern name: {0}", fillPattern.Name );
        if( fillPattern.Target == FillPatternTarget.Model )
          Debug.Print( "FillPattern type: Model" );
        else
          Debug.Print( "FillPattern type: Drafting" );
        Debug.Print( "Matrix scale: {0}", matrixScale );
        Debug.Print( "Grids count: {0}", fillGrids.Count );
        Debug.Print( "Len\\Area: {0}", fillPattern.LengthPerArea );
        Debug.Print( "Lines\\Len: {0}", fillPattern.LinesPerLength );
        Debug.Print( "Strokes\\Area: {0}", fillPattern.StrokesPerArea );

        foreach( var fillGrid in fillGrids )
        {
          var degreeAngle = (float) RadianToGradus( fillGrid.Angle );
          Debug.Print( new string( '-', 50 ) );
          Debug.Print( "Origin: U:{0} V:{1}", 
            fillGrid.Origin.U, fillGrid.Origin.V );
          Debug.Print( "Offset: {0}", fillGrid.Offset );
          Debug.Print( "Angle: {0}", degreeAngle );
          Debug.Print( "Shift: {0}", fillGrid.Shift );

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
          var matrix = new Matrix( 
            1, 0, 0, -1, centerX, centerY ); //-1 reflects about x-axis
          matrix.Scale( matrixScale, matrixScale );
          matrix.Translate( (float) fillGrid.Origin.U, 
            (float) fillGrid.Origin.V );
          var backMatrix = matrix.Clone();
          backMatrix.Multiply( rotateMatrix );
          matrix.Multiply( rotateMatrix );

          var offset = ( -10 ) * dashLength;
          matrix.Translate( offset, 0 );
          backMatrix.Translate( offset, 0 );
          Debug.Print( "Offset: {0}", offset );


          bool moving_forward = true;
          bool moving_back = true;
          int safety = 500;
          double alternator = 0;
          while( moving_forward || moving_back ) // draw segments shifting and offsetting each time
          {
            Debug.Write( "*" );
            var rectF1 = new RectangleF( 
              -2 / matrixScale, -2 / matrixScale, 
              4 / matrixScale, 4 / matrixScale );

            if( moving_forward && LineIntersectsRect( 
              matrix, viewRect ) )
            {
              g.Transform = matrix;
              g.DrawLine( pen, new PointF( 0, 0 ), 
                new PointF( LENGTH, 0 ) );
            }
            else
            {
              moving_forward = false;
              Debug.Print( "\n----> Matrix does not intersect view" );
            }

            if( moving_back && LineIntersectsRect( 
              backMatrix, viewRect ) )
            {
              g.Transform = backMatrix;
              g.DrawLine( pen, new PointF( 0, 0 ), 
                new PointF( LENGTH, 0 ) );
            }
            else
            {
              moving_back = false;
              Debug.Print( "\n----> Back matrix does not intersect view" );
            }

            if( safety == 0 )
            {
              Debug.Print( "\n--------> Safety limit exceeded" );
              break;
            }
            else
              --safety;

            matrix.Translate( (float) fillGrid.Shift, 
              (float) fillGrid.Offset );
            backMatrix.Translate( -(float) fillGrid.Shift, 
              -(float) fillGrid.Offset );

            alternator += fillGrid.Shift;
            if( Math.Abs( alternator ) > Math.Abs( offset ) )
            {
              Debug.Print( "\n----> Alternating" );
              matrix.Translate( offset, 0 );
              backMatrix.Translate( offset, 0 );
              alternator = 0d;
            }
          }
        }
        sw.Stop();
        g.ResetTransform();

#if DEBUG
        g.DrawString( string.Format( 
          "{0} ms", sw.ElapsedMilliseconds ), 
          System.Drawing.SystemFonts.DefaultFont, 
          Brushes.Red, 0, 0 );
#endif

        Debug.Print( new string( '-', 50 ) );

        Pen p = new Pen( System.Drawing.Color.Black );
        p.Width = 1f / matrixScale;
        Debug.Print( "Finished" );
      }
      catch( Exception ex )
      {
        Debug.Print( ex.ToString() );
      }
    }

    public bool LineIntersectsRect( Matrix rayMatrix, Rectangle r )
    {
      Matrix m = rayMatrix.Clone();
      m.Translate( 200, 0 );
      return LineIntersectsRect(
        new System.Drawing.Point( (int) rayMatrix.OffsetX, 
          (int) rayMatrix.OffsetY ),
        new System.Drawing.Point( (int) m.OffsetX, 
          (int) m.OffsetY ),
        r );
    }

    public bool LineIntersectsRect( 
      System.Drawing.Point p1, 
      System.Drawing.Point p2, 
      Rectangle r )
    {
      return LineIntersectsLine( p1, p2, new System.Drawing.Point( r.X, r.Y ), new System.Drawing.Point( r.X + r.Width, r.Y ) ) ||
        LineIntersectsLine( p1, p2, new System.Drawing.Point( r.X + r.Width, r.Y ), new System.Drawing.Point( r.X + r.Width, r.Y + r.Height ) ) ||
        LineIntersectsLine( p1, p2, new System.Drawing.Point( r.X + r.Width, r.Y + r.Height ), new System.Drawing.Point( r.X, r.Y + r.Height ) ) ||
        LineIntersectsLine( p1, p2, new System.Drawing.Point( r.X, r.Y + r.Height ), new System.Drawing.Point( r.X, r.Y ) ) ||
        ( r.Contains( p1 ) && r.Contains( p2 ) );
    }

    private bool LineIntersectsLine( 
      System.Drawing.Point l1p1, 
      System.Drawing.Point l1p2, 
      System.Drawing.Point l2p1, 
      System.Drawing.Point l2p2 )
    {
      try
      {
        Int64 d = ( l1p2.X - l1p1.X ) * ( l2p2.Y - l2p1.Y ) - ( l1p2.Y - l1p1.Y ) * ( l2p2.X - l2p1.X );
        if( d == 0 ) return false;

        Int64 q = ( l1p1.Y - l2p1.Y ) * ( l2p2.X - l2p1.X ) - ( l1p1.X - l2p1.X ) * ( l2p2.Y - l2p1.Y );
        Int64 r = q / d;

        Int64 q1 = (Int64) ( l1p1.Y - l2p1.Y ) * (Int64) ( l1p2.X - l1p1.X );
        Int64 q2 = (Int64) ( l1p1.X - l2p1.X ) * (Int64) ( l1p2.Y - l1p1.Y );

        q = q1 - q2;
        Int64 s = q / d;

        if( r < 0 || r > 1 || s < 0 || s > 1 )
          return false;

        return true;
      }
      catch( OverflowException err )
      {
        Debug.Print( "----------------------------------" );
        Debug.Print( err.Message );
        Debug.Print( l1p1.ToString() );
        Debug.Print( l1p2.ToString() );
        Debug.Print( l2p1.ToString() );
        Debug.Print( l2p2.ToString() );
        return false;
      }
    }

    private double GetDistance( PointF point1, PointF point2 )
    {
      // Pythagorean theorem c^2 = a^2 + b^2
      // thus c = square root(a^2 + b^2)

      double a = (double) ( point2.X - point1.X );
      double b = (double) ( point2.Y - point1.Y );

      return Math.Sqrt( a * a + b * b );
    }

    private double RadianToGradus( double radian )
    {
      return radian * 180 / Math.PI;
    }
  }
}
