using UnityEngine;

/// <summary>
/// RN01 - Representa o líquido dentro de um recipiente (tubo de ensaio ou copo de béquer).
/// Controla o nível atual e ajusta a escala/posição em Y do mesh do líquido para
/// representar visualmente o quanto tem dentro, respeitando uma capacidade máxima.
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

    private float volumeAtual;
    private float posYCheio;

    public float VolumeAtual => volumeAtual;
    public float CapacidadeMaxima => capacidadeMaxima;
    public float Fracao => capacidadeMaxima > 0f ? Mathf.Clamp01(volumeAtual / capacidadeMaxima) : 0f;
    public bool TemLiquido => volumeAtual > 0.0001f;
    public float EspacoDisponivel => Mathf.Max(0f, capacidadeMaxima - volumeAtual);

    private void Awake()
    {
        if (liquido != null)
            posYCheio = liquido.localPosition.y;

        volumeAtual = Mathf.Clamp(volumeInicial, 0f, capacidadeMaxima);
        AtualizarVisual();
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
}
