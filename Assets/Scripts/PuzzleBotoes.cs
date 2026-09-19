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

    private int pressoes;
    private bool resolvido;

    private void Start()
    {
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
        outlinePorta.OutlineWidth = 5f;
        audioVitoria.Play();
        porta.Destrancar();
    }
}
