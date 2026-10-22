using System;
using System.Collections.Generic;
using CrimeLife.Core;
using CrimeLife.NPC;
using UnityEngine;

namespace CrimeLife.Social
{
    // NPC별 친밀도를 들고 저장/복원한다. 저장된 값이 없는 NPC는 프로필의 초기 친밀도에서 시작한다.
    public class RelationshipBook : MonoBehaviour, ISaveable
    {
        private readonly Dictionary<string, float> _values = new Dictionary<string, float>();

        // (NPC, 바뀐 뒤 값, 변화량)
        public event Action<NpcAgent, float, float> Changed;

        public float Get(NpcAgent npc) =>
            _values.TryGetValue(npc.Id, out float value) ? value : npc.InitialFriendship;

        public float Add(NpcAgent npc, float delta)
        {
            float before = Get(npc);
            float after = RelationshipLogic.Clamp(before + delta);
            _values[npc.Id] = after;
            Changed?.Invoke(npc, after, after - before);
            return after;
        }

        public void Save(SaveData data)
        {
            data.Relationships = new List<RelationshipRecord>();
            foreach (KeyValuePair<string, float> pair in _values)
                data.Relationships.Add(new RelationshipRecord { NpcId = pair.Key, Friendship = pair.Value });
        }

        public void Load(SaveData data)
        {
            _values.Clear();
            foreach (RelationshipRecord record in data.Relationships)
                _values[record.NpcId] = RelationshipLogic.Clamp(record.Friendship);
        }
    }
}
