using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

// RN04: encaixar a peça do Bromo no quadro completa "BREAKING BAD" e destranca a porta.
public class PuzzleBromo : MonoBehaviour
{
    [SerializeField] private XRSocketInteractor soquete;
    [SerializeField] private XRGrabInteractable pecaDoBromo;
    [SerializeField] private TextMeshPro textoQuadro;
    [SerializeField] private string textoIncompleto = "EAKING\nBAD";
    [SerializeField] private string textoCompleto = "BREAKING\nBAD";

    [Header("Ao resolver")]
    [SerializeField] private DoorController porta;
    [SerializeField] private Outline outlinePorta;
    [SerializeField] private AudioSource audioVitoria;
    [SerializeField] private Color corDaPecaFixada = new Color(0.09f, 0.2f, 0.15f);
    [SerializeField] private GameObject marcadorEncaixe;

    private bool resolvido;

    private void Start()
    {
        textoQuadro.text = textoIncompleto;
        soquete.selectEntered.AddListener(OnPecaEncaixada);
    }

    private void OnPecaEncaixada(SelectEnterEventArgs args)
    {
        // só a peça do bromo resolve: qualquer outra coisa encaixada é ignorada
        if (resolvido || args.interactableObject as XRGrabInteractable != pecaDoBromo)
            return;

        Resolver();
    }

    private void Resolver()
    {
        resolvido = true;
        textoQuadro.text = textoCompleto;
        marcadorEncaixe.SetActive(false);

        pecaDoBromo.enabled = false;
        FixarCorDaPeca();
        EsconderEscritaDaPeca();

        outlinePorta.OutlineWidth = 5f;
        audioVitoria.Play();
        porta.Destrancar();
    }

    private void EsconderEscritaDaPeca()
    {
        foreach (TextMeshPro texto in pecaDoBromo.GetComponentsInChildren<TextMeshPro>(true))
            texto.gameObject.SetActive(false);
    }

    private void FixarCorDaPeca()
    {
        pecaDoBromo.GetComponent<Renderer>().material.SetColor("_BaseColor", corDaPecaFixada);
    }
}
