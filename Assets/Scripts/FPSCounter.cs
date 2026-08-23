using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
[DisallowMultipleComponent]
public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float updateInterval = 0.5f;

    private float _accum;
    private int _frames;
    private int _lastFps = -1;

    private readonly char[] _displayBuffer = new char[] { 'F', 'P', 'S', ':', ' ', '0', '0', '0' };

    private void Awake()
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
        _accum = 0f;
        _frames = 0;
        _lastFps = -1;
    }

    private void Update()
    {
        _frames++;
        _accum += Time.unscaledDeltaTime;

        if(_accum >= updateInterval)
        {
            int currentFps = Mathf.RoundToInt(_frames / _accum);

            if(currentFps != _lastFps)
            {
                _lastFps = currentFps;
                UpdateFPSText(currentFps);
            }

            _accum -= updateInterval;
            
            if(_accum > updateInterval)
            {
                _accum = 0f;
            }

            _frames = 0;
        }
    }

    private void UpdateFPSText(int fps)
    {
        fps = Mathf.Clamp(fps, 0, 999);

        int hundreds = fps / 100;
        int tens = (fps / 10) % 10;
        int ones = fps % 10;

        int bufferIndex = 5;

        if(hundreds > 0)
        {
            _displayBuffer[bufferIndex++] = (char)('0' + hundreds);
            _displayBuffer[bufferIndex++] = (char)('0' + tens);
        }
        else if(tens > 0)
        {
            _displayBuffer[bufferIndex++] = (char)('0' + tens);
        }

        _displayBuffer[bufferIndex++] = (char)('0' + ones);

        fpsText.SetText(_displayBuffer, 0, bufferIndex);
    }
}