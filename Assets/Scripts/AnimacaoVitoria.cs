using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// RN06: painel de vitoria com confete. Usa tempo nao escalado porque o jogo
// fica pausado (timeScale = 0) depois que o jogador vence.
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
        new Color(1f, 0.84f, 0.1f),
        new Color(0.15f, 0.85f, 0.9f),
        new Color(0.95f, 0.3f, 0.55f),
        new Color(0.4f, 0.9f, 0.4f),
        new Color(1f, 1f, 1f),
    };

    private readonly List<Transform> confetes = new List<Transform>();
    private readonly List<Vector3> quedas = new List<Vector3>();
    private readonly List<Vector3> giros = new List<Vector3>();
    private Transform raizDoConfete;

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

        // entrada
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

        // repouso
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

    private void AnimarLetras(float progressoDaEntrada, float tempo)
    {
        texto.ForceMeshUpdate();
        TMP_TextInfo info = texto.textInfo;

        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo caractere = info.characterInfo[i];
            if (!caractere.isVisible)
                continue;

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

            Vector3 queda = quedas[i];
            float balanco = Mathf.Sin(Time.unscaledTime * 2f + i) * 0.12f;

            pedaco.localPosition += new Vector3(queda.x + balanco, queda.y, queda.z) * dt;
            pedaco.Rotate(giros[i] * dt, Space.Self);

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

    // sobe ate exageroDaEscala e volta para 1
    private float EscalaComExagero(float p)
    {
        if (p < 0.6f)
        {
            float q = p / 0.6f;
            return Mathf.Lerp(0f, exageroDaEscala, 1f - (1f - q) * (1f - q));
        }

        float r = (p - 0.6f) / 0.4f;
        return Mathf.Lerp(exageroDaEscala, 1f, r * r * (3f - 2f * r));
    }
}
