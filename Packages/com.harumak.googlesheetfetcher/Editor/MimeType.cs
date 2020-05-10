using System;

namespace GoogleSheetFetcher.Editor
{
    /// <summary>
    /// The MimeTypes.
    /// </summary>
    internal static class MimeType
    {
        /// <summary>
        /// MimeType of the folder.
        /// </summary>
        private const string Folder = "application/vnd.google-apps.folder";
        /// <summary>
        /// MimeType of the spreadsheet.
        /// </summary>
        private const string Spreadsheet = "application/vnd.google-apps.spreadsheet";

        /// <summary>
        /// Convert <see cref="FileType"/> to MimeType
        /// </summary>
        /// <param name="fileType"><see cref="FileType"/>.</param>
        /// <returns>MimeType string.</returns>
        internal static string FromFileType(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Folder:
                    return Folder;
                case FileType.Spreadsheet:
                    return Spreadsheet;
                default:
                    throw new InvalidOperationException($"Invalid FileType : {fileType}.");
            }
        }

        /// <summary>
        /// Convert MimeType to <see cref="FileType"/>
        /// </summary>
        /// <param name="mimeType">MimeType string.</param>
        /// <returns><see cref="FileType"/>.</returns>
        internal static FileType ToFileType(string mimeType)
        {
            switch (mimeType)
            {
                case Folder:
                    return FileType.Folder;
                case Spreadsheet:
                    return FileType.Spreadsheet;
                default:
                    throw new InvalidOperationException($"Invalid MimeType : {mimeType}.");
            }
        }
    }
}