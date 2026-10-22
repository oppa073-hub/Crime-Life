using UnityEngine;
using UnityEngine.AI;

namespace CrimeLife.NPC
{
    // 움직이는 NPC를 따라가서 사정거리 안에 들어오면 알려 주는 공용 로직.
    // 범죄(소매치기/강도)와 사교(대화/선물)가 같은 추적을 쓴다.
    public class NpcChaser
    {
        private const float RepathInterval = 0.25f;

        private float _range;
        private float _repathTimer;

        public NpcAgent Target { get; private set; }
        public bool Active => Target != null;

        public void Begin(NpcAgent target, float range)
        {
            Target = target;
            _range = range;
            _repathTimer = 0f;
        }

        public void Cancel()
        {
            Target = null;
        }

        // 사정거리에 닿은 프레임에 대상을 돌려주고 추적을 끝낸다. 아직이면 null.
        public NpcAgent Tick(Transform self, NavMeshAgent agent, float deltaTime)
        {
            if (Target == null) return null;

            Vector3 offset = Target.transform.position - self.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= _range * _range)
            {
                NpcAgent reached = Target;
                Target = null;
                agent.ResetPath();
                return reached;
            }

            _repathTimer -= deltaTime;
            if (_repathTimer > 0f) return null;
            _repathTimer = RepathInterval;
            agent.SetDestination(Target.transform.position);
            return null;
        }
    }
}
