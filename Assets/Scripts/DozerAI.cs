﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DozerAI : MonoBehaviour
{
    string dozerState;
    // Idle ---- Ready for a command
    // Wait ---- Waiting around for a bit
    // Turn ---- Turning at a set speed to a random direction
    // Move ---- Moving at a set speed for a random time
    // Box ----- Sttaches itself to the closest box, marking it off a global list as chosen, to move to and push for a certain amount of time
    //           Once the time is up, it will move on. To ensure it doesn't get stuck pushing the same box, it keeps the box as marked as used until it finds a new box to push
    //           It will also have a box cooldown timer so that it won't automatically gravitate to any and all boxes it drives near
    // Inspect - Driving to and looking at the player
    int randomInt;
    int waitWeight;
    int turnWeight;
    int moveWeight;
    float speed = 10f;
    Rigidbody rb;
    
    void Start()
    {
        dozerState = "Idle";
        randomInt = Random.Range(1, 30);
        waitWeight = Random.Range(25, 40);
        turnWeight = Random.Range(20, 30);
        moveWeight = Random.Range(20, 30);
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        switch (dozerState)
        {
            case "Idle":
                randomInt = Random.Range(1, 100);
                if (randomInt <= waitWeight)
                {
                    dozerState = "Wait";
                    randomInt = Random.Range(10, 30);
                }
                else if (randomInt > waitWeight && randomInt <= waitWeight + turnWeight)
                {
                    dozerState = "Turn";
                    randomInt = Random.Range(-180, 180);
                }
                else if (randomInt > waitWeight + turnWeight && randomInt <= waitWeight + turnWeight + moveWeight)
                {
                    dozerState = "Move";
                    randomInt = Random.Range(10, 40);
                }
                else
                {
                    dozerState = "Inspect";
                }
                break;
            case "Wait":
                randomInt--;
                if (randomInt >= 0)
                {
                    dozerState = "Idle";
                }
                break;
            case "Turn":
                if (transform.rotation.y > randomInt)
                {
                    transform.Rotate(Vector3.up, -speed * Time.deltaTime);
                }
                else if (transform.rotation.y < randomInt)
                {
                    transform.Rotate(Vector3.up, speed * Time.deltaTime);
                }
                else if (transform.rotation.y >= randomInt + 3 && transform.rotation.y <= randomInt - 3)
                {
                    dozerState = "Idle";
                }
                break;
            case "Move":
                Vector3 movement = transform.forward * speed * Time.deltaTime;
                rb.MovePosition(movement);
                randomInt--;
                if (randomInt >= 0)
                {
                    dozerState = "Idle";
                }
                break;
            case "Inspect":
                dozerState = "Idle";
                break;
        }
        //print("The dozer had a " + waitWeight + "% chance to wait, a " + turnWeight + "% chance to turn, and a " + moveWeight + "% chance to move. It chose to " + dozerState);
    }
}
