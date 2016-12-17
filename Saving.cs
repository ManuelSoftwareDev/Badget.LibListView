using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using static System.Windows.Forms.ListViewItem;

namespace Badget.LibListview.Saving
{
    /// <summary>
    /// Provides a <see cref="CSVItemsFile"/> Class 
    /// </summary>
    public static class CSVItemsFile
    {
        /// <summary>
        /// Load Items from a CSV File (only Item Text)
        /// </summary>
        /// <param name="listview">ListView</param>
        /// <param name="file">File to load Items from</param>
        public static void LoadItemsFromFile(ListView listview, string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException();

            if (listview == null)
                throw new ArgumentNullException("listview");

            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                ListViewItem item = new ListViewItem();
                if (line.Contains("|"))
                {
                    string[] splitted = line.Split('|');
                    item.Text = splitted[0];
                    foreach (string split in splitted)
                        item.SubItems.Add(split);
                }
                else
                    item.Text = line.Replace("%25x1", "|");
                listview.Items.Add(item);
            }
        }

        /// <summary>
        /// Load Items from a CSV File (only Item Text)
        /// </summary>
        /// <param name="file">File which contains items</param>
        /// <returns></returns>
        public static ListViewItem[] LoadItemsFromFile(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException();

            string[] lines = File.ReadAllLines(file);
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (string line in lines)
            {
                ListViewItem item = new ListViewItem();
                if (line.Contains("|"))
                {
                    string[] splitted = line.Split('|');
                    item.Text = splitted[0];
                    foreach (string split in splitted)
                        item.SubItems.Add(split);
                }
                else
                    item.Text = line.Replace("%25x1", "|");
                items.Add(item);
            }

            return items.ToArray();
        }

        /// <summary>
        /// Save Items to a CSV File (only Item Text)
        /// </summary>
        /// <param name="listview">ListView which contains the Items to save</param>
        /// <param name="file">File to save Items in</param>
        public static void SaveItemsToFile(ListView listview, string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            if (listview == null)
                throw new ArgumentNullException(nameof(listview));

            if (GlobalFunctions.ContainsIllegalFilenameChar(file) || GlobalFunctions.ContainsIllegalPathnameChars(file))
                throw new ArgumentException("Illegal characters in path. Parameter \"file\"");

            if (!Directory.Exists(Path.GetPathRoot(file)))
                throw new DriveNotFoundException("Drive \"" + Path.GetPathRoot(file) + "\" not found");

            if (!Directory.Exists(Path.GetDirectoryName(file)))
                Directory.CreateDirectory(file);

            SaveItemsToFile(listview.Items, file);
        }

