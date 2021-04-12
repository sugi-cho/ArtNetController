using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

class UniverseManager
{
    public static UniverseManager Instance => _instance;
    private static UniverseManager _instance = new UniverseManager();

    public List<DmxOutputUniverse> Universes => m_universes;

    [SerializeField] List<DmxOutputUniverse> m_universes;
    readonly string folderPath = Path.Combine(Application.streamingAssetsPath, "Universes");

    public int ActiveUniverseIdx { get; set; }
    public DmxOutputUniverse ActiveUniverse => m_universes[ActiveUniverseIdx];

    List<IDmxOutput> m_selectingOutputList = new List<IDmxOutput>();
    List<int> m_selectingChannelList = new List<int>();
    public event System.Action<IEnumerable<int>> onSelectNullChannel;
    public event System.Action<IEnumerable<IDmxOutput>> onSelectOutput;

    private UniverseManager()
    {
        m_universes = Directory.GetFiles(folderPath, "*.json")
            .Select(path => File.ReadAllText(path))
            .Select(json => JsonUtility.FromJson<DmxOutputUniverse>(json))
            .ToList();
        if (m_universes.Count == 0)
            CreateUniverse();
    }
    public void ClearSelection()
    {
        m_selectingChannelList.Clear();
        m_selectingOutputList.Clear();
    }
    public void SelectChannel(int ch, bool multiple = true)
    {
        if (!multiple)
            m_selectingChannelList.Clear();
        m_selectingChannelList.Add(ch);

        onSelectNullChannel?.Invoke(m_selectingChannelList);
    }
    public void SelectOutput(IDmxOutput output, bool multiple = true)
    {
        if (!multiple)
            m_selectingOutputList.Clear();
        m_selectingOutputList.Add(output);

        onSelectOutput?.Invoke(m_selectingOutputList);
    }
    public void CreateUniverse()
    {
        var exists = m_universes.Select(u => u.Universe);
        var newUniverse = m_universes == null ?
            0 : Enumerable.Range(0, 512).Where(idx => !exists.Contains(idx)).FirstOrDefault();
        var newOutputUniverse = new DmxOutputUniverse { Universe = newUniverse, Label = "Set" };
        m_universes.Add(newOutputUniverse);
    }
    public void SaveAllUniverses()
    {
        m_universes.ForEach(u => SaveUniverse(u));
    }
    public void SaveUniverse(DmxOutputUniverse universe)
    {
        var fileName = $"{universe.Label}_u{universe.Universe:000}.json";
        var path = Path.Combine(folderPath, fileName);
        var json = JsonUtility.ToJson(universe);
        File.WriteAllText(path, json);
    }
}
