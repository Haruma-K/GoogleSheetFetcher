using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using NUnit.Framework;

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
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the initialization process.</param>
        public async Task InitializeAsync(string clientId, string clientSecret, string applicationId, CancellationToken? cancellationToken = null)
        {
            var scopes = new List<string>
            {
                SheetsService.Scope.DriveReadonly,
                SheetsService.Scope.SpreadsheetsReadonly,
            };
            await InitializeAsync(clientId, clientSecret, applicationId, scopes, cancellationToken);
        }
        
        /// <summary>
        /// Initialize the <see cref="Fetcher"/>.
        /// </summary>
        /// <param name="clientId">Google OAuth2 Client ID.</param>
        /// <param name="clientSecret">Google OAuth2 Client Secret.</param>
        /// <param name="applicationId">The identifier to use the for file to store the credentials.</param>
        /// <param name="scopes">The scopes you want to use.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the initialization process.</param>
        public async Task InitializeAsync(string clientId, string clientSecret, string applicationId, List<string> scopes, CancellationToken? cancellationToken = null)
        {
            Assert.That(!DidInitialize);
            Assert.That(clientId, Is.Not.Null.Or.Empty);
            Assert.That(clientSecret, Is.Not.Null.Or.Empty);
            Assert.That(applicationId, Is.Not.Null.Or.Empty);
            Assert.That(scopes, Is.Not.Null.Or.Empty);

            // Add the required scopes if it is not included.
            if (!scopes.Contains(SheetsService.Scope.Drive) && !scopes.Contains(SheetsService.Scope.DriveReadonly))
            {
                scopes.Add(SheetsService.Scope.DriveReadonly);
            }
            if (!scopes.Contains(SheetsService.Scope.Spreadsheets) && !scopes.Contains(SheetsService.Scope.SpreadsheetsReadonly))
            {
                scopes.Add(SheetsService.Scope.SpreadsheetsReadonly);
            }

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                scopes,
                applicationId,
                cancellationToken ?? CancellationToken.None);

            var initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            };
            _driveService = new DriveService(initializer);
            _sheetService = new SheetsService(initializer);
            
            DidInitialize = true;
        }

        /// <summary>
        /// Fetch the metadata of the files in the specified folder.
        /// </summary>
        /// <param name="folderIdOrName">A folder id or a folder name.</param>
        /// <param name="fileTypes">An array of FileType you needed.</param>
        /// <param name="pageSize">Number of data to be fetched at one time.</param>
        /// <param name="pageToken">A token to fetch the next page.</param>
        public async Task<FileList> FetchFilesAsync(string folderIdOrName, FileType[] fileTypes, int pageSize = 100, string pageToken = null)
        {
            Assert.That(folderIdOrName, Is.Not.Null.Or.Empty);
            Assert.That(fileTypes, Is.Not.Null.Or.Empty);
            
            var listRequest = _driveService.Files.List();
            listRequest.PageSize = pageSize;
            listRequest.PageToken = pageToken;
            listRequest.Q = $"'{folderIdOrName}' in parents and " +
                            "trashed=false and ";
            
            // Add MimeTypes.
            listRequest.Q += "(";
            for (var i = 0; i < fileTypes.Length; i++)
            {
                var fileType = fileTypes[i];
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
            return new FileList
            {
                NextPageToken = result.NextPageToken,
                Files = files
            };
        }
        
        /// <summary>
        /// Fetch the information of all the sheets contained in the spread sheet.
        /// </summary>
        /// <param name="spreadsheetId"><para>The spreadsheet id.</para></param>
        /// <returns>The list of the information of all the sheets.</returns>
        public async Task<IList<Sheet>> FetchSheetsAsync(string spreadsheetId)
        {
            Assert.That(DidInitialize);
            Assert.That(spreadsheetId, Is.Not.Null.Or.Empty);
            
            var result = await _sheetService.Spreadsheets
                .Get(spreadsheetId)
                .ExecuteAsync();
            
            return result.Sheets
                .Select(x => new Sheet
                {
                    Id = x.Properties.SheetId.HasValue ? x.Properties.SheetId.ToString() : null,
                    Name = x.Properties.Title
                })
                .ToList();
        }
        
        /// <summary>
        /// Fetch all the values contained in the sheet.
        /// </summary>
        /// <param name="spreadsheetId">The spreadsheet id.</param>
        /// <param name="sheetName">The sheet name.</param>
        /// <returns>The list of the rows.</returns>
        public async Task<IList<IList<object>>> FetchValuesAsync(string spreadsheetId, string sheetName = null)
        {
            Assert.That(DidInitialize);
            Assert.That(spreadsheetId, Is.Not.Null.Or.Empty);
            
            var result = await _sheetService.Spreadsheets
                .Values
                .Get(spreadsheetId, sheetName)
                .ExecuteAsync();
            return result.Values;
        }

        /// <summary>
        /// Fetch all the values contained in the sheet.
        /// </summary>
        /// <param name="spreadsheetId">The spreadsheet id.</param>
        /// <param name="sheet">The <see cref="Sheet"/> instance obtained as the result of <see cref="FetchSheetsAsync"/>.</param>
        /// <returns>The list of the rows.</returns>
        public Task<IList<IList<object>>> FetchValuesAsync(string spreadsheetId, Sheet sheet = null)
        {
            return FetchValuesAsync(spreadsheetId, sheet?.Name);
        }
    }
}