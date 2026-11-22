using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HorrorButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Effect")]
    public float hoverScale = 1.07f;
    public float pressedScale = 0.96f;
    public float animSpeed = 14f;

    [Header("Color Tint")]
    public Graphic targetGraphic;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 0.85f, 0.82f, 1f);
    public Color pressedColor = new Color(0.8f, 0.45f, 0.45f, 1f);

    [Header("Menu Controller (Optional)")]
    public HorrorMenuController menuController;

    private Vector3 baseScale;
    private Vector3 desiredScale;
    private Color desiredColor;
    private bool isHovered = false;
    private bool isPressed = false;

    private void Awake()
    {
        baseScale = transform.localScale;
        desiredScale = baseScale;

        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }

        if (targetGraphic != null)
        {
            targetGraphic.color = normalColor;
            desiredColor = normalColor;
        }

        if (menuController == null)
        {
            menuController = FindObjectOfType<HorrorMenuController>();
        }
    }

    private void OnEnable()
    {
        isHovered = false;
        isPressed = false;
        transform.localScale = baseScale;
        desiredScale = baseScale;

        if (targetGraphic != null)
        {
            targetGraphic.color = normalColor;
            desiredColor = normalColor;
        }
    }

    private void Update()
    {
        float step = animSpeed * Time.unscaledDeltaTime;
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, step);

        if (targetGraphic != null)
        {
            targetGraphic.color = Color.Lerp(targetGraphic.color, desiredColor, step);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        desiredScale = baseScale * hoverScale;
        desiredColor = hoverColor;

        if (menuController != null)
        {
            menuController.PlayHoverSound();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
        desiredScale = baseScale;
        desiredColor = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        desiredScale = baseScale * pressedScale;
        desiredColor = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        desiredScale = isHovered ? baseScale * hoverScale : baseScale;
        desiredColor = isHovered ? hoverColor : normalColor;
    }
}