using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// RN06: painel de vitória com confete. Usa tempo não escalado porque o jogo
// fica pausado (timeScale = 0) depois que o jogador vence.
public class AnimacaoVitoria : MonoBehaviour
{
    [SerializeField] private Transform painel;
    [SerializeField] private CanvasGroup grupo;
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private AudioSource audioVitoria;
    [SerializeField] private Transform cameraJogador;

    [Header("Posição na frente do jogador")]
    [SerializeField] private float distanciaDaCamera = 2f;
    [SerializeField] private float alturaDosOlhos = -0.15f;

    [Header("Entrada")]
    [SerializeField] private float duracaoEntrada = 0.7f;
    [SerializeField] private float exageroDaEscala = 1.18f;

    [Header("Cores do texto")]
    [SerializeField] private Color corInicial = Color.white;
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
        Color.white,
    };

    private readonly List<Transform> confetes = new List<Transform>();
    private readonly List<Vector3> quedas = new List<Vector3>();
    private readonly List<Vector3> giros = new List<Vector3>();
    private Transform raizDoConfete;

    public void Mostrar()
    {
        gameObject.SetActive(true);
        PosicionarNaFrenteDoJogador();
        audioVitoria.Play();
        CriarConfete();
        StartCoroutine(Animar());
    }

    private void PosicionarNaFrenteDoJogador()
    {
        Vector3 frente = cameraJogador.forward;
        frente.y = 0f;
        frente.Normalize();

        transform.position = cameraJogador.position + frente * distanciaDaCamera
                             + Vector3.up * alturaDosOlhos;
        transform.rotation = Quaternion.LookRotation(frente, Vector3.up);
    }

    private IEnumerator Animar()
    {
        Vector3 escalaFinal = painel.localScale;

        float t = 0f;
        while (t < duracaoEntrada)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duracaoEntrada);

            painel.localScale = escalaFinal * EscalaComExagero(p);
            grupo.alpha = Mathf.Clamp01(p * 2f);
            texto.color = Color.Lerp(corInicial, corDourada, p);

            AtualizarConfete();
            yield return null;
        }

        painel.localScale = escalaFinal;
        grupo.alpha = 1f;

        Vector3 posicaoBase = transform.position;
        while (true)
        {
            float tempo = Time.unscaledTime;

            transform.position = posicaoBase + Vector3.up * (Mathf.Sin(tempo * 1.6f) * 0.04f);
            painel.localScale = escalaFinal * (1f + Mathf.Sin(tempo * 2.2f) * 0.025f);
            texto.color = Color.Lerp(corDourada, corInicial, Mathf.Abs(Mathf.Sin(tempo * 1.1f)) * 0.45f);

            AtualizarConfete();
            yield return null;
        }
    }

    // sobe até exageroDaEscala e volta para 1
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

    private void CriarConfete()
    {
        if (raizDoConfete != null)
            return;

        // fora do Canvas, senão herdaria a escala minúscula dele
        raizDoConfete = new GameObject("Confetes").transform;
        raizDoConfete.position = transform.position;
        raizDoConfete.rotation = transform.rotation;

        Material[] materiais = MateriaisDoConfete();

        for (int i = 0; i < quantidadeDeConfete; i++)
        {
            GameObject pedaco = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(pedaco.GetComponent<Collider>());
            pedaco.GetComponent<Renderer>().sharedMaterial = materiais[Random.Range(0, materiais.Length)];

            pedaco.transform.SetParent(raizDoConfete, false);
            pedaco.transform.localPosition = SorteioNaChuva(-alturaDaChuva * 0.5f);
            pedaco.transform.localRotation = Random.rotation;
            pedaco.transform.localScale = new Vector3(tamanhoDoConfete,
                                                      tamanhoDoConfete * 1.6f,
                                                      tamanhoDoConfete * 0.15f);

            confetes.Add(pedaco.transform);
            quedas.Add(new Vector3(Random.Range(-0.15f, 0.15f),
                                   Random.Range(-0.9f, -0.45f),
                                   Random.Range(-0.1f, 0.1f)));
            giros.Add(new Vector3(Random.Range(-180f, 180f),
                                  Random.Range(-180f, 180f),
                                  Random.Range(-180f, 180f)));
        }
    }

    // um material por cor, compartilhado: criar um por papelzinho engasga o Quest
    private Material[] MateriaisDoConfete()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        var materiais = new Material[coresDoConfete.Length];
        for (int i = 0; i < materiais.Length; i++)
        {
            materiais[i] = new Material(shader);
            materiais[i].SetColor("_BaseColor", coresDoConfete[i]);
        }
        return materiais;
    }

    private Vector3 SorteioNaChuva(float alturaMinima)
    {
        return new Vector3(Random.Range(-larguraDaChuva * 0.5f, larguraDaChuva * 0.5f),
                           Random.Range(alturaMinima, alturaDaChuva),
                           Random.Range(-0.4f, 0.4f));
    }

    private void AtualizarConfete()
    {
        float dt = Time.unscaledDeltaTime;

        for (int i = 0; i < confetes.Count; i++)
        {
            Vector3 queda = quedas[i];
            float balanco = Mathf.Sin(Time.unscaledTime * 2f + i) * 0.12f;

            confetes[i].localPosition += new Vector3(queda.x + balanco, queda.y, queda.z) * dt;
            confetes[i].Rotate(giros[i] * dt, Space.Self);

            // chegou embaixo: volta pro alto
            if (confetes[i].localPosition.y < -alturaDaChuva * 0.6f)
                confetes[i].localPosition = SorteioNaChuva(alturaDaChuva);
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
}
