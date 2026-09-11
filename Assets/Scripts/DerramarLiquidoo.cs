using UnityEngine;
public class DerramarLiquido : MonoBehaviour
{
 [SerializeField]
 private float anguloMin = 65f; // Ângulo mínimo para considerar "virado"

 [Header("RN01 - transferência de líquido")]
 [SerializeField]
 private RecipienteLiquido origem; // líquido deste tubo de ensaio
 [SerializeField]
 private RecipienteLiquido destino; // líquido do copo de béquer (atribuído ao entrar na zona)
 [SerializeField]
 private float taxaPorSegundo = 20f; // unidades de volume transferidas por segundo

 [Header("RN03 - áudio do derramar")]
 [SerializeField]
 private AudioSource audioDerramar;

 private bool inZonaDerramar = false;
 private bool estahDerramando = false;

 void Update()
 {
 if (!inZonaDerramar || origem == null)
 {
 PararDeDerramar();
 return;
 }

 float angulo = Vector3.Angle(transform.up, Vector3.up);
 bool podeDerramar = angulo > anguloMin && origem.TemLiquido;

 if (podeDerramar)
 {
 if (!estahDerramando)
 Pour();
 Transferir(Time.deltaTime);
 }
 else
 {
 PararDeDerramar();
 }
 }

 void Pour()
 {
 estahDerramando = true;
 if (audioDerramar != null)
 audioDerramar.Play();
 print("Ingrediente derramado!");
 }

 void PararDeDerramar()
 {
 if (!estahDerramando)
 return;

 estahDerramando = false;
 if (audioDerramar != null)
 audioDerramar.Stop();
 }

 void Transferir(float deltaTime)
 {
 if (destino == null)
 return;

 float quantidadeDesejada = taxaPorSegundo * deltaTime;
 float quantidadeMaxima = Mathf.Min(quantidadeDesejada, origem.VolumeAtual, destino.EspacoDisponivel);
 if (quantidadeMaxima <= 0f)
 return;

 float retirado = origem.Retirar(quantidadeMaxima);
 destino.Adicionar(retirado);
 destino.RegistrarOrigem(origem); // RN02 - mistura a cor deste tubo no bequer
 }

 private void OnTriggerEnter(Collider other)
 {
 if (other.CompareTag("ZonaDerramar"))
 {
 gameObject.GetComponent<Outline>().OutlineWidth = 5f;
 inZonaDerramar = true;
 destino = other.GetComponentInParent<RecipienteLiquido>();
 print("In ZonaDerramar!");
 }
 }
 private void OnTriggerExit(Collider other)
 {
 if (other.CompareTag("ZonaDerramar"))
 {

 gameObject.GetComponent<Outline>().OutlineWidth = 0f;
 inZonaDerramar = false;
 PararDeDerramar();
 print("Out ZonaDerramar!");
 }
 }
}
