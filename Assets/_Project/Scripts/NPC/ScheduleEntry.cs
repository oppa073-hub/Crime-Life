using System;
using UnityEngine;

namespace CrimeLife.NPC
{
    [Serializable]
    public struct ScheduleEntry
    {
        [Range(0f, 24f)] public float StartHour;
        [Tooltip("씬의 Waypoint Id와 일치해야 한다")]
        public string WaypointId;
        public string Label;
    }
}
