using System.Collections;
using System.IO;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class ScreenshotCapturer : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Object chứa các nhân vật con cần chụp")]
        public Transform parentObject;
        
        [Tooltip("Độ phân giải nhân thêm (1 là bình thường, 2 là gấp đôi...)")]
        public int superSize = 1;

        [Tooltip("Thư mục lưu ảnh (sẽ nằm trong thư mục Project)")]
        public string folderName = "CharacterScreenshots";

        [ContextMenu("Start Capturing")]
        public void StartCapturing()
        {
            if (parentObject == null)
            {
                Debug.LogError("Vui lòng kéo Parent Object vào!");
                return;
            }
            StartCoroutine(CaptureRoutine());
        }

        private void Start()
        {
            // Tự động chạy quy trình khi nhấn Play
            StartCapturing();
        }

        private IEnumerator CaptureRoutine()
        {
            // Tạo thư mục nếu chưa có
            string path = Path.Combine(Application.dataPath, "..", folderName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Debug.Log($"Bắt đầu chụp ảnh. Ảnh sẽ được lưu tại: {path}");

            // Tắt tất cả các con trước
            foreach (Transform child in parentObject)
            {
                child.gameObject.SetActive(false);
            }

            // Quét từng đứa
            int count = 0;
            foreach (Transform child in parentObject)
            {
                // Bật con hiện tại
                child.gameObject.SetActive(true);
                
                // Đợi đến cuối frame để đảm bảo Unity đã render xong object đó
                yield return new WaitForEndOfFrame();

                // Đặt tên file theo tên object con (loại bỏ các ký tự không hợp lệ)
                string safeName = string.Join("_", child.name.Split(Path.GetInvalidFileNameChars()));
                string fileName = safeName + ".png";
                string fullPath = Path.Combine(path, fileName);

                // Chụp màn hình
                ScreenCapture.CaptureScreenshot(fullPath, superSize);
                
                Debug.Log($"Đã chụp: {fileName}");
                count++;

                // Đợi một chút để tránh việc chụp quá nhanh
                yield return new WaitForSecondsRealtime(0.2f);

                // Tắt con hiện tại để chuẩn bị cho đứa tiếp theo
                child.gameObject.SetActive(false);
            }

            Debug.Log($"Xong! Đã chụp tổng cộng {count} ảnh.");
            
            // Mở thư mục chứa ảnh sau khi xong
            Application.OpenURL("file://" + path);
        }
    }
}
