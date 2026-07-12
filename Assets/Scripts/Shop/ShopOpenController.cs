using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopOpenController : MonoBehaviour
{
    [SerializeField] private RectTransform shopWindow;
    [SerializeField] private float transitionTime = 0.2f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;

    private enum ShopState { Closed, Opening, Open, Closing }
    private ShopState currentState = ShopState.Closed;

    private void Start()
    {
        visiblePosition = new Vector2(0f, shopWindow.anchoredPosition.y);
        hiddenPosition = new Vector2(shopWindow.rect.width, shopWindow.anchoredPosition.y);

        shopWindow.anchoredPosition = hiddenPosition;
    }

    private void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        // Only allow toggling if fully closed or fully open
        if (currentState == ShopState.Closed)
        {
            StartCoroutine(AnimateWindow(visiblePosition, ShopState.Opening, ShopState.Open));
        }
        else if (currentState == ShopState.Open)
        {
            StartCoroutine(AnimateWindow(hiddenPosition, ShopState.Closing, ShopState.Closed));
        }
    }

    private void ShopOpened()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShopClosed()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator AnimateWindow(Vector2 targetPos, ShopState transitionState, ShopState finalState)
    {
        // Lock the state to opening/closing so input is ignored
        currentState = transitionState;

        Vector2 startPos = shopWindow.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < transitionTime)
        {
            // Move the shop window based on progress on the curve
            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / transitionTime;
            float curvePercent = easeCurve.Evaluate(percent);

            shopWindow.anchoredPosition = Vector2.Lerp(startPos, targetPos, curvePercent);

            // Wait until the next frame before looping
            yield return null;
        }

        shopWindow.anchoredPosition = targetPos;
        currentState = finalState;

        if (finalState == ShopState.Open)
        {
            ShopOpened();

        }
        else if (finalState == ShopState.Closed)
        {
            ShopClosed();
        }
    }
}