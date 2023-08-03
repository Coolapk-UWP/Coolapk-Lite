// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// This structure encapsulate the cell information necessary when clipboard content is prepared.
    /// </summary>
    public readonly struct DataGridClipboardCellContent
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridClipboardCellContent"/> struct.
        /// </summary>
        /// <param name="item">DataGrid row item containing the cell.</param>
        /// <param name="column">DataGridColumn containing the cell.</param>
        /// <param name="content">DataGrid cell value.</param>
        public DataGridClipboardCellContent(object item, DataGridColumn column, object content)
        {
            Item = item;
            Column = column;
            Content = content;
        }

        /// <summary>
        /// Gets the <see cref="DataGridColumn"/> column containing the cell.
        /// </summary>
        public DataGridColumn Column { get; }

        /// <summary>
        /// Gets the <see cref="DataGridCell"/> cell content.
        /// </summary>
        public object Content { get; }

        /// <summary>
        /// Gets the <see cref="DataGridRow"/> row item containing the cell.
        /// </summary>
        public object Item { get; }

        /// <summary>
        /// Field-by-field comparison to avoid reflection-based ValueType.Equals.
        /// </summary>
        /// <param name="obj">DataGridClipboardCellContent to compare.</param>
        /// <returns>True if this and data are equal</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is DataGridClipboardCellContent))
            {
                return false;
            }

            DataGridClipboardCellContent clipboardCellContent = (DataGridClipboardCellContent)obj;
            return Column == clipboardCellContent.Column && Content == clipboardCellContent.Content && Item == clipboardCellContent.Item;
        }

        /// <summary>
        /// Returns a deterministic hash code.
        /// </summary>
        /// <returns>Hash value.</returns>
        public override int GetHashCode()
        {
            return Column.GetHashCode() ^ Content.GetHashCode() ^ Item.GetHashCode();
        }

        /// <summary>
        /// Field-by-field comparison to avoid reflection-based ValueType.Equals.
        /// </summary>
        /// <param name="clipboardCellContent1">The first DataGridClipboardCellContent.</param>
        /// <param name="clipboardCellContent2">The second DataGridClipboardCellContent.</param>
        /// <returns>True if and only if clipboardCellContent1 and clipboardCellContent2 are equal.</returns>
        public static bool operator ==(DataGridClipboardCellContent clipboardCellContent1, DataGridClipboardCellContent clipboardCellContent2)
        {
            return clipboardCellContent1.Column == clipboardCellContent2.Column && clipboardCellContent1.Content == clipboardCellContent2.Content && clipboardCellContent1.Item == clipboardCellContent2.Item;
        }

        /// <summary>
        /// Field-by-field comparison to avoid reflection-based ValueType.Equals.
        /// </summary>
        /// <param name="clipboardCellContent1">The first DataGridClipboardCellContent.</param>
        /// <param name="clipboardCellContent2">The second DataGridClipboardCellContent.</param>
        /// <returns>True if clipboardCellContent1 and clipboardCellContent2 are NOT equal.</returns>
        public static bool operator !=(DataGridClipboardCellContent clipboardCellContent1, DataGridClipboardCellContent clipboardCellContent2)
        {
            return clipboardCellContent1.Column != clipboardCellContent2.Column || clipboardCellContent1.Content != clipboardCellContent2.Content
|| clipboardCellContent1.Item != clipboardCellContent2.Item;
        }
    }
}