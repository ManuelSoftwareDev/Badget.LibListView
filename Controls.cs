using Microsoft.Win32;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Badget.LibListview.Controls
{
    /// <summary>
    /// An native ListView code by Stefan Baumann (copied from AeroSuite)
    /// </summary>
    /// <remarks>
    /// A ListView with the "Explorer"-WindowTheme applied.
    /// If the operating system is Windows XP or older, nothing will be changed.
    /// </remarks>
    [DesignerCategory("Code")]
    [DisplayName("Native ListView")]
    [Description("An native ListView.")]
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(ListView))]
    public class NativeListView
        : ListView
    {
        private const int LVS_EX_DOUBLEBUFFER = 0x10000;
        private const int LVM_SETEXTENDEDLISTVIEWSTYLE = 4150;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeListView"/> class.
        /// </summary>
        public NativeListView()
            : base()
        {
           this.FullRowSelect = true;
        }
        
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr window, int message, int wParam, IntPtr lParam);

        /// <summary>
        /// Sets the GroupState
        /// </summary>
        /// <param name="state"></param>
        public void SetGroupCollapse(GroupState state)
        {

            for (int i = 0; i <= this.Groups.Count; i++)
            {

                LVGROUP group = new LVGROUP();
                group.cbSize = Marshal.SizeOf(group);
                group.state = (int)state; // LVGS_COLLAPSIBLE 
                group.mask = 4; // LVGF_STATE 
                group.iGroupId = i;

                IntPtr ip = IntPtr.Zero;
                try
                {
                    ip = Marshal.AllocHGlobal(group.cbSize);
                    Marshal.StructureToPtr(group, ip, true);
                    SendMessage(this.Handle, 0x1000 + 147, i, ip); // #define LVM_SETGROUPINFO (LVM_FIRST + 147) 
                }

                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                }
                finally
                {
                    if (null != ip) Marshal.FreeHGlobal(ip);
                }
            }

        }


        /// <summary>
        /// Raises the <see cref="E:HandleCreated" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (PlatformHelper.VistaOrHigher)
            {
                NativeMethods.SetWindowTheme(this.Handle, "explorer", null);
                NativeMethods.SendMessage(this.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, new IntPtr(LVS_EX_DOUBLEBUFFER), new IntPtr(LVS_EX_DOUBLEBUFFER));
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct LVGROUP
        {

            public int cbSize;

            public int mask;

            [MarshalAs(UnmanagedType.LPTStr)]

            public string pszHeader;

            public int cchHeader;

            [MarshalAs(UnmanagedType.LPTStr)]

            public string pszFooter;

            public int cchFooter;

            public int iGroupId;

            public int stateMask;

            public int state;

            public int uAlign;

        }

        /// <summary>
        /// Provides States for the GroupExpander
        /// </summary>
        public enum GroupState
        {
            /// <summary>
            /// The Group is collapsible
            /// </summary>
            COLLAPSIBLE = 8,
            /// <summary>
            /// The Group is collapsed
            /// </summary>
            COLLAPSED = 1,
            /// <summary>
            /// The Group is expanded
            /// </summary>
            EXPANDED = 0
        }

    }
    internal static class NativeMethods
    {
        private const string user32 = "user32.dll";
        private const string uxtheme = "uxtheme.dll";

        [DllImport(user32, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport(user32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, string lParam);

        [DllImport(user32, SetLastError = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport(user32, SetLastError = true)]
        public static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport(user32, SetLastError = true)]
        public static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);


        [DllImport(uxtheme, CharSet = CharSet.Unicode, SetLastError = true)]
        public extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
    }
    /// <summary>
    /// Provides helper functions for platform management
    /// </summary>
    internal static class PlatformHelper
    {
        /// <summary>
        /// Initializes the <see cref="PlatformHelper"/> class.
        /// </summary>
        static PlatformHelper()
        {
            PlatformHelper.Win32NT = Environment.OSVersion.Platform == PlatformID.Win32NT;
            PlatformHelper.XpOrHigher = PlatformHelper.Win32NT && Environment.OSVersion.Version.Major >= 5;
            PlatformHelper.VistaOrHigher = PlatformHelper.Win32NT && Environment.OSVersion.Version.Major >= 6;
            PlatformHelper.SevenOrHigher = PlatformHelper.Win32NT && (Environment.OSVersion.Version >= new Version(6, 1));
            PlatformHelper.EightOrHigher = PlatformHelper.Win32NT && (Environment.OSVersion.Version >= new Version(6, 2, 9200));
            PlatformHelper.VisualStylesEnabled = VisualStyleInformation.IsEnabledByUser;

            SystemEvents.UserPreferenceChanged += (s, e) =>
            {
                var vsEnabled = VisualStyleInformation.IsEnabledByUser;
                if (vsEnabled != PlatformHelper.VisualStylesEnabled)
                {
                    PlatformHelper.VisualStylesEnabled = vsEnabled;
                    //Maybe raise an event here
                }
            };
        }

        /// <summary>
        /// Returns a indicating whether the Operating System is Windows 32 NT based.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Operating System is Windows 32 NT based; otherwise, <c>false</c>.
        /// </value>
        public static bool Win32NT { get; private set; }

        /// <summary>
        /// Returns a value indicating whether the Operating System is Windows XP or higher.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Operating System is Windows 8 or higher; otherwise, <c>false</c>.
        /// </value>
        public static bool XpOrHigher { get; private set; }

        /// <summary>
        /// Returns a value indicating whether the Operating System is Windows Vista or higher.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Operating System is Windows Vista or higher; otherwise, <c>false</c>.
        /// </value>
        public static bool VistaOrHigher { get; private set; }

        /// <summary>
        /// Returns a value indicating whether the Operating System is Windows 7 or higher.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Operating System is Windows 7 or higher; otherwise, <c>false</c>.
        /// </value>
        public static bool SevenOrHigher { get; private set; }

        /// <summary>
        /// Returns a value indicating whether the Operating System is Windows 8 or higher.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Operating System is Windows 8 or higher; otherwise, <c>false</c>.
        /// </value>
        public static bool EightOrHigher { get; private set; }

        /// <summary>
        /// Returns a value indicating whether Visual Styles are enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if Visual Styles are enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool VisualStylesEnabled { get; private set; }
    }

    /// <summary>
	/// Provides a specialed ListView what embeds Controls
	/// </summary>
	public class ListViewEmbedControls : ListView
    {
        #region Interop-Defines
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wPar, IntPtr lPar);

        // ListView messages
        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETCOLUMNORDERARRAY = (LVM_FIRST + 59);

        // Windows Messages
        private const int WM_PAINT = 0x000F;
        #endregion

        /// <summary>
        /// Structure to hold an embedded control's info
        /// </summary>
        private struct EmbeddedControl
        {
            public Control Control;
            public int Column;
            public int Row;
            public DockStyle Dock;
            public ListViewItem Item;
        }

        private ArrayList _embeddedControls = new ArrayList();

        /// <summary>
        /// Constructs a new instance of <see cref="ListViewEmbedControls"/> 
        /// </summary>
        public ListViewEmbedControls() { }

        /// <summary>
        /// Retrieve the order in which columns appear
        /// </summary>
        /// <returns>Current display order of column indices</returns>
        protected int[] GetColumnOrder()
        {
            IntPtr lPar = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * Columns.Count);

            IntPtr res = SendMessage(Handle, LVM_GETCOLUMNORDERARRAY, new IntPtr(Columns.Count), lPar);
            if (res.ToInt32() == 0) // Something went wrong
            {
                Marshal.FreeHGlobal(lPar);
                return null;
            }

            int[] order = new int[Columns.Count];
            Marshal.Copy(lPar, order, 0, Columns.Count);

            Marshal.FreeHGlobal(lPar);

            return order;
        }

        /// <summary>
        /// Retrieve the bounds of a ListViewSubItem
        /// </summary>
        /// <param name="Item">The Item containing the SubItem</param>
        /// <param name="SubItem">Index of the SubItem</param>
        /// <returns>Subitem's bounds</returns>
        protected Rectangle GetSubItemBounds(ListViewItem Item, int SubItem)
        {
            Rectangle subItemRect = Rectangle.Empty;

            if (Item == null)
                throw new ArgumentNullException("Item");

            int[] order = GetColumnOrder();
            if (order == null) // No Columns
                return subItemRect;

            if (SubItem >= order.Length)
                throw new IndexOutOfRangeException("SubItem " + SubItem + " out of range");

            // Retrieve the bounds of the entire ListViewItem (all subitems)
            Rectangle lviBounds = Item.GetBounds(ItemBoundsPortion.Entire);
            int subItemX = lviBounds.Left;

            // Calculate the X position of the SubItem.
            // Because the columns can be reordered we have to use Columns[order[i]] instead of Columns[i] !
            ColumnHeader col;
            int i;
            for (i = 0; i < order.Length; i++)
            {
                col = this.Columns[order[i]];
                if (col.Index == SubItem)
                    break;
                subItemX += col.Width;
            }

            subItemRect = new Rectangle(subItemX, lviBounds.Top, this.Columns[order[i]].Width, lviBounds.Height);

            return subItemRect;
        }

        /// <summary>
        /// Add a control to the ListView
        /// </summary>
        /// <param name="c">Control to be added</param>
        /// <param name="col">Index of column</param>
        /// <param name="row">Index of row</param>
        public void AddEmbeddedControl(Control c, int col, int row)
        {
            AddEmbeddedControl(c, col, row, DockStyle.Fill);
        }
        /// <summary>
        /// Add a control to the ListView
        /// </summary>
        /// <param name="c">Control to be added</param>
        /// <param name="col">Index of column</param>
        /// <param name="row">Index of row</param>
        /// <param name="dock">Location and resize behavior of embedded control</param>
        public void AddEmbeddedControl(Control c, int col, int row, DockStyle dock)
        {
            if (c == null)
                throw new ArgumentNullException();
            if (col >= Columns.Count || row >= Items.Count)
                throw new ArgumentOutOfRangeException();

            EmbeddedControl ec;
            ec.Control = c;
            ec.Column = col;
            ec.Row = row;
            ec.Dock = dock;
            ec.Item = Items[row];

            _embeddedControls.Add(ec);

            // Add a Click event handler to select the ListView row when an embedded control is clicked
            c.Click += new EventHandler(_embeddedControl_Click);

            this.Controls.Add(c);
        }

        /// <summary>
        /// Remove a control from the ListView
        /// </summary>
        /// <param name="c">Control to be removed</param>
        public void RemoveEmbeddedControl(Control c)
        {
            if (c == null)
                throw new ArgumentNullException();

            for (int i = 0; i < _embeddedControls.Count; i++)
            {
                EmbeddedControl ec = (EmbeddedControl)_embeddedControls[i];
                if (ec.Control == c)
                {
                    c.Click -= new EventHandler(_embeddedControl_Click);
                    this.Controls.Remove(c);
                    _embeddedControls.RemoveAt(i);
                    return;
                }
            }
            throw new Exception("Control not found!");
        }

        /// <summary>
        /// Retrieve the control embedded at a given location
        /// </summary>
        /// <param name="col">Index of Column</param>
        /// <param name="row">Index of Row</param>
        /// <returns>Control found at given location or null if none assigned.</returns>
        public Control GetEmbeddedControl(int col, int row)
        {
            foreach (EmbeddedControl ec in _embeddedControls)
                if (ec.Row == row && ec.Column == col)
                    return ec.Control;

            return null;
        }
        
        /// <summary>
        /// Gets or sets the View
        /// </summary>
        [DefaultValue(View.LargeIcon)]
        public new View View
        {
            get
            {
                return base.View;
            }
            set
            {
                // Embedded controls are rendered only when we're in Details mode
                foreach (EmbeddedControl ec in _embeddedControls)
                    ec.Control.Visible = (value == View.Details);

                base.View = value;
            }
        }

        /// <summary>
        /// <see cref="WndProc(ref Message)"/> 
        /// </summary>
        /// <param name="m">Message</param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_PAINT:
                    if (View != View.Details)
                        break;

                    // Calculate the position of all embedded controls
                    foreach (EmbeddedControl ec in _embeddedControls)
                    {
                        Rectangle rc = this.GetSubItemBounds(ec.Item, ec.Column);

                        if ((this.HeaderStyle != ColumnHeaderStyle.None) && 
                            (rc.Top < this.Font.Height)) // Control overlaps ColumnHeader
                        {
                            ec.Control.Visible = false;
                            continue;
                        }
                        else
                        {
                            ec.Control.Visible = true;
                        }

                        switch (ec.Dock)
                        {
                            case DockStyle.Fill:
                                break;
                            case DockStyle.Top:
                                rc.Height = ec.Control.Height;
                                break;
                            case DockStyle.Left:
                                rc.Width = ec.Control.Width;
                                break;
                            case DockStyle.Bottom:
                                rc.Offset(0, rc.Height - ec.Control.Height);
                                rc.Height = ec.Control.Height;
                                break;
                            case DockStyle.Right:
                                rc.Offset(rc.Width - ec.Control.Width, 0);
                                rc.Width = ec.Control.Width;
                                break;
                            case DockStyle.None:
                                rc.Size = ec.Control.Size;
                                break;
                        }

                        // Set embedded control's bounds
                        ec.Control.Bounds = rc;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private void _embeddedControl_Click(object sender, EventArgs e)
        {
            // When a control is clicked the ListViewItem holding it is selected
            foreach (EmbeddedControl ec in _embeddedControls)
            {
                if (ec.Control == (Control)sender)
                {
                    this.SelectedItems.Clear();
                    ec.Item.Selected = true;
                }
            }
        }
    }
}