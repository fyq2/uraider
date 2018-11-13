﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGrabbing : StateBase<PlayerController>
{
    private float timeTracker = 0f;
    private float grabTime = 0f;

    private Vector3 grabPoint;
    private Vector3 startPosition;
    private Quaternion targetRot;
    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.MinimizeCollider();

        player.Anim.SetBool("isAutoGrabbing", true);
        player.ForceHeadLook = true;

        grabPoint = new Vector3(ledgeDetector.GrabPoint.x - (player.transform.forward.x * player.grabForwardOffset),
                        ledgeDetector.GrabPoint.y - player.grabUpOffset,
                        ledgeDetector.GrabPoint.z - (player.transform.forward.z * player.grabForwardOffset));

        Vector3 calcGrabPoint = ledgeDetector.GrabPoint - player.transform.forward * 0f
            - 1.8f * Vector3.up;

        targetRot = Quaternion.LookRotation(ledgeDetector.Direction); 

        startPosition = player.transform.position;

        player.Velocity = UMath.VelocityToReachPoint(player.transform.position,
            calcGrabPoint,
            4.3f,
            player.gravity,
            out grabTime);

        timeTracker = Time.time;
    }

    public override void OnExit(PlayerController player)
    {
        player.MaximizeCollider();

        player.Anim.SetBool("isAutoGrabbing", false);
        player.ForceHeadLook = false;
    }

    public override void Update(PlayerController player)
    {
        player.ApplyGravity(player.gravity);

        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRot, Time.deltaTime);

        player.HeadLookAt = ledgeDetector.GrabPoint;

        if (Time.time - timeTracker >= grabTime)
        {
            player.transform.position = grabPoint;
            player.transform.rotation = targetRot;

            if (ledgeDetector.WallType == LedgeType.Free)
                player.StateMachine.GoToState<Freeclimb>();
            else
                player.StateMachine.GoToState<Climbing>();
        }
    }
}