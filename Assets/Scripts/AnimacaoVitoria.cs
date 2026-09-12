using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// RN06 - Animacao de vitoria, disparada quando o jogador sai do laboratorio.
/// O painel cai na frente do jogador girando e estourando a escala, as letras
/// despencam uma a uma e depois ficam ondulando, e cai uma chuva de confete
/// em volta. Tudo em tempo nao escalado, porque o jogo congela (timeScale = 0)
/// assim que o jogador vence.
/// </summary>
public class AnimacaoVitoria : MonoBehaviour
{
    [SerializeField] private Transform painel;
    [SerializeField] private CanvasGroup grupo;
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private AudioSource audioVitoria;
    [SerializeField] private Transform cameraJogador;

    [Header("Posicao na frente do jogador")]
    [SerializeField] private float distanciaDaCamera = 2f;
    [SerializeField] private float alturaDosOlhos = -0.15f;

    [Header("Tempos")]
    [SerializeField] private float duracaoEntrada = 0.7f;
    [SerializeField] private float exageroDaEscala = 1.18f;
    [SerializeField] private float giroDeEntrada = 12f;

    [Header("Brilho do texto")]
    [SerializeField] private Color corInicial = new Color(1f, 1f, 1f);
    [SerializeField] private Color corDourada = new Color(1f, 0.84f, 0.1f);

    [Header("Confete")]
    [SerializeField] private int quantidadeDeConfete = 70;
    [SerializeField] private float larguraDaChuva = 3.2f;
    [SerializeField] private float alturaDaChuva = 2.6f;
    [SerializeField] private float tamanhoDoConfete = 0.045f;

    private static readonly Color[] coresDoConfete =
    {
        new Color(1f, 0.84f, 0.1f),    // dourado
        new Color(0.15f, 0.85f, 0.9f), // ciano (a cor do Br)
        new Color(0.95f, 0.3f, 0.55f), // rosa
        new Color(0.4f, 0.9f, 0.4f),   // verde
        new Color(1f, 1f, 1f),         // branco
    };

    private readonly List<Transform> confetes = new List<Transform>();
    private readonly List<Vector3> quedas = new List<Vector3>();
    private readonly List<Vector3> giros = new List<Vector3>();
    private Transform raizDoConfete;

    // o painel comeca desativado na cena: quem liga e o Mostrar()
    public void Mostrar()
    {
        gameObject.SetActive(true);
        PosicionarNaFrenteDoJogador();

        if (audioVitoria != null)
            audioVitoria.Play();

        CriarConfete();
        StartCoroutine(Animar());
    }

    private void PosicionarNaFrenteDoJogador()
    {
        if (cameraJogador == null)
            return;

        // so a direcao horizontal, pra o painel nao nascer torto se o jogador
        // estiver olhando pra cima ou pra baixo
        Vector3 frente = cameraJogador.forward;
        frente.y = 0f;
        if (frente.sqrMagnitude < 0.001f)
            frente = Vector3.forward;
        frente.Normalize();

        transform.position = cameraJogador.position + frente * distanciaDaCamera
                             + Vector3.up * alturaDosOlhos;
        transform.rotation = Quaternion.LookRotation(frente, Vector3.up);
    }

    private IEnumerator Animar()
    {
        Vector3 escalaFinal = painel != null ? painel.localScale : Vector3.one;
        Quaternion giroFinal = transform.rotation;
        Quaternion giroTorto = giroFinal * Quaternion.Euler(0f, 0f, giroDeEntrada);

        // --- entrada: painel desentorta e estoura a escala, letras despencam ---
        float t = 0f;
        while (t < duracaoEntrada)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duracaoEntrada);

            if (painel != null)
                painel.localScale = escalaFinal * EscalaComExagero(p);

            transform.rotation = Quaternion.Slerp(giroTorto, giroFinal, p * p);

            if (grupo != null)
                grupo.alpha = Mathf.Clamp01(p * 2f);

            if (texto != null)
            {
                texto.color = Color.Lerp(corInicial, corDourada, p);
                AnimarLetras(p, Time.unscaledTime);
            }

