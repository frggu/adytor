using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using OxyPlot;

namespace Adytor
{
    public static class StaticAdytorCurveCalculation
    {
        public enum HighFreqRollOffType
        {
            RollOff1,
            RollOff2,
        }
        public enum CrossOverFreq
        {
            f40,
            f60,
            f80,
            f100,
            f120,
            f140,
            f160,
            f180,
            f200,
        }
        public enum MidRangeCompensation
        {
            On,
            Off,
        }

        public static ObservableCollection<DataPoint> GetTargetCurve(HighFreqRollOffType hfroType, CrossOverFreq xFreq, MidRangeCompensation midComp)
        {
            //Build identity filter
            ObservableCollection<DataPoint> TargetCurve = new ObservableCollection<DataPoint>();
            for (int freq = 20; freq < 19000; freq++)
            {
                TargetCurve.Add(new DataPoint(freq, 1.0));
            }

            //Apply high frequency roll off
            switch (hfroType)
            {
                case HighFreqRollOffType.RollOff1:
                    TargetCurve = ApplyHighRollOff1(TargetCurve);
                    break;
                case HighFreqRollOffType.RollOff2:
                    TargetCurve = ApplyHighRollOff2(TargetCurve);
                    break;
            }


            //Apply mid range compensation
            switch (midComp)
            {
                case MidRangeCompensation.On:
                    TargetCurve = ApplyMidRangeCompensation(TargetCurve);
                    break;
            }

            //Final step: must come after all other filters!
            //Copy amplification from 20Hz to below 10-19Hz
            double level = TargetCurve[0].Y;
            for (int freq = 10; freq < 20; freq++)
            {
                TargetCurve.Insert(0, new DataPoint(freq, level));
            }
            //Copy amplification from 20.000Hz to above 20.000Hz
            level = TargetCurve[TargetCurve.Count - 1].Y;
            for (int freq = 20001; freq < 24000; freq++)
            {
                TargetCurve.Add(new DataPoint(freq, level));
            }

            return TargetCurve;
        }

        private static ObservableCollection<DataPoint> ApplyHighRollOff1(ObservableCollection<DataPoint> curve)
        {
            //Fitting data of high freq roll off 1
            /* This fitting is good between 3000Hz and 19000Hz
            Intercept	1.166568949
            Degree  1	-0.000692164
            Degree  2	6.73174E-07
            Degree  3	-2.52575E-10
            Degree  4	4.43275E-14
            Degree  5	-4.24227E-18
            Degree  6	2.28862E-22
            Degree  7	-6.57673E-27
            Degree  8	7.85802E-32
            */
            ObservableCollection<DataPoint> changed_curve = new ObservableCollection<DataPoint>();
            Double X, Y;
            foreach (var point in curve)
            {
                X = point.X;
                Y = point.Y;
                if (Y != Double.NegativeInfinity && X > 3000) // Fit starting from 3000Hz to be more accurate
                {
                    Y = Y - (1.0 - (
                        1.166568949
                        - 0.000692164 * Math.Pow(X, 1)
                        + 6.73174E-07 * Math.Pow(X, 2)
                        + -2.52575E-10 * Math.Pow(X, 3)
                        + 4.43275E-14 * Math.Pow(X, 4)
                        + -4.24227E-18 * Math.Pow(X, 5)
                        + 2.28862E-22 * Math.Pow(X, 6)
                        + -6.57673E-27 * Math.Pow(X, 7)
                        + 7.85802E-32 * Math.Pow(X, 8)
                   ));
                }
                changed_curve.Add(new DataPoint(X, Y));
            }
            return changed_curve;
        }

        private static ObservableCollection<DataPoint> ApplyHighRollOff2(ObservableCollection<DataPoint> curve)
        {
            //Fitting data of high freq roll off 2
            /* This fitting is good between 3000Hz and 19000Hz
            Intercept	1.200582698
            Degree  1	-0.000908785
            Degree  2	8.92631E-07
            Degree  3	-3.22846E-10
            Degree  4	5.21325E-14
            Degree  5	-4.48776E-18
            Degree  6	2.14608E-22
            Degree  7	-5.38736E-27
            Degree  8	5.53723E-32
            */
            ObservableCollection<DataPoint> changed_curve = new ObservableCollection<DataPoint>();
            Double X, Y;
            foreach (var point in curve)
            {
                X = point.X;
                Y = point.Y;
                if (Y != Double.NegativeInfinity && X > 3000) // Fit starting from 3000Hz to be more accurate
                {
                    Y = Y - (1.0 - (
                        1.200582698
                        + -0.000908785 * Math.Pow(X, 1)
                        + 8.92631E-07 * Math.Pow(X, 2)
                        + -3.22846E-10 * Math.Pow(X, 3)
                        + 5.21325E-14 * Math.Pow(X, 4)
                        + -4.48776E-18 * Math.Pow(X, 5)
                        + 2.14608E-22 * Math.Pow(X, 6)
                        + -5.38736E-27 * Math.Pow(X, 7)
                        + 5.53723E-32 * Math.Pow(X, 8)
                    ));
                }
                changed_curve.Add(new DataPoint(X, Y));
            }
            return changed_curve;
        }

        private static ObservableCollection<DataPoint> ApplyMidRangeCompensation(ObservableCollection<DataPoint> curve)
        {
            ObservableCollection<DataPoint> changed_curve = new ObservableCollection<DataPoint>();
            Double X, Y;
            foreach (var point in curve)
            {
                X = point.X;
                Y = point.Y;

                //Fitting data of mid range compensation
                /* This fitting is good between 500Hz and 2000Hz
                Intercept	1.131421808
                Degree  1	-0.003316652
                Degree  2	2.27864E-05
                Degree  3	-6.48028E-08
                Degree  4	8.89715E-11
                Degree  5	-6.07378E-14
                Degree  6	1.78965E-17
                Degree  7	-7.45727E-22
                Degree  8	-3.8917E-25
                */
                if (Y != Double.NegativeInfinity && X > 500 && X < 2000)
                {
                    Y = Y - (1.0 - (
                        1.131421808
                        + -0.003316652 * Math.Pow(X, 1)
                        + 2.27864E-05 * Math.Pow(X, 2)
                        + -6.48028E-08 * Math.Pow(X, 3)
                        + 8.89715E-11 * Math.Pow(X, 4)
                        + -6.07378E-14 * Math.Pow(X, 5)
                        + 1.78965E-17 * Math.Pow(X, 6)
                        + -7.45727E-22 * Math.Pow(X, 7)
                        + -3.8917E-25 * Math.Pow(X, 8)
                    ));
                }

                //Fitting data of mid range compensation
                /* This fitting is good between 2000Hz and 3800Hz
                Intercept	-40.64471932
                Degree  1	0.220198459
                Degree  2	-0.000318887
                Degree  3	2.04179E-07
                Degree  4	-6.60588E-11
                Degree  5	1.06282E-14
                Degree  6	-6.77933E-19
                */
                if (Y != Double.NegativeInfinity && X >= 2000 && X < 3800)
                {
                    Y = Y - (1.0 - (
                        -40.64471932
                        + 0.220198459 * Math.Pow(X, 1)
                        + -0.000318887 * Math.Pow(X, 2)
                        + 2.04179E-07 * Math.Pow(X, 3)
                        + -6.60588E-11 * Math.Pow(X, 4)
                        + 1.06282E-14 * Math.Pow(X, 5)
                        + -6.77933E-19 * Math.Pow(X, 6)
                    ));
                }

                changed_curve.Add(new DataPoint(X, Y));
            }
            return changed_curve;
        }


        //Definitions for some popular curves
        public static class AdytorCurvePrebuiltCurves
        {
            public static ObservableCollection<FreqAmpPair> ConvertToCollection(double[,] array)
            {
                ObservableCollection<FreqAmpPair> collection = new ObservableCollection<FreqAmpPair>();
                for (int i = 0; i < array.GetLength(0); i++)
                    collection.Add(new FreqAmpPair(array[i, 0], array[i, 1]));
                return collection;
            }

