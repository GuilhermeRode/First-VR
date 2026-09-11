using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using System.Collections;

// Anexe este script no objeto "Dobradica" (o pivô vazio, não na folha da porta)
[RequireComponent(typeof(XRSimpleInteractable))]
public class DoorController : MonoBehaviour
{
    [Header("Configuração da Porta")]
    [Tooltip("Ângulo que a porta gira ao abrir, em graus")]
    public float anguloAberto = 90f;

    [Tooltip("Velocidade da animação de abrir/fechar")]
    public float velocidadeAbertura = 2f;

    [Tooltip("Área de teleporte do outro lado da porta, habilitada só quando a porta está aberta")]
    public TeleportationArea teleporte;

    private bool aberta = false;
    private Quaternion rotacaoFechada;
    private Quaternion rotacaoAberta;
    private Coroutine rotacionando;

    void Start()
    {
        rotacaoFechada = transform.localRotation;
        rotacaoAberta = rotacaoFechada * Quaternion.Euler(0f, anguloAberto, 0f);

        XRSimpleInteractable interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(_ => AlternarPorta());
    }

    void AlternarPorta()
    {
        aberta = !aberta;

        // começou a fechar: trava o teleporte na hora, não espera a porta terminar de fechar
        if (!aberta && teleporte != null)
            teleporte.enabled = false;

        if (rotacionando != null)
            StopCoroutine(rotacionando);

        rotacionando = StartCoroutine(RotacionarPorta(aberta ? rotacaoAberta : rotacaoFechada));
    }

    IEnumerator RotacionarPorta(Quaternion alvo)
    {
        while (Quaternion.Angle(transform.localRotation, alvo) > 0.5f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, alvo, Time.deltaTime * velocidadeAbertura);
            yield return null;
        }
        transform.localRotation = alvo;

        // só libera o teleporte quando a porta termina de abrir de verdade
        if (aberta && teleporte != null)
            teleporte.enabled = true;
    }
}
