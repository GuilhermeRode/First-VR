using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// RN05 - Puzzle da segunda sala: a sala tem tres botoes e a porta de correr.
/// Basta apertar tres vezes o botao do meio pra resolver. Ao resolver: acende o
/// Outline na porta, toca o som de vitoria e destranca a porta (o Outline some
/// sozinho quando a porta abre, isso fica com o EventosPortaCorrer).
/// </summary>
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
        print("Botao do meio pressionado: " + pressoes + "/" + pressoesNecessarias);

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
