using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4;
using GoogleSheetFetcher.Editor.AsyncControl;

namespace GoogleSheetFetcher.Editor
{
    public class FetchSheetValuesAsyncControl : AsyncControlBase<IList<IList<object>>>
    {
        private readonly SheetsService _sheetsService;
        private readonly string _spreadsheetId;
        private readonly string _sheetName;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sheetsService">A instance of the <see cref="SheetsService"/>.</param>
        /// <param name="spreadsheetId">The name of the Spreadsheet.</param>
        /// <param name="sheetName">The sheet name.</param>
        public FetchSheetValuesAsyncControl(SheetsService sheetsService, string spreadsheetId, string sheetName = null)
        {
            _sheetsService = sheetsService;
            _spreadsheetId = spreadsheetId;
            _sheetName = sheetName;
        }
        
        protected override async void OnStart()
        {
            try
            {
                var result = await _sheetsService.Spreadsheets
                    .Values
                    .Get(_spreadsheetId, _sheetName)
                    .ExecuteAsync();
                
                Succeeded(result.Values);
            }
            catch (Exception e)
            {
                Failed(e);
            }
        }
    }
}