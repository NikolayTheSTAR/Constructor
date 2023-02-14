using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionController : MonoBehaviour
{
    [SerializeField] private List<MissionData> _missionDatas = new List<MissionData>();
    [SerializeField] private List<MissionIconData> _iconsData = new List<MissionIconData>();

    public List<MissionData> MissionDatas => _missionDatas;

    public Sprite GetIcon(MissionIconType iconType)
    {
        Sprite value = _iconsData.Find(info => info.missionIconType == iconType).icon;

        return value;
    }

    [Serializable]
    public class MissionData
    {
        public string nameKey;
        public string infoKey;
        public MissionIconType missionIconType;
    }

    [Serializable]
    public class MissionIconData
    {
        public MissionIconType missionIconType;
        public Sprite icon;
    }

    public enum MissionIconType
    {
        Master,
        PerfectSphere
    }
}