using System.Collections;
using UnityEngine;

public class ButtonPuzzleController : MonoBehaviour
{
    private int currentPressed = 0;

    [Header("Cerchi da illuminare")]
    public SpriteRenderer[] circles;

    [Header("Muro da dissolvere")]
    public GameObject wall;

    [Header("Particelle")]
    public ParticleSystem particlePrefab;

    public float dissolveDuration = 1f;

    public void ButtonPressed(Transform buttonPos)
    {
        if (currentPressed >= circles.Length) return;

        Transform target = circles[currentPressed].transform;

        SpawnParticles(buttonPos, target);

        IlluminateCircle(currentPressed);

        currentPressed++;

        if (currentPressed >= circles.Length)
        {
            StartCoroutine(DissolveWall());
        }
    }

    void IlluminateCircle(int index)
    {
        if (circles[index] != null)
        {
            circles[index].color = Color.white;
        }
    }

    IEnumerator DissolveWall()
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

    void SpawnParticles(Transform start, Transform target)
    {
        ParticleSystem ps = Instantiate(particlePrefab, start.position, Quaternion.identity);

        ParticlesToTarget mover = ps.GetComponent<ParticlesToTarget>();
        mover.target = target;

        ps.Play();
    }
}