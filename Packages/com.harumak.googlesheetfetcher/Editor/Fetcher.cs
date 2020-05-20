using System;
using System.Collections.Generic;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using GoogleSheetFetcher.Editor.AsyncControl;

namespace GoogleSheetFetcher.Editor
{
    public class Fetcher
    {
        public bool DidInitialize { get; private set; }
        
        private DriveService _driveService;
        private SheetsService _sheetService;
        
        /// <summary>
        /// <para>Initialize the <see cref="Fetcher"/> with the following scopes.</para>
        /// <para>* https://www.googleapis.com/auth/drive.readonly</para>
        /// <para>* https://www.googleapis.com/auth/spreadsheets.readonly</para>
        /// </summary>
        /// <param name="clientId">Google OAuth2 Client ID.</param>
        /// <param name="clientSecret">Google OAuth2 Client Secret.</param>
        /// <param name="applicationId">The identifier to use the for file to store the credentials.</param>
        public AsyncControlHandle InitializeAsync(string clientId, string clientSecret, string applicationId)
        {
            var scopes = new List<string>
            {
                SheetsService.Scope.DriveReadonly,
                SheetsService.Scope.SpreadsheetsReadonly,
            };
            return InitializeAsync(clientId, clientSecret, applicationId, scopes);
        }
        
        /// <summary>
        /// Initialize the <see cref="Fetcher"/>.
        /// </summary>
        /// <param name="clientId">Google OAuth2 Client ID.</param>
        /// <param name="clientSecret">Google OAuth2 Client Secret.</param>
        /// <param name="applicationId">The identifier to use the for file to store the credentials.</param>
        /// <param name="scopes">The scopes you want to use.</param>
        public AsyncControlHandle InitializeAsync(string clientId, string clientSecret, string applicationId, List<string> scopes)
        {
            var control = new AuthorizeAsyncControl(clientId, clientSecret, applicationId, scopes);
            control.Completed += () =>
            {

                var initializer = new BaseClientService.Initializer
                {
                    HttpClientInitializer = control.Result
                };
                _driveService = new DriveService(initializer);
                _sheetService = new SheetsService(initializer);
                DidInitialize = true;
            };
            control.Start();
            return new AsyncControlHandle(control);
        }

        /// <summary>
        /// Fetch the metadata of the files in the specified folder.
        /// </summary>
        /// <param name="folderIdOrName">A folder id or a folder name.</param>
        /// <param name="fileTypes">An array of FileType you needed.</param>
        /// <param name="pageSize">Number of data to be fetched at one time.</param>
        /// <param name="pageToken">A token to fetch the next page.</param>
        public AsyncControlHandle<FileList> FetchFilesAsync(string folderIdOrName, FileType[] fileTypes, int pageSize = 100, string pageToken = null)
        {
            if (!DidInitialize)
            {
                throw new InvalidOperationException("The Fetcher is not be initialized.");
            }

            var control = new FetchFilesAsyncControl(_driveService, folderIdOrName, fileTypes, pageSize, pageToken);
            control.Start();
            return new AsyncControlHandle<FileList>(control);
        }
        
        /// <summary>
        /// Fetch the information of all the sheets contained in the spread sheet.
        /// </summary>
        /// <param name="spreadsheetId"><para>The spreadsheet id.</para></param>
        /// <returns>The list of the information of all the sheets.</returns>
        public AsyncControlHandle<IList<Sheet>> FetchSheetsAsync(string spreadsheetId)
        {
            if (!DidInitialize)
            {
                throw new InvalidOperationException("The Fetcher is not be initialized.");
            }

            var control = new FetchSheetsAsyncControl(_sheetService, spreadsheetId);
            control.Start();
            return new AsyncControlHandle<IList<Sheet>>(control);
        }
        
        /// <summary>
        /// Fetch all the values contained in the sheet.
        /// </summary>
        /// <param name="spreadsheetId">The spreadsheet id.</param>
        /// <param name="sheetName">The sheet name.</param>
        /// <returns>The list of the rows.</returns>
        public AsyncControlHandle<IList<IList<object>>> FetchValuesAsync(string spreadsheetId, string sheetName = null)
        {
            if (!DidInitialize)
            {
                throw new InvalidOperationException("The Fetcher is not be initialized.");
            }

            var control = new FetchSheetValuesAsyncControl(_sheetService, spreadsheetId);
            control.Start();
            return new AsyncControlHandle<IList<IList<object>>>(control);
        }

        /// <summary>
        /// Fetch all the values contained in the sheet.
        /// </summary>
        /// <param name="spreadsheetId">The spreadsheet id.</param>
        /// <param name="sheet">The <see cref="Sheet"/> instance obtained as the result of <see cref="FetchSheetsAsync"/>.</param>
        /// <returns>The list of the rows.</returns>
        public AsyncControlHandle<IList<IList<object>>> FetchValuesAsync(string spreadsheetId, Sheet sheet = null)
        {
            return FetchValuesAsync(spreadsheetId, sheet?.Name);
        }
    }
}