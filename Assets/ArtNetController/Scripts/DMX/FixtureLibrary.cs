using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class FixtureLibrary
{
    public static List<string> FixtureLabelList
    {
        get
        {
            if (_fixtureLabelList == null)
                LoadFixtureList();
            return _fixtureLabelList;
        }
    }
    static List<string> _fixtureLabelList;
    readonly static string folderPath = Path.Combine(Application.streamingAssetsPath, "Fixtures");

    public static void LoadFixtureList()
    {
        var filePathes = Directory.GetFiles(folderPath, "*.json");
        if (_fixtureLabelList == null)
            _fixtureLabelList = new List<string>();
        FixtureLabelList.Clear();
        foreach (var path in filePathes)
        {
            var label = Path.GetFileNameWithoutExtension(path);
            _fixtureLabelList.Add(label);
        }
    }
    public static DmxOutputFixture CreateFixture()
    {
        var label = GenerateUniqueLabel("Fixture");
        var newFixture = new DmxOutputFixture { Label = label };
        return newFixture;
    }
    static string GenerateUniqueLabel(string label)
    {
        if (!FixtureLabelList.Contains(label))
            return label;
        var splits = label.Split(' ');
        if (splits.Length == 1)
            label = $"{label} 1";
        var intStr = label.Split(' ').Last();
        int idx = 0;
        if (int.TryParse(intStr, out idx))
            while (FixtureLabelList.Contains(label))
                label = $"{splits[0]} {idx++}";
        return label;
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
