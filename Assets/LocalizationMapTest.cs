using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.MagicLeapSupport;
using UnityEngine.XR.OpenXR.NativeTypes;

public class LocalizationMapTest : MonoBehaviour
{
    private MagicLeapLocalizationMapFeature localizationMapFeature;
    private MagicLeapLocalizationMapFeature.LocalizationMap[] maps;
    private string saveFilePath => Path.Combine(Application.persistentDataPath, spaceFileName);
    private string spaceFileName = "exported_space.bin";

    private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

    [SerializeField] TextMeshProUGUI statusText;

    private void Start()
    {
        Debug.Log("LocalizationMapTest.Start");

        // Setup event listeners for permissions and localization events.
        permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;

        //initialize the status text
        statusText.text = "Localization Map Test";

        // Request permissions for space import/export and define the path for the exported space file.
        MLPermissions.RequestPermission(MLPermission.SpaceImportExport, permissionCallbacks);
    }

    void OnPermissionDenied(string permission)
    {
        Debug.LogError($"{permission} Denied, Test Won't Function. Disabling");
        enabled = false;
    }

    void OnPermissionGranted(string permission)
    {
        // When the requested permission is granted, test the API features.
        InitializeFeatureTest();
    }

    private void InitializeFeatureTest()
    {
        // Get the Magic Leap Localization Map Feature and make sure it is accessible
        localizationMapFeature = OpenXRSettings.Instance.GetFeature<MagicLeapLocalizationMapFeature>();
        if (localizationMapFeature == null || !localizationMapFeature.enabled)
        {
            Debug.LogError("Magic Leap Localization Map Feature is unavailable or disabled. Disabling script.");
            return;
        }
        localizationMapFeature.EnableLocalizationEvents(true);

        MagicLeapLocalizationMapFeature.OnLocalizationChangedEvent += OnLocalizationChangedEvent;
        ExecuteFeatureTests();
    }

    private void ExecuteFeatureTests()
    {
        Debug.Log("CheckLocalizationStatus");
        statusText.text = "CheckLocalizationStatus";
        CheckLocalizationStatus();
        Debug.Log("QueryMaps");
        statusText.text = "QueryMaps";
        QueryMaps();
        Debug.Log("ExportMap");
        statusText.text = "ExportMap";
        ExportMap();
        Debug.Log("ImportMap");
        statusText.text = "ImportMap";
        ImportMap();
        Debug.Log("LocalizeIntoMap");
        statusText.text = "LocalizeIntoMap";
        LocalizeIntoMap();
        statusText.text = "Feature Tests Complete";
    }

    void OnDestroy()
    {
        // Unsubscribe event listeners when the object is destroyed.
        permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;

        if (localizationMapFeature == null || !localizationMapFeature.enabled)
            return;

        MagicLeapLocalizationMapFeature.OnLocalizationChangedEvent -= OnLocalizationChangedEvent;
    }

    // Gets an array of all the available maps
    public void QueryMaps()
    {
        if (localizationMapFeature == null || !localizationMapFeature.enabled)
        {
            Debug.Log("Localization Map Feature is unavailable or disabled. Disabling script.");

            //update the status text
            statusText.text = "Disabling script.";
            return;
        }

        statusText.text = "Querying Maps... Please Wait";
        XrResult result = localizationMapFeature.GetLocalizationMapsList(out maps);
        Debug.Log($"Query maps result: {result}");
        statusText.text = "Result: " + result + " \n" + "Maps Queried Successfully";

        if (result == XrResult.Success)
        {
            statusText.text = "Maps Queried Successfully";
            foreach (MagicLeapLocalizationMapFeature.LocalizationMap map in maps)
            {
                Debug.Log($"Map Name: {map.Name} \n" + $" Map UUID: {map.MapUUID} \n" + $" Map Type: {map.MapType}");
                //update the status text
                statusText.text = $"Map Name: {map.Name} \n" + $" Map UUID: {map.MapUUID} \n" + $" Map Type: {map.MapType}";
            }
        }
    }

    // Poll the localization status manually.
    void CheckLocalizationStatus()
    {
        if (localizationMapFeature.GetLatestLocalizationMapData(out MagicLeapLocalizationMapFeature.LocalizationEventData data))
        {
            statusText.text = "Getting latest localization map data";
            OnLocalizationChangedEvent(data);
        }
    }

    // Called when the localization status changes via the MagicLeapLocalizationMapFeature.OnLocalizationChangedEvent callback.
    private void OnLocalizationChangedEvent(MagicLeapLocalizationMapFeature.LocalizationEventData localizationEventData)
    {
        // Update and log the localization status and the name of the localized space.
        string status = localizationEventData.State.ToString();

        string mapName = localizationEventData.State
                         == MagicLeapLocalizationMapFeature.LocalizationMapState.Localized ? localizationEventData.Map.Name : "None";

        Debug.Log("Localization Status: " + status);
        Debug.Log("Localized Map Name: " + mapName);
        statusText.text = "Map: " + mapName;
    }

    // Export the first Map from the map array to a file on the device using the SaveMapToFile() function
    public void ExportMap()
    {
        if (localizationMapFeature == null || !localizationMapFeature.enabled)
            return;

        if (maps.Length == 0)
        {
            Debug.LogError("Must query available maps first");
            return;
        }

        var result = localizationMapFeature.ExportLocalizatioMap(maps[0].MapUUID, out byte[] mapData);
        Debug.Log($"Export map result: {result}");
        SaveBytesToFile(mapData, saveFilePath);
    }

    // Import a Map from a saved file path
    public void ImportMap()
    {
        if (localizationMapFeature == null || !localizationMapFeature.enabled)
            return;

        byte[] binaryData = LoadFileAsBytes(saveFilePath);
        if (binaryData.Length > 0)
        {
            // Import the byte array as a map
            XrResult result = localizationMapFeature.ImportLocalizationMap(binaryData, out string importedMapID);
            Debug.Log($"Import map result: {result}. Imported Map UUID: {importedMapID}");
        }
    }

    // Localize into the first map queried from the device
    public void LocalizeIntoMap()
    {
        if (localizationMapFeature == null || !localizationMapFeature.enabled)
            return;

        if (maps.Length == 0)
        {
            Debug.LogError("Must query available maps first");
            return;
        }

        XrResult result = localizationMapFeature.RequestMapLocalization(maps[0].MapUUID);
        Debug.Log($"Localize request result: {result}");
    }


    // Saves binary data into a file on the device
    private void SaveBytesToFile(byte[] binaryData, string filePath)
    {
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            fileStream.Write(binaryData, 0, binaryData.Length);
        }

        if (File.Exists(filePath))
        {
            Debug.Log("Binary data saved to file: " + filePath);
        }
        else
        {
            Debug.LogError("Failed to save binary data to file: " + filePath);
        }
    }

    // Loads a file as bytes if successful. Otherwise returns an empty byte array.
    private byte[] LoadFileAsBytes(string filePath)
    {
        //Read file from bytes
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("No maps have been exported, cannot import last exported map.");
            return Array.Empty<byte>();
        }

        using FileStream fileStream = new FileStream(filePath, FileMode.Open);
        byte[] binaryData = new byte[fileStream.Length];
        int bytesRead = fileStream.Read(binaryData, 0, binaryData.Length);
        if (bytesRead < binaryData.Length)
        {
            Debug.LogError($"Could not read all bytes from: {filePath} . Import failed!");
            return Array.Empty<byte>();
        }

        return binaryData;
    }
}
