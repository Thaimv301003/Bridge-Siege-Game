using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class RewardMultiplier : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Cái tam giác chỉ hướng")]
        public RectTransform arrow;
        
        [Tooltip("Thanh màu sắc chứa các mốc multiplier")]
        public RectTransform bar;

        [Header("Settings")]
        [Tooltip("Tốc độ di chuyển (Dao động điều hòa) - Càng nhỏ càng chậm")]
        public float moveSpeed = 1.5f;
        
        [Tooltip("Trạng thái đang di chuyển hay không")]
        public bool isMoving = false; // Mặc định đứng yên cho đến khi xem xong Ads

        [Header("Dynamic Coin Text")]
        [Tooltip("Text hiển thị tiền sẽ tự động nhảy số theo vị trí tam giác")]
        public TMPro.TMP_Text dynamicCoinText;
        [HideInInspector] public int baseCoins = 0; // Được truyền từ GameManager

        private float barHalfWidth;
        private float timer = 0f;

        private void Start()
        {
            if (bar != null)
            {
                // Lấy một nửa chiều rộng của thanh bar để làm biên giới hạn
                barHalfWidth = bar.rect.width / 2f;
            }
        }

        // Gọi mỗi khi bảng Victory hiện lên
        private void OnEnable()
        {
            isMoving = false; // Luôn đảm bảo nó đứng yên lúc đầu
            timer = 0f;       // Reset thời gian
            if (arrow != null)
            {
                arrow.localPosition = new Vector3(0, arrow.localPosition.y, 0); // Reset về chính giữa (x5)
            }
        }

        private void Update()
        {
            if (isMoving && arrow != null && bar != null)
            {
                // Tích lũy thời gian để dao động điều hòa mượt mà
                timer += Time.unscaledDeltaTime * moveSpeed;

                // Công thức Dao động điều hòa: x = A * Sin(t)
                float sinValue = Mathf.Sin(timer);
                
                float xPos = sinValue * (barHalfWidth * 0.95f); // 0.95 để không bị chạm sát mép quá
                arrow.localPosition = new Vector3(xPos, arrow.localPosition.y, 0);

                RefreshCoinText();
            }
        }

        // Cập nhật số tiền hiển thị dựa trên vị trí hiện tại
        public void RefreshCoinText()
        {
            if (dynamicCoinText != null && baseCoins > 0)
            {
                int currentMultiplier = GetMultiplier();
                // Hiển thị trực tiếp tổng tiền nhận được sau khi nhân
                dynamicCoinText.text = (baseCoins * currentMultiplier).ToString();
            }
        }

        /// <summary>
        /// Trả về hệ số nhân dựa trên vị trí hiện tại của tam giác
        /// </summary>
        public int GetMultiplier()
        {
            if (arrow == null) return 2;

            if (barHalfWidth == 0 && bar != null)
            {
                barHalfWidth = bar.rect.width / 2f;
            }

            if (barHalfWidth == 0) return 5; // Default center multiplier

            // Tính tỉ lệ khoảng cách từ tâm (0 là tâm, 1 là kịch rìa)
            float absPosNormalized = Mathf.Abs(arrow.localPosition.x) / barHalfWidth;

            // Phân chia vùng Multiplier (Có thể điều chỉnh con số để tăng/giảm độ khó)
            if (absPosNormalized < 0.15f) return 5;  // Vùng tâm cực hẹp là x5
            if (absPosNormalized < 0.55f) return 3;  // Vùng tiếp theo là x3
            return 2;                                // Vùng rìa là x2
        }

        public void StopMoving()
        {
            isMoving = false;
        }

        public void StartMoving()
        {
            isMoving = true;
        }
    }
}
