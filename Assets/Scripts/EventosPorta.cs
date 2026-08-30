using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
public class EventosPorta : MonoBehaviour
{
 private bool isOpen = false;
 private HingeJoint hinge;
 public TeleportationArea teleporte;
 void Start()
 {
 hinge = GetComponent<HingeJoint>(); 
 }
 void Update()
 {
 float angle = hinge.angle;

 // abriu
 if (!isOpen && angle == -120)
 {
 isOpen = true;
 teleporte.enabled = true;
 } else
 {
 // Porta fechou
 // por causa da precisao do float, tive que testar com 1 grau a menos
 if (isOpen && angle > -119)
 {
 isOpen = false;
 teleporte.enabled = false;
 }
 }
 }
} 
