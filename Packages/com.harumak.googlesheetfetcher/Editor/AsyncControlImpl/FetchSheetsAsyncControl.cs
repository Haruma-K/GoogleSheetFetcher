using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4;
using GoogleSheetFetcher.Editor.AsyncControls;

namespace GoogleSheetFetcher.Editor
{
    public class FetchSheetsAsyncControl : AsyncControlBase<IList<Sheet>>
    {
        private readonly SheetsService _sheetsService;
        private readonly string _spreadsheetId;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sheetsService">A instance of the <see cref="SheetsService"/>.</param>
        /// <param name="spreadsheetId">The name of the Spreadsheet.</param>
        public FetchSheetsAsyncControl(SheetsService sheetsService, string spreadsheetId)
        {
            _sheetsService = sheetsService;
            _spreadsheetId = spreadsheetId;
        }
        
        protected override async void OnStart()
        {
            try
            {
                var result = await _sheetsService.Spreadsheets
                    .Get(_spreadsheetId)
                    .ExecuteAsync();
            
                var sheets = result.Sheets
                    .Select(x => new Sheet
                    {
                        Id = x.Properties.SheetId.HasValue ? x.Properties.SheetId.ToString() : null,
                        Name = x.Properties.Title
                    })
                    .ToList();
                
                Succeeded(sheets);
            }
            catch (Exception e)
            {
                Failed(e);
            }
        }
    }
}