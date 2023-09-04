// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    internal class DataGridCellCollection
    {
        private readonly List<DataGridCell> _cells;
        private readonly DataGridRow _owningRow;

        internal event EventHandler<DataGridCellEventArgs> CellAdded;

        internal event EventHandler<DataGridCellEventArgs> CellRemoved;

        public DataGridCellCollection(DataGridRow owningRow)
        {
            _owningRow = owningRow;
            _cells = new List<DataGridCell>();
        }

        public int Count
        {
            get
            {
                return _cells.Count;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _cells.GetEnumerator();
        }

        public void Insert(int cellIndex, DataGridCell cell)
        {
            Debug.Assert(cellIndex >= 0 && cellIndex <= _cells.Count, "Expected cellIndex between 0 and _cells.Count inclusive.");
            Debug.Assert(cell != null, "Expected non-null cell.");

            cell.OwningRow = _owningRow;
            _cells.Insert(cellIndex, cell);

            CellAdded?.Invoke(this, new DataGridCellEventArgs(cell));
        }

        public void RemoveAt(int cellIndex)
        {
            DataGridCell dataGridCell = _cells[cellIndex];
            _cells.RemoveAt(cellIndex);
            dataGridCell.OwningRow = null;
            CellRemoved?.Invoke(this, new DataGridCellEventArgs(dataGridCell));
        }

        public DataGridCell this[int index]
        {
            get
            {
                return index < 0 || index >= _cells.Count
                    ? throw DataGridError.DataGrid.ValueMustBeBetween("index", "Index", 0, true, _cells.Count, false)
                    : _cells[index];
            }
        }
    }
}