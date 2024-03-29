﻿using System;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls
{
    public class PersonPicture : ImageControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageControl"/> class.
        /// </summary>
        public PersonPicture() : base() => DefaultStyleKey = typeof(PersonPicture);

        #region DisplayName

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(
                nameof(DisplayName),
                typeof(string),
                typeof(PersonPicture),
                new PropertyMetadata(string.Empty, OnDisplayNamePropertyChanged));

        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        private static void OnDisplayNamePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PersonPicture)sender).OnDisplayNamePropertyChanged();
        }

        #endregion

        #region InitialsGlyph

        public static readonly DependencyProperty InitialsGlyphProperty =
            DependencyProperty.Register(
                nameof(InitialsGlyph),
                typeof(string),
                typeof(PersonPicture),
                new PropertyMetadata("\uE77B"));

        public string InitialsGlyph
        {
            get => (string)GetValue(InitialsGlyphProperty);
            set => SetValue(InitialsGlyphProperty, value);
        }

        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            OnDisplayNamePropertyChanged();
        }

        private void OnDisplayNamePropertyChanged()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                _ = VisualStateManager.GoToState(this, "NoInitials", false);
            }
            else
            {
                try
                {
                    string m_displayNameInitials = InitialsGenerator.InitialsFromDisplayName(DisplayName);
                    if (string.IsNullOrWhiteSpace(m_displayNameInitials))
                    {
                        _ = VisualStateManager.GoToState(this, "NoInitials", false);
                    }
                    else
                    {
                        TemplateSettings.ActualInitials = m_displayNameInitials;
                        _ = VisualStateManager.GoToState(this, "Initials", false);
                    }
                }
                catch (Exception)
                {
                    _ = VisualStateManager.GoToState(this, "NoInitials", false);
                }
            }
        }
    }
}
