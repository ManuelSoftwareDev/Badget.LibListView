using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Badget.LibListview.Sorting
{
    /// <summary>
    /// Provides a class which Supporting Sorting on Columns Click
    /// </summary>
    public sealed class Sorter : IDisposable
    {
        internal Sorter(ListView lst, bool ShowSortArrow)
        {
            this.lst = lst;
            lst.ColumnClick += eventDoSort;
            showSortArrow = ShowSortArrow;
        }

        private void eventDoSort(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != sortColumn)
            {
                sortColumn = e.Column;
                lst.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (lst.Sorting == SortOrder.Ascending)
                    lst.Sorting = SortOrder.Descending;
                else
                    lst.Sorting = SortOrder.Ascending;
            }
            
            this.lst.ListViewItemSorter = new SortingFunctions.ListViewItemComparer(e.Column,
                                                              lst.Sorting);

            lst.Sort();

            if(showSortArrow)
                SortingFunctions.ListViewExtensions.SetSortIcon(lst, e.Column, lst.Sorting);
        }

        private bool showSortArrow;
        private ListView lst;
        private int sortColumn = -1;

        /// <summary>
        /// Registers a Sorter for your ListView
        /// </summary>
        /// <param name="listview">ListView where Sorter is working</param>
        /// <param name="showSortArrow">Wheter a SortArrow is shown when SortOrder Changes</param>
        /// <returns></returns>
        public static Sorter RegisterSorter(ListView listview, bool showSortArrow = false)
        {
            return new Sorter(listview, showSortArrow);
        }

        /// <summary>
        /// Unregisters a Sorter
        /// </summary>
        /// <param name="sorter">Sorter Instance</param>
        public static void UnregisterSorter(Sorter sorter)
        {
            sorter.Dispose();
        }

        /// <summary>
        /// Disposes all used Resources and unregister the SortEvent
        /// </summary>
        public void Dispose()
        {
            lst.ColumnClick -= eventDoSort;
            lst = null;
        }
    }

    internal class SortingFunctions
    {
        internal static class ListViewExtensions
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct HDITEM
            {
                public Mask mask;
                public int cxy;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string pszText;
                public IntPtr hbm;
                public int cchTextMax;
                public Format fmt;
                public IntPtr lParam;
                // _WIN32_IE >= 0x0300 
                public int iImage;
                public int iOrder;
                // _WIN32_IE >= 0x0500
                public uint type;
                public IntPtr pvFilter;
                // _WIN32_WINNT >= 0x0600
                public uint state;

                [Flags]
                public enum Mask
                {
                    Format = 0x4,       // HDI_FORMAT
                };

                [Flags]
                public enum Format
                {
                    SortDown = 0x200,   // HDF_SORTDOWN
                    SortUp = 0x400,     // HDF_SORTUP
                };
            };

            public const int LVM_FIRST = 0x1000;
            public const int LVM_GETHEADER = LVM_FIRST + 31;

            public const int HDM_FIRST = 0x1200;
            public const int HDM_GETITEM = HDM_FIRST + 11;
            public const int HDM_SETITEM = HDM_FIRST + 12;

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, ref HDITEM lParam);

            public static void SetSortIcon(ListView listViewControl, int columnIndex, SortOrder order)
            {
                IntPtr columnHeader = SendMessage(listViewControl.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
                for (int columnNumber = 0; columnNumber <= listViewControl.Columns.Count - 1; columnNumber++)
                {
                    var columnPtr = new IntPtr(columnNumber);
                    var item = new HDITEM
                    {
                        mask = HDITEM.Mask.Format
                    };

                    if (SendMessage(columnHeader, HDM_GETITEM, columnPtr, ref item) == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }

                    if (order != SortOrder.None && columnNumber == columnIndex)
                    {
                        switch (order)
                        {
                            case SortOrder.Ascending:
                                item.fmt &= ~HDITEM.Format.SortDown;
                                item.fmt |= HDITEM.Format.SortUp;
                                break;
                            case SortOrder.Descending:
                                item.fmt &= ~HDITEM.Format.SortUp;
                                item.fmt |= HDITEM.Format.SortDown;
                                break;
                        }
                    }
                    else
                    {
                        item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
                    }

                    if (SendMessage(columnHeader, HDM_SETITEM, columnPtr, ref item) == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                }
            }
        }
        public class ListViewItemComparer : IComparer
        {
            private int col;
            private SortOrder order;
            public ListViewItemComparer()
            {
                col = 0;
                order = SortOrder.Ascending;
            }
            public ListViewItemComparer(int column, SortOrder order)
            {
                col = column;
                this.order = order;
            }
            public int Compare(object x, object y)
            {
                int returnVal;
                // Determine whether the type being compared is a date type.
                try
                {
                    // Parse the two objects passed as a parameter as a DateTime.
                    System.DateTime firstDate =
                            DateTime.Parse(((ListViewItem)x).SubItems[col].Text);
                    System.DateTime secondDate =
                            DateTime.Parse(((ListViewItem)y).SubItems[col].Text);
                    // Compare the two dates.
                    returnVal = DateTime.Compare(firstDate, secondDate);
                }
                // If neither compared object has a valid date format, compare
                // as a string.
                catch
                {
                    // Compare the two items as a string.
                    returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
                                ((ListViewItem)y).SubItems[col].Text);
                }
                // Determine whether the sort order is descending.
                if (order == SortOrder.Descending)
                    // Invert the value returned by String.Compare.
                    returnVal *= -1;
                return returnVal;
            }
        }
    }
}
