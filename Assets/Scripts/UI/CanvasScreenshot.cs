using System.Collections;
using System.IO;
using UnityEngine;

public class CanvasScreenshot : MonoBehaviour
{
    [ContextMenu("Capture Screenshot")] // 👈 thêm dòng này
    public void CaptureScreen()
    {
        StartCoroutine(CaptureRoutine());
    }

    IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame();

        string path = Path.Combine(
            Application.persistentDataPath,
            "screenshot.png"
        );

        ScreenCapture.CaptureScreenshot(path);

        Debug.Log("Saved: " + path);
    }
}