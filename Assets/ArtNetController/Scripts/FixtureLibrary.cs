using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UniRx;

public class FixtureLibrary
{
    public static FixtureLibrary Instance => _instance;
    static FixtureLibrary _instance = new FixtureLibrary();

    private FixtureLibrary() => LoadFixtureList();

    public IObservable<Unit> OnFixtureLabelListLoaded => m_fixtureLabelLoadedSubject;
    Subject<Unit> m_fixtureLabelLoadedSubject = new Subject<Unit>();
    public IObservable<string> OnSaveFixture => m_onSaveFixture;
    Subject<string> m_onSaveFixture = new Subject<string>();

    public List<string> FixtureLabelList
    {
        get
        {
            if (m_fixtureLabelList == null)
                LoadFixtureList();
            return m_fixtureLabelList;
        }
    }
    List<string> m_fixtureLabelList;
    readonly string folderPath = Path.Combine(Application.streamingAssetsPath, "Fixtures");

    public void LoadFixtureList()
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
        m_fixtureLabelLoadedSubject.OnNext(Unit.Default);
    }
    string GenerateUniqueLabel(string label)
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
    public DmxOutputFixture LoadFixture(string label)
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
    public void ReloadFixture(DmxOutputFixture fixture)
    {
        if (File.Exists(fixture.FilePath))
        {
            var startChannel = fixture.StartChannel;
            var json = File.ReadAllText(fixture.FilePath);
            JsonUtility.FromJsonOverwrite(json, fixture);
            fixture.Initialize();
            fixture.StartChannel = startChannel;
        }
    }
    public void DeleteFixture(DmxOutputFixture fixture)
    {
        if (File.Exists(fixture.FilePath))
            File.Delete(fixture.FilePath);
        LoadFixtureList();
    }
    public void SaveFixture(DmxOutputFixture fixture)
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
        m_onSaveFixture.OnNext(fixture.Label);
        LoadFixtureList();
    }
}
