using UnityEngine;
using System.Collections.Generic;


public class CloudsManager : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 0.5f; // Velocit� base orizzontale

    [Header("Floating")]
    public float baseFloatAmount = 0.5f; // Ampiezza base galleggiamento
    public float floatAmountVariation = 0.2f; // Variazione ampiezza
    public float baseFloatSpeed = 1f; // Velocit� base galleggiamento
    public float floatSpeedVariation = 0.3f; // Variazione velocit�

    [Header("Loop Settings")]
    public float minSpacing = 10f;
    public float maxSpacing = 20f;

    [Header("Out Of View")]
    public float leftLimit = -50f;

    public Color[] RealcloudColors; // Array di colori per le nuvole reali (bianco o grigio chiaro)
    public Color[] FantasycloudColors; // Array di colori per le nuvole fantasy (sul viola)
    private Transform[] clouds;
    private float[] baseYPositions;
    private float[] randomOffsets;
    private float[] floatAmounts;
    private float[] floatSpeeds;
   
    //Valori di default settati in start
    private float defaultMoveSpeed;
    private float defaultBaseFloatSpeed;
    private float defaultBaseFloatAmount;
    //========================================//
    private SpriteRenderer[] spriteRenderers;
    

    public static List<CloudsManager> AllCloudManagers = new List<CloudsManager>();

    private void Start()
    {
        defaultMoveSpeed = moveSpeed;
        defaultBaseFloatSpeed = baseFloatSpeed;
        defaultBaseFloatAmount = baseFloatAmount;

        int count = transform.childCount;

        clouds = new Transform[count];
        spriteRenderers = new SpriteRenderer[count];
        baseYPositions = new float[count];
        randomOffsets = new float[count];
        floatAmounts = new float[count];
        floatSpeeds = new float[count];

        for (int i = 0; i < count; i++)
        {
            clouds[i] = transform.GetChild(i);
            spriteRenderers[i] = clouds[i].GetComponent<SpriteRenderer>();

            baseYPositions[i] = clouds[i].position.y;

            randomOffsets[i] = Random.Range(0f, 100f);

            floatAmounts[i] = baseFloatAmount + Random.Range(-floatAmountVariation, floatAmountVariation);
            floatSpeeds[i] = baseFloatSpeed + Random.Range(-floatSpeedVariation, floatSpeedVariation);

            // Colore iniziale (es. mondo reale)
            ApplyRandomColor(i, RealcloudColors);
        }
    }

    private void Update()
    {
        for (int i = 0; i < clouds.Length; i++)
        {
            Transform cloud = clouds[i];

            // Movimento orizzontale
            cloud.position += Vector3.left * moveSpeed * Time.deltaTime;

            // Movimento verticale morbido e indipendente
            float newY = baseYPositions[i] + Mathf.Sin((Time.time + randomOffsets[i]) * floatSpeeds[i]) * floatAmounts[i];
            cloud.position = new Vector3(cloud.position.x, newY, cloud.position.z);

            // Controllo uscita
            if (cloud.position.x < leftLimit)
            {
                RepositionCloud(i);
            }
        }
    }

    private void RepositionCloud(int index)
    {
        float rightMostX = GetRightMostCloudX();
        float randomSpacing = Random.Range(minSpacing, maxSpacing);

        Transform cloud = clouds[index];

        cloud.position = new Vector3(rightMostX + randomSpacing, cloud.position.y, cloud.position.z);
        baseYPositions[index] = cloud.position.y;

        // Ri-assegna nuova ampiezza e velocita leggermente casuali per il nuovo ciclo
        floatAmounts[index] = baseFloatAmount + Random.Range(-floatAmountVariation, floatAmountVariation);
        floatSpeeds[index] = baseFloatSpeed + Random.Range(-floatSpeedVariation, floatSpeedVariation);
        randomOffsets[index] = Random.Range(0f, 100f);
    }

    // Ottiene la posizione X della nuvola piu a destra per posizionare la nuova nuvola fuori vista
    private float GetRightMostCloudX()
    {
        float maxX = float.MinValue;

        foreach (Transform cloud in clouds)
        {
            if (cloud.position.x > maxX)
                maxX = cloud.position.x;
        }

        return maxX;
    }

    // Applica un colore casuale da una palette specifica alla nuvola
    private void ApplyRandomColor(int index, Color[] palette)
    {
        if (palette.Length == 0) return;

        Color randomColor = palette[Random.Range(0, palette.Length)];
        spriteRenderers[index].color = randomColor;
    }

    //Chiamato da worldSwitch quando cambia mondo, aggiorna i colori e la velocità delle nuvole in base al mondo attivo
    public void UpdateCloudDimension(bool isFantasy)
    {
        for (int i = 0; i < clouds.Length; i++)
        {
            // Cambio Colori
            if (isFantasy)
                ApplyRandomColor(i, FantasycloudColors);
            else
                ApplyRandomColor(i, RealcloudColors);
        }

        // Cambio velocità
            if (isFantasy)
            {
                moveSpeed = defaultMoveSpeed + 0.15f;
                baseFloatSpeed = defaultBaseFloatSpeed + 0.15f;
                baseFloatAmount = defaultBaseFloatAmount + 0.1f;
            }
            else
            {
                moveSpeed = defaultMoveSpeed;
                baseFloatSpeed = defaultBaseFloatSpeed;
                baseFloatAmount = defaultBaseFloatAmount;
            }   
    }


    private void OnEnable()
    {
        if (!AllCloudManagers.Contains(this))
            AllCloudManagers.Add(this);
    }

    private void OnDisable()
    {
        if (AllCloudManagers.Contains(this))
            AllCloudManagers.Remove(this);
    }
}