using UnityEngine;

using sugi.cc.udp.artnet;

public class test : MonoBehaviour
{
    public DmxOutputFixture fixture;
    public DmxOutputFixture newFixture;

    [ContextMenu("test")]
    void Test()
    {
        fixture = FixtureLibrary.CreateFixture();
        FixtureLibrary.SaveFixture(fixture);
        newFixture = FixtureLibrary.CreateFixture();
        FixtureLibrary.SaveFixture(newFixture);
    }
}
