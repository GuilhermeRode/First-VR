using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// RN06 - Fim de jogo: dispara quando o jogador pisa (ou se teleporta) nesta area,
/// que fica fora do laboratorio, depois da segunda porta. Congela o jogo, tira o
/// movimento e chama a animacao de vitoria.
/// </summary>
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
        // a animacao roda em tempo nao escalado, por isso pode congelar antes
        if (animacaoVitoria != null)
            animacaoVitoria.Mostrar();

        Time.timeScale = 0;                 // pausar o jogo
        locomotionSystem.enabled = false;   // nao conseguir mais andar

        leftRay.enabled = false;            // desativa linha de teleporte do controle esquerdo
        rightRay.enabled = false;           // desativa linha de teleporte do controle direito
    }
}