            public static double[,] CURVE_TOOLEBASS = new double[,]
            {
                {20.0, 6.3},
                {22.4, 6.3},
                {25.2, 6.3},
                {28.3, 6.3},
                {31.7, 6.3},
                {35.6, 6.2},
                {39.9, 6.2},
                {44.8, 6.1},
                {50.2, 6.0},
                {56.4, 5.8},
                {63.2, 5.5},
                {71.0, 5.1},
                {79.6, 4.6},
                {89.3, 4.0},
                {100.2, 3.3},
                {112.5, 2.6},
                {126.2, 1.9},
                {141.6, 1.4},
                {158.9, 1.2},
                {178.3, 1.1},
                {200.0, 1.0},
                {224.4, 0.9},
                {251.8, 0.9},
                {282.5, 0.7},
                {317.0, 0.7},
                {355.7, 0.6},
                {399.1, 0.4},
                {447.7, 0.4},
                {502.4, 0.3},
                {563.7, 0.2},
                {632.5, 0.2},
                {709.6, 0.1},
                {796.2, 0.1},
                {893.4, 0.0},
                {1002.4, 0.0},
                {1124.7, -0.1},
                {1261.9, -0.2},
                {1415.9, -0.3},
                {1588.7, -0.4},
                {1782.5, -0.5},
                {2000.0, -0.6},
                {2244.0, -0.6},
                {2517.9, -0.6},
                {2825.1, -0.7},
                {3169.8, -0.7},
                {3556.6, -0.7},
                {3990.5, -0.8},
                {4477.4, -0.8},
                {5023.8, -0.9},
                {5636.8, -0.9},
                {6324.6, -1.0},
                {7096.3, -1.1},
                {7962.1, -1.2},
                {8933.7, -1.4},
                {10023.7, -1.6},
                {11246.8, -1.9},
                {12619.1, -2.3},
                {14158.9, -2.7},
                {15886.6, -3.3},
                {17825.0, -4.2},
                {20000.0, -5.0},
            };

            public static double[,] CURVE_TOOLEHT = new double[,]
            {       
                {20.0, 3.6},
                {22.4, 3.6},
                {25.2, 3.6},
                {28.3, 3.6},
                {31.7, 3.5},
                {35.6, 3.5},
                {39.9, 3.5},
                {44.8, 3.5},
                {50.2, 3.5},
                {56.4, 3.4},
                {63.2, 3.4},
                {71.0, 3.4},
                {79.6, 3.3},
                {89.3, 3.2},
                {100.2, 3.2},
                {112.5, 3.1},
                {126.2, 2.9},
                {141.6, 2.8},
                {158.9, 2.7},
                {178.3, 2.5},
                {200.0, 2.4},
                {224.4, 2.3},
                {251.8, 2.2},
                {282.5, 2.0},
                {317.0, 1.9},
                {355.7, 1.9},
                {399.1, 1.7},
                {447.7, 1.6},
                {502.4, 1.5},
                {563.7, 1.4},
                {632.5, 1.3},
                {709.6, 1.2},
                {796.2, 1.0},
                {893.4, 0.9},
                {1002.4, 0.8},
                {1124.7, 0.7},
                {1261.9, 0.6},
                {1415.9, 0.4},
                {1588.7, 0.4},
                {1782.5, 0.3},
                {2000.0, 0.2},
                {2244.0, 0.2},
                {2517.9, 0.2},
                {2825.1, 0.2},
                {3169.8, 0.2},
                {3556.6, 0.2},
                {3990.5, 0.1},
                {4477.4, 0.1},
                {5023.8, 0.1},
                {5636.8, 0.0},
                {6324.6, 0.0},
                {7096.3, -0.2},
                {7962.1, -0.2},
                {8933.7, -0.3},
                {10023.7, -0.4},
                {11246.8, -0.3},
                {12619.1, -0.3},
                {14158.9, -0.3},
                {15886.6, -0.3},
                {17825.0, -0.3},
                {20000.0, -0.3},
            };

