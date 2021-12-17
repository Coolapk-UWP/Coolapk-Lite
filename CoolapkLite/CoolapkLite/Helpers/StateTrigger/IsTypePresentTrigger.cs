using CoolapkLite.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace CoolapkLite.Helpers.StateTrigger
{
    public class IsClassPresentTrigger : StateTriggerBase
    {
        public string TypeName { get; set; }
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
                    UIHelper.IsTypePresent(AssemblyName, TypeName);
                }

                // If the property presence matches _isPresent then the trigger will be activated;
                SetActive(_isPresent == _isPropertyPresent);
            }
        }
    }
}
