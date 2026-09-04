using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ShowMessageNaArea : MonoBehaviour
{
    public Transform playerCamera;
    public GameObject messageObject;
    public LocomotionMediator locomotionSystem;

    public XRRayInteractor leftRay;
    public XRRayInteractor rightRay;
    public float maxDistance = 3f;
    private bool triggered = false;

    void Update()
    {
        if (triggered) return;

        Ray ray = new Ray(playerCamera.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                messageObject.SetActive(true);
                Time.timeScale = 0; // pausar o jogo
                locomotionSystem.enabled = false; // nao conseguir mais andar

                leftRay.enabled = false; // desativa linha de teleporte do controle esquerdo
                rightRay.enabled = false; // desativa linha de teleporte do controle direito
                triggered = true;
            }
        }
    }
}