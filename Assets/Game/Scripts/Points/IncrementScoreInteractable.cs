using UnityEngine;

using System.Collections;





/// <summary>

/// Increments current score on object collision.

/// </summary>

public class IncrementScoreInteractable : MonoBehaviour, Interactable

{

    public void Interact(GameObject interacted)

    {

        Score.instance.IncrementScore();

    }

}