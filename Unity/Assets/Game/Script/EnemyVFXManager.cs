using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EnemyVFXManager : MonoBehaviour
{
    public VisualEffect FootStep;
    public VisualEffect AttackVFX;
    public ParticleSystem BeingHitVFX;
    public VisualEffect BeingHitSplashVFX;

    public void PlayAttackVFX()
    {
        AttackVFX.SendEvent("OnPlay");
    }

    public void BurstFootStep()
    {
        FootStep.SendEvent("OnPlay");
    }

    public void PlayBeingHitVFX(Vector3 attackerPos)
    {
        Vector3 forceForward = transform.position - attackerPos;
        forceForward.Normalize();
        forceForward.y = 0;//Remove pitch
        BeingHitVFX.transform.rotation = Quaternion.LookRotation(forceForward);//Calc new rotation that align with new direction
        BeingHitVFX.Play();

        Vector3 splashPos = transform.position;//For pos of bloodsplash VFX
        splashPos.y += 2f;//Slightly up
        VisualEffect newSplashVFX = Instantiate(BeingHitSplashVFX,splashPos,Quaternion.identity);//Spawn new bloodsplash VFX (on runtime)
        newSplashVFX.SendEvent("OnPlay");
        Destroy(newSplashVFX.gameObject,10f);//10f is optional time delay before destroy
    }
}
