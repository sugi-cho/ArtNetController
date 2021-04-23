using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

class UniverseManager
{
    public static UniverseManager Instance => _instance;
    private static UniverseManager _instance = new UniverseManager();

    public event System.Action<List<DmxOutputUniverse>> onEditUniverses;
    public event System.Action<DmxOutputUniverse> onActiveUniverseChanged;

    public List<DmxOutputUniverse> Universes => m_universes;

    [SerializeField] List<DmxOutputUniverse> m_universes;
    readonly string folderPath = Path.Combine(Application.streamingAssetsPath, "Universes");

    public int ActiveUniverseIdx
    {
        get => m_activeUniverseIdx;
        set
        {
            m_activeUniverseIdx = value;
            onActiveUniverseChanged?.Invoke(ActiveUniverse);
        }
    }
    int m_activeUniverseIdx;
    public DmxOutputUniverse ActiveUniverse => m_universes[ActiveUniverseIdx];

    private UniverseManager()
    {
        m_universes = Directory.GetFiles(folderPath, "*.json")
            .Select(path => File.ReadAllText(path))
            .Select(json => JsonUtility.FromJson<DmxOutputUniverse>(json))
            .ToList();
        if (m_universes.Count == 0)
            CreateUniverse();
    }
    public void CreateUniverse()
    {
        var exists = m_universes.Select(u => u.Universe);
        var newUniverse = m_universes == null ?
            0 : Enumerable.Range(0, 512).Where(idx => !exists.Contains(idx)).FirstOrDefault();
        var newOutputUniverse = new DmxOutputUniverse { Universe = newUniverse, Label = "Set" };
        m_universes.Add(newOutputUniverse);
        onEditUniverses?.Invoke(m_universes);
    }
    public void SaveAllUniverses()
    {
        m_universes.ForEach(u => SaveUniverse(u));
    }
    public void SaveUniverse(DmxOutputUniverse universe)
    {
        var idx = Universes.IndexOf(universe);
        var fileName = $"Universe_{idx:000}.json";
        var path = Path.Combine(folderPath, fileName);
        var json = JsonUtility.ToJson(universe);
        File.WriteAllText(path, json);
    }
}
