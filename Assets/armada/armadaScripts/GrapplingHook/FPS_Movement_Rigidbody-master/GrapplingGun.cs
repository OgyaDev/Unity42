using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrapplingGun : MonoBehaviour {

    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, camera, player;
    [SerializeField] float maxDistance = 100f;
    private SpringJoint joint;
    private Rigidbody playerRb;
    public float pullForce = 10f;

    void Awake() {
        lr = GetComponent<LineRenderer>();
        playerRb = player.GetComponent<Rigidbody>();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0)) {
            StopGrapple();
        }
    }

    void FixedUpdate() {
        if (joint != null) {
            ApplyPullForce();
        }
    }

    void LateUpdate() {
        DrawRope();
    }

    void StartGrapple() {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable)) {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 2f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }

    void StopGrapple() {
        lr.positionCount = 0;
        Destroy(joint);
    }

    private Vector3 currentGrapplePosition;

    void DrawRope() {
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);
        
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }

    void ApplyPullForce() {
        Vector3 directionToGrapplePoint = (grapplePoint - player.position).normalized;
        playerRb.AddForce(directionToGrapplePoint * pullForce, ForceMode.Acceleration);
    }

    public bool IsGrappling() {
        return joint != null;
    }

    public Vector3 GetGrapplePoint() {
        return grapplePoint;
    }
}

