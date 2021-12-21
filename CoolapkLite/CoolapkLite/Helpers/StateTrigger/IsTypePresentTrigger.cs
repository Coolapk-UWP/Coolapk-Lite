using Windows.UI.Xaml;

namespace CoolapkLite.Helpers.StateTrigger
{
    public class IsTypePresentTrigger : StateTriggerBase
    {
        public string ClassTypeName { get; set; }
        public string AssemblyName { get; set; }

        private bool _isPresent;
        private bool? _isPropertyPresent = null;

        public bool IsPresent
        {
            get { return _isPresent; }
            set
            {
                _isPresent = value;
                if (_isPropertyPresent == null)
                {
                    // Call into ApiInformation method to determine if property is present.
                    _isPropertyPresent =
                    UIHelper.IsTypePresent(AssemblyName, ClassTypeName);
                }

                // If the property presence matches _isPresent then the trigger will be activated;
                SetActive(_isPresent == _isPropertyPresent);
            }
        }
    }
}
