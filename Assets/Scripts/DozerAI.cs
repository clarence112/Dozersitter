﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DozerAI : MonoBehaviour {
    public string dozerState;
    //What the dozer is doing. Can be these values:
    // Idle ---- Ready for a command
    // Wait ---- Waiting around for a bit
    // Turn ---- Turning at a set speed to a random direction
    // Move ---- Moving at a set speed for a random time
    // Box ----- Attaches itself to the closest box to move to and push for a certain amount of time
    //           Once the time is up, it will move on
    //           It will also have a box cooldown timer so that it won't automatically gravitate to any and all boxes it drives near
    // Inspect - Driving to and looking at the player
    // Reverse - Backing up away from a wall
    // NewMove - Reviseed movement with actual AI

    public string dozerMood = "Neutral"; 
    //The dozer's current mood, mostly cosmetic but is what is used to find the score. Can be these values, best to worst:
    // Excited
    // Happy
    // Neutral
    // Unhappy
    // Angry

    int randomInt;
    int waitWeight;
    int turnWeight;
    int moveWeight;
    float targPosX = 0;
    float targPosZ = 0;
    float speed = 6f;
    Rigidbody rb;
    int turnDirection;
    public static bool legacyAI = false;
    Vector3 lastPos;
    Vector3 lastRot;
    Transform eyes;
    Transform camPan;
    Transform camLift;
    Transform wheelRF;
    Transform wheelRR;
    Transform wheelLF;
    Transform wheelLR;
    Quaternion OriginalRot;
    Quaternion NewRot;
    Transform currentBox;
    CapsuleCollider boxFinder;
    int boxPushTimer = 300;
    int boxCooldownTimer;
    bool pushingBox;
    Vector3 boxPos;

    void Start() {
        dozerState = "Wait";
        randomInt = Random.Range(1, 30);
        waitWeight = Random.Range(25, 40);
        turnWeight = Random.Range(20, 30);
        moveWeight = Random.Range(20, 30);
        rb = GetComponent<Rigidbody>();

        eyes = this.gameObject.transform.GetChild(0).GetChild(2);
        camPan = this.gameObject.transform.GetChild(0).GetChild(1);
        camLift = this.gameObject.transform.GetChild(0).GetChild(1).GetChild(0);
        wheelLF = this.gameObject.transform.GetChild(0).GetChild(4);
        wheelLR = this.gameObject.transform.GetChild(0).GetChild(5);
        wheelRF = this.gameObject.transform.GetChild(0).GetChild(6);
        wheelRR = this.gameObject.transform.GetChild(0).GetChild(7);
        boxFinder = GetComponent<CapsuleCollider>();
    }

    void FixedUpdate() {
        if ((rb.position.x > 14 || rb.position.x < -14 || rb.position.z > 24 || rb.position.z < -24) && legacyAI) {
            dozerState = "Reverse";
            randomInt = 20;
        }
        else if (boxPos.x != 0 && boxPos.y != 0 && boxPos.z != 0)
        {
            dozerState = "Box";
        }
        if (randomInt <= 0) {
            dozerState = "Idle";
        }
        switch (dozerState) {
            case "Idle":
                randomInt = Random.Range(1, 100);
                if (randomInt <= waitWeight) {
                    dozerState = "Wait";
                    randomInt = Random.Range(25, 75);
                } else if (randomInt > waitWeight && randomInt <= waitWeight + turnWeight) {
                    dozerState = "Turn";
                    randomInt = Random.Range(25, 75);
                    turnDirection = Random.Range(1, 3);
                } else if (randomInt > waitWeight + turnWeight && randomInt <= waitWeight + turnWeight + moveWeight) {
                    if (legacyAI) {
                        dozerState = "Move";
                        randomInt = Random.Range(25, 75);
                    } else {
                        randomInt = 20;
                        targPosX = Random.Range(-15f, 15f);
                        targPosZ = Random.Range(-25f, 25f);
                        dozerState = "NewMove";
                        //print(targPosX);
                    }
                } else {
                    dozerState = "Inspect";
                    randomInt = 100;
                }
                break;
            case "Wait":
                randomInt--;
                //print("Dozer is waiting for another " + randomInt + " ticks.");
                break;
            case "Turn":
                if (turnDirection == 1) {
                    transform.Rotate(Vector3.up, speed * 5 * Time.deltaTime);
                }
                else if (turnDirection == 2) {
                    transform.Rotate(Vector3.up, -speed * 5 * Time.deltaTime);
                }
                randomInt--;
                //print("Dozer is turning for another " + randomInt + " ticks.");
                break;
            case "Move":
                rb.AddForce(transform.forward * speed, ForceMode.Force);
                randomInt--;
                //print("Dozer is moving for another " + randomInt + " ticks.");
                break;
            case "Inspect":

                targPosX = PlayerMovement.pos.x;
                targPosZ = PlayerMovement.pos.z;

                OriginalRot = transform.rotation;
                transform.LookAt(new Vector3(targPosX, 0.5f, targPosZ));
                NewRot = transform.rotation;
                transform.rotation = OriginalRot;
                transform.rotation = Quaternion.Lerp(transform.rotation, NewRot, speed * 0.3f * Time.deltaTime);

                if (2f < Mathf.Sqrt(Mathf.Pow(targPosZ - transform.position.z, 2f) + Mathf.Pow(targPosX - transform.position.x, 2))) {
                    rb.AddForce(transform.forward * ((speed * 2) / ((Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z)) + 0.1f)), ForceMode.Force);

                    camPan.LookAt(new Vector3(targPosX, 0.5f, targPosZ));
                    camLift.LookAt(new Vector3(targPosX, 2f, targPosZ));

                    Debug.DrawLine(transform.position, new Vector3(targPosX, 0, targPosZ));
                    Debug.DrawLine(camLift.position, new Vector3(targPosX, 2, targPosZ));
                } else {
                    randomInt--;
                }
                break;
            case "Reverse":
                rb.AddForce(transform.forward * -speed, ForceMode.Force);
                randomInt--;
                //print("Dozer is reversing for another " + randomInt + " ticks.");
                break;
            case "NewMove":
                OriginalRot = transform.rotation;
                transform.LookAt(new Vector3(targPosX, 0.5f, targPosZ));
                NewRot = transform.rotation;
                transform.rotation = OriginalRot;
                transform.rotation = Quaternion.Lerp(transform.rotation, NewRot, speed * 0.3f * Time.deltaTime);

                if (1f < Mathf.Sqrt(Mathf.Pow(targPosZ - transform.position.z, 2f) + Mathf.Pow(targPosX - transform.position.x, 2))) {
                    rb.AddForce(transform.forward * ((speed * 2) / ((Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z)) + 0.1f)), ForceMode.Force);

                    camPan.LookAt(new Vector3(targPosX, 0.5f, targPosZ));
                    camLift.LookAt(new Vector3(targPosX, 0f, targPosZ));

                    Debug.DrawLine(transform.position, new Vector3(targPosX, 0, targPosZ));
                    Debug.DrawLine(camLift.position, new Vector3(targPosX, 0, targPosZ));
                } else {
                    randomInt--;
                }

                break;
            case "Box":
                OriginalRot = transform.rotation;
                transform.LookAt(new Vector3(boxPos.x, 0.5f, boxPos.z));
                NewRot = transform.rotation;
                transform.rotation = OriginalRot;
                transform.rotation = Quaternion.Lerp(transform.rotation, NewRot, speed * 0.3f * Time.deltaTime);

                if (1f < Mathf.Sqrt(Mathf.Pow(boxPos.z - transform.position.z, 2f) + Mathf.Pow(boxPos.x - transform.position.x, 2)))
                {
                    pushingBox = true;
                }

                if (pushingBox)
                {
                    boxPushTimer--;
                }

                if (boxPushTimer > 0)
                {
                    rb.AddForce(transform.forward * ((speed * 2) / ((Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z)) + 0.1f)), ForceMode.Force);
                
                    camPan.LookAt(new Vector3(boxPos.x, 0.5f, boxPos.z));
                    camLift.LookAt(new Vector3(boxPos.x, 0f, boxPos.z));
                
                    Debug.DrawLine(transform.position, new Vector3(targPosX, 0, targPosZ));
                    Debug.DrawLine(camLift.position, new Vector3(targPosX, 0, targPosZ));
                }
                else
                {
                    dozerState = "Idle";
                    pushingBox = false;
                    boxPushTimer = 300;
                    boxCooldownTimer = 200;
                    boxFinder.radius = 0.1f;
                }
                break;
        }

        float movement;
        movement = Mathf.Sqrt(Mathf.Pow(lastPos.z - transform.position.z, 2f) + Mathf.Pow(lastPos.x - transform.position.x, 2));
        float turnDist;
        turnDist = transform.eulerAngles.y - lastRot.y;

        wheelLF.Rotate(new Vector3(movement * 200, 0, 0));
        wheelLR.Rotate(new Vector3(movement * 200, 0, 0));
        wheelRF.Rotate(new Vector3(movement * 200, 0, 0));
        wheelRR.Rotate(new Vector3(movement * 200, 0, 0));

        wheelLF.Rotate(new Vector3(turnDist * 2, 0, 0));
        wheelLR.Rotate(new Vector3(turnDist * 2, 0, 0));
        wheelRF.Rotate(new Vector3(turnDist * -2, 0, 0));
        wheelRR.Rotate(new Vector3(turnDist * -2, 0, 0));

        lastPos = transform.position;
        lastRot = transform.eulerAngles;
        
        //print("The dozer had a " + waitWeight + "% chance to wait, a " + turnWeight + "% chance to turn, and a " + moveWeight + "% chance to move. It chose to " + dozerState);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Box"))
        {
            GameObject boxObj = other.gameObject;
            Transform boxTrans = boxObj.transform;
            boxPos = new Vector3(boxTrans.position.x, boxTrans.position.y, boxTrans.position.z);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Box"))
        {
            boxPos = new Vector3(0, 0, 0);
        }
    }
}
