using System;
using System.Linq;
using System.Threading.Tasks;
using GoogleSheetFetcher.Editor;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "GoogleSheetFetcher/Demo")]
public class Demo : ScriptableObject
{
    [SerializeField] private string _clientId;
    [SerializeField] private string _clientSecret;
    [SerializeField] private string _applicationId = "google_sheet_fetcher_demo";
    [SerializeField] private string _spreadsheetId;

    private static Fetcher _fetcher;
    public bool DidFetcherInitialize => _fetcher?.DidInitialize ?? false;
    
    /// <summary>
    /// Initialize the spread sheet fetcher.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _fetcher = new Fetcher();
            await _fetcher.InitializeAsync(_clientId, _clientSecret, _applicationId);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// Log the names of all the sheets.
    /// </summary>
    public async Task LogAllSheetNamesAsync()
    {
        try
        {
            var sheets = await _fetcher.FetchSheetsAsync(_spreadsheetId);
            foreach (var sheet in sheets)
            {
                Debug.Log(sheet.Name);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    /// <summary>
    /// Log all the values of the first sheets.
    /// </summary>
    public async Task LogAllValuesOfFirstSheetAsync()
    {
        try
        {
            var sheets = await _fetcher.FetchSheetsAsync(_spreadsheetId);
            var values = await _fetcher.FetchValuesAsync(_spreadsheetId, sheets[0]);
            foreach (var row in values)
            {
                Debug.Log(row.Count >= 1 ? row.Aggregate((a, b) => $"{a},{b}") : "");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// Log the names of all the files in the root of drive.
    /// </summary>
    public async Task LogAllRootFileNamesAsync()
    {
        try
        {
            var result = await _fetcher.FetchFilesAsync("root", new []{ FileType.Folder, FileType.Spreadsheet });
            
            foreach (var file in result.Files)
            {
                Debug.Log($"{file.Name} ({file.FileType})");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }
}

[CustomEditor(typeof(Demo))]
public class DemoInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        var component = (Demo) target;
        var clientIdProp = serializedObject.FindProperty("_clientId");
        var clientSecretProp = serializedObject.FindProperty("_clientSecret");
        var applicationIdProp = serializedObject.FindProperty("_applicationId");
        var spreadsheetIdProp = serializedObject.FindProperty("_spreadsheetId");

        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Initialize", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(clientIdProp);
            EditorGUILayout.PropertyField(clientSecretProp);
            EditorGUILayout.PropertyField(applicationIdProp);
            if (GUILayout.Button("Execute"))
            {
                component.InitializeAsync();
            }

            GUI.enabled = false;
            EditorGUILayout.Toggle("Did Initialize", component.DidFetcherInitialize);
            GUI.enabled = true;
        }

        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Fetch files", EditorStyles.boldLabel);
            if (GUILayout.Button("Log all the names of all the files in the root of drive."))
            {
                component.LogAllRootFileNamesAsync();
            }
        }

        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Fetch spreadsheet", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spreadsheetIdProp);
            if (GUILayout.Button("Log the names of all sheets"))
            {
                component.LogAllSheetNamesAsync();
            }
            if (GUILayout.Button("Log all the values of the first sheet"))
            {
                component.LogAllValuesOfFirstSheetAsync();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}