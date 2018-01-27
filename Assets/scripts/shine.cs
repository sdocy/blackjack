using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class shine : MonoBehaviour {

    public Image flare1;
    public Image flare2;
    public Image flare3;
    public bool burstOnStart;
    const float burst_delay = 0.15f;
    const float initial_wait = 3.0f;
    const float followup_wait = 15.0f;
    const float maxRandomStartDelay = 2.0f;

    // Expects any empty flare images to be the later ones, so valid
    // flare settings are:
    //      - none set
    //      - flare1 set
    //      - flare1 and flare2 set
    //      - flare1, flare2 and flare3 set
    IEnumerator doBurst()
    {
        List<Image> bursts = new List<Image>();

        if (flare1)     bursts.Add(flare1);
        if (flare2)     bursts.Add(flare2);
        if (flare3)     bursts.Add(flare3);

        // Small random delay to make it more natural and
        // keep multiple different bursts out of sync.
        yield return new WaitForSeconds(Random.Range(0.0f, maxRandomStartDelay));

        // shuffle the order
        for (int i = 0; i < bursts.Count; i++)
        {
            Image tmp = bursts[i];
            int randomIndex = Random.Range(i, bursts.Count);
            bursts[i] = bursts[randomIndex];
            bursts[randomIndex] = tmp;
        }



        foreach (Image b in bursts)
        {
                b.CrossFadeAlpha(1.0f, burst_delay, false);
                yield return new WaitForSeconds(burst_delay * 2);
                b.CrossFadeAlpha(0.0f, burst_delay, false);
        }
    }

    public void burst()
    {
        StartCoroutine(doBurst());
    }

    public void cancelBurst()
    {
        CancelInvoke();
    }

    public void enableBurst()
    {
        InvokeRepeating("burst", initial_wait, followup_wait);
    }

    void Start()
    {
        if (flare1)
        {
            flare1.CrossFadeAlpha(0.0f, 0.0f, false);
            flare1.sprite = Resources.Load<Sprite>("flare/flare2");
        }
            
        if (flare2)
        {
            flare2.CrossFadeAlpha(0.0f, 0.0f, false);
            flare2.sprite = Resources.Load<Sprite>("flare/flare2");
        }

            
        if (flare3)
        {
            flare3.CrossFadeAlpha(0.0f, 0.0f, false);
            flare3.sprite = Resources.Load<Sprite>("flare/flare2");
        }

        if (burstOnStart) enableBurst();
    }
}
