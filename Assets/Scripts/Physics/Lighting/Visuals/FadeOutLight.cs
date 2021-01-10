using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutLight : MonoBehaviour
{
    private ParticleSystem ps;
    private LightEmitting le;
    public float FadeSpeed;
    private IEnumerator Start()
    {
        ps = GetComponent<ParticleSystem>();
        le = GetComponent<LightEmitting>();
        yield return new WaitForSeconds(0.075f);
        StartCoroutine(FadeOut());
    }
    public void StartFade()
    {
        StartCoroutine(FadeOut());
    }
    private IEnumerator FadeOut()
    {
        while(le.light.Power > 0)
        {
            yield return new WaitForSeconds(0.035f);
            le.light.Power -= FadeSpeed;
        }
        Destroy(gameObject);
    }
}
