using Windows.Globalization.NumberFormatting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;

namespace CoolapkLite.Controls
{
    public class RatingControlAutomationPeer :
        FrameworkElementAutomationPeer,
        IValueProvider,
        IRangeValueProvider
    {
        public RatingControlAutomationPeer(RatingControl owner)
            : base(owner)
        {
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return "Rating Slider";
        }

        // Properties.
        public bool IsReadOnly => GetRatingControl().IsReadOnly;

        string IValueProvider.Value
        {
            get
            {
                double ratingValue = GetRatingControl().Value;
                string valueString;

                if (ratingValue == -1)
                {
                    double placeholderValue = GetRatingControl().PlaceholderValue;
                    if (placeholderValue == -1)
                    {
                        valueString = "Rating Unset";
                    }
                    else
                    {
                        valueString = GenerateValue_ValueString("Community Rating, {0} of {1}", placeholderValue);
                    }
                }
                else
                {
                    valueString = GenerateValue_ValueString("Rating, {0} of {1}", ratingValue);
                }

                return valueString;
            }
        }

        public void SetValue(string value)
        {
            if (double.TryParse(value, out double potentialRating))
            {
                GetRatingControl().Value = potentialRating;
            }
        }

        // IRangeValueProvider overrides
        public double SmallChange => 1.0;

        public double LargeChange => 1.0;

        public double Maximum => GetRatingControl().MaxRating;

        public double Minimum => 0;

        public double Value
        {
            get
            {
                // Change this to provide a placeholder value too.
                double value = GetRatingControl().Value;
                if (value == -1)
                {
                    return 0;
                }
                else
                {
                    return value;
                }
            }
        }

        public void SetValue(double value)
        {
            GetRatingControl().Value = value;
        }

        //IAutomationPeerOverrides

        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value || patternInterface == PatternInterface.RangeValue)
            {
                return this;
            }

            return base.GetPatternCore(patternInterface);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Slider;
        }

        // Protected methods
        internal void RaisePropertyChangedEvent(double newValue)
        {
            // UIA doesn't tolerate a null doubles, so we convert them to zeroes.
            double oldValue = GetRatingControl().Value;

            if (newValue == -1)
            {
                newValue = 0.0;
            }

            RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue.ToString(), newValue.ToString());
            RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, oldValue, newValue);
        }

        // private methods

        RatingControl GetRatingControl()
        {
            UIElement owner = Owner;
            return (RatingControl)owner;
        }

        int DetermineFractionDigits(double value)
        {
            value *= 100;
            int intValue = (int)value;

            // When reading out the Value_Value, we want clients to read out the least number of digits
            // possible. We don't want a 3 (represented as a double) to be read out as 3.00...
            // Here we determine the number of digits past the decimal point we care about,
            // and this number is used by the caller to truncate the Value_Value string.

            if (intValue % 100 == 0)
            {
                return 0;
            }
            else if (intValue % 10 == 0)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        int DetermineSignificantDigits(double value, int fractionDigits)
        {
            int sigFigsInt = (int)value;
            int length = 0;

            while (sigFigsInt > 0)
            {
                sigFigsInt /= 10;
                length++;
            }

            return length + fractionDigits;
        }

        string GenerateValue_ValueString(string resourceString, double ratingValue)
        {
            DecimalFormatter formatter = new DecimalFormatter();
            SignificantDigitsNumberRounder rounder = new SignificantDigitsNumberRounder();

            string maxRatingString = GetRatingControl().MaxRating.ToString();

            int fractionDigits = DetermineFractionDigits(ratingValue);
            int sigDigits = DetermineSignificantDigits(ratingValue, fractionDigits);
            formatter.FractionDigits = fractionDigits;
            rounder.SignificantDigits = (uint)sigDigits;

            string ratingString = formatter.Format(ratingValue);

            return string.Format(resourceString, ratingString, maxRatingString);
        }
    }
}
