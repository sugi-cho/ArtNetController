using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class DmxFixtureLibrary
{
    public static List<string> FixtureLabelList { get; private set; }
    readonly static string folderPath= Path.Combine(Application.streamingAssetsPath, "Fixtures");

    public static void LoadFixtureList()
    {
        var filePathes = Directory.GetFiles(folderPath, "*.json");
        if (FixtureLabelList == null)
            FixtureLabelList = new List<string>();
        FixtureLabelList.Clear();
        foreach(var path in filePathes)
        {
            var label = Path.GetFileNameWithoutExtension(path);
            FixtureLabelList.Add(label);
        }
    }
    public static DmxOutputFixture LoadFixture(string label)
    {
        var filePath = Path.Combine(folderPath, $"{label}.json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Fixture: Label='{label}' does not exist.");
            var fixture = new DmxOutputFixture { Label = label };
            var json = JsonUtility.ToJson(fixture);
            File.WriteAllText(filePath, json);
            return fixture;
        }
        else
        {
            var json = File.ReadAllText(filePath);
            var fixture = JsonUtility.FromJson<DmxOutputFixture>(json);
            fixture.Initialize();
            return fixture;
        }
    }
    public static void ReloadFixture(DmxOutputFixture fixture)
    {
        var filePath = Path.Combine(folderPath, $"{fixture.Label}.json");
        var json = File.ReadAllText(filePath);
        JsonUtility.FromJsonOverwrite(json, fixture);
    }
    public static void SaveFixture(DmxOutputFixture fixture)
    {
        var filePath = Path.Combine(folderPath, $"{fixture.Label}.json");
        var json = JsonUtility.ToJson(fixture);
        File.WriteAllText(filePath, json);
        LoadFixtureList();
    }
}
