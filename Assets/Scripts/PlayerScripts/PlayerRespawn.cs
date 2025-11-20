using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{

    private PlayerControls controls; // riferimento alla classe PlayerControls generata

    public Vector3 respawnPoint;
    public float respawnDelay = 1.5f;
    public GameObject deathParticle;

    private Animator animator;
    private bool isDying = false;

    /*private void Awake()
    {
        // Inizializza i controlli
        controls = new PlayerControls();
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }*/ //Disabilitato perchè in futuro vorrò fermare il gicatore quando muore

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDying && collision.gameObject.CompareTag("Death"))
        {
            StartCoroutine(DeathSequence());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDying && other.gameObject.CompareTag("Death"))
        {
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        isDying = true;
        /*GetComponent<Rigidbody2D>().linearVelocity = 0; // blocca eventuali movimenti
        GetComponent<Collider2D>().enabled = false; // evita ulteriori collisioni durante morte

        // Disabilita Controller
        controls.Disable();*/  //Disabilitato perchè in futuro vorrò fermare il gicatore quando muore

        //Lancia animazione di morte
        if (animator != null)
            animator.SetTrigger("Death");

        //Particelle
        if (deathParticle != null)
            Instantiate(deathParticle, transform.position, Quaternion.identity);

        //Aspetta l'animazione + tempo extra
        yield return new WaitForSeconds(respawnDelay);

        // Respawn
        transform.position = respawnPoint;

        //Animazione Respawn
        if (animator != null)
            animator.SetTrigger("Respawn");

        //GetComponent<Collider2D>().enabled = true; //Disabilitato perchè in futuro vorrò fermare il gicatore quando muore

        /*// Riattiva controlli
        controls.Enable();*///Disabilitato perchè in futuro vorrò fermare il gicatore quando muore

        isDying = false;
    }
}
