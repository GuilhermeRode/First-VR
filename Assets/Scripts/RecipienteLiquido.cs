using System.Collections.Generic;
using UnityEngine;

// RN01/RN02: nivel e cor do liquido de um tubo de ensaio ou do bequer.
public class RecipienteLiquido : MonoBehaviour
{
    [Header("Visual do líquido")]
    [SerializeField] private Transform liquido;

    [Tooltip("Escala em Y do mesh do líquido quando está 100% cheio (nesses assets, 1 = cheio, 0 = vazio)")]
    [SerializeField] private float escalaYCheio = 1f;

    [Tooltip("0 = pivô do mesh na base (só escala), 1 = pivô no topo, 0.5 = pivô no centro")]
    [SerializeField, Range(0f, 1f)] private float pivoY = 0f;

    [Header("Capacidade")]
    [SerializeField] private float capacidadeMaxima = 100f;
    [SerializeField] private float volumeInicial = 100f;

    [Header("RN02 - cor do líquido")]
    [Tooltip("Cor do líquido deste tubo. Num béquer, é a cor atual (recalculada conforme os tubos derramam).")]
    [SerializeField] private Color cor = Color.white;

    private float volumeAtual;
    private float posYCheio;
    private Renderer rendererLiquido;
    private readonly List<RecipienteLiquido> origensMisturadas = new List<RecipienteLiquido>();

    public float VolumeAtual => volumeAtual;
    public float CapacidadeMaxima => capacidadeMaxima;
    public float Fracao => capacidadeMaxima > 0f ? Mathf.Clamp01(volumeAtual / capacidadeMaxima) : 0f;
    public bool TemLiquido => volumeAtual > 0.0001f;
    public float EspacoDisponivel => Mathf.Max(0f, capacidadeMaxima - volumeAtual);
    public Color Cor => cor;

    private void Awake()
    {
        if (liquido != null)
        {
            posYCheio = liquido.localPosition.y;
            rendererLiquido = liquido.GetComponent<Renderer>();
        }

        volumeAtual = Mathf.Clamp(volumeInicial, 0f, capacidadeMaxima);
        AtualizarVisual();
        AtualizarCor();
    }

    public float Retirar(float quantidade)
    {
        if (quantidade <= 0f) return 0f;

        float retirado = Mathf.Min(quantidade, volumeAtual);
        volumeAtual -= retirado;
        AtualizarVisual();
        return retirado;
    }

    public float Adicionar(float quantidade)
    {
        if (quantidade <= 0f) return 0f;

        float adicionado = Mathf.Min(quantidade, EspacoDisponivel);
        volumeAtual += adicionado;
        AtualizarVisual();
        return adicionado;
    }

    private void AtualizarVisual()
    {
        if (liquido == null) return;

        float novaEscalaY = escalaYCheio * Fracao;
        liquido.localScale = new Vector3(liquido.localScale.x, novaEscalaY, liquido.localScale.z);

        float deltaAltura = (escalaYCheio - novaEscalaY) * pivoY;
        liquido.localPosition = new Vector3(liquido.localPosition.x, posYCheio - deltaAltura, liquido.localPosition.z);
    }

    // RN02: a cor passa a ser a media das cores dos tubos que ja derramaram aqui
    public void RegistrarOrigem(RecipienteLiquido origem)
    {
        if (origem == null || origensMisturadas.Contains(origem))
            return;

        origensMisturadas.Add(origem);

        Color soma = new Color(0f, 0f, 0f, 0f);
        foreach (var o in origensMisturadas)
            soma += o.cor;
        int n = origensMisturadas.Count;
        cor = new Color(soma.r / n, soma.g / n, soma.b / n, 1f);

        AtualizarCor();
    }

    private void AtualizarCor()
    {
        if (rendererLiquido == null) return;

        Material mat = rendererLiquido.material;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", cor);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", cor);

        // os materiais do pack vem com emissao colorida
        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", Color.black);
    }
}
