using UnityEngine;

using sugi.cc.udp.artnet;

public class test : MonoBehaviour
{
    [SerializeField] float single;
    [SerializeField] Color color;
    [SerializeField] bool useFine;
    [SerializeField] int channel;
    [SerializeField] byte[] dmx = new byte[512];

    private void OnValidate()
    {
        var dmxFloat = new DmxOutputFloat();
        var dmxColor = new DmxOutputColor();
        var output = dmxColor as IDmxOutputModule;

        dmxFloat.UseFine = useFine;
        dmxFloat.Value = single;
        dmxColor.UseFine = useFine;
        dmxColor.Value = color;

        output.SetChannel(channel);
        output.SetDmx(ref dmx);
        (dmxFloat as IDmxOutputModule).SetDmx(ref dmx);
    }
}
