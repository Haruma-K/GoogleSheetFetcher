namespace GoogleSheetFetcher.Editor
{
    /// <summary>
    /// A list of metadata of the files on Google Drive.
    /// </summary>
    public class FileList
    {
        /// <summary>
        /// A token to fetch the next page. 
        /// </summary>
        public string NextPageToken { get; internal set; }
        /// <summary>
        /// Metadata of the files.
        /// </summary>
        public File[] Files { get; internal set; }
    }
}