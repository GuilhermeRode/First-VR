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

    [Header("RN04 - trava do puzzle")]
    [Tooltip("Outline da porta: some quando ela termina de abrir")]
    public Outline outlinePorta;

    [Tooltip("Se marcado, a porta começa trancada e só abre depois de PuzzleAmostra chamar Destrancar()")]
    public bool comecaTrancada = true;

    private bool aberta = false;
    private bool trancada;
    private Quaternion rotacaoFechada;
    private Quaternion rotacaoAberta;
    private Coroutine rotacionando;

    void Start()
    {
        trancada = comecaTrancada;

        rotacaoFechada = transform.localRotation;
        rotacaoAberta = rotacaoFechada * Quaternion.Euler(0f, anguloAberto, 0f);

        XRSimpleInteractable interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(_ => AlternarPorta());
    }

    public void Destrancar()
    {
        trancada = false;
    }

    void AlternarPorta()
    {
        if (trancada)
            return;

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

        // RN04 - o Outline de vitória some quando a porta termina de abrir
        if (aberta && outlinePorta != null)
            outlinePorta.OutlineWidth = 0f;
    }
}
