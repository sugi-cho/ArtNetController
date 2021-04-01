using UnityEngine;

using sugi.cc.udp.artnet;

public class test : MonoBehaviour
{
    public DmxOutputFixture fixture;
    public DmxOutputFixture newFixture;

    [ContextMenu("test")]
    void Test()
    {
        fixture.SetModuleList();
        var dmxModule = (fixture as IDmxOutputModule);
        dmxModule.SetChannel(0);

        Debug.Log((fixture as IDmxOutputModule).NumChannels);
        var json = JsonUtility.ToJson(dmxModule);
        Debug.Log(json);

        (newFixture = JsonUtility.FromJson<DmxOutputFixture>(json)).SetModuleList();
        Debug.Log((newFixture as IDmxOutputModule).NumChannels);
    }
}