            AtualizarConfete();
            yield return null;
        }

        if (painel != null)
            painel.localScale = escalaFinal;
        if (grupo != null)
            grupo.alpha = 1f;
        transform.rotation = giroFinal;

        // --- repouso: o painel flutua, as letras ondulam, o confete continua caindo ---
        Vector3 posicaoBase = transform.position;
        while (true)
        {
            float tempo = Time.unscaledTime;

            transform.position = posicaoBase + Vector3.up * (Mathf.Sin(tempo * 1.6f) * 0.04f);

            if (painel != null)
                painel.localScale = escalaFinal * (1f + Mathf.Sin(tempo * 2.2f) * 0.025f);

            if (texto != null)
            {
                texto.color = Color.Lerp(corDourada, corInicial,
                                         Mathf.Abs(Mathf.Sin(tempo * 1.1f)) * 0.45f);
                AnimarLetras(1f, tempo);
            }

            AtualizarConfete();
            yield return null;
        }
    }

    /// <summary>
    /// Mexe direto nos vertices do TMP: na entrada cada letra cai do alto com um
    /// atraso propria, e depois todas ficam ondulando em onda senoidal.
    /// </summary>
    private void AnimarLetras(float progressoDaEntrada, float tempo)
    {
        texto.ForceMeshUpdate();
        TMP_TextInfo info = texto.textInfo;

        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo caractere = info.characterInfo[i];
            if (!caractere.isVisible)
                continue;

            // cada letra entra um pouquinho depois da anterior
            float atraso = i * 0.025f;
            float entradaDaLetra = Mathf.Clamp01((progressoDaEntrada - atraso) / 0.35f);
            float queda = (1f - entradaDaLetra) * (1f - entradaDaLetra) * 90f;

            float onda = Mathf.Sin(tempo * 3f + i * 0.35f) * 4f * entradaDaLetra;
            Vector3 deslocamento = new Vector3(0f, queda + onda, 0f);

            int vertice = caractere.vertexIndex;
            int material = caractere.materialReferenceIndex;
            Vector3[] vertices = info.meshInfo[material].vertices;

            for (int v = 0; v < 4; v++)
                vertices[vertice + v] += deslocamento;
        }

        for (int m = 0; m < info.meshInfo.Length; m++)
        {
            info.meshInfo[m].mesh.vertices = info.meshInfo[m].vertices;
            texto.UpdateGeometry(info.meshInfo[m].mesh, m);
        }
    }

    private void CriarConfete()
    {
        if (raizDoConfete != null)
            return;

        // fora do Canvas, senao herdaria a escala minuscula dele
        raizDoConfete = new GameObject("Confetes").transform;
        raizDoConfete.position = transform.position;
        raizDoConfete.rotation = transform.rotation;

        for (int i = 0; i < quantidadeDeConfete; i++)
        {
            GameObject pedaco = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(pedaco.GetComponent<Collider>());

            pedaco.transform.SetParent(raizDoConfete, false);
            pedaco.transform.localPosition = PosicaoSorteada(true);
            pedaco.transform.localRotation = Random.rotation;
            // achatado, pra parecer papelzinho e nao cubo
            pedaco.transform.localScale = new Vector3(tamanhoDoConfete,
                                                      tamanhoDoConfete * 1.6f,
                                                      tamanhoDoConfete * 0.15f);

            PintarConfete(pedaco.GetComponent<Renderer>(),
                          coresDoConfete[Random.Range(0, coresDoConfete.Length)]);

            confetes.Add(pedaco.transform);
            quedas.Add(new Vector3(Random.Range(-0.15f, 0.15f),
                                   Random.Range(-0.9f, -0.45f),
                                   Random.Range(-0.1f, 0.1f)));
            giros.Add(new Vector3(Random.Range(-180f, 180f),
                                  Random.Range(-180f, 180f),
                                  Random.Range(-180f, 180f)));
        }
    }

    private Vector3 PosicaoSorteada(bool espalhadoNaAltura)
    {
        float altura = espalhadoNaAltura
            ? Random.Range(-alturaDaChuva * 0.5f, alturaDaChuva)
            : alturaDaChuva;

        return new Vector3(Random.Range(-larguraDaChuva * 0.5f, larguraDaChuva * 0.5f),
                           altura,
                           Random.Range(-0.4f, 0.4f));
    }

    private void PintarConfete(Renderer rendererConfete, Color cor)
    {
        if (rendererConfete == null)
            return;

        // seta direto em _BaseColor/_Color: no URP o .color nem sempre bate na
        // propriedade que o shader usa
        Material material = rendererConfete.material;
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", cor);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", cor);
    }

    private void AtualizarConfete()
    {
        float dt = Time.unscaledDeltaTime;

        for (int i = 0; i < confetes.Count; i++)
        {
            Transform pedaco = confetes[i];
            if (pedaco == null)
                continue;

            // balanca de lado enquanto cai, pra nao descer em linha reta
            Vector3 queda = quedas[i];
            float balanco = Mathf.Sin(Time.unscaledTime * 2f + i) * 0.12f;

            pedaco.localPosition += new Vector3(queda.x + balanco, queda.y, queda.z) * dt;
            pedaco.Rotate(giros[i] * dt, Space.Self);

            // chegou embaixo: volta pro alto
            if (pedaco.localPosition.y < -alturaDaChuva * 0.6f)
                pedaco.localPosition = PosicaoSorteada(false);
        }
    }

    private void OnDisable()
    {
        if (raizDoConfete != null)
            Destroy(raizDoConfete.gameObject);

        confetes.Clear();
        quedas.Clear();
        giros.Clear();
        raizDoConfete = null;
    }

    /// <summary>
    /// Curva de "pop": sobe passando do ponto (exageroDaEscala) e volta pro 1.
    /// </summary>
    private float EscalaComExagero(float p)
    {
        if (p < 0.6f)
        {
            // 0 -> exagero, desacelerando
            float q = p / 0.6f;
            return Mathf.Lerp(0f, exageroDaEscala, 1f - (1f - q) * (1f - q));
        }

        // exagero -> 1, assentando
        float r = (p - 0.6f) / 0.4f;
        return Mathf.Lerp(exageroDaEscala, 1f, r * r * (3f - 2f * r));
    }
}
