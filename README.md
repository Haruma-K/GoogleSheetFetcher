# GoogleSheetFetcher

## Overview
The Simple Google Sheets reader for Unity editor.
You can do OAuth2 authorization and fetch the spreadsheet values by writing the following.

```cs
// Google OAuth2 authorization.
var fetcher = new Fetcher();
await fetcher.InitializeAsync(_clientId, _clientSecret, _applicationId);

// Get all the values in the sheet.
var values = await fetcher.FetchValuesAsync(_spreadsheetId, sheets[0]);
```

## Usage
Please refer to [the manual](https://haruma-k.github.io/GoogleSheetFetcher/manual/index.html) for usage.

## License
This library is under [the MIT License](https://opensource.org/licenses/mit-license.php).

This software includes the work that is distributed in [the Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0).
