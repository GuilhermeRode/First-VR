using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HeadCollisionDetector : MonoBehaviour
{
 [SerializeField, Range(0, 0.5f)]
 private float _detectionDelay = 0.05f;
 [SerializeField]
 private float _detectionDistance = 0.2f;
 [SerializeField]
 private LayerMask _detectionLayers;
 public List<RaycastHit> DetectedColliderHits { get; private set; }
 private float _currentTime = 0;
 private List<RaycastHit> PreformDetection
 (Vector3 position, float distance, LayerMask mask)
 {
 List<RaycastHit> detectedHits = new();

 // flat directions, so looking down still detects the wall
 Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
 if (forward.sqrMagnitude < 0.0001f)
 forward = Vector3.ProjectOnPlane(transform.up, Vector3.up);
 forward.Normalize();
 Vector3 right = Vector3.Cross(Vector3.up, forward);
 List<Vector3> directions
 = new() { forward, right, -right, -forward };
 RaycastHit hit;
 foreach (var dir in directions)
 {
 if (Physics.Raycast(position, dir, out hit, distance, mask))
 {
 detectedHits.Add(hit);
 }
 }
 return detectedHits;
 }
 private void Start()
 {
 DetectedColliderHits = PreformDetection(transform.position,
 _detectionDistance, _detectionLayers);
 }
 void Update()
 {
 _currentTime += Time.deltaTime;
 if (_currentTime > _detectionDelay)
 {
 _currentTime = 0;
 DetectedColliderHits = PreformDetection(transform.position,
 _detectionDistance, _detectionLayers);
 }
 }
}
