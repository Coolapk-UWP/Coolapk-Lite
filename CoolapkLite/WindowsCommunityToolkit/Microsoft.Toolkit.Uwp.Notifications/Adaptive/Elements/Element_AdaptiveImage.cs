// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Toolkit.Uwp.Notifications.Adaptive.Elements
{
    internal sealed class Element_AdaptiveImage : IElement_TileBindingChild, IElement_ToastBindingChild, IElement_AdaptiveSubgroupChild, IHaveXmlName, IHaveXmlNamedProperties
    {
        internal const AdaptiveImagePlacement DEFAULT_PLACEMENT = AdaptiveImagePlacement.Inline;
        internal const AdaptiveImageCrop DEFAULT_CROP = AdaptiveImageCrop.Default;
        internal const AdaptiveImageAlign DEFAULT_ALIGN = AdaptiveImageAlign.Default;

        public int? Id { get; set; }

        public string Src { get; set; }

        public string Alt { get; set; }

        public bool? AddImageQuery { get; set; }

        public AdaptiveImagePlacement Placement { get; set; } = DEFAULT_PLACEMENT;

        public AdaptiveImageAlign Align { get; set; } = DEFAULT_ALIGN;

        public AdaptiveImageCrop Crop { get; set; } = DEFAULT_CROP;

        public bool? RemoveMargin { get; set; }

        private int? _overlay;

        public int? Overlay
        {
            get
            {
                return _overlay;
            }

            set
            {
                if (value != null)
                {
                    Element_TileBinding.CheckOverlayValue(value.Value);
                }

                _overlay = value;
            }
        }

        public string SpriteSheetSrc { get; set; }

        public uint? SpriteSheetHeight { get; set; }

        public uint? SpriteSheetFps { get; set; }

        public uint? SpriteSheetStartingFrame { get; set; }

        /// <inheritdoc/>
        string IHaveXmlName.Name => "image";

        /// <inheritdoc/>
        IEnumerable<KeyValuePair<string, object>> IHaveXmlNamedProperties.EnumerateNamedProperties()
        {
            yield return new KeyValuePair<string, object>("id", Id);
            yield return new KeyValuePair<string, object>("src", Src);
            yield return new KeyValuePair<string, object>("alt", Alt);
            yield return new KeyValuePair<string, object>("addImageQuery", AddImageQuery);

            if (Placement != DEFAULT_PLACEMENT)
            {
                yield return new KeyValuePair<string, object>("placement", Placement.ToPascalCaseString());
            }

            if (Align != DEFAULT_ALIGN)
            {
                yield return new KeyValuePair<string, object>("hint-align", Align.ToPascalCaseString());
            }

            if (Crop != DEFAULT_CROP)
            {
                yield return new KeyValuePair<string, object>("hint-crop", Crop.ToPascalCaseString());
            }

            yield return new KeyValuePair<string, object>("hint-removeMargin", RemoveMargin);
            yield return new KeyValuePair<string, object>("hint-overlay", Overlay);
            yield return new KeyValuePair<string, object>("spritesheet-src", SpriteSheetSrc);
            yield return new KeyValuePair<string, object>("spritesheet-height", SpriteSheetHeight);
            yield return new KeyValuePair<string, object>("spritesheet-fps", SpriteSheetFps);
            yield return new KeyValuePair<string, object>("spritesheet-startingFrame", SpriteSheetStartingFrame);
        }
    }
}