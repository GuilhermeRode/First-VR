using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// RN04 - Puzzle "Br": o quadro mostra "__EAKING BAD" com um soquete no lugar do Br.
/// O jogador arrasta a peça do elemento Bromo (Br) e encaixa no soquete pra completar
/// "BREAKING BAD". Ao encaixar: acende o Outline na porta, toca som de vitória e destranca.
/// </summary>
public class PuzzleBromo : MonoBehaviour
{
    [SerializeField] private XRSocketInteractor soquete;
    [SerializeField] private TextMeshPro textoQuadro;
    [SerializeField] private string textoIncompleto = "__EAKING BAD";
    [SerializeField] private string textoCompleto = "BREAKING BAD";

    [Header("Ao resolver")]
    [SerializeField] private DoorController porta;
    [SerializeField] private Outline outlinePorta;
    [SerializeField] private AudioSource audioVitoria;
    [SerializeField] private float outlineWidthVitoria = 5f;

    [Tooltip("Quando encaixa, a peça muda pra essa cor (a mesma do fundo do quadro), como se fosse fixada nele")]
    [SerializeField] private Color corDaPecaFixada = new Color(0.09f, 0.2f, 0.15f);

    [Tooltip("Marcador visual do encaixe: some quando a peça é colocada, deixando só o quadro escrito")]
    [SerializeField] private GameObject marcadorEncaixe;

    private bool resolvido = false;

    private void Start()
    {
        if (textoQuadro != null)
            textoQuadro.text = textoIncompleto;

        if (soquete != null)
            soquete.selectEntered.AddListener(OnPecaEncaixada);
    }

    private void OnPecaEncaixada(SelectEnterEventArgs args)
    {
        if (resolvido)
            return;

        Resolver(args.interactableObject as XRGrabInteractable);
    }

    private void Resolver(XRGrabInteractable peca)
    {
        resolvido = true;

        if (textoQuadro != null)
            textoQuadro.text = textoCompleto;

        // esconde o marcador do encaixe, pra sobrar só o quadro com a palavra completa
        if (marcadorEncaixe != null)
            marcadorEncaixe.SetActive(false);

        // trava a peça encaixada, pra não dar pra puxar de volta depois de resolvido
        if (peca != null)
        {
            peca.enabled = false;
            FixarCorDaPeca(peca.GetComponent<Renderer>());
            EsconderEscritaDaPeca(peca.gameObject);
        }

        if (outlinePorta != null)
            outlinePorta.OutlineWidth = outlineWidthVitoria;

        if (audioVitoria != null)
            audioVitoria.Play();

        if (porta != null)
            porta.Destrancar();
    }

    // some com o "35 / Br / Bromo" da peça, pra sobrar só o quadrado na cor do quadro
    private void EsconderEscritaDaPeca(GameObject objetoPeca)
    {
        foreach (var texto in objetoPeca.GetComponentsInChildren<TextMeshPro>(true))
            texto.gameObject.SetActive(false);
    }

    private void FixarCorDaPeca(Renderer rendererPeca)
    {
        if (rendererPeca == null) return;

        // seta direto em _BaseColor/_Color, porque Renderer.material.color pode
        // não bater na propriedade que o shader URP realmente usa
        Material mat = rendererPeca.material;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", corDaPecaFixada);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", corDaPecaFixada);
    }
}
