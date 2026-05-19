using UnityEngine;
using System.Collections;
using TMPro; // Fondamentale per accedere a TextMeshPro

public class TextWaveAnimation : MonoBehaviour
{
    [Header("Impostazioni Onda")]
    [Tooltip("Quanto velocemente oscillano le lettere")]
    [SerializeField] private float velocitaOnda = 2.0f;
    
    [Tooltip("Quanto vanno in alto e in basso le lettere")]
    [SerializeField] private float ampiezzaOnda = 10.0f;
    
    [Tooltip("Lo sfasamento tra una lettera e l'altra (crea l'effetto onda vera e propria)")]
    [SerializeField] private float offsetTraLettere = 0.2f;

    private TMP_Text textComponent;
    private TMP_TextInfo textInfo;
    private Mesh mesh;
    private Vector3[] vertices;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        // Forziamo l'aggiornamento del testo per generare i dati della mesh
        textComponent.ForceMeshUpdate();
        textInfo = textComponent.textInfo;
    }

    private void Update()
    {
        // Dobbiamo forzare l'aggiornamento della mesh ogni frame per l'animazione
        textComponent.ForceMeshUpdate();
        textInfo = textComponent.textInfo;
        mesh = textComponent.mesh;

        // Otteniamo i vertici attuali della mesh del testo
        vertices = mesh.vertices;

        // Cicliamo attraverso ogni carattere visibile
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // Saltiamo se il carattere non è visibile (spazi, etc.)
            if (!charInfo.isVisible)
                continue;

            // Otteniamo l'indice del vertice iniziale per questo carattere (ogni lettera ha 4 vertici)
            int vertexIndex = charInfo.vertexIndex;

            // Calcoliamo l'offset Y basato sul tempo e sull'indice del carattere (Seno)
            // L'uso di Sin ci dà un'oscillazione morbida
            float yOffset = Mathf.Sin(Time.time * velocitaOnda + i * offsetTraLettere) * ampiezzaOnda;

            // Applichiamo l'offset a tutti e 4 i vertici del carattere
            // vertices[vertexIndex + 0] = Basso Sinistra
            // vertices[vertexIndex + 1] = Alto Sinistra
            // vertices[vertexIndex + 2] = Alto Destra
            // vertices[vertexIndex + 3] = Basso Destra
            
            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j].y += yOffset;
            }
        }

        // Assegniamo i vertici modificati alla mesh
        mesh.vertices = vertices;
        textComponent.canvasRenderer.SetMesh(mesh);
    }
}