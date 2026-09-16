using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

// Porta de correr da segunda sala: so abre depois do puzzle dos botoes (RN05)
// e, aberta, libera o teleporte para fora do laboratorio (RN06).
public class EventosPortaCorrer : MonoBehaviour
{
    public TeleportationArea teleporte;
    public XRGrabInteractable pegarPorta;
    public Outline outlinePorta;
    public bool comecaTrancada = true;
    public float aberturaParaAbrir = 0.6f;
    public string layerDasParedes = "Obsctaculos";

    private ConfigurableJoint joint;
    private bool trancada;
    private bool aberta;

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

    // A porta corre no mesmo plano da parede; sem isso ela trava encostada nela.
    private void IgnorarColisaoComAsParedes()
    {
        int layerParede = LayerMask.NameToLayer(layerDasParedes);
        if (layerParede < 0)
            return;

        Collider[] colidersDaPorta = GetComponentsInChildren<Collider>();
        foreach (Collider parede in FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (parede.gameObject.layer != layerParede || parede.transform.IsChildOf(transform))
                continue;

            foreach (Collider colider in colidersDaPorta)
                Physics.IgnoreCollision(colider, parede);
        }
    }

    private void Trancar()
    {
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

    private float Abertura()
    {
        Vector3 ancora = joint.transform.TransformPoint(joint.anchor);
        Vector3 eixo = joint.transform.TransformDirection(Vector3.right);
        return Mathf.Abs(Vector3.Dot(ancora - joint.connectedAnchor, eixo));
    }

    void Update()
    {
        if (trancada)
            return;

        bool passouDoPonto = Abertura() >= aberturaParaAbrir;
        if (passouDoPonto == aberta)
            return;

        aberta = passouDoPonto;
        if (teleporte != null)
            teleporte.enabled = aberta;

        if (aberta && outlinePorta != null)
            outlinePorta.OutlineWidth = 0f;
    }
}
