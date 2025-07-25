using UnityEngine;

public class FPSTest : MonoBehaviour
{
    private float[] dts;
    private int i;

    // Start is called before the first frame update
    private void Start() => dts = new float[numFrames];

    public TMPro.TMP_Text fpsUI;
    public int numFrames = 5;

    // Update is called once per frame
    private void Update()
    {
        dts[i] = Time.deltaTime * 1000;
        i++;
        i %= numFrames;

        if (Time.frameCount >= numFrames)
        {
            float sum = 0;
            var min = float.MaxValue;
            var max = float.MinValue;

            for (var i = 0; i < numFrames; i++)
            {
                sum += dts[i];
                min = Mathf.Min(min, dts[i]);
                max = Mathf.Max(max, dts[i]);
            }

            var avg = sum / numFrames;

            fpsUI.text = "FPS: " + ToFPS(avg);
            fpsUI.text += "\nBest: " + ToFPS(min);
            fpsUI.text += "\nWorst: " + ToFPS(max);
            fpsUI.text += "\nAvg dt: " + avg + " ms";
        }
    }

    private string ToFPS(float millis)
    {
        var fps = 1000 / millis;

        return (int)fps * 10000 / 10000f + "";
    }
}