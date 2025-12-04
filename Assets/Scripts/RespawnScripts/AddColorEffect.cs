using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AddColorEffect : MonoBehaviour
{
    private class RendererColorPair
    {
        public SpriteRenderer renderer;
        public Color startColor;
    }

    public Light2D rockLight;

    public float glowStrength = 1.0f;
    public float particlesStrength = 1.0f;

    //Colori della runa e particelle prima che ci passo sopra
    public Color runeColor;
    public Color particlesColor;
   
    //Colori della runa e particelle dopo che ci passo sopra
    public Color runeColor_Then;
    public Color particlesColor_Then;
    public Color rockLightColor;

    private Color runeColorLastFrame;
    private float glowStrengthLastFrame;
    private Color particlesColorLastFrame;
    private float particlesStrengthLastFrame;
    private List<RendererColorPair> rendsColors;
    private List<ParticleSystemRenderer> partSystems;

    void Start()
    {
        runeColorLastFrame = Color.black;
        rendsColors = new List<RendererColorPair>();

        List<SpriteRenderer> spriteRends = this.GetComponentsInChildren<SpriteRenderer>().ToList();
        foreach (SpriteRenderer rend in spriteRends)
        {
            rendsColors.Add(new RendererColorPair() { renderer = rend, startColor = rend.color });
        }

        partSystems = this.GetComponentsInChildren<ParticleSystemRenderer>().ToList();

    }

    void Update()
    {
        if ((runeColor != runeColorLastFrame) || (glowStrength != glowStrengthLastFrame))
        {
            foreach (RendererColorPair rendColorPair in rendsColors)
            {
                float targetBrightness = rendColorPair.startColor.r + rendColorPair.startColor.g + rendColorPair.startColor.b;
                Color combinedColor = new Color(rendColorPair.startColor.r + (runeColor.r * glowStrength), rendColorPair.startColor.g + (runeColor.g * glowStrength), rendColorPair.startColor.b + (runeColor.b * glowStrength), rendColorPair.startColor.a);
                float combinedColorBrightness = combinedColor.r + combinedColor.g + combinedColor.b;
                float correctionCoef = targetBrightness / combinedColorBrightness;
                Color correctedColor = new Color(combinedColor.r * correctionCoef, combinedColor.g * correctionCoef, combinedColor.b * correctionCoef);

                rendColorPair.renderer.color = correctedColor;
            }
            runeColorLastFrame = runeColor;
            glowStrengthLastFrame = glowStrength;
        }

        if ((particlesColor != particlesColorLastFrame) || (particlesStrength != particlesStrengthLastFrame))
        {
            foreach (ParticleSystemRenderer partSysRenderer in partSystems)
            {
                partSysRenderer.material.color = new Color(particlesColor.r * particlesStrength, particlesColor.g * particlesStrength, particlesColor.b * particlesStrength, particlesColor.a);
            }
            particlesColorLastFrame = particlesColor;
            particlesStrengthLastFrame = particlesStrength;
        }

        if (rockLight != null)
        {
            float targetIntensity = glowStrength; 
            rockLight.intensity = Mathf.Lerp(rockLight.intensity, targetIntensity, Time.deltaTime * 3f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        runeColor = runeColor_Then;
        particlesColor = particlesColor_Then;
        rockLight.color = rockLightColor;
    }
}
