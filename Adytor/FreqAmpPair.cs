using System;
using System.ComponentModel;

namespace Adytor
{
    public class FreqAmpPair : INotifyPropertyChanged, IEquatable<FreqAmpPair>, IComparable<FreqAmpPair>
    {
        private static readonly double FrequencyMin = 10; //10Hz Chris Kyriakakis
        private static readonly double FrequencyMax = 24000; //24000Hz Chris Kyriakakis
        private static readonly double LevelMin = -20; //-12dB AUDYSSEY MultiEQ app -> -20dB Chris Kyriakakis
        private static readonly double LevelMax = 12; //12dB AUDYSSEY MultiEQ app -> +9dB Chris Kyriakakis -> +12 dB in ady afile!
        double _freq;
        double _level;
        public double Frequency
        {
            get
            {
                return _freq;
            }
            set
            {
                double oldValue = _freq;
                _freq = ApplyFrequencyLimit(value);

                if (oldValue != _freq)
                    RaisePropertyChanged("Frequency");
            }
        }
        public double Level
        {
            get
            {
                return _level;
            }
            set
            {
                double oldValue = _level;
                _level = ApplyLevelLimit(value);

                if(oldValue != _level)
                    RaisePropertyChanged("Level");
            }
        }
        public FreqAmpPair(string freq, string lvl)
        {
            Frequency = Double.Parse(freq.Trim(), System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            Level = Double.Parse(lvl.Trim(), System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        }
        public FreqAmpPair(FreqAmpPair amp_pair)
        {
            Frequency = amp_pair.Frequency;
            Level = amp_pair.Level;
        }
        public FreqAmpPair(double freq, double lvl)
        {
            Frequency = freq;
            Level = lvl;
        }
        public override string ToString()
        {
            return "{" + Frequency.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + ", " + Level.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "}";
        }

        public static double ApplyFrequencyLimit(double X)
        {
            return Math.Max(Math.Min(Math.Round(X, 1), FrequencyMax), FrequencyMin);
        }

        public static double ApplyLevelLimit(double Y)
        {
            return Math.Max(Math.Min(Math.Round(Y,1), LevelMax), LevelMin);
        }

        /*
            * IEquatable implemention
            */

        bool IEquatable<FreqAmpPair>.Equals(FreqAmpPair other)
        {
            return (this.Frequency == other.Frequency) && (this.Level == other.Level);
        }

        public bool Equals(FreqAmpPair other)
        {
            return (this.Frequency == other.Frequency)
                && (this.Level == other.Level);
        }

        public static bool operator ==(FreqAmpPair term1, FreqAmpPair term2)
        { 
            return term1.Equals(term2);
        }

        public static bool operator !=(FreqAmpPair term1, FreqAmpPair term2)
        {
            return !term1.Equals(term2);
        }

        public override bool Equals(object other)
        {
            if (other is FreqAmpPair)
                return this.Equals((FreqAmpPair)other);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Frequency, this.Level);  
        }

        /*
            * INotifyPropertyChanged implemention
            */

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /*
        * IComparable implemention
        */

        int IComparable<FreqAmpPair>.CompareTo(FreqAmpPair other)
        {
            if (other == null) return 1;
            return this.Frequency.CompareTo(other.Frequency);
        }
    }
   
}