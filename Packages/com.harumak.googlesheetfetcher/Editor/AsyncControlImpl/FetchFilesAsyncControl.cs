using System;
using System.Linq;
using Google.Apis.Drive.v3;
using GoogleSheetFetcher.Editor.AsyncControl;

namespace GoogleSheetFetcher.Editor
{
    public class FetchFilesAsyncControl : AsyncControlBase<FileList>
    {
        private readonly DriveService _driveService;
        private readonly string _folderIdOrName;
        private readonly FileType[] _fileTypes;
        private readonly int _pageSize;
        private readonly string _pageToken;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="driveService">A instance of the <see cref="DriveService"/>.</param>
        /// <param name="folderIdOrName">A folder id or a folder name.</param>
        /// <param name="fileTypes">An array of FileType you needed.</param>
        /// <param name="pageSize">Number of data to be fetched at one time.</param>
        /// <param name="pageToken">A token to fetch the next page.</param>
        public FetchFilesAsyncControl(DriveService driveService, string folderIdOrName, FileType[] fileTypes, int pageSize = 100, string pageToken = null)
        {
            _driveService = driveService;
            _folderIdOrName = folderIdOrName;
            _fileTypes = fileTypes;
            _pageSize = pageSize;
            _pageToken = pageToken;
        }
        
        protected override async void OnStart()
        {
            try
            {
                var listRequest = _driveService.Files.List();
                listRequest.PageSize = _pageSize;
                listRequest.PageToken = _pageToken;
                listRequest.Q = $"'{_folderIdOrName}' in parents and " +
                                "trashed=false and ";
            
                // Add MimeTypes.
                listRequest.Q += "(";
                for (var i = 0; i < _fileTypes.Length; i++)
                {
                    var fileType = _fileTypes[i];
                    if (i >= 1)
                    {
                        listRequest.Q += " or ";
                    }
                    listRequest.Q += $"mimeType='{MimeType.FromFileType(fileType)}'";
                }
                listRequest.Q += ")";
            
                var result = await listRequest.ExecuteAsync();
                var files = result.Files.Select(x => new File
                {
                    Id = x.Id,
                    Name = x.Name,
                    FileType = MimeType.ToFileType(x.MimeType)
                }).ToArray();
                var fileList = new FileList
                {
                    NextPageToken = result.NextPageToken,
                    Files = files
                };
                Succeeded(fileList);
            }
            catch (Exception e)
            {
                Failed(e);
            }
        }
    }
}