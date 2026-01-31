using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    public TextMeshProUGUI displayTexte;

    void OnEnable()
    {
        if (displayTexte != null) {
            displayTexte.text = SaveManager.GetLeaderboardFormatted();
        }
    }
}