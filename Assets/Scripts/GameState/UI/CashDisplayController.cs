using TMPro;
using UnityEngine;

public class CashDisplayController : MonoBehaviour
{
    [SerializeField] private TMP_Text cashLabel;
    [SerializeField] private Color normalColor = new Color(0, 1, 0);
    [SerializeField] private Color negativeColor = new Color(1, 0, 0);

    private void UpdateUI(float newCash)
    {
        if (newCash < 0)
        {
            cashLabel.text = "-" + Mathf.Abs(newCash).ToString("C0");
            cashLabel.color = negativeColor;
        } else
        {
            cashLabel.text = newCash.ToString("C0");
            cashLabel.color = normalColor;
        }
        
    }

    void Start()
    {
        UpdateUI(CashManager.Instance.Cash);
        CashManager.OnCashChanged += UpdateUI;
    }
}
