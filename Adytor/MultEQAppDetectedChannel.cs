using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Adytor.MultEQ;
using OxyPlot;

namespace Adytor
{
    public class DetectedChannel : MultEQLists, INotifyPropertyChanged
    {
        //
        private string _customCrossover = null;
        private int? _enChannelType = null;
        private bool? _isSkipMeasurement = null;
        private string _customLevel = null;
        private decimal? _customDistance = null;
        private decimal? _frequencyRangeRolloff = null;
        private string _commandId = null;
        private string _customSpeakerType = null;
        private string _delayAdjustment = null;
        private ChannelReport _channelReport = null;
        private Dictionary<string, string[]> _responseData = null;
        private string _trimAdjustment = null;
        private bool? _midrangeCompensation = null;

        // local for data binding (converted when serialised)
        private ObservableCollection<FreqAmpPair> _customTargetCurveCollection = new ObservableCollection<FreqAmpPair>();

        // local for data binding (not serialised)
        private int _customCrossoverIndex = -1;
        private ObservableCollection<DataPoint> _audysseyReferenceCollection = new ObservableCollection<DataPoint>();
        private ObservableCollection<DataPoint> _customResultingCurveCollection = new ObservableCollection<DataPoint>();
        private bool _isSelectedForCopyTargetPoint = false;

