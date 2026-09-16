using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

// RN04: encaixar a peça do Bromo no quadro completa "BREAKING BAD" e destranca a porta.
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

    [SerializeField] private Color corDaPecaFixada = new Color(0.09f, 0.2f, 0.15f);
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

        if (marcadorEncaixe != null)
            marcadorEncaixe.SetActive(false);

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

    private void EsconderEscritaDaPeca(GameObject objetoPeca)
    {
        foreach (var texto in objetoPeca.GetComponentsInChildren<TextMeshPro>(true))
            texto.gameObject.SetActive(false);
    }

    private void FixarCorDaPeca(Renderer rendererPeca)
    {
        if (rendererPeca == null) return;

        Material mat = rendererPeca.material;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", corDaPecaFixada);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", corDaPecaFixada);
    }
}
