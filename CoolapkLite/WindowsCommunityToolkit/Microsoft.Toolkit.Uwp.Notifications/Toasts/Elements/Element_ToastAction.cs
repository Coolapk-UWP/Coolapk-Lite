// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Toolkit.Uwp.Notifications
{
    internal sealed class Element_ToastAction : IElement_ToastActionsChild, IElement_ToastActivatable, IHaveXmlName, IHaveXmlNamedProperties
    {
        internal const Element_ToastActivationType DEFAULT_ACTIVATION_TYPE = Element_ToastActivationType.Foreground;
        internal const ToastAfterActivationBehavior DEFAULT_AFTER_ACTIVATION_BEHAVIOR = ToastAfterActivationBehavior.Default;
        internal const Element_ToastActionPlacement DEFAULT_PLACEMENT = Element_ToastActionPlacement.Inline;

        /// <summary>
        /// Gets or sets the text to be displayed on the button.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the arguments attribute describing the app-defined data that the app can later retrieve once it is activated from user taking this action.
        /// </summary>
        public string Arguments { get; set; }

        public Element_ToastActivationType ActivationType { get; set; } = DEFAULT_ACTIVATION_TYPE;

        public string ProtocolActivationTargetApplicationPfn { get; set; }

        public ToastAfterActivationBehavior AfterActivationBehavior { get; set; } = DEFAULT_AFTER_ACTIVATION_BEHAVIOR;

        /// <summary>
        /// Gets or sets optional value to provide an image icon for this action to display inside the button alone with the text content.
        /// </summary>
        public string ImageUri { get; set; }

        /// <summary>
        /// Gets or sets value used for the quick reply scenario.
        /// </summary>
        public string InputId { get; set; }

        public Element_ToastActionPlacement Placement { get; set; } = DEFAULT_PLACEMENT;

        public string HintActionId { get; set; }

        /// <inheritdoc/>
        string IHaveXmlName.Name => "action";

        /// <inheritdoc/>
        IEnumerable<KeyValuePair<string, object>> IHaveXmlNamedProperties.EnumerateNamedProperties()
        {
            yield return new KeyValuePair<string, object>("content", Content);
            yield return new KeyValuePair<string, object>("arguments", Arguments);

            if (ActivationType != DEFAULT_ACTIVATION_TYPE)
            {
                yield return new KeyValuePair<string, object>("activationType", ActivationType.ToPascalCaseString());
            }

            yield return new KeyValuePair<string, object>("protocolActivationTargetApplicationPfn", ProtocolActivationTargetApplicationPfn);

            if (AfterActivationBehavior != DEFAULT_AFTER_ACTIVATION_BEHAVIOR)
            {
                yield return new KeyValuePair<string, object>("afterActivationBehavior", AfterActivationBehavior.ToPascalCaseString());
            }

            yield return new KeyValuePair<string, object>("imageUri", ImageUri);
            yield return new KeyValuePair<string, object>("hint-inputId", InputId);

            if (Placement != DEFAULT_PLACEMENT)
            {
                yield return new KeyValuePair<string, object>("placement", Placement.ToPascalCaseString());
            }

            yield return new KeyValuePair<string, object>("hint-actionId", HintActionId);
        }
    }

    internal enum Element_ToastActionPlacement
    {
        Inline,
        ContextMenu
    }

    internal enum Element_ToastActivationType
    {
        /// <summary>
        /// Default value. Your foreground app is launched.
        /// </summary>
        Foreground,

        /// <summary>
        /// Your corresponding background task (assuming you set everything up) is triggered, and you can execute code in the background (like sending the user's quick reply message) without interrupting the user.
        /// </summary>
        Background,

        /// <summary>
        /// Launch a different app using protocol activation.
        /// </summary>
        Protocol,

        /// <summary>
        /// System handles the activation.
        /// </summary>
        System
    }
}