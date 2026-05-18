using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class BossPuzzleController : MonoBehaviour, IButtonPuzzle
{
    [Header("Buttons")]
    [SerializeField] private int totalButtons = 3;

    private int pressedButtons = 0;

    [Header("Targets")]
    [SerializeField] private Transform[] circleTargets;

    [Header("Flying Particles")]
    [SerializeField] private FlyingRewardParticle flyingParticlePrefab;

    [Header("Blocks To Dissolve")]
    [SerializeField] private GameObject[] blocksToRemove;

    [SerializeField] private float dissolveTime = 1f;

    [Header("Shader Settings")]
    [SerializeField] private float outlineThickness = 0.1f;

    [SerializeField] private float dissolveScale = 30f;

    [ColorUsage(true, true)]
    [SerializeField] private Color outlineColor = Color.white;

    [SerializeField] private float spiralStrength = 5f;

    [SerializeField] private bool useVerticalDissolve = false;

    private int arrivedParticles = 0;

    private int dissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    private int verticalDissolveID = Shader.PropertyToID("_VerticalDissolve");
    private int outlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    private int outlineColorID = Shader.PropertyToID("_OutlineColor");
    private int spiralStrengthID = Shader.PropertyToID("_SpiralStrength");
    private int dissolveScaleID = Shader.PropertyToID("_DissolveScale");

    public void PressButton(Transform buttonTransform, int targetCircleIndex)
    {
        pressedButtons++;

        SpawnFlyingParticle(buttonTransform, targetCircleIndex);
    }

    private void SpawnFlyingParticle(Transform startPoint, int targetIndex)
    {
        if (flyingParticlePrefab == null) return;

        if (targetIndex < 0 || targetIndex >= circleTargets.Length) return;

        FlyingRewardParticle particle =
            Instantiate(flyingParticlePrefab, startPoint.position, Quaternion.identity);

        particle.Setup(circleTargets[targetIndex], null);

        StartCoroutine(WaitParticleArrival(particle));
    }

    private IEnumerator WaitParticleArrival(FlyingRewardParticle particle)
    {
        if (particle == null)
            yield break;

        Transform target = particle.transform;

        while (particle != null)
        {
            yield return null;
        }

        arrivedParticles++;

        // Suono arrivo particella
        if (AudioManager.Instance.glowingSound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.glowingSound);
        }

        // Quando arriva l'ultima particella
        if (arrivedParticles >= totalButtons)
        {
            CompletePuzzle();
        }
    }

    private void CompletePuzzle()
    {
        if (AudioManager.Instance.wallDisappearingSound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.wallDisappearingSound);
        }

        foreach (GameObject block in blocksToRemove)
        {
            if (block == null) continue;

            // Se il blocco è già disattivato
            // lo lasciamo stare
            if (!block.activeSelf)
                continue;

            TilemapRenderer tilemapRenderer = block.GetComponent<TilemapRenderer>();

            if (tilemapRenderer == null)
                continue;

            Material mat = tilemapRenderer.material;

            if (mat.HasProperty(outlineThicknessID))
                mat.SetFloat(outlineThicknessID, outlineThickness);

            if (mat.HasProperty(outlineColorID))
                mat.SetColor(outlineColorID, outlineColor);

            if (mat.HasProperty(spiralStrengthID))
                mat.SetFloat(spiralStrengthID, spiralStrength);

            if (mat.HasProperty(dissolveScaleID))
                mat.SetFloat(dissolveScaleID, dissolveScale);

            if (mat.HasProperty(verticalDissolveID))
                mat.SetFloat(verticalDissolveID, useVerticalDissolve ? 0f : 0f);

            StartCoroutine(DissolveBlock(mat, 0f, 1.1f, block));
        }
    }

    private IEnumerator DissolveBlock(
        Material mat,
        float start,
        float end,
        GameObject block
    )
    {
        float elapsed = 0f;

        while (elapsed < dissolveTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / dissolveTime;

            float value = Mathf.Lerp(start, end, t);

            mat.SetFloat("_DissolveAmount", value);

            yield return null;
        }

        mat.SetFloat("_DissolveAmount", end);

        block.SetActive(false);
    }
}