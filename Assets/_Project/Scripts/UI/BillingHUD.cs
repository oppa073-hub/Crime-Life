using CrimeLife.Economy;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    // 생활비 상태(미납액/남은 시간)와 청구·연체·납부 알림. NeedsHUD 문서의 "bill"/"message" 라벨을 쓴다.
    [RequireComponent(typeof(UIDocument))]
    public class BillingHUD : MonoBehaviour
    {
        [SerializeField] private BillingManager _billing;

        private Label _bill;
        private Label _message;

        // BillingManager.Start가 먼저 돌 수도 있어서, 첫 표시는 직접 갱신해 둔다
        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _bill = root.Q<Label>("bill");
            _message = root.Q<Label>("message");

            _billing.Changed += Refresh;
            _billing.Notice += ShowNotice;
            _billing.BillPaid += ShowPaid;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_billing == null) return;
            _billing.Changed -= Refresh;
            _billing.Notice -= ShowNotice;
            _billing.BillPaid -= ShowPaid;
        }

        private void Refresh()
        {
            string time = FormatHours(_billing.HoursLeft);
            _bill.text = _billing.Unpaid > 0
                ? $"청구서 미납 {_billing.Unpaid} (연체까지 {time})"
                : $"다음 청구까지 {time}";
        }

        private void ShowNotice(string text) => _message.text = text;

        private void ShowPaid(int amount) => _message.text = $"청구서 납부: {amount}";

        private static string FormatHours(float hours)
        {
            int total = Mathf.Max(0, Mathf.CeilToInt(hours));
            return total >= 24 ? $"{total / 24}일 {total % 24}시간" : $"{total}시간";
        }
    }
}
