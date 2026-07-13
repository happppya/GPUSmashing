using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class ShopItemBuilder : MonoBehaviour
{
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] TMP_Text descriptionLabel;
    [SerializeField] TMP_Text costLabel;
    [SerializeField] Button buyButton;

    [SerializeField] Color affordableColor = new Color(1.0f, 1.0f, 1.0f);
    [SerializeField] Color notAffordableColor = new Color(0.0f, 0.0f, 0.0f);

    public void Initialize(string name, string description, float cost)
    {
        nameLabel.text = name;
        descriptionLabel.text = description;
        costLabel.text = cost.ToString("C0");
    }

    public void SetCost(float cost)
    {
        costLabel.text = cost.ToString("C0");
    }

    public void SetCostRaw(string text)
    {
        costLabel.text = text;
    }

    public void SetName(string newName)
    {
        nameLabel.text = newName;
    }

    void Start()
    {
        buyButton.onClick.AddListener(BuyButtonPressed);
        CashManager.OnCashChanged += CashChanged;
        CashChanged(CashManager.Instance.Cash);
    }

    void CashChanged(float cash)
    {
        Image buttonImage = buyButton.GetComponent<Image>();
        if (CanBeBought())
        {
            buttonImage.color = affordableColor;
        } else
        {
            buttonImage.color = notAffordableColor;
        }
    }

    protected abstract void BuyButtonPressed();
    protected abstract bool CanBeBought();

}
