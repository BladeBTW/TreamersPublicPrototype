using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropWeapons : MonoBehaviour
{
    public List<GameObject> Weapons;

    public void DropSwords()
    {
        foreach (GameObject weapon in Weapons)
        {
            weapon.AddComponent<Rigidbody>();//To make the weapon fall
            weapon.AddComponent<BoxCollider>();//To make the weapon collide with the ground
            weapon.transform.parent = null;//To make the weapon not a child of the player, making it fall.
        }
    }
}