        #region Properties
        [JsonIgnore]
        public bool Sticky { get; set; } = false;
        public int? EnChannelType
        {
            get
            {
                return _enChannelType;
            }
            set
            {
                _enChannelType = value;
                RaisePropertyChanged("EnChannelType");
            }
        }
        public bool? IsSkipMeasurement
        {
            get
            {
                return _isSkipMeasurement;
            }
            set
            {
                _isSkipMeasurement = value;
                RaisePropertyChanged("IsSkipMeasurement");
            }
        }
        public string DelayAdjustment
        {
            get
            {
                return _delayAdjustment;
            }
            set
            {
                _delayAdjustment = value;
                RaisePropertyChanged("DelayAdjustment");
            }
        }
        public string CommandId
        {
            get
            {
                return _commandId;
            }
            set
            {
                _commandId = value;
                RaisePropertyChanged("CommandId");
            }
        }
        public string TrimAdjustment
        {
            get
            {
                return _trimAdjustment;
            }
            set
            {
                _trimAdjustment = value;
                RaisePropertyChanged("TrimAdjustment");
            }
        }
        public ChannelReport ChannelReport
        {
            get
            {
                return _channelReport;
            }
            set
            {
                _channelReport = value;
                RaisePropertyChanged("ChannelReport");
            }
        }
        public Dictionary<string, string[]> ResponseData
        {
            get
            {
                return _responseData;
            }
            set
            {
                _responseData = value;
                RaisePropertyChanged("ResponseData");
            }
        }
        public string[] CustomTargetCurvePoints
        {
            get
            {
                return ConvertCollectionToStringArray(CustomTargetCurveCollection);
            }
            set
            {
                CustomTargetCurveCollection = ConvertStringArrayToCollection(value);
                RaisePropertyChanged("CustomTargetCurvePoints");
            }
        }
        [JsonIgnore]
        public bool IsSelectedForCopyTargetPoint
        {
            get
            {
                return _isSelectedForCopyTargetPoint;
            }
            set
            {
                _isSelectedForCopyTargetPoint = value;
                RaisePropertyChanged("IsSelectedForCopyTargetPoint");
            }
        }
        [JsonIgnore]
        public ObservableCollection<DataPoint> AudysseyReferenceCollection
        {
            get
            {
                return _audysseyReferenceCollection;
            }
            set
            {
                _audysseyReferenceCollection = value;
                RaisePropertyChanged("AudysseyReferenceCollection");
                _audysseyReferenceCollection.CollectionChanged += AudysseyReferenceCollectionChangedHandler;
            }
        }
        [JsonIgnore]
        public ObservableCollection<DataPoint> CustomResultingCurveCollection
        {
            get
            {
                return _customResultingCurveCollection;
            }
            set
            {
                _customResultingCurveCollection = value;
                RaisePropertyChanged("CustomResultingCurveCollection");
                _customResultingCurveCollection.CollectionChanged += CustomResultingCurveCollectionChangedHandler;
            }
        }
        [JsonIgnore]
        public ObservableCollection<FreqAmpPair> CustomTargetCurveCollection
        {
            get
            {
                return _customTargetCurveCollection;
            }
            set
            {
                _customTargetCurveCollection = value;
                RaisePropertyChanged("CustomTargetCurveCollection");
                _customTargetCurveCollection.CollectionChanged += CustomTargetCurveCollectionChangedHandler;
            }
        }
        public bool? MidrangeCompensation
        {
            get
            {
                return _midrangeCompensation;
            }
            set
            {
                _midrangeCompensation = value;
                RaisePropertyChanged("MidrangeCompensation");
            }
        }
        public decimal? FrequencyRangeRolloff
        {
            get
            {
                return _frequencyRangeRolloff;
            }
            set
            {
                _frequencyRangeRolloff = value;
                RaisePropertyChanged("FrequencyRangeRolloff");
            }
        }
        public string CustomLevel
        {
            get
            {
                return _customLevel;
            }
            set
            {
                _customLevel = value;
                RaisePropertyChanged("CustomLevel");
            }
        }
        public string CustomSpeakerType
        {
            get
            {
                return _customSpeakerType;
            }
            set
            {
                _customSpeakerType = value;
                RaisePropertyChanged("CustomSpeakerType");
            }
        }
        public decimal? CustomDistance
        {
            get
            {
                return _customDistance;
            }
            set
            {
                _customDistance = value;
                RaisePropertyChanged("CustomDistance");
            }
        }
        public string CustomCrossover
        {
            get
            {
                return _customCrossover;
            }
            set
            {
                _customCrossover = value;
                RaisePropertyChanged("CustomCrossover");
                _customCrossoverIndex = CrossoverList.IndexOf(value + "0");
                RaisePropertyChanged("CustomCrossoverIndex");
            }
        }
        [JsonIgnore]
        public int CustomCrossoverIndex
        {
            get
            {
                return _customCrossoverIndex;
            }
            set
            {
                _customCrossoverIndex = value;
                RaisePropertyChanged("CustomCrossoverIndex");
                _customCrossover = CrossoverList[value];
                RaisePropertyChanged("CustomCrossover");
            }
        }
        #endregion
        public bool ShouldSerializeResponseData()
        {
            return true;
        }
        public bool ShouldSerializeCustomTargetCurvePointsDictionary()
        {
            return false;
        }
        public bool ShouldSerializeCustomTargetCurvePoints()
        {
            if (EnChannelType == 55)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool ShouldSerializeCustomLevel()
        {
            return true;
        }
        public bool ShouldSerializeCustomSpeakerType()
        {
            return (CustomSpeakerType != null);
        }
        public bool ShouldSerializeCustomDistance()
        {
            return (CustomDistance.HasValue);
        }
        public bool ShouldSerializeCustomCrossover()
        {
            return (CustomCrossover != null);
        }

        private ObservableCollection<FreqAmpPair> ConvertStringArrayToCollection(string[] array)
        {
            ObservableCollection<FreqAmpPair> result = new ObservableCollection<FreqAmpPair>();
            foreach (string s in array)
            {
                string str = s.Substring(1, s.Length - 2);
                string[] arr = str.Split(',');
                result.Add(new FreqAmpPair(arr[0], arr[1]));
            }
            return new ObservableCollection<FreqAmpPair>(result.OrderBy(x => x.Frequency));
        }
        private string[] ConvertCollectionToStringArray(ObservableCollection<FreqAmpPair> dict)
        {
            string[] result = new string[dict.Count];
            for (int i = 0; i < dict.Count; i++)
            {
                result[i] = dict[i].ToString();
            }
            return result;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var property in this.GetType().GetProperties())
            {
                sb.Append(property + "=" + property.GetValue(this, null) + "\r\n");
            }

            if (ChannelReport != null) sb.Append(ChannelReport.ToString());
            if (ResponseData != null) sb.Append(ResponseData.ToString());

            return sb.ToString();
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        //Propagating changes of collection properties to DetectedChannel class
        private void CustomTargetCurveCollectionChangedHandler(object sender, EventArgs e)
        {
            RaisePropertyChanged("CustomTargetCurveCollection");
        }
        private void AudysseyReferenceCollectionChangedHandler(object sender, EventArgs e)
        {
            RaisePropertyChanged("AudysseyReferenceCollection");
        }
        private void CustomResultingCurveCollectionChangedHandler(object sender, EventArgs e)
        {
            RaisePropertyChanged("CustomResultingCurveCollection");
        }
    }
    
}