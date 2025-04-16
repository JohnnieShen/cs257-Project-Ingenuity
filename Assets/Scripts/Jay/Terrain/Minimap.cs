using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    /*
    Author: Jay
    Summary: Scripts a minimap to allow for the player to obtain a birds-eye
    orthographic view of the surroudning area. The minimap is based off of a 
    camera game object, and this script allows for the camera to follow the
    player around, staying at a constant Y level above them.
    */

    /*
    Simple update for the camera's coords to the player's
    */

    public Transform player;
    void LateUpdate () {
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;
    }
}
