using System.Net;
using System.Windows.Forms;

namespace Badget.LibListview.Web
{
    /// <summary>
    /// Provides a class which supports Web-Download and Parsing
    /// </summary>
    public static class WebSupport
    {
        /// <summary>
        /// Downloads XML Data from a URL and try to parse it in a supported XML Format
        /// </summary>
        /// <param name="url">URL to Data</param>
        /// <returns></returns>
        public static ListViewItem[] LoadItemsFromWebXML(string url)
        {
            using (WebClient client = new WebClient())
            {
                string tempfile = System.IO.Path.GetTempFileName();
                client.DownloadFile(url, tempfile);
                var res = Saving.XMLItemsFile.LoadItemsFromFile(tempfile);
                try
                {
                    System.IO.File.Delete(tempfile);
                }catch { }
                return res;
            }
        }

        /// <summary>
        /// Download CSV Data from a URL and try to parse it to a valid CSV ItemTable
        /// </summary>
        /// <param name="url">URL to Data</param>
        /// <returns></returns>
        public static ListViewItem[] LoadItemsFromWebCSV(string url)
        {
            using (WebClient client = new WebClient())
            {
                string tempfile = System.IO.Path.GetTempFileName();
                client.DownloadFile(url, tempfile);
                var res = Saving.CSVItemsFile.LoadItemsFromFile(tempfile);
                try
                {
                    System.IO.File.Delete(tempfile);
                }catch { }
                return res;
            }
        }
    }
}