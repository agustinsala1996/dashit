using UnityEngine;

using System;

using System.Collections;







public class ActionOnDisabled : MonoBehaviour

{



    public Action onDisabled;



    void OnDisable()

    {

        if (!IsGameOver() && onDisabled != null)

        {

            onDisabled();

        }

    }



    private bool IsGameOver()

    {

        return GameStateController.instance == null || GameStateController.instance.isGameOver;

    }

}