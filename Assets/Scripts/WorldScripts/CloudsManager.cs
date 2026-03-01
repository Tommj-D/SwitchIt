using UnityEngine;

public class CloudsManager : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 0.5f; // Velocità base orizzontale

    [Header("Floating")]
    public float baseFloatAmount = 0.5f; // Ampiezza base galleggiamento
    public float floatAmountVariation = 0.2f; // Variazione ampiezza
    public float baseFloatSpeed = 1f; // Velocità base galleggiamento
    public float floatSpeedVariation = 0.3f; // Variazione velocità

    [Header("Loop Settings")]
    public float minSpacing = 10f;
    public float maxSpacing = 20f;

    [Header("Out Of View")]
    public float leftLimit = -50f;

    private Transform[] clouds;
    private float[] baseYPositions;
    private float[] randomOffsets;
    private float[] floatAmounts;
    private float[] floatSpeeds;

    private void Start()
    {
        int count = transform.childCount;
        clouds = new Transform[count];
        baseYPositions = new float[count];
        randomOffsets = new float[count];
        floatAmounts = new float[count];
        floatSpeeds = new float[count];

        for (int i = 0; i < count; i++)
        {
            clouds[i] = transform.GetChild(i);
            baseYPositions[i] = clouds[i].position.y;

            // Offset casuale per non sincronizzare le sinusoidi
            randomOffsets[i] = Random.Range(0f, 100f);

            // Ampiezza e velocità leggermente diverse
            floatAmounts[i] = baseFloatAmount + Random.Range(-floatAmountVariation, floatAmountVariation);
            floatSpeeds[i] = baseFloatSpeed + Random.Range(-floatSpeedVariation, floatSpeedVariation);
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

        // Ri-assegna nuova ampiezza e velocità leggermente casuali per il nuovo ciclo
        floatAmounts[index] = baseFloatAmount + Random.Range(-floatAmountVariation, floatAmountVariation);
        floatSpeeds[index] = baseFloatSpeed + Random.Range(-floatSpeedVariation, floatSpeedVariation);
        randomOffsets[index] = Random.Range(0f, 100f);
    }

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
}