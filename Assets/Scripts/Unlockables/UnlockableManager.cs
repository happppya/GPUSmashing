using UnityEngine;

public class UnlockableManager : MonoBehaviour
{
    [SerializeField] private GameObject spikesContainer;
    [SerializeField] private GameObject bladesContainer;
    
    void Start()
    {
        spikesContainer.SetActive(false);
        bladesContainer.SetActive(false);

        UpgradeManager.OnUpgradeTypeChanged += (UpgradeType upgrade) =>
        {
            bool isActive = UpgradeManager.Instance.GetStat(upgrade) > 0;
            if (upgrade == UpgradeType.UnlockedSpikes)
            {
                spikesContainer.SetActive(isActive);
            } else if (upgrade == UpgradeType.UnlockedBlades)
            {
                bladesContainer.SetActive(isActive);
            }
        };
    }

}
