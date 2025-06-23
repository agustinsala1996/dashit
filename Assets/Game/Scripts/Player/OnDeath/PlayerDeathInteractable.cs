using UnityEngine;
using System.Collections;

public class PlayerDeathInteractable : MonoBehaviour, Interactable
{
    public void Interact(GameObject interacted)
    {
        interacted.GetComponent<PlayerHealth>().ApplyDamage();
    }
}
