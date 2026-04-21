using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI matchTimerText;
    [SerializeField] private TextMeshProUGUI playersRemainingText;
    [SerializeField] private TextMeshProUGUI ringRadiusText;
    [SerializeField] private TextMeshProUGUI nextShrinkText;
    [SerializeField] private TextMeshProUGUI sizeText;
    [SerializeField] private Image sizeFill;
    [SerializeField] private ShrinkingRing shrinkingRing;

    private void Update()
    {
        UpdateMatchTimer();
        UpdatePlayerCount();
        UpdateRingInfo();
        UpdateSizeDisplay();
    }

    private void UpdateMatchTimer()
    {
        if (shrinkingRing == null)
        {
            return;
        }

        float remaining = shrinkingRing.MatchTimeRemaining;
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        matchTimerText.text = $"{minutes:D2}:{seconds:D2}";
    }

    private void UpdatePlayerCount()
    {
        playersRemainingText.text = $"{PlayerStats.ActivePlayers.Count} PLAYERS";
    }

    private void UpdateRingInfo()
    {
        if (shrinkingRing == null)
        {
            return;
        }

        ringRadiusText.text = $"RING  {shrinkingRing.CurrentRadius:F0}m";
        nextShrinkText.text = $"SHRINKS IN  {shrinkingRing.TimeUntilNextShrink:F0}s";
    }

    private void UpdateSizeDisplay()
    {
        PlayerStats localPlayer = GetLocalPlayer();
        if (localPlayer == null)
        {
            return;
        }

        sizeText.text = $"SIZE  {localPlayer.playerCurrentSize:F0}";

        if (sizeFill != null)
        {
            sizeFill.fillAmount = localPlayer.playerCurrentSize / localPlayer.playerMaxSize;
        }
    }

    private PlayerStats GetLocalPlayer()
    {
        PlayerStats fallback = null;
        for (int i = 0; i < PlayerStats.ActivePlayers.Count; i++)
        {
            PlayerStats player = PlayerStats.ActivePlayers[i];
            if (player.HasStateAuthority)
            {
                return player;
            }
            if (fallback == null)
            {
                fallback = player;
            }
        }
        return fallback;
    }
}
