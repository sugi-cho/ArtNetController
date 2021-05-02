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

    public IObservable<int> OnUniverseListCountChanged => m_universeList.ObserveCountChanged();
    public IObservable<int> OnActiveUniverseChanged => m_activeUniverseIdx;
    public IObservable<Unit> OnEditChannel => m_onEditChannel.ThrottleFrame(1);
    Subject<Unit> m_onEditChannel = new Subject<Unit>();
    public IObservable<Unit> OnValueChanged => m_onValueChanged.ThrottleFrame(1);
    Subject<Unit> m_onValueChanged = new Subject<Unit>();

    public IList<DmxOutputUniverse> UniverseList => m_universeList;
    ReactiveCollection<DmxOutputUniverse> m_universeList;

    readonly string folderPath = Path.Combine(Application.streamingAssetsPath, "Universes");

    public int ActiveUniverseIdx
    {
        get => m_activeUniverseIdx.Value;
        set => m_activeUniverseIdx.Value = value;
    }
    ReactiveProperty<int> m_activeUniverseIdx = new ReactiveProperty<int>();
    public DmxOutputUniverse ActiveUniverse => m_universeList[ActiveUniverseIdx];

    private UniverseManager()
    {
        m_universeList = new ReactiveCollection<DmxOutputUniverse>(
            Directory.GetFiles(folderPath, "*.json")
            .Select(path => File.ReadAllText(path))
            .Select(json =>
            {
                var univ = JsonUtility.FromJson<DmxOutputUniverse>(json);
                univ.Initialize();
                univ.OnValueChanged.Subscribe(_ => m_onValueChanged.OnNext(_));
                univ.OnEditChannel.Subscribe(_ => m_onEditChannel.OnNext(_));
                return univ;
            })
        );
        if (m_universeList.Count == 0)
            CreateUniverse();
        m_universeList.ObserveAdd().Subscribe(evt =>
        {
            var output = evt.Value;
            output.OnValueChanged.Subscribe(_ => m_onValueChanged.OnNext(_));
            output.OnEditChannel.Subscribe(_ => m_onEditChannel.OnNext(_));
        });
    }
    public void CreateUniverse()
    {
        var exists = m_universeList.Select(u => u.Universe);
        var newUniverse = m_universeList == null ?
            0 : Enumerable.Range(0, 512).Where(idx => !exists.Contains((short)idx)).FirstOrDefault();
        var newOutputUniverse = new DmxOutputUniverse { Universe = (short)newUniverse, Label = "NewSet" };
        m_universeList.Add(newOutputUniverse);
    }
    public void ValidateAllUniverses() =>
        m_universeList.ToList().ForEach(u => u.ValidateOutputs());
    public void SaveAllUniverses() =>
        m_universeList.ToList().ForEach(u => SaveUniverse(u));

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