            public static double[,] CURVE_TOOLE = new double[,]
            {
                {20.0, 2.2},
                {22.4, 2.2},
                {25.2, 2.2},
                {28.3, 2.2},
                {31.7, 2.1},
                {35.6, 2.1},
                {39.9, 2.1},
                {44.8, 2.1},
                {50.2, 2.1},
                {56.4, 2.0},
                {63.2, 2.0},
                {71.0, 2.0},
                {79.6, 1.8},
                {89.3, 1.7},
                {100.2, 1.6},
                {112.5, 1.5},
                {126.2, 1.4},
                {141.6, 1.4},
                {158.9, 1.2},
                {178.3, 1.1},
                {200.0, 1.0},
                {224.4, 0.9},
                {251.8, 0.9},
                {282.5, 0.7},
                {317.0, 0.7},
                {355.7, 0.6},
                {399.1, 0.4},
                {447.7, 0.4},
                {502.4, 0.3},
                {563.7, 0.2},
                {632.5, 0.2},
                {709.6, 0.1},
                {796.2, 0.1},
                {893.4, 0.0},
                {1002.4, 0.0},
                {1124.7, -0.1},
                {1261.9, -0.2},
                {1415.9, -0.3},
                {1588.7, -0.4},
                {1782.5, -0.5},
                {2000.0, -0.6},
                {2244.0, -0.6},
                {2517.9, -0.6},
                {2825.1, -0.7},
                {3169.8, -0.7},
                {3556.6, -0.7},
                {3990.5, -0.8},
                {4477.4, -0.8},
                {5023.8, -0.9},
                {5636.8, -0.9},
                {6324.6, -1.0},
                {7096.3, -1.1},
                {7962.1, -1.2},
                {8933.7, -1.4},
                {10023.7, -1.6},
                {11246.8, -1.9},
                {12619.1, -2.3},
                {14158.9, -2.7},
                {15886.6, -3.3},
                {17825.0, -4.2},
                {20000.0, -5.0},
            };

            public static double[,] CURVE_BK = new double[,]
            {
                {20.0, 1.9},
                {22.4, 2.0},
                {25.2, 2.1},
                {28.3, 2.1},
                {31.7, 2.2},
                {35.6, 2.2},
                {39.9, 2.3},
                {44.8, 2.3},
                {50.2, 2.3},
                {56.4, 2.2},
                {63.2, 2.2},
                {71.0, 2.2},
                {79.6, 2.2},
                {89.3, 2.2},
                {100.2, 2.2},
                {112.5, 2.1},
                {126.2, 2.1},
                {141.6, 2.1},
                {158.9, 2.0},
                {178.3, 1.9},
                {200.0, 1.8},
                {224.4, 1.7},
                {251.8, 1.6},
                {282.5, 1.5},
                {317.0, 1.4},
                {355.7, 1.3},
                {399.1, 1.2},
                {447.7, 1.0},
                {502.4, 0.8},
                {563.7, 0.7},
                {632.5, 0.6},
                {709.6, 0.5},
                {796.2, 0.4},
                {893.4, 0.2},
                {1002.4, 0.0},
                {1124.7, -0.1},
                {1261.9, -0.2},
                {1415.9, -0.2},
                {1588.7, -0.3},
                {1782.5, -0.4},
                {2000.0, -0.6},
                {2244.0, -0.7},
                {2517.9, -0.8},
                {2825.1, -1.0},
                {3169.8, -1.3},
                {3556.6, -1.4},
                {3990.5, -1.6},
                {4477.4, -1.8},
                {5023.8, -1.9},
                {5636.8, -2.0},
                {6324.6, -2.2},
                {7096.3, -2.4},
                {7962.1, -2.5},
                {8933.7, -2.7},
                {10023.7, -2.9},
                {11246.8, -3.1},
                {12619.1, -3.4},
                {14158.9, -3.6},
                {15886.6, -3.9},
                {17825.0, -4.0},
                {20000.0, -4.2},
            };

        }
    }
}