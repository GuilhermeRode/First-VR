using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RN01/RN02 - Representa o líquido dentro de um recipiente (tubo de ensaio ou copo de béquer).
/// Controla o nível atual e ajusta a escala/posição em Y do mesh do líquido para
/// representar visualmente o quanto tem dentro, respeitando uma capacidade máxima.
/// Também controla a cor do líquido: cada tubo tem a sua; o béquer mistura as cores
/// dos tubos que já derramaram nele (média simples, sem proporção de volume).
/// </summary>
public class RecipienteLiquido : MonoBehaviour
{
    [Header("Visual do líquido")]
    [SerializeField] private Transform liquido; // mesh que representa o líquido (ex.: "... water")

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

    /// <summary>Tenta retirar "quantidade" do recipiente. Retorna quanto foi realmente retirado.</summary>
    public float Retirar(float quantidade)
    {
        if (quantidade <= 0f) return 0f;

        float retirado = Mathf.Min(quantidade, volumeAtual);
        volumeAtual -= retirado;
        AtualizarVisual();
        return retirado;
    }

    /// <summary>Tenta adicionar "quantidade" ao recipiente. Retorna quanto foi realmente adicionado.</summary>
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

    /// <summary>
    /// RN02 - Registra que "origem" derramou (pelo menos um pouco) neste recipiente.
    /// Na primeira vez que cada tubo derrama, a cor é recalculada como a média simples
    /// das cores de todos os tubos que já derramaram até agora (sem pesar por volume).
    /// </summary>
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

        // seta direto na propriedade do URP Lit (_BaseColor) e também na legada (_Color),
        // porque Renderer.material.color pode não bater na que o shader realmente usa
        Material mat = rendererLiquido.material;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", cor);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", cor);

        // esses materiais de líquido vêm com uma emissão colorida própria do asset
        // (ex.: verde, magenta) que ofusca a cor que a gente seta acima. Zera pra a
        // cor do líquido ser só a que definimos.
        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", Color.black);
    }
}