        /// <summary>
        /// Save Items to a CSV File (only Item Text)
        /// </summary>
        /// <param name="items">Items to save</param>
        /// <param name="file">File to save items in</param>
        public static void SaveItemsToFile(ListView.ListViewItemCollection items, string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (GlobalFunctions.ContainsIllegalFilenameChar(file) || GlobalFunctions.ContainsIllegalPathnameChars(file))
                throw new ArgumentException("Illegal characters in path. Parameter \"file\"");

            if (!Directory.Exists(Path.GetPathRoot(file)))
                throw new DriveNotFoundException("Drive \"" + Path.GetPathRoot(file) + "\" not found");

            if (!Directory.Exists(Path.GetDirectoryName(file)))
                Directory.CreateDirectory(file);

            List<string> fileLines = new List<string>();
            foreach (ListViewItem item in items)
            {
                string finalString = string.Empty;
                foreach (ListViewSubItem subEntry in item.SubItems)
                {
                    if (string.IsNullOrEmpty(finalString))
                        finalString = subEntry.Text.Replace("|", "%25x1");
                    else
                        finalString = "|" + subEntry.Text.Replace("|", "%25x1");
                }
                fileLines.Add(finalString);
            }

            File.WriteAllLines(file, fileLines.ToArray());
        }
    }

    /// <summary>
    /// Provides a <see cref="XMLItemsFile"/> Class
    /// </summary>
    public static class XMLItemsFile
    {

        /// <summary>
        /// Loads items from a file in a specific ListView
        /// </summary>
        /// <param name="listview">specific ListView</param>
        /// <param name="file">file to save</param>
        public static void LoadItemsFromFile(ListView listview, string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException();

            if (listview == null)
                throw new ArgumentNullException("listview");

            XmlSerializer serializer = new XmlSerializer(typeof(ListViewSerializedItem[]));
            StreamReader reader = new StreamReader(file);

            if (serializer.CanDeserialize(XmlReader.Create(reader)))
            {
                ListViewSerializedItem[] items = (ListViewSerializedItem[])serializer.Deserialize(reader);

                reader.Close();
                reader.Dispose();
                foreach (var item in items)
                {
                    ListViewItem i = item.ToListViewItem();
                    listview.Items.Add(i);
                }

                items = null;
            }
            else
                throw new FormatException("File isn't a valid XML Input File");
            try
            {
                reader.Close();
                reader.Dispose();
            }
            finally { }
        }

        /// <summary>
        /// Loads items from a file and returns there
        /// </summary>
        /// <param name="file">file wich contains the XML Data</param> 
        /// <returns></returns>
        public static ListViewItem[] LoadItemsFromFile(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException();

            XmlSerializer serializer = new XmlSerializer(typeof(ListViewSerializedItem[]));
            StreamReader reader = new StreamReader(file);

            List<ListViewItem> Items = new List<ListViewItem>();

            if (serializer.CanDeserialize(XmlReader.Create(reader)))
            {
                ListViewSerializedItem[] items = (ListViewSerializedItem[])serializer.Deserialize(reader);

                foreach (var item in items)
                {
                    ListViewItem i = item.ToListViewItem();
                    Items.Add(i);
                }

                items = null;
            }
            else
                throw new FormatException("File isn't a valid XML Input File");
            reader.Close();
            reader.Dispose();
            return Items.ToArray();
        }

        /// <summary>
        /// Save Items from a ListView to a File
        /// </summary>
        /// <param name="listview">ListView which contains the Items</param>
        /// <param name="file">File to save</param>
        public static void SaveItemsToFile(ListView listview, string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            if (listview == null)
                throw new ArgumentNullException(nameof(listview));

            if (GlobalFunctions.ContainsIllegalFilenameChar(file) || GlobalFunctions.ContainsIllegalPathnameChars(file))
                throw new ArgumentException("Illegal characters in path. Parameter \"file\"");

            if (!Directory.Exists(Path.GetDirectoryName(file)) && file.Contains("\\"))
                Directory.CreateDirectory(file);

            SaveItemsToFile(listview.Items, file);
        }

        /// <summary>
        /// Save Items to a File
        /// </summary>
        /// <param name="items">Items to save</param>
        /// <param name="file">File to save</param>
        public static void SaveItemsToFile(ListView.ListViewItemCollection items, string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (GlobalFunctions.ContainsIllegalFilenameChar(file) || GlobalFunctions.ContainsIllegalPathnameChars(file))
                throw new ArgumentException("Illegal characters in path. Parameter \"file\"");

            if (!Directory.Exists(Path.GetPathRoot(file)) && Path.GetPathRoot(file).Length > 0)
                throw new DriveNotFoundException("Drive \"" + Path.GetPathRoot(file) + "\" not found");

            if (!Directory.Exists(Path.GetDirectoryName(file)) && file.Contains("\\"))
                Directory.CreateDirectory(file);

            List<ListViewSerializedItem> modAll = new List<ListViewSerializedItem>();

            foreach (ListViewItem i in items)
            {
                ListViewSerializedItem item = new ListViewSerializedItem(i);
                foreach (ListViewSubItem itemSub in i.SubItems)
                {
                    
                }
                modAll.Add(item);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(ListViewSerializedItem[]));
            StreamWriter writer;

            if (System.IO.File.Exists(file))
                writer = new StreamWriter(file);
            else
            {
                var str = File.Create(file);
                str.Close();
                writer = new StreamWriter(file);
            }
            serializer.Serialize(writer, modAll.ToArray());

            writer.Close();
            writer.Dispose();
        }
        /// <summary>
        /// Provides a struct for a serializable ListView Item
        /// </summary>
        public class ListViewSerializedItem
        {
            public ListViewItem ToListViewItem()
            {
                ListViewItem i = new ListViewItem();
                i.Text = Text;
                List<ListViewSubItem> subitems = new List<ListViewSubItem>();
                foreach (var item in SubItems)
                {
                    var lstItem = item.ToListViewItem();
                    ListViewSubItem subItem = new ListViewSubItem(i, lstItem.Text, lstItem.ForeColor, lstItem.BackColor, lstItem.Font);
                    subitems.Add(subItem);
                }
                i.SubItems.AddRange(subitems.ToArray());
                i.Name = Name;
                i.BackColor = ColorTranslator.FromHtml(BackColor.ColorHEX);
                i.ForeColor = ColorTranslator.FromHtml(ForeColor.ColorHEX);
                i.Checked = Checked;
                i.Focused = Focused;
                i.Font = new Font(Font.FontName, Font.FontSize, Font.FontStyle);
                i.ImageIndex = ImageIndex;
                i.ImageKey = ImageKey;
                i.IndentCount = IndentCount;
                i.Selected = Selected;
                i.StateImageIndex = StateImageIndex;
                i.UseItemStyleForSubItems = UseItemStyleForSubItems;
                i.ToolTipText = ToolTipText;
                return i;
            }
            public ListViewSerializedItem()
            { }
            public ListViewSerializedItem(ListViewItem item)
            {
                Text = item.Text;

                List<ListViewSerializedItem> sub = new List<ListViewSerializedItem>();
                foreach (ListViewSubItem itemS in item.SubItems)
                    sub.Add(new ListViewSerializedItem(itemS));
                SubItems = sub.ToArray();

                Name = item.Name;
                BackColor = new ColorCC(ColorTranslator.ToHtml(item.BackColor));
                ForeColor = new ColorCC(ColorTranslator.ToHtml(item.ForeColor));
                Checked = item.Checked;
                Focused = item.Focused;
                Font = new FontCC(item.Font.Name, item.Font.Size, item.Font.Style);
                ImageIndex = item.ImageIndex;
                ImageKey = item.ImageKey;
                IndentCount = item.IndentCount;
                Selected = item.Selected;
                StateImageIndex = item.StateImageIndex;
                UseItemStyleForSubItems = item.UseItemStyleForSubItems;
                ToolTipText = item.ToolTipText;
            }

            public ListViewSerializedItem(ListViewSubItem item)
            {
                Text = item.Text;

                SubItems = null;

                Name = item.Name;
                BackColor = new ColorCC(ColorTranslator.ToHtml(item.BackColor));
                ForeColor = new ColorCC(ColorTranslator.ToHtml(item.ForeColor));
                Font = new FontCC(item.Font.Name, item.Font.Size, item.Font.Style);
                Focused = false;
                Checked = false;
                ImageIndex = 0;
                ImageKey = string.Empty;
                IndentCount = 0;
                Selected = false;
                StateImageIndex = 0;
                UseItemStyleForSubItems = true;
                ToolTipText = string.Empty;
            }

            public string Text;
            public ListViewSerializedItem[] SubItems;
            public string Name;
            public ColorCC BackColor;
            public ColorCC ForeColor;
            public bool Checked;
            public bool Focused;
            public FontCC Font;
            public int ImageIndex;
            public string ImageKey;
            public int IndentCount;
            public bool Selected;
            public int StateImageIndex;
            public bool UseItemStyleForSubItems;
            public string ToolTipText;
        }
        public class ColorCC
        {
            public ColorCC()
            { }
            public ColorCC(string hex)
            {
                ColorHEX = hex;
            }
            public string ColorHEX
            {
                get;
                set;
            }
        }
        public class FontCC
        {
            public FontCC()
            { }
            public FontCC(string fontname, float fontsize, FontStyle style)
            {
                FontName = fontname;
                FontSize = fontsize;
                FontStyle = style;
            }
            public string FontName;
            public float FontSize;
            public FontStyle FontStyle;
        }
    }

    internal static class GlobalFunctions
    {
        public static bool ContainsIllegalFilenameChar(string name)
        {
            name = Path.GetFileName(name);
            char[] chars = Path.GetInvalidFileNameChars();
            foreach (char c in chars)
                if (name.Contains(c))
                    return true;
            return false;
        }
        public static bool ContainsIllegalPathnameChars(string name)
        {
            name = Path.GetDirectoryName(name);
            char[] chars = Path.GetInvalidPathChars();
            foreach (char c in chars)
                if (name.Contains(c))
                    return true;
            return false;
        }
    }
}
