using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFXManager : MonoBehaviour
{
    public VisualEffect footStep;
    public ParticleSystem Blade01;
    public ParticleSystem Blade02;
    public ParticleSystem Blade03;
    public VisualEffect Heal;
    public VisualEffect Slash;

    public void Update_FootStep(bool state)
    {
        if (state)
            footStep.Play();
        else
            footStep.Stop();
    }

    public void PlayBlade01()
    {
        Blade01.Play();
        GetComponent<SFXManager>().SFXSword_Swipe_5();
    }

    public void PlayBlade02()
    {
        Blade02.Play();
        GetComponent<SFXManager>().SFXSword_Swipe_1();
    }

    public void PlayBlade03()
    {
        Blade03.Play();
        GetComponent<SFXManager>().SFXSword_Swipe_2();
    }

    public void StopBlade()//Stops all animations when interrupted
    {
        Blade01.Simulate(0);
        Blade01.Stop();

        Blade02.Simulate(0);
        Blade02.Stop();

        Blade03.Simulate(0);
        Blade03.Stop();
    }

    public void PlayHealVFX()
    {
        Heal.Play();
    }

    public void PlaySlash(Vector3 pos)
    {
        Slash.transform.position = pos;
        Slash.Play();
    }

}
