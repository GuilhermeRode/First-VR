using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// RN06: o jogo termina quando o jogador chega nesta area, fora do laboratorio.
public class ShowMessageNaArea : MonoBehaviour
{
    public Transform playerCamera;
    public AnimacaoVitoria animacaoVitoria;
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
        Debug.DrawRay(playerCamera.position, Vector3.down * maxDistance, Color.red);
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                triggered = true;
                Vencer();
            }
        }
    }

    private void Vencer()
    {
        animacaoVitoria.Mostrar();

        Time.timeScale = 0;
        locomotionSystem.enabled = false;
        leftRay.enabled = false;
        rightRay.enabled = false;
    }
}
