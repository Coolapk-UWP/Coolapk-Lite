﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels
{
    internal interface ICanComboBoxChangeSelectedIndex
    {
        List<string> ItemSource { get; }
        int ComboBoxSelectedIndex { get; }
        void SetComboBoxSelectedIndex(int value);
    }

    internal interface ICanToggleChangeSelectedIndex
    {
        bool ToggleIsOn { get; }
    }

    public interface IViewModel : INotifyPropertyChanged
    {
        Task Refresh(bool reset);
        string Title { get; }
    }
}
