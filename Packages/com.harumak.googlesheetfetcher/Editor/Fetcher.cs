using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using NUnit.Framework;

namespace GoogleSheetFetcher.Editor
{
    public class Fetcher
    {
        public bool DidInitialize { get; private set; }
        
        private SheetsService _service;
        
        /// <summary>
        /// Initialize the <see cref="Fetcher"/>.
        /// </summary>
        /// <param name="clientId">Google OAuth2 Client ID.</param>
        /// <param name="clientSecret">Google OAuth2 Client Secret.</param>
        /// <param name="applicationId">The identifier to use the for file to store the credentials.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the initialization process.</param>
        public async Task InitializeAsync(string clientId, string clientSecret, string applicationId, CancellationToken? cancellationToken = null)
        {
            Assert.That(clientId, Is.Not.Null.Or.Empty);
            Assert.That(clientSecret, Is.Not.Null.Or.Empty);
            Assert.That(applicationId, Is.Not.Null.Or.Empty);

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }, 
                new[]
                {
                    SheetsService.Scope.SpreadsheetsReadonly
                },
                applicationId,
                cancellationToken ?? CancellationToken.None);
            
            _service = new SheetsService(new BaseClientService.Initializer{
                HttpClientInitializer = credential
            });

            DidInitialize = true;
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
            
            var result = await _service.Spreadsheets
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
            
            var result = await _service.Spreadsheets
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