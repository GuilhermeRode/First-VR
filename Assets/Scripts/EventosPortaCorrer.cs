using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

/// <summary>
/// Porta de correr da segunda sala. O deslize e fisico: o XRGrabInteractable
/// empurra o Rigidbody e o ConfigurableJoint segura a porta no trilho.
/// RN05: so pode ser aberta depois do puzzle dos botoes.
/// RN06: quando abre, libera a area de teleporte pra fora do laboratorio.
/// </summary>
public class EventosPortaCorrer : MonoBehaviour
{
    private bool isOpen = false;
    private ConfigurableJoint joint;
    public TeleportationArea teleporte;

    [Header("RN05 - so abre depois do puzzle dos botoes")]
    [Tooltip("Interactable de arrastar a porta: fica desligado enquanto ela esta trancada")]
    public XRGrabInteractable pegarPorta;
    [Tooltip("Outline que acende quando o puzzle e resolvido e some quando a porta abre")]
    public Outline outlinePorta;
    public bool comecaTrancada = true;

    [Tooltip("A partir de quanto de deslize a porta conta como aberta")]
    public float aberturaParaAbrir = 0.6f;

    [Tooltip("Layer das paredes. A porta corre dentro do plano da parede, entao os colliders " +
             "dela precisam ignorar os da parede, senao a porta trava")]
    public string layerDasParedes = "Obsctaculos";

    private bool trancada;

    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();

        if (pegarPorta == null)
            pegarPorta = GetComponent<XRGrabInteractable>();

        if (outlinePorta != null)
            outlinePorta.OutlineWidth = 0f;

        IgnorarColisaoComAsParedes();

        trancada = comecaTrancada;
        if (trancada)
            Trancar();
    }

    /// <summary>
    /// A porta de celeiro corre dentro do mesmo plano da parede (mesma faixa de X),
    /// entao o collider do painel encostaria na parede o tempo todo e a porta ficaria
    /// travada. Aqui os colliders dela sao marcados pra ignorar os das paredes - ela
    /// continua barrando o jogador, que esta em outro layer.
    /// </summary>
    private void IgnorarColisaoComAsParedes()
    {
        int layerParede = LayerMask.NameToLayer(layerDasParedes);
        if (layerParede < 0)
        {
            Debug.LogWarning($"Layer '{layerDasParedes}' nao existe: a porta pode travar na parede.", this);
            return;
        }

        Collider[] colidersDaPorta = GetComponentsInChildren<Collider>(true);
        Collider[] todos = FindObjectsByType<Collider>(FindObjectsSortMode.None);

        foreach (Collider outro in todos)
        {
            if (outro.gameObject.layer != layerParede)
                continue;

            foreach (Collider meu in colidersDaPorta)
                Physics.IgnoreCollision(meu, outro, true);
        }
    }

    private void Trancar()
    {
        // alem de tirar o grab, trava o eixo do joint pra porta nao ser empurrada
        if (pegarPorta != null)
            pegarPorta.enabled = false;
        if (joint != null)
            joint.xMotion = ConfigurableJointMotion.Locked;
    }

    public void Destrancar()
    {
        trancada = false;
        if (pegarPorta != null)
            pegarPorta.enabled = true;
        if (joint != null)
            joint.xMotion = ConfigurableJointMotion.Limited;
    }

    float GetJointLinearX()
    {
        // Calcula posicao do anchor no mundo
        Vector3 worldAnchor = joint.transform.TransformPoint(joint.anchor);
        Vector3 connectedAnchor = joint.connectedAnchor;
        // Delta entre anchors
        Vector3 delta = worldAnchor - connectedAnchor;
        // Eixo X do joint no espaco global
        Vector3 axisX = joint.transform.TransformDirection(Vector3.right);
        // Projecao do deslocamento no eixo X
        float displacementX = Vector3.Dot(delta, axisX);
        return displacementX;
    }

    void Update()
    {
        if (trancada)
            return;

        float abertura = Mathf.Abs(GetJointLinearX());

        // abriu
        if (!isOpen && abertura >= aberturaParaAbrir)
        {
            isOpen = true;

            if (teleporte != null)
                teleporte.enabled = true;

            // RN05: o Outline some assim que a porta abre
            if (outlinePorta != null)
                outlinePorta.OutlineWidth = 0f;
        }
        else
        {
            // Porta fechou
            if (isOpen && abertura < aberturaParaAbrir)
            {
                isOpen = false;

                if (teleporte != null)
                    teleporte.enabled = false;
            }
        }
    }
}
