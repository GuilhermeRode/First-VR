using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

// RN05: apertar tres vezes o botao do meio destranca a porta de correr.
public class PuzzleBotoes : MonoBehaviour
{
    [SerializeField] private XRSimpleInteractable botaoDoMeio;
    [SerializeField] private int pressoesNecessarias = 3;

    [Header("Ao resolver")]
    [SerializeField] private EventosPortaCorrer porta;
    [SerializeField] private Outline outlinePorta;
    [SerializeField] private AudioSource audioVitoria;
    [SerializeField] private float outlineWidthVitoria = 5f;

    private int pressoes = 0;
    private bool resolvido = false;

    private void Start()
    {
        if (outlinePorta != null)
            outlinePorta.OutlineWidth = 0f;

        if (botaoDoMeio != null)
            botaoDoMeio.selectEntered.AddListener(OnBotaoDoMeioPressionado);
    }

    private void OnBotaoDoMeioPressionado(SelectEnterEventArgs args)
    {
        if (resolvido)
            return;

        pressoes++;
        if (pressoes >= pressoesNecessarias)
            Resolver();
    }

    private void Resolver()
    {
        resolvido = true;

        if (outlinePorta != null)
            outlinePorta.OutlineWidth = outlineWidthVitoria;

        if (audioVitoria != null)
            audioVitoria.Play();

        if (porta != null)
            porta.Destrancar();
    }
}
