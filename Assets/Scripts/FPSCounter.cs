using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
[DisallowMultipleComponent]
public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float updateInterval = 0.5f;

    private float accum = 0f;
    private int frames = 0;

    private readonly char[] displayBuffer = new char[] { 'F', 'P', 'S', ':', ' ', ' ', ' ', ' ' };

    void Awake()
    {
        if(fpsText == null)
        {
            fpsText = GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        ResetCounters();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if(hasFocus)
        {
            ResetCounters();
        }
    }

    private void ResetCounters()
    {
        accum = 0f;
        frames = 0;
    }

    void Update()
    {
        frames++;
        accum += Time.unscaledDeltaTime;

        if(accum >= updateInterval)
        {
            if(accum > 0f)
            {
                int fps = Mathf.RoundToInt(frames / accum);
                UpdateFPSTextNonAlloc(fps);
            }

            if(accum >= updateInterval * 2)
            {
                accum = 0f;
            }
            else
            {
                accum -= updateInterval;
            }

            frames = 0;
        }
    }

    private void UpdateFPSTextNonAlloc(int fps)
    {
        fps = Mathf.Clamp(fps, 0, 999);

        int hundreds = fps / 100;
        int tens = (fps / 10) % 10;
        int ones = fps % 10;

        displayBuffer[5] = hundreds > 0 ? (char)('0' + hundreds) : ' ';
        displayBuffer[6] = (hundreds > 0 || tens > 0) ? (char)('0' + tens) : ' ';
        displayBuffer[7] = (char)('0' + ones);

        fpsText.SetText(displayBuffer, 0, displayBuffer.Length);
    }
}