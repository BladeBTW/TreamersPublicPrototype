using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    //Set to public so I can access it outside of the GameObject (other scripts, the input data needs to be read)
    public float HorizontalInput;
    public float VerticalInput;
    public bool MouseButtonDown;
    public bool SpaceKeyDown;

    // Update is called once per frame
    // Preferable over FixedUpdate() since it misses keypresses quite frequently.
    void Update()
    {
        if (!MouseButtonDown && Time.timeScale != 0)//We want to be able to toggle timescale for our pause menu, so we check if it's not 0
        {
            MouseButtonDown = Input.GetMouseButtonDown(0);//0 is the left mouse button
        }

        if (!SpaceKeyDown && Time.timeScale != 0)//Spacekey not pressed, and game is not paused
        {
            SpaceKeyDown = Input.GetKeyDown(KeyCode.Space);
        }

        HorizontalInput = Input.GetAxisRaw("Horizontal");//String name of the axis needs to be the same as the one in the Input Manager
        VerticalInput = Input.GetAxisRaw("Vertical");//String name of the axis needs to be the same as the one in the Input Manager
    }

    private void OnDisable()//Built in Unity function, gets called when the GameObject this script is attached to is disabled
    {
        ClearCache();
    }

    public void ClearCache()
    {
        MouseButtonDown = false;
        SpaceKeyDown = false;
        HorizontalInput = 0;
        VerticalInput = 0;
    }
}
