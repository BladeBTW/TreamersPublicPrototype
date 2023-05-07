using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public enum PickUpType
    {
        Heal, Coin
    }

    public PickUpType Type;
    public int Value = 20;
    public ParticleSystem CollectedVFX;

    private void OnTriggerEnter(Collider other)//Gets called whenever this object collides with another object
    {
        Debug.Log(other.name);
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<Character>().PickUpItem(this);

            if (CollectedVFX != null)
                Instantiate(CollectedVFX, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
