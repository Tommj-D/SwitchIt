using System.Collections;
using UnityEngine;

public class ButtonPuzzleController : MonoBehaviour
{
    private int currentPressed = 0;

    [Header("Cerchi da illuminare")]
    public SpriteRenderer[] circles;

    [Header("Oggetto da dissolvere")]
    public GameObject wall;

    [Header("Particelle")]
    public ParticleSystem particlePrefab;

    [Header("Colori")]
    public Color glowColor = Color.yellow;

    public float dissolveDuration = 1f;

    public void ButtonPressed(Transform buttonPos)
    {
        if (currentPressed >= circles.Length) return;

        Transform target = circles[currentPressed].transform;

        SpawnParticles(buttonPos, target, currentPressed);

        currentPressed++;
    }

    public void IlluminateCircle(int index)
    {
        if (circles[index] != null)
        {
            //circles[index].color = Color.white;

            SpriteRenderer glow = circles[index].transform.GetChild(0).GetComponent<SpriteRenderer>();
            glow.color = glowColor;
        }

        if (index == circles.Length - 1)
        {
            StartCoroutine(DissolveWall());
        }
    }

    private IEnumerator DissolveWall()
    {
        SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();

        float timer = 0f;
        Color c = sr.color;

        while (timer < dissolveDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, timer / dissolveDuration);
            sr.color = c;

            yield return null;
        }

        wall.SetActive(false);
    }

    private void SpawnParticles(Transform start, Transform target, int index)
    {
        ParticleSystem ps = Instantiate(particlePrefab, start.position, Quaternion.identity);

        ParticlesToTarget mover = ps.GetComponent<ParticlesToTarget>();
        mover.target = target;
        mover.controller = this;
        mover.circleIndex = index;

        ps.Play();
    }
}