using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIConfig : ScriptableObject
{
    static UIConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.LoadAll<UIConfig>("DataObjects")[0];
            return _instance;
        }
    }
    static UIConfig _instance;
    public static Color GetTypeColor(DmxOutputType type) =>
        Instance.typeColors.FirstOrDefault(tc => tc.type == type).color;

    public TypeColor[] typeColors = new[]{
        new TypeColor{type = DmxOutputType.Empty},
        new TypeColor{type = DmxOutputType.Bool},
        new TypeColor{type = DmxOutputType.Int},
        new TypeColor{type = DmxOutputType.Selector},
        new TypeColor{type = DmxOutputType.Float},
        new TypeColor{type = DmxOutputType.XY},
        new TypeColor{type = DmxOutputType.Color},
        new TypeColor{type = DmxOutputType.Fixture},
        new TypeColor{type = DmxOutputType.Universe},
    };

    [System.Serializable]
    public struct TypeColor
    {
        public DmxOutputType type;
        public Color color;
    }
}
