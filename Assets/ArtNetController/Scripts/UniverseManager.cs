using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UniverseManager
{
    [SerializeField] List<DmxOutputUniverse> universes;
    [SerializeField] List<string> fixtures;
    readonly string folderPath = Path.Combine(Application.streamingAssetsPath, "Universes");

    public IDmxOutputModule[] activeModules;

    public UniverseManager()
    {
        universes = Directory.GetFiles(folderPath, "*.json")
            .Select(path => File.ReadAllText(path))
            .Select(json => JsonUtility.FromJson<DmxOutputUniverse>(json))
            .ToList();
        if (universes.Count == 0)
            CreateUniverse();
        FixtureLibrary.LoadFixtureList();
        fixtures = FixtureLibrary.FixtureLabelList;
    }
    public void CreateUniverse()
    {
        var exists = universes.Select(u => u.Universe);
        var newUniverse = universes == null ?
            0 : Enumerable.Range(0, 512).Where(idx => !exists.Contains(idx)).FirstOrDefault();
        var newOutputUniverse = new DmxOutputUniverse { Universe = newUniverse, Label = "New Universe" };
        universes.Add(newOutputUniverse);
    }
    public void SaveAllUniverses()
    {
        universes.ForEach(u => SaveUniverse(u));
    }
    public void SaveUniverse(DmxOutputUniverse universe)
    {
        var fileName = $"Universe_{universe.Universe:000}.json";
        var path = Path.Combine(folderPath, fileName);
        var json = JsonUtility.ToJson(universe);
        File.WriteAllText(path, json);
    }
}
