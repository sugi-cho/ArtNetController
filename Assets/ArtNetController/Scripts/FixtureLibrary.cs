using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UniRx;

public static class FixtureLibrary
{
    public static IObservable<Unit> OnFixtureLabelListLoaded => fixtureLabelLoadedSubject;
    static Subject<Unit> fixtureLabelLoadedSubject = new Subject<Unit>();

    public static List<string> FixtureLabelList
    {
        get
        {
            if (m_fixtureLabelList == null)
                LoadFixtureList();
            return m_fixtureLabelList;
        }
    }
    static List<string> m_fixtureLabelList;
    readonly static string folderPath = Path.Combine(Application.streamingAssetsPath, "Fixtures");

    public static void LoadFixtureList()
    {
        var filePathes = Directory.GetFiles(folderPath, "*.json");
        if (m_fixtureLabelList == null)
            m_fixtureLabelList = new List<string>();
        FixtureLabelList.Clear();
        foreach (var path in filePathes)
        {
            var label = Path.GetFileNameWithoutExtension(path);
            m_fixtureLabelList.Add(label);
        }
        fixtureLabelLoadedSubject.OnNext(Unit.Default);
    }
    static string GenerateUniqueLabel(string label)
    {
        if (!FixtureLabelList.Contains(label))
            return label;
        int idx;
        if (!int.TryParse(label.Split(' ').Last(), out idx))
            label = $"{label} 1";

        var splits = label.Split(' ');
        var baseName = "";
        for (var i = 0; i < splits.Length - 1; i++)
            baseName += $"{splits[i]} ";
        while (FixtureLabelList.Contains(label))
            label = $"{baseName}{idx++}";

        return label;
    }
    public static DmxOutputFixture LoadFixture(string label)
    {
        var filePath = Path.Combine(folderPath, $"{label}.json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Fixture: Label='{label}' does not exist.");
            var fixture = new DmxOutputFixture { Label = GenerateUniqueLabel(label) };
            fixture.Initialize();
            return fixture;
        }
        else
        {
            var json = File.ReadAllText(filePath);
            var fixture = JsonUtility.FromJson<DmxOutputFixture>(json);
            fixture.Initialize();
            fixture.FilePath = filePath;
            return fixture;
        }
    }
    public static void DeleteFixture(DmxOutputFixture fixture)
    {
        if (File.Exists(fixture.FilePath))
            File.Delete(fixture.FilePath);
        LoadFixtureList();
    }
    public static void SaveFixture(DmxOutputFixture fixture)
    {
        if (fixture.NumChannels < 1)
            return;
        fixture.BuildDefinitions();
        var filePath = Path.Combine(folderPath, $"{fixture.Label}.json");
        var json = JsonUtility.ToJson(fixture);
        if (fixture.FilePath != filePath)
            if (File.Exists(fixture.FilePath))
                File.Move(fixture.FilePath, filePath);
        File.WriteAllText(filePath, json);
        LoadFixtureList();
    }
}
