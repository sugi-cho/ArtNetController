using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UniRx;

class UniverseManager
{
    public static UniverseManager Instance => _instance;
    private static UniverseManager _instance = new UniverseManager();

    public IObservable<int> OnEditUniverses => m_universes.ObserveCountChanged();
    public IObservable<int> OnActiveUniverseChanged => m_activeUniverseIdx;

    public IList<DmxOutputUniverse> UniverseList => m_universes;
    ReactiveCollection<DmxOutputUniverse> m_universes;

    readonly string folderPath = Path.Combine(Application.streamingAssetsPath, "Universes");

    public int ActiveUniverseIdx
    {
        get => m_activeUniverseIdx.Value;
        set => m_activeUniverseIdx.Value = value;
    }
    ReactiveProperty<int> m_activeUniverseIdx = new ReactiveProperty<int>();
    public DmxOutputUniverse ActiveUniverse => m_universes[ActiveUniverseIdx];

    private UniverseManager()
    {
        m_universes = new ReactiveCollection<DmxOutputUniverse>(
            Directory.GetFiles(folderPath, "*.json")
            .Select(path => File.ReadAllText(path))
            .Select(json =>
            {
                var univ= JsonUtility.FromJson<DmxOutputUniverse>(json);
                univ.Initialize();
                return univ;
            })
        );
        if (m_universes.Count == 0)
            CreateUniverse();
    }
    public void CreateUniverse()
    {
        var exists = m_universes.Select(u => u.Universe);
        var newUniverse = m_universes == null ?
            0 : Enumerable.Range(0, 512).Where(idx => !exists.Contains((short)idx)).FirstOrDefault();
        var newOutputUniverse = new DmxOutputUniverse { Universe = (short)newUniverse, Label = "Set" };
        m_universes.Add(newOutputUniverse);
    }
    public void ValidateAllUniverses() =>
        m_universes.ToList().ForEach(u => u.ValidateOutputs());
    public void SaveAllUniverses() =>
        m_universes.ToList().ForEach(u => SaveUniverse(u));

    public void SaveUniverse(DmxOutputUniverse universe)
    {
        universe.BuildDefinitions();
        var idx = UniverseList.IndexOf(universe);
        var fileName = $"Universe_{idx:000}.json";
        var path = Path.Combine(folderPath, fileName);
        var json = JsonUtility.ToJson(universe);
        File.WriteAllText(path, json);
    }
}
