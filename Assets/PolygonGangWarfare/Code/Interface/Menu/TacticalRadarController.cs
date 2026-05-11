using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class RadarBlip
{
    public Image blipImage;      
    public float angle;         
    [HideInInspector] public float currentAlpha = 0f;
}

public class TacticalRadarController : MonoBehaviour
{
    [Header("Елементи радара")]
    public RectTransform radarSweep;      
    public float sweepSpeed = -150f;      

    [Header("Точки виявлення")]
    public List<RadarBlip> blips = new List<RadarBlip>();
    public float fadeSpeed = 0.5f;       
    public float detectionThreshold = 10f; 

    void Update()
    {
        if (radarSweep == null) return;

        radarSweep.Rotate(0, 0, sweepSpeed * Time.deltaTime);

        float sweepAngle = radarSweep.localEulerAngles.z;

        foreach (var blip in blips)
        {
            if (blip.blipImage == null) continue;

            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(sweepAngle, blip.angle));

            if (angleDiff < detectionThreshold)
            {
                blip.currentAlpha = 1f;
            }
            else
            {
                blip.currentAlpha -= fadeSpeed * Time.deltaTime;
            }

            blip.currentAlpha = Mathf.Clamp01(blip.currentAlpha);
            Color c = blip.blipImage.color;
            c.a = blip.currentAlpha;
            blip.blipImage.color = c;
        }
    }
}