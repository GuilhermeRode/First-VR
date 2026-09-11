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
 print("Ingrediente derramado!");
 }

 void PararDeDerramar()
 {
 estahDerramando = false;
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
