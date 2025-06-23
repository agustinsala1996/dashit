using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class DashButton : MonoBehaviour
{
    [Header("Cooldown Animation")]
    public Image dashIconImage;                 // Imagen donde se muestra el ícono
    public Sprite readySprite;                 // Sprite que se muestra cuando el dash está listo
    public Sprite[] cooldownSprites;           // Sprites del cooldown (en orden)

    [Header("UI")]
    public Button dashButton;
    public TMP_Text countdownText;

    [Header("Dash Settings")]
    public float cooldown = 5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;

    [Header("Estela (Afterimage)")]
    public SpriteRenderer playerSprite;
    public int ghostCount = 5;
    public float ghostInterval = 0.02f;
    public float ghostLifetime = 0.2f;
    public Color ghostColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Joystick")]
    public Joystick movementJoystick;

    public string invulnerableLayerName = "Invulnerable";
    private int originalLayer;
    private GameObject player;

    private float cooldownTimer = 0f;
    private Rigidbody2D rb;
    private bool isDashing = false;
    private Vector2 dashDirection;
    private Vector2 lastDirection;

    private bool isAnimatingCooldown = false;

    void Start()
    {
        dashButton.interactable = true;

        Canvas canvas = dashButton.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 999;
        }

        AddEventTriggerToBypassRaycast();

        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null && found.transform.parent != null && found.transform.parent.CompareTag("Player"))
        {
            player = found.transform.parent.gameObject;
        }
        else
        {
            player = found;
        }

        if (player != null)
        {
            rb = player.GetComponent<Rigidbody2D>();
            originalLayer = player.layer;

            if (playerSprite == null)
                playerSprite = player.GetComponentInChildren<SpriteRenderer>();
        }

        countdownText.text = "";
        dashIconImage.sprite = readySprite;
    }

    void Update()
    {
        UpdateDashDirection();

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (!isAnimatingCooldown)
            {
                StartCoroutine(PlayCooldownAnimation());
            }
        }
        else
        {
            cooldownTimer = 0f;
            countdownText.text = "";
            dashIconImage.sprite = readySprite;
        }
    }

    IEnumerator PlayCooldownAnimation()
    {
        isAnimatingCooldown = true;
        float frameDuration = cooldown / cooldownSprites.Length;

        for (int i = 0; i < cooldownSprites.Length; i++)
        {
            dashIconImage.sprite = cooldownSprites[i];
            yield return new WaitForSeconds(frameDuration);
        }

        isAnimatingCooldown = false;
        dashIconImage.sprite = readySprite;
    }

    void UpdateDashDirection()
    {
        Vector2 input = movementJoystick.Direction;
        if (input.magnitude > 0.1f)
            lastDirection = input.normalized;
    }

    void ExecuteDash()
    {
        if (cooldownTimer > 0f)
        {
            Debug.Log("Dash en cooldown");
            return;
        }

        Vector2 input = movementJoystick.Direction;
        if (input.magnitude > 0.1f)
        {
            lastDirection = input.normalized;
        }

        if (lastDirection.magnitude <= 0.1f)
        {
            Debug.Log("Dash bloqueado: sin dirección válida");
            return;
        }

        // 🔊 Reproducir sonido del dash
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayDash();

        dashDirection = lastDirection;
        StartCoroutine(PerformDash());

        cooldownTimer = cooldown;
        isAnimatingCooldown = false;

        Debug.Log("✅ Dash ejecutado con dirección: " + dashDirection);
    }

    IEnumerator PerformDash()
    {
        isDashing = true;
        StartCoroutine(SpawnGhostTrail());

        int invulnerableLayer = LayerMask.NameToLayer(invulnerableLayerName);
        if (player != null)
            player.layer = invulnerableLayer;
        if (playerSprite != null)
            playerSprite.gameObject.layer = invulnerableLayer;

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            Vector2 newPos = rb.position + dashDirection * dashSpeed * Time.deltaTime;
            rb.MovePosition(newPos);
            yield return null;
        }

        if (player != null)
            player.layer = originalLayer;
        if (playerSprite != null)
            playerSprite.gameObject.layer = originalLayer;

        isDashing = false;
    }

    IEnumerator SpawnGhostTrail()
    {
        for (int i = 0; i < ghostCount; i++)
        {
            CreateGhost();
            yield return new WaitForSeconds(ghostInterval);
        }
    }

    void CreateGhost()
    {
        if (playerSprite == null) return;

        GameObject ghost = new GameObject("Ghost");
        SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
        sr.sprite = playerSprite.sprite;
        sr.sortingLayerID = playerSprite.sortingLayerID;
        sr.sortingOrder = playerSprite.sortingOrder - 1;
        sr.color = ghostColor;

        ghost.transform.position = rb.transform.position;
        ghost.transform.rotation = rb.transform.rotation;
        ghost.transform.localScale = rb.transform.localScale * 0.06f;

        Destroy(ghost, ghostLifetime);
    }

    public bool GetIsDashing()
    {
        return isDashing;
    }

    void AddEventTriggerToBypassRaycast()
    {
        EventTrigger trigger = dashButton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = dashButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        entry.callback.AddListener((eventData) => {
            ExecuteDash();
        });

        trigger.triggers.Clear();
        trigger.triggers.Add(entry);
    }
}