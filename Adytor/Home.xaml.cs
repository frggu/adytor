using System;
using System.IO;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.ObjectModel;
using OxyPlot;
using static Adytor.StaticAdytorCurveCalculation;
using Microsoft.Win32;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Windows.Media;
using System.Reflection;

namespace Adytor
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private static MultEQApp audysseyMultEQApp = null;

        private PlotModel plotModel = new PlotModel();

        private List<int> keys = new List<int>();
        private bool drawCustomTargetPoints;
        private Dictionary<int, Brush> colors = new Dictionary<int, Brush>();

        private double smoothingFactor = 0;

        public DetectedChannel selectedChannel = null;
        private List<DetectedChannel> stickyChannel = new List<DetectedChannel>();

        private string selectedAxisLimits = "rbtnXRangeFull";
        private Dictionary<string, AxisLimit> AxisLimits = new Dictionary<string, AxisLimit>()
        {
            {"rbtnXRangeFull", new AxisLimit { XMin = 10, XMax = 24000, YMin = -35, YMax = 20, YShift = 0, MajorStep = 5, MinorStep = 1 } },
            {"rbtnXRangeSubwoofer", new AxisLimit { XMin = 10, XMax = 1000, YMin = -35, YMax = 20, YShift = 0, MajorStep = 5, MinorStep = 1 } },
            {"rbtnXRangeChirp", new AxisLimit { XMin = 0, XMax = 350, YMin = -0.1, YMax = 0.1, YShift = 0, MajorStep = 0.01, MinorStep = 0.001 } }
        };

        public Home()
        {
            InitializeComponent();

            //Register handler for plot
            plot.PreviewMouseWheel += Plot_PreviewMouseWheel;

            //Create MultEQApp object that is used as context
            audysseyMultEQApp = new MultEQApp();
            this.DataContext = audysseyMultEQApp;
            this.WindowTitle = "Adytor v" + Assembly.GetExecutingAssembly().GetName().Version;
        }

        ~Home()
        {
        }

        public void EnableUiAmpSettings()
        {
            ((MultEQApp)this.DataContext).UiAmpSettingsEnabled = true;
        }

        public void EnableUiChannelSettings()
        {
            ((MultEQApp)this.DataContext).UiChannelSettingsEnabled = true;
        }

        public void DisableUiAmpSettings()
        {
            ((MultEQApp)this.DataContext).UiAmpSettingsEnabled = false;
        }

        public void DisableUiChannelSettings()
        {
            ((MultEQApp)this.DataContext).UiChannelSettingsEnabled = false;
        }

        private void HandleDroppedFile(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                if (files.Length > 0)
                    OpenFile(files[0]);
            }
        }

        private void ParseFileToAudysseyMultEQApp(string FileName)
        {
            if (File.Exists(FileName))
            {
                string Serialized = File.ReadAllText(FileName);
                audysseyMultEQApp = JsonConvert.DeserializeObject<MultEQApp>(Serialized, new JsonSerializerSettings
                {
                    FloatParseHandling = FloatParseHandling.Decimal
                });

                //Add handler
                foreach (var channel in audysseyMultEQApp.DetectedChannels)
                {
                    channel.PropertyChanged += RegenerateTargetCurveHandler;
                }
            }
        }

        private void ParseAudysseyMultEQAppToFile(string FileName)
        {
            if(audysseyMultEQApp != null)
            {
                string Serialized = JsonConvert.SerializeObject(audysseyMultEQApp, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                if ((Serialized != null) && (!string.IsNullOrEmpty(FileName)))
                {
                    File.WriteAllText(FileName, Serialized);
                }
            }
        }

        private void OpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".ady";
            dlg.Filter = "Audyssey files (*.ady)|*.ady";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                OpenFile(dlg.FileName);
            }
        }

        private void ReloadFile_OnClick(object sender, RoutedEventArgs e)
        {
            bool result = (bool)(new MessageBoxYesNo("This will reload the .ady file and discard all changes since last save", "Are you sure?").ShowDialog());
            if (!result)
                return;

            if (File.Exists(currentFile.Content.ToString()))
            {
                ParseFileToAudysseyMultEQApp(currentFile.Content.ToString());
                if ((audysseyMultEQApp != null) /*&& (tabControl.SelectedIndex == 0)*/ )
                {
                    this.DataContext = audysseyMultEQApp;
                    InitializeDataContext();
                }
            }
            
        }

        private void OpenFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                currentFile.Content = filePath;
                ParseFileToAudysseyMultEQApp(currentFile.Content.ToString());
                if ((audysseyMultEQApp != null) /*&& (tabControl.SelectedIndex == 0)*/)
                {
                    this.DataContext = audysseyMultEQApp;
                    InitializeDataContext();
                }

            }
        }

        private void SaveFile_OnClick(object sender, RoutedEventArgs e)
        {
            ParseAudysseyMultEQAppToFile(currentFile.Content.ToString());
        }

        private void SaveFileAs_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = Path.GetFileName(currentFile.Content.ToString());
            dlg.DefaultExt = ".ady";
            dlg.Filter = "Audyssey calibration (.ady)|*.ady";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                currentFile.Content = dlg.FileName;
                ParseAudysseyMultEQAppToFile(currentFile.Content.ToString());
            }
        }


        private void ExitProgram_OnClick(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Close();
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Adytor v" + Assembly.GetExecutingAssembly().GetName().Version);
        }

        private void InitializeDataContext()
        {
            //Enable amp settings, disable channel settings
            EnableUiAmpSettings();
            DisableUiChannelSettings();

            //Initialize collection that is bound to the apply curve combo box
            audysseyMultEQApp.TargetCurveApplyCollection.Clear();
            audysseyMultEQApp.TargetCurveApplyCollection.Add(
                new AdytorCurve(new ObservableCollection<FreqAmpPair>(AdytorCurvePrebuiltCurves.ConvertToCollection(AdytorCurvePrebuiltCurves.CURVE_BK)), "BK Curve")
            );
            audysseyMultEQApp.TargetCurveApplyCollection.Add(
                new AdytorCurve(new ObservableCollection<FreqAmpPair>(AdytorCurvePrebuiltCurves.ConvertToCollection(AdytorCurvePrebuiltCurves.CURVE_TOOLE)), "Toole")
            );
            audysseyMultEQApp.TargetCurveApplyCollection.Add(
                new AdytorCurve(new ObservableCollection<FreqAmpPair>(AdytorCurvePrebuiltCurves.ConvertToCollection(AdytorCurvePrebuiltCurves.CURVE_TOOLEBASS)), "Toole + Bass Boost")
            );
            audysseyMultEQApp.TargetCurveApplyCollection.Add(
                new AdytorCurve(new ObservableCollection<FreqAmpPair>(AdytorCurvePrebuiltCurves.ConvertToCollection(AdytorCurvePrebuiltCurves.CURVE_TOOLEHT)), "Toole HT")
            );
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion

        #region methods
        protected void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion


        private void DrawChart()
        {
            Console.WriteLine(System.DateTime.Now.ToString() + ": DrawChart()");
            if (plot != null)
            {
                //Clear
                ClearPlot();

                //Draw measurement of the selected channel
                if (selectedChannel != null)
                    PlotLine(selectedChannel);

                //Draw measurements of all sticky channels
                if (stickyChannel != null)
                {
                    foreach (var channel in stickyChannel)
                    {
                        if (channel.Sticky == true)
                        {
                            PlotLine(channel, true);
                        }
                    }
                }

                //Draw audyssey curve
                if (selectedChannel != null)
                    PlotAudysseyReference(selectedChannel);

                //Plot custom target points + line
                if (drawCustomTargetPoints && selectedChannel != null)
                    PlotTargetCurvePoints(selectedChannel);

                //Finish drawing
                PlotAxis();
                PlotChart();
            }
        }
        private void ClearPlot()
        {
            if (plot.Model != null && plot.Model.Series != null)
            {
                plot.Model.Series.Clear();
                plot.Model = null;
            }
        }
        private void PlotChart()
        {
            plot.Model = plotModel;
        }
        private void PlotAxis()
        {
            plotModel.Axes.Clear();
            AxisLimit Limits = AxisLimits[selectedAxisLimits];
            if (selectedAxisLimits == "rbtnXRangeChirp")
            {
                if (chbxLogarithmicAxis != null)
                {
                    if (chbxLogarithmicAxis.IsChecked == true)
                    {
                        plotModel.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Bottom, Title = "ms", Minimum = Limits.XMin, Maximum = Limits.XMax, MajorGridlineStyle = LineStyle.Dot });
                    }
                    else
                    {
                        plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "ms", Minimum = Limits.XMin, Maximum = Limits.XMax, MajorGridlineStyle = LineStyle.Dot });
                    }
                }
                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "", Minimum = Limits.YMin, Maximum = Limits.YMax, MajorStep = Limits.MajorStep, MinorStep = Limits.MinorStep, MajorGridlineStyle = LineStyle.Solid });
            }
            else
            {
                if (chbxLogarithmicAxis != null)
                {
                    if (chbxLogarithmicAxis.IsChecked == true)
                    {
                        plotModel.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Bottom, Title = "Hz", Minimum = Limits.XMin, Maximum = Limits.XMax, MajorGridlineStyle = LineStyle.Dot });
                    }
                    else
                    {
                        plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Hz", Minimum = Limits.XMin, Maximum = Limits.XMax, MajorGridlineStyle = LineStyle.Dot });
                    }
                }
                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "dB", Minimum = Limits.YMin + Limits.YShift, Maximum = Limits.YMax + Limits.YShift, MajorStep = Limits.MajorStep, MinorStep = Limits.MinorStep, MajorGridlineStyle = LineStyle.Solid });
            }
        }

        private void PlotTargetCurvePoints(DetectedChannel selectedChannel)
        {
            OxyColor color = OxyColor.FromRgb(0, 190, 0);
            LineSeries lineserie = new LineSeries
            {
                ItemsSource = selectedChannel.CustomResultingCurveCollection,
                DataFieldX = "X",
                DataFieldY = "Y",
                StrokeThickness = 3,
                LineStyle = LineStyle.Solid,
                Color = color,
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStrokeThickness = 2,
                MarkerFill = OxyColor.FromRgb(0, 190, 0),
                MarkerStroke = OxyColor.FromRgb(0, 100, 0),
            };

            //Add handler to series
            lineserie.MouseDown += Plot_TargetPoints_MouseDown;
            lineserie.MouseUp += Plot_TargetPoints_MouseUp;
            lineserie.MouseMove += Plot_TargetPoints_MouseMove;

            PlotController plotController = new PlotController();
            plotModel.Series.Add(lineserie);
        }

        private void PlotAudysseyReference(DetectedChannel selectedChannel)
        {
            OxyColor color = OxyColor.FromRgb(255, 0, 0);
            plotModel.Series.Add(new LineSeries
            {
                ItemsSource = selectedChannel.AudysseyReferenceCollection,
                DataFieldX = "X",
                DataFieldY = "Y",
                StrokeThickness = 2,
                MarkerSize = 0,
                LineStyle = LineStyle.Solid,
                Color = color,
                MarkerType = MarkerType.None,
            });
        }

        private void PlotLine(DetectedChannel selectedChannel, bool secondaryChannel = false)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                Collection<DataPoint> points = new Collection<DataPoint>();

                string s = keys[i].ToString();
                if (!selectedChannel.ResponseData.ContainsKey(s))
                    continue;

                string[] values = selectedChannel.ResponseData[s];
                int count = values.Length;
                Complex[] cValues = new Complex[count];
                double[] Xs = new double[count];

                float sample_rate = 48000;
                float total_time = count / sample_rate;

                AxisLimit Limits = AxisLimits[selectedAxisLimits];
                if (selectedAxisLimits == "rbtnXRangeChirp")
                {
                    Limits.XMax = 1000 * total_time; // horizotal scale: s to ms
                    for (int j = 0; j < count; j++)
                    {
                        double d = Double.Parse(values[j], NumberStyles.AllowExponent | NumberStyles.Float, CultureInfo.InvariantCulture);
                        points.Add(new DataPoint(1000 * j * total_time / count, d));
                    }
                }
                else
                {
                    for (int j = 0; j < count; j++)
                    {
                        decimal d = Decimal.Parse(values[j], NumberStyles.AllowExponent | NumberStyles.Float, CultureInfo.InvariantCulture);
                        Complex cValue = (Complex)d;
                        cValues[j] = 100 * cValue;
                        Xs[j] = (double)j / count * sample_rate;
                    }

                    MathNet.Numerics.IntegralTransforms.Fourier.Forward(cValues);

                    int x = 0;
                    if (radioButtonSmoothingFactorNone.IsChecked.Value)
                    {
                        foreach (Complex cValue in cValues)
                        {
                            points.Add(new DataPoint(Xs[x++], Limits.YShift + 20 * Math.Log10(cValue.Magnitude)));
                            if (x == count / 2) break;
                        }
                    }
                    else
                    {
                        double[] smoothed = new double[count];
                        for (int j = 0; j < count; j++)
                        {
                            smoothed[j] = cValues[j].Magnitude;
                        }

                        LinSpacedFracOctaveSmooth(smoothingFactor, ref smoothed, 1, 1d / 48);

                        foreach (double smoothetResult in smoothed)
                        {
                            points.Add(new DataPoint(Xs[x++], Limits.YShift + 20 * Math.Log10(smoothetResult)));
                            if (x == count / 2) break;
                        }
                    }
                }

                OxyColor color = OxyColor.Parse(colors[keys[i]].ToString());
                LineSeries lineserie = new LineSeries
                {
                    ItemsSource = points,
                    DataFieldX = "X",
                    DataFieldY = "Y",
                    StrokeThickness = 1,
                    MarkerSize = 0,
                    LineStyle = secondaryChannel ? LineStyle.Dot : LineStyle.Solid,
                    Color = color,
                    MarkerType = MarkerType.None,
                };

                plotModel.Series.Add(lineserie);
            }

        }


        private void LinSpacedFracOctaveSmooth(double frac, ref double[] smoothed, float startFreq, double freqStep)
        {
            int passes = 8;
            // Scale octave frac to allow for number of passes
            double scaledFrac = 7.5 * frac; //Empirical tweak to better match Gaussian smoothing
            double octMult = Math.Pow(2, 0.5 / scaledFrac);
            double bwFactor = (octMult - 1 / octMult);
            double b = 0.5 + bwFactor * startFreq / freqStep;
            int N = smoothed.Length;
            double xp;
            double yp;
            // Smooth from HF to LF to avoid artificial elevation of HF data
            for (int pass = 0; pass < passes; pass++)
            {
                xp = smoothed[N - 1];
                yp = xp;
                // reverse pass
                for (int i = N - 2; i >= 0; i--)
                {
                    double a = 1 / (b + i * bwFactor);
                    yp += ((xp + smoothed[i]) / 2 - yp) * a;
                    xp = smoothed[i];
                    smoothed[i] = (float)yp;
                }
                // forward pass
                for (int i = 1; i < N; i++)
                {
                    double a = 1 / (b + i * bwFactor);
                    yp += ((xp + smoothed[i]) / 2 - yp) * a;
                    xp = smoothed[i];
                    smoothed[i] = (float)yp;
                }
            }
        }

        private void CheckBoxMeasurementPositionUnchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int val = int.Parse(checkBox.Content.ToString()) - 1;
            if (keys.Contains(val))
            {
                keys.Remove(val);
                colors.Remove(val);
                DrawChart();
            }
        }
        private void CheckBoxMeasurementPositionChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int val = int.Parse(checkBox.Content.ToString()) - 1;
            if (selectedChannel != null && !selectedChannel.ResponseData.ContainsKey(val.ToString()))
            {
                // This channel has not been measured in this Audyssey calibration. Don't attempt to plot it, and clear the checkbox.
                checkBox.IsChecked = false;
            }
            else if (!keys.Contains(val))
            {
                keys.Add(val);
                colors.Add(val, checkBox.Foreground);
                DrawChart();
            }
        }
        private void AllCheckBoxMeasurementPositionChecked(object sender, RoutedEventArgs e)
        {
            chbx1.IsChecked = true;
            chbx2.IsChecked = true;
            chbx3.IsChecked = true;
            chbx4.IsChecked = true;
            chbx5.IsChecked = true;
            chbx6.IsChecked = true;
            chbx7.IsChecked = true;
            chbx8.IsChecked = true;
            DrawChart();
        }
        private void AllCheckBoxMeasurementPositionUnchecked(object sender, RoutedEventArgs e)
        {
            chbx1.IsChecked = false;
            chbx2.IsChecked = false;
            chbx3.IsChecked = false;
            chbx4.IsChecked = false;
            chbx5.IsChecked = false;
            chbx6.IsChecked = false;
            chbx7.IsChecked = false;
            chbx8.IsChecked = false;
            DrawChart();
        }
        private void CheckBoxTargetPointsChecked(object sender, RoutedEventArgs e)
        {
            drawCustomTargetPoints = true;
            DrawChart();
        }
        private void CheckBoxTargetPointsUnchecked(object sender, RoutedEventArgs e)
        {
            drawCustomTargetPoints = false;
            DrawChart();
        }
        private void ChannelsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<CheckBox> checkBoxes = new List<CheckBox> {
                chbx1, chbx2, chbx3, chbx4, chbx5, chbx6, chbx7, chbx8
            };

            // Disable all the check boxes
            foreach (var checkBox in checkBoxes)
            {
                checkBox.IsEnabled = false;
            }

            var selectedValue = channelsView.SelectedValue as DetectedChannel;
            if (selectedValue != null && selectedValue.ResponseData != null)
            {
                // Enable the check boxes corresponding to those positions for which the measurement is available
                foreach (var measurementPosition in selectedValue.ResponseData)
                {
                    int positionIndex = int.Parse(measurementPosition.Key);
                    Debug.Assert(positionIndex >= 0 && positionIndex < checkBoxes.Count);
                    checkBoxes[positionIndex].IsEnabled = true;
                }

                if (selectedValue.ResponseData.Count > 0)
                {
                    //Save selected channel
                    selectedChannel = (DetectedChannel)channelsView.SelectedValue;

                    //Regenerate audyssey reference curve and the target curve without invalidating plot
                    // The chart needs to be re-drawn anyway
                    RegenerateAudysseyReferenceCurve(selectedChannel);
                    RegenerateCustomResultingCurve(selectedChannel);
                }
            }

            // Un-check all the disabled check boxes
            bool IsSomethingSelected = false;
            foreach (var checkBox in checkBoxes)
            {
                if (!checkBox.IsEnabled && checkBox.IsChecked == true)
                    checkBox.IsChecked = false;

                IsSomethingSelected = IsSomethingSelected || checkBox.IsChecked == true;
            }

            //Enable the first checkbox that can be enabled - only if there is nothing selected
            bool isFirstEnabled = false;
            foreach (var checkBox in checkBoxes)
            {
                if (!isFirstEnabled && !IsSomethingSelected)
                {
                    isFirstEnabled = true;
                    checkBox.IsChecked = true;
                }
            }

            //Draw chart
            DrawChart();

            //Enable channel input fields
            EnableUiChannelSettings();
        }


        private void ChannelsView_OnClickSticky(object sender, RoutedEventArgs e)
        {
            foreach (var channel in audysseyMultEQApp.DetectedChannels)
            {
                if (channel.Sticky)
                {
                    stickyChannel.Add(channel);
                    DrawChart();
                }
                else if (stickyChannel.Contains(channel))
                {
                    stickyChannel.Remove(channel);
                    DrawChart();
                }
            }
        }

        private void ButtonClickCopyTargetCurvePoints(object sender, RoutedEventArgs e)
        {
            bool result = (bool)(new CopySelectChannelWindow { DataContext = audysseyMultEQApp }.ShowDialog());
            if (!result)
                return;

            if (selectedChannel.CustomTargetCurveCollection.Count == 0)
                return;

            //Copy
            foreach (var channel in audysseyMultEQApp.DetectedChannels)
            {
                if (channel.IsSelectedForCopyTargetPoint && channel != selectedChannel)
                {
                    channel.CustomTargetCurveCollection.Clear();
                    foreach (var freqAmpPair in selectedChannel.CustomTargetCurveCollection)
                    {
                        channel.CustomTargetCurveCollection.SortedAdd(new FreqAmpPair(freqAmpPair.Frequency, freqAmpPair.Level));
                    }
                }
            }
        }//

        private void ButtonClickClearTargetCurvePoints(object sender, RoutedEventArgs e)
        {
            ((DetectedChannel)channelsView.SelectedValue).CustomTargetCurveCollection.Clear();
        }

        private void ButtonClickExportTargetCurvePoints(object sender, RoutedEventArgs e)
        {
            if (((DetectedChannel)channelsView.SelectedValue).CustomTargetCurveCollection.Count == 0)
            {
                // Do something
                return;
            }

            // Save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = ((DetectedChannel)channelsView.SelectedValue).CommandId;
            saveFileDialog.Filter = "Target curve csv (*.csv)|*.csv|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }

            // Write to file
            using var stream = File.OpenWrite(saveFileDialog.FileName);
            using var writer = new StreamWriter(stream);
            foreach (var freqPair in ((DetectedChannel)channelsView.SelectedValue).CustomTargetCurveCollection)
            {
                writer.WriteLine(freqPair.Frequency.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + ";" + freqPair.Level.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")));
            }

            writer.Close();
            stream.Close();
        }

        private void ButtonClickImportTargetCurvePoints(object sender, RoutedEventArgs e)
        {
            // Open file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Target curve csv (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }

            // Read from file to target curve
            using var stream = File.OpenRead(openFileDialog.FileName);
            using var reader = new StreamReader(stream);
            String Freq = "";
            String Amp = "";
            int row = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                //Clear targets on first valid row of the file
                if (row == 0)
                    ((DetectedChannel)channelsView.SelectedValue).CustomTargetCurveCollection.Clear();

                //Write line from file to target points
                if (line.Contains(";"))
                {
                    Freq = line.Split(';')[0];
                    Amp = line.Split(';')[1];
                    ((DetectedChannel)channelsView.SelectedValue).CustomTargetCurveCollection.Add(new FreqAmpPair(Freq, Amp));
                }
                row++;
            }// end while

            reader.Close();
            stream.Close();
        }

        private void ButtonClickAddTargetCurvePoint(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(keyTbx.Text) && !string.IsNullOrEmpty(valueTbx.Text))
            {
                ((DetectedChannel)channelsView.SelectedValue).CustomTargetCurveCollection.SortedAdd(new FreqAmpPair(keyTbx.Text, valueTbx.Text));
            }
        }

        private void ButtonClickRemoveTargetCurvePoint(object sender, RoutedEventArgs e)
        {
            FreqAmpPair pair = (sender as Button).DataContext as FreqAmpPair;
            ((DetectedChannel)channelsView.SelectedValue).CustomTargetCurveCollection.Remove(pair);
        }

        private void TargetPointsEdited(object sender, EventArgs e)
        {
            FreqAmpPair pair = (sender as TextBox).DataContext as FreqAmpPair;
            ((DetectedChannel)channelsView.SelectedValue).CustomTargetCurveCollection.ResortWrongElement(pair);
        }

        private void RadioButtonSmoothingFactorChecked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            switch (radioButton.Name)
            {
                case "radioButtonSmoothingFactorNone":
                    smoothingFactor = 1;
                    break;
                case "radioButtonSmoothingFactor2":
                    smoothingFactor = 2;
                    break;
                case "radioButtonSmoothingFactor3":
                    smoothingFactor = 3;
                    break;
                case "radioButtonSmoothingFactor6":
                    smoothingFactor = 6;
                    break;
                case "radioButtonSmoothingFactor12":
                    smoothingFactor = 12;
                    break;
                case "radioButtonSmoothingFactor24":
                    smoothingFactor = 24;
                    break;
                case "radioButtonSmoothingFactor48":
                    smoothingFactor = 48;
                    break;
                default:
                    break;
            }
            DrawChart();
        }

        private void ButtonClickApplySelectedCurve(object sender, RoutedEventArgs e)
        {
            //Get selected curve
            AdytorCurve selectedCurve = (AdytorCurve)this.cmbCurveSelector.SelectedItem;
            if (selectedCurve == null)
                return;

            selectedChannel.CustomTargetCurveCollection.Clear();
            foreach(var freqAmpPair in selectedCurve.CurveCollection)
            {
                selectedChannel.CustomTargetCurveCollection.Add(new FreqAmpPair(freqAmpPair.Frequency, freqAmpPair.Level - GetReferenceLevelAtFrequency(selectedChannel, freqAmpPair.Frequency)));
            }
        }

        private void rbtnXRange_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            selectedAxisLimits = radioButton.Name;
            DrawChart();
        }
        private void chbxStickSubwoofer_Checked(object sender, RoutedEventArgs e)
        {
            DrawChart();
        }
        private void chbxLogarithmicAxis_Checked(object sender, RoutedEventArgs e)
        {
            DrawChart();
        }
        private void chbxLogarithmicAxis_Unchecked(object sender, RoutedEventArgs e)
        {
            DrawChart();
        }
        private void TargetCurveTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RegenerateReferenceAndTargetCurveHandler(null, e);

            //Redraw chart
            DrawChart();
        }

        private void MidRangeCompCheckboxChanged(object sender, RoutedEventArgs e)
        {
            RegenerateReferenceAndTargetCurveHandler(selectedChannel, e);

            //Redraw chart
            DrawChart();
        }

        void textbox_generic_hint_toggle(object sender, TextChangedEventArgs e)
        {
            TextBox srcTextBox = sender as TextBox;
            if (srcTextBox.Text == "")
            {
                srcTextBox.Background.Opacity = 100;
            }
            else
            {
                srcTextBox.Background.Opacity = 0;
            }
        }

        private void RegenerateReferenceAndTargetCurveHandler(object sender, EventArgs e)
        {
            RegenerateAudysseyReferenceCurve(sender, false);
            RegenerateCustomResultingCurve(sender, true);
        }

        private void RegenerateTargetCurveHandler(object sender, PropertyChangedEventArgs e)
        {
            //Leads to stack overflow, because in RegenerateCustomResultingCurve()
            // the Trim Adjumstents are recalculated
            if (e.PropertyName == "TrimAdjustment")
                return;

            RegenerateCustomResultingCurve(sender, true);
            
        }

        private void RegenerateAudysseyReferenceCurve(object sender, bool invalidatePlot = false)
        {
            DetectedChannel senderChannel = (DetectedChannel)sender;

            //Re-calculate Audyssey target curve points
            foreach(var channel in audysseyMultEQApp.DetectedChannels)
            {
                if(senderChannel is null || channel == senderChannel)
                {
                    channel.AudysseyReferenceCollection.Clear();
                    switch (audysseyMultEQApp.EnTargetCurveType)
                    {
                        case 0:
                            return;
                        case 1:
                            channel.AudysseyReferenceCollection = (ObservableCollection<DataPoint>)GetTargetCurve(
                                    HighFreqRollOffType.RollOff1, 0,
                                        chbxMidRangeComp.IsChecked == true ? MidRangeCompensation.On : MidRangeCompensation.Off);
                            break;
                        case 2:
                            channel.AudysseyReferenceCollection = (ObservableCollection<DataPoint>)GetTargetCurve(
                                    HighFreqRollOffType.RollOff2, 0,
                                        chbxMidRangeComp.IsChecked == true ? MidRangeCompensation.On : MidRangeCompensation.Off);
                            break;
                        default:
                            break;
                    }
                }
            }

            //Invalidate plot
            if (invalidatePlot)
                plotModel.PlotView.InvalidatePlot();
        }

        private void RegenerateCustomResultingCurve(object sender, bool invalidatePlot = false)
        {
            DetectedChannel senderChannel = (DetectedChannel)sender;

            //Calculate data points for resulting curve
            DataPoint targetPoint;
            double mean_gain_bass;
            double mean_gain_mid;
            int mean_gain_bass_count;
            int mean_gain_mid_count;

            //Re-calculate Audyssey target curve points
            foreach (var channel in audysseyMultEQApp.DetectedChannels)
            {
                if (senderChannel is null || channel == senderChannel)
                {
                    mean_gain_bass = 0.0;
                    mean_gain_mid = 0.0;
                    mean_gain_bass_count = 0;
                    mean_gain_mid_count = 0;
                    channel.CustomResultingCurveCollection.Clear();
                    foreach (var freqPair in channel.CustomTargetCurveCollection)
                    {
                        //Get a target point
                        targetPoint = new DataPoint(freqPair.Frequency, freqPair.Level + GetReferenceLevelAtFrequency(channel, freqPair.Frequency));

                        //Add level from reference curve to get the resulting dB
                        channel.CustomResultingCurveCollection.Add(new DataPoint(targetPoint.X, targetPoint.Y));

                        //Caclulate subwoofer trim level
                        if(targetPoint.X >= 500 && targetPoint.X <= 1588)
                        {
                            mean_gain_mid += targetPoint.Y;
                            mean_gain_mid_count += 1;
                        }
                        if (targetPoint.X <= 80)
                        {
                            mean_gain_bass += targetPoint.Y;
                            mean_gain_bass_count += 1;
                        }
                    }//end foreach

                    //Recalculate sub trim adjustments for channel
                    try
                    {
                        if (mean_gain_mid_count != 0 && mean_gain_bass_count != 0 && channel.CommandId.Substring(0, 2) == "SW")
                            channel.TrimAdjustment = Math.Round(((mean_gain_bass / mean_gain_bass_count) - (mean_gain_mid / mean_gain_mid_count)), 1).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                    }
                    catch (ArgumentOutOfRangeException) { }
                }
            }

            //Invalidate plot
            if (invalidatePlot)
                plotModel.PlotView.InvalidatePlot();
        }

        private double GetReferenceLevelAtFrequency(DetectedChannel channel, double frequency)
        {
            //Get dB from reference curve
            if (channel != null && channel.AudysseyReferenceCollection != null)
            {
                foreach (var refPointIterate in channel.AudysseyReferenceCollection)
                {
                    if (refPointIterate.X >= frequency)
                    {
                        return refPointIterate.Y;
                    }
                }
            }

            return 0.0;
        }//

    }

    class AxisLimit
    {
        public double XMin { get; set; }
        public double XMax { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }
        public double YShift { get; set; }
        public double MajorStep { get; set; }
        public double MinorStep { get; set; }
    }
}