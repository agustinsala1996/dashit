using UnityEngine;
using System.Collections;

public class Interact : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Interactable[] interactions = other.GetComponents<Interactable>();
        if (interactions != null && interactions.Length > 0)
        {
            foreach (var i in interactions)
            {
                i.Interact(gameObject);
            }

            // ✅ Solo reproducir sonido si el objeto tiene el tag "Collectable"
            if (other.CompareTag("Collectable") && SFXManager.Instance != null)
            {
                SFXManager.Instance.PlayCollect();
            }
        }
    }
}