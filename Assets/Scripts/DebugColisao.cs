using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

/// <summary>
/// TEMPORARIO - so pra diagnosticar por que o jogador atravessa parede.
/// Poe no XR Origin, roda o Play, anda contra uma parede e olha o Console.
/// Pode apagar depois que o problema estiver resolvido.
/// </summary>
public class DebugColisao : MonoBehaviour
{
    [SerializeField] private XRBodyTransformer bodyTransformer;
    [SerializeField] private Transform cameraJogador;
    [SerializeField] private float intervalo = 1f;

    private CharacterController controlador;
    private float proximoLog;

    private void Start()
    {
        controlador = GetComponent<CharacterController>();

        if (bodyTransformer == null)
            bodyTransformer = GetComponentInChildren<XRBodyTransformer>();

        string manipulador = bodyTransformer == null
            ? "SEM XRBodyTransformer"
            : (bodyTransformer.constrainedBodyManipulator == null
                ? "NULO -> movimento NAO colide"
                : bodyTransformer.constrainedBodyManipulator.GetType().Name);

        Debug.Log($"[DebugColisao] ConstrainedBodyManipulator: {manipulador}");
        Debug.Log($"[DebugColisao] CharacterController: {(controlador == null ? "NAO EXISTE" : "ok")} " +
                  $"layer do rig = {LayerMask.LayerToName(gameObject.layer)} ({gameObject.layer})");
    }

    private void Update()
    {
        if (Time.time < proximoLog)
            return;
        proximoLog = Time.time + intervalo;

        if (controlador != null)
        {
            Debug.Log($"[DebugColisao] capsula: altura={controlador.height:F2} raio={controlador.radius:F2} " +
                      $"centro={controlador.center} enabled={controlador.enabled} " +
                      $"flags={controlador.collisionFlags} noChao={controlador.isGrounded}");
        }

        if (cameraJogador != null)
        {
            Vector3 frente = cameraJogador.forward;
            frente.y = 0f;

            if (Physics.Raycast(cameraJogador.position, frente.normalized, out RaycastHit hit, 3f))
            {
                Debug.Log($"[DebugColisao] na frente ({hit.distance:F2}m): {hit.collider.name} " +
                          $"layer={LayerMask.LayerToName(hit.collider.gameObject.layer)} " +
                          $"tipo={hit.collider.GetType().Name} trigger={hit.collider.isTrigger}");
            }
            else
            {
                Debug.Log("[DebugColisao] nada num raio de 3m na frente da camera");
            }
        }
    }
}
