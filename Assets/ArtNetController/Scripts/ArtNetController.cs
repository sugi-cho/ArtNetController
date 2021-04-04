using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ArtNet.Packets;

public class ArtNetController : MonoBehaviour
{
    [SerializeField] ArtNetDmxPacket packetToOutput;
    [SerializeField] UniverseManager universeManager;


    private void OnEnable()
    {
        universeManager = new UniverseManager();
    }
    private void OnDisable()
    {
        universeManager.SaveAllUniverses();
    }
}
