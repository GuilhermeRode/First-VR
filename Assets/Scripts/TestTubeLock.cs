using UnityEngine;

public class TestTubeLock : MonoBehaviour
{
 [SerializeField]
 private Transform rackTopLimit; // ponto acima do rack
 private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
 private Rigidbody rb;
 private bool isLockedInRack = true;
 void Start()
 {
 grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
 rb = GetComponent<Rigidbody>();
 LockInRack();
 }
 void Update()
 {
 if (isLockedInRack)
 {
 CheckIfReleased();
 }
 }
 void LockInRack()
 {
 rb.constraints = RigidbodyConstraints.FreezeRotation
 | RigidbodyConstraints.FreezePositionX
 | RigidbodyConstraints.FreezePositionZ;
 }
 void UnlockFromRack()
 {
 rb.constraints = RigidbodyConstraints.None;
 isLockedInRack = false;
 } 
 void CheckIfReleased()
 {
 if (transform.position.y > rackTopLimit.position.y)
 {
 UnlockFromRack();
 }
 }
} 