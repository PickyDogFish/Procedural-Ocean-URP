using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI avgFPSText;
    [SerializeField] private TextMeshProUGUI minFPSText;
    [SerializeField] private float pollingTime = 1f;
    private float time;
    private int frameCount;
    private int minFPS = int.MaxValue;
    public void Update() {
        if (1f / Time.unscaledDeltaTime < minFPS) {
            minFPS = (int)(1f / Time.unscaledDeltaTime);
        }
        time += Time.unscaledDeltaTime;
        frameCount++;
        if (time >= pollingTime) {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            avgFPSText.text = frameRate.ToString() + " FPS";
            frameCount = 0;
            time -= pollingTime;


            minFPSText.text = minFPS.ToString();
            minFPS = int.MaxValue;

        }
    }
}
