using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOrb : MonoBehaviour
{
    public float Speed = 2f;//Defines how fast the orb travels
    public int Damage = 25;
    public ParticleSystem HitVFX;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        GetComponent<SFXManager>().SFXShootFireball();
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(transform.position + transform.forward * Speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        GetComponent<SFXManager>().SFXShootFireball_Hit();
        Character cc = other.gameObject.GetComponent<Character>();

        if (cc != null && cc.IsPlayer)
        {
            cc.ApplyDamage(Damage, transform.position);
        }

        Instantiate(HitVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
