// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Windows.UI.Notifications;

namespace Microsoft.Toolkit.Uwp.Notifications
{
    /// <summary>
    /// Allows you to show and schedule toast notifications.
    /// </summary>
    public sealed class ToastNotifierCompat
    {
        private ToastNotifier _notifier;

        internal ToastNotifierCompat(ToastNotifier notifier)
        {
            _notifier = notifier;
        }

        /// <summary>
        /// Displays the specified toast notification.
        /// </summary>
        /// <param name="notification">The object that contains the content of the toast notification to display.</param>
        public void Show(ToastNotification notification)
        {
            _notifier.Show(notification);
        }

        /// <summary>
        /// Hides the specified toast notification from the screen (moves it into Action Center).
        /// </summary>
        /// <param name="notification">The object that specifies the toast to hide.</param>
        public void Hide(ToastNotification notification)
        {
            _notifier.Hide(notification);
        }

        /// <summary>
        /// Adds a ScheduledToastNotification for later display by Windows.
        /// </summary>
        /// <param name="scheduledToast">The scheduled toast notification, which includes its content and timing instructions.</param>
        public void AddToSchedule(ScheduledToastNotification scheduledToast)
        {
            _notifier.AddToSchedule(scheduledToast);
        }

        /// <summary>
        /// Cancels the scheduled display of a specified ScheduledToastNotification.
        /// </summary>
        /// <param name="scheduledToast">The notification to remove from the schedule.</param>
        public void RemoveFromSchedule(ScheduledToastNotification scheduledToast)
        {
            _notifier.RemoveFromSchedule(scheduledToast);
        }

        /// <summary>
        /// Gets the collection of ScheduledToastNotification objects that this app has scheduled for display.
        /// </summary>
        /// <returns>The collection of scheduled toast notifications that the app bound to this notifier has scheduled for timed display.</returns>
        public IReadOnlyList<ScheduledToastNotification> GetScheduledToastNotifications()
        {
            return _notifier.GetScheduledToastNotifications();
        }

        /// <summary>
        /// Updates the existing toast notification that has the specified tag and belongs to the specified notification group.
        /// </summary>
        /// <param name="data">An object that contains the updated info.</param>
        /// <param name="tag">The identifier of the toast notification to update.</param>
        /// <param name="group">The ID of the ToastCollection that contains the notification.</param>
        /// <returns>A value that indicates the result of the update (failure, success, etc).</returns>
        public NotificationUpdateResult Update(NotificationData data, string tag, string group)
        {
            return _notifier.Update(data, tag, group);
        }

        /// <summary>
        /// Updates the existing toast notification that has the specified tag.
        /// </summary>
        /// <param name="data">An object that contains the updated info.</param>
        /// <param name="tag">The identifier of the toast notification to update.</param>
        /// <returns>A value that indicates the result of the update (failure, success, etc).</returns>
        public NotificationUpdateResult Update(NotificationData data, string tag)
        {
            return _notifier.Update(data, tag);
        }

        /// <summary>
        /// Gets a value that tells you whether there is an app, user, or system block that prevents the display of a toast notification.
        /// </summary>
        public NotificationSetting Setting
        {
            get
            {
                return _notifier.Setting;
            }
        }
    }
}