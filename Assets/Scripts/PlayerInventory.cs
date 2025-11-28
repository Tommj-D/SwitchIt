using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int keyCount = 0;

    // Aggiunge una chiave all'inventario
    public void AddKey()
    {
        keyCount++;
    }

    // Controlla se il player ha almeno una chiave
    public bool HasKey()
    {
        return keyCount > 0;
    }

    // Usa una chiave (se presente)
    public void UseKey()
    {
        if (keyCount > 0)
        {
            keyCount--;
        }
    }
}
