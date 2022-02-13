using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.ObjectModel;

namespace Adytor
{
    public partial class Home : Page
    {
        private static DataPoint CurrentLocationPoint;
        private static DataPoint LastLocationPoint;
        private static DateTime lastClick = DateTime.Now;
        private static bool movingBlocked = false;

        private void Plot_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void Plot_TargetPoints_MouseDown(object sender, OxyMouseEventArgs e)
        {
            e.Handled = true;

            //Get coordinates
            LineSeries series = (LineSeries)sender;
            Double X, Y;
            X = series.InverseTransform(e.Position).X;
            Y = series.InverseTransform(e.Position).Y;

            //Get nearest point from series:
            CurrentLocationPoint = series.GetNearestPoint(e.Position, false).DataPoint;

            //Calculate distance from nearest point, if distance is too high, don't move point
            double dist = Math.Sqrt(Math.Pow(e.Position.X - series.Transform(CurrentLocationPoint).X,2) 
                                        + Math.Pow(e.Position.Y - series.Transform(CurrentLocationPoint).Y,2));
            if(dist > 10)
                movingBlocked = true;

        }//

        private void Plot_TargetPoints_MouseMove(object sender, OxyMouseEventArgs e)
        {
            if (movingBlocked)
                return;

            //Get coordinates
            LineSeries series = (LineSeries)sender;
            Double X, Y;
            X = series.InverseTransform(e.Position).X;
            Y = series.InverseTransform(e.Position).Y;

            //Save current location
            LastLocationPoint = new DataPoint(CurrentLocationPoint.X, CurrentLocationPoint.Y);

            //Change current location point to mouse position
            CurrentLocationPoint = new DataPoint(FreqAmpPair.ApplyFrequencyLimit(X), FreqAmpPair.ApplyLevelLimit(Y));
            
            //Move point in target point collection by delta of moved coordinates
            foreach (var freqPair in selectedChannel.CustomTargetCurveCollection)
            {
                if (freqPair.Frequency == LastLocationPoint.X) {
                    freqPair.Frequency += CurrentLocationPoint.X - LastLocationPoint.X;
                    freqPair.Level += CurrentLocationPoint.Y - LastLocationPoint.Y - (GetReferenceLevelAtFrequency(selectedChannel, CurrentLocationPoint.X) - GetReferenceLevelAtFrequency(selectedChannel, LastLocationPoint.X));

                    selectedChannel.CustomTargetCurveCollection.ResortWrongElement(freqPair);
                    break;
                }
            }

            //Redraw
            plotModel.PlotView.InvalidatePlot();

            e.Handled = true;
        }//

        private void Plot_TargetPoints_MouseUp(object sender, OxyMouseEventArgs e)
        {
            e.Handled = true;

            //Release blocking
            movingBlocked = false;

            //Get coordinates
            LineSeries series = (LineSeries)sender;
            Double X, Y;
            X = series.InverseTransform(e.Position).X;
            Y = series.InverseTransform(e.Position).Y;

            //Double click detection... thanks OxyPlot
            Boolean IsDoubleClick = false;
            if (DateTime.Now.Subtract(lastClick).TotalMilliseconds < 300)
                IsDoubleClick = true;
            lastClick = DateTime.Now;

            if (IsDoubleClick)
            {
                //Add point
                selectedChannel.CustomTargetCurveCollection.SortedAdd(new FreqAmpPair(X, Y - GetReferenceLevelAtFrequency(selectedChannel, X) ));

                //Redraw
                plotModel.PlotView.InvalidatePlot();
            }

            //Select point at co-ordinates
            CurrentLocationPoint = series.GetNearestPoint(e.Position, false).DataPoint;

        }//
    }
}
