using System;
using CrimeLife.Crime;
using CrimeLife.NPC;
using UnityEngine;
using UnityEngine.AI;

namespace CrimeLife.Social
{
    // NPC에게 걸어가서 대화/선물을 하고 친밀도를 올린다.
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerSocialController : MonoBehaviour
    {
        [SerializeField] private RelationshipConfig _config;
        [SerializeField] private RelationshipBook _book;
        [SerializeField] private Wallet _wallet;

        private readonly NpcChaser _chaser = new NpcChaser();
        private NavMeshAgent _agent;
        private SocialKind _kind;

        // 못 하는 이유(경계 중, 쿨다운, 현금 부족)를 UI에 알린다
        public event Action<string> Rejected;
        public event Action<SocialKind, NpcAgent> Performed;

        public int GiftCost => _config.Settings.GiftCost;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void Request(NpcAgent target, SocialKind kind)
        {
            if (!CanDo(target, kind)) return;

            _kind = kind;
            _chaser.Begin(target, _config.Settings.SocialRange);
        }

        public void Cancel()
        {
            _chaser.Cancel();
        }

        private bool CanDo(NpcAgent target, SocialKind kind)
        {
            if (target.IsWary)
            {
                Rejected?.Invoke($"{target.DisplayName}: 경계 중이라 대화할 기분이 아니다");
                return false;
            }
            if (target.IsOnSocialCooldown)
            {
                Rejected?.Invoke($"{target.DisplayName}: 방금 얘기했다. 잠시 뒤에 다시");
                return false;
            }
            if (kind == SocialKind.Gift && _wallet.Cash < _config.Settings.GiftCost)
            {
                Rejected?.Invoke($"선물할 현금이 부족하다 ({_config.Settings.GiftCost} 필요)");
                return false;
            }
            return true;
        }

        private void Update()
        {
            if (!_chaser.Active) return;

            NpcAgent reached = _chaser.Tick(transform, _agent, Time.deltaTime);
            if (reached == null) return;

            // 쫓아오는 사이 상황이 바뀌었을 수 있어서 도착했을 때 다시 확인한다
            if (!CanDo(reached, _kind)) return;

            RelationshipSettings s = _config.Settings;
            if (_kind == SocialKind.Gift)
            {
                _wallet.TrySpend(s.GiftCost);
                _book.Add(reached, s.GiftGain);
            }
            else
            {
                _book.Add(reached, s.ChatGain);
            }

            reached.StartSocialCooldown(s.SocialCooldown);
            Performed?.Invoke(_kind, reached);
        }
    }
}
