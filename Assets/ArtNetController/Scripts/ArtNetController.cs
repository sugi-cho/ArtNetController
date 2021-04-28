using UnityEngine;

using ArtNet.Packets;
using sugi.cc.udp;
using UniRx;

public class ArtNetController : MonoBehaviour
{
    [SerializeField] ArtNetDmxPacket packetToOutput;
    [SerializeField] UdpSender sender;
    UniverseManager universeManager => UniverseManager.Instance;
    DmxOutputUniverse activeUniverse => universeManager.ActiveUniverse;

    public bool UseBroadCast
    {
        get => m_useBroadcast;
        set
        {
            m_useBroadcast = value;
            sender.useBroadCast = m_useBroadcast;
            sender.CreateRemoteEP(RemoteIp, 6454);
        }
    }
    [SerializeField] bool m_useBroadcast;
    public string RemoteIp
    {
        get => m_remoteIP;
        set
        {
            m_remoteIP = value;
            sender.CreateRemoteEP(RemoteIp, 6454);
        }
    }
    [SerializeField] string m_remoteIP = "localhost";

    private void OnEnable()
    {
        sender = new UdpSender();
        sender.useBroadCast = UseBroadCast;
        sender.CreateRemoteEP(RemoteIp, 6454);

        universeManager.onActiveUniverseChanged += univ => packetToOutput.Universe = univ.Universe;
        activeUniverse.OnValueChanged.Subscribe(_ =>
        {
            var dmx = new byte[512];
            activeUniverse.SetDmx(ref dmx);
            packetToOutput.DmxData = dmx;
            sender.Send(packetToOutput.ToArray());
        });
    }
    private void OnDisable()
    {
        universeManager.SaveAllUniverses();
    }

}
