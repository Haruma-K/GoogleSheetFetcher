using System;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using GoogleSheetFetcher.Editor.AsyncControl;

namespace GoogleSheetFetcher.Editor
{
    public class AuthorizeAsyncControl : AsyncControlBase<UserCredential>
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _applicationId;
        private readonly List<string> _scopes;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sheetsService">A instance of the <see cref="SheetsService"/>.</param>
        /// <param name="spreadsheetId">The name of the Spreadsheet.</param>
        /// <param name="sheetName">The sheet name.</param>
        public AuthorizeAsyncControl(string clientId, string clientSecret, string applicationId, List<string> scopes)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _applicationId = applicationId;
            _scopes = scopes;
        }
        
        protected override async void OnStart()
        {
            try
            {
                // Add the required scopes if it is not included.
                if (!_scopes.Contains(SheetsService.Scope.Drive) && !_scopes.Contains(SheetsService.Scope.DriveReadonly))
                {
                    _scopes.Add(SheetsService.Scope.DriveReadonly);
                }
                if (!_scopes.Contains(SheetsService.Scope.Spreadsheets) && !_scopes.Contains(SheetsService.Scope.SpreadsheetsReadonly))
                {
                    _scopes.Add(SheetsService.Scope.SpreadsheetsReadonly);
                }
                CancellationToken.ThrowIfCancellationRequested();
                
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = _clientId,
                        ClientSecret = _clientSecret
                    },
                    _scopes,
                    _applicationId,
                    CancellationToken);
                
                Succeeded(credential);
            }
            catch (Exception e)
            {
                Failed(e);
            }
        }
    }
}