using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ArtNet.Packets;

public class ArtNetController : MonoBehaviour
{
    [SerializeField] ArtNetDmxPacket packetToOutput;
    [SerializeField] UniverseManager universeManager = UniverseManager.Instance;


    private void OnEnable()
    {
        universeManager = UniverseManager.Instance;
    }
    private void OnDisable()
    {
        universeManager.SaveAllUniverses();
    }
}
