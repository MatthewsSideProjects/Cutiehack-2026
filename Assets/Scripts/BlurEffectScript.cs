using UnityEngine;
using UnityEngine.Rendering;
public enum fadeState {Dark, Light}
public class BlurEffectScript : MonoBehaviour
{
    public float duration;
    public Light lighting;
    private Volume blurVolume;
    private float timePassed;
    private fadeState fade = fadeState.Light;
    void Start()
    {
        blurVolume = GetComponent<Volume>();
        blurVolume.weight = 0f;
    }
    void Update()
    {
        if (fade == fadeState.Dark)
        {
            timePassed += Time.deltaTime;
            blurVolume.weight = Mathf.Clamp01(timePassed / duration);
            lighting.intensity = 1 - Mathf.Clamp01(timePassed / duration);
            RenderSettings.ambientIntensity = Mathf.Lerp(1f, 0f, Mathf.Clamp01(timePassed / duration));
        }
        if (fade == fadeState.Light && timePassed >= 0)
        {
            timePassed -= Time.deltaTime;
            blurVolume.weight = Mathf.Clamp01(timePassed / duration);
            lighting.intensity = 1 - Mathf.Clamp01(timePassed / duration);
            RenderSettings.ambientIntensity = Mathf.Lerp(1f, 0f, Mathf.Clamp01(timePassed / duration));
        }
        
    }
    public void setFade(fadeState state)
    {
        fade = state;
    }
}
