// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.Notifications;

namespace Microsoft.Toolkit.Uwp.Notifications
{
    /// <summary>
    /// Provides access to sending and managing toast notifications. Works for all types of apps, even Win32 non-MSIX/sparse apps.
    /// </summary>
    public static class ToastNotificationManagerCompat
    {
        /// <summary>
        /// Creates a toast notifier.
        /// </summary>
        /// <returns><see cref="ToastNotifierCompat"/>An instance of the toast notifier.</returns>
        public static ToastNotifierCompat CreateToastNotifier()
        {
            return new ToastNotifierCompat(ToastNotificationManager.CreateToastNotifier());
        }

        /// <summary>
        /// Gets the <see cref="ToastNotificationHistoryCompat"/> object.
        /// </summary>
        public static ToastNotificationHistoryCompat History
        {
            get
            {
                return new ToastNotificationHistoryCompat(null);
            }
        }

        /// <summary>
        /// Gets a value indicating whether http images can be used within toasts. This is true if running with package identity (UWP, MSIX, or sparse package).
        /// </summary>
        public static bool CanUseHttpImages
        {
            get
            {
                return true;
            }
        }
    }
}