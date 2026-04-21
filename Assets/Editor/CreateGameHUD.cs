using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateGameHUD
{
    public static void Execute()
    {
        // Canvas
        GameObject hudRoot = new GameObject("GameHUD");
        Canvas canvas = hudRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = hudRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        hudRoot.AddComponent<GraphicRaycaster>();

        // Match timer — top center, large
        GameObject matchTimerGO = CreateText(hudRoot, "MatchTimer", "02:00", 56, FontStyles.Bold, TextAlignmentOptions.Center);
        Place(matchTimerGO, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(240f, 70f));

        // Players remaining — top right
        GameObject playersGO = CreateText(hudRoot, "PlayersRemaining", "8 PLAYERS", 30, FontStyles.Bold, TextAlignmentOptions.Right);
        Place(playersGO, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(260f, 50f));

        // Ring radius — top left line 1
        GameObject ringRadiusGO = CreateText(hudRoot, "RingRadius", "RING  150m", 26, FontStyles.Normal, TextAlignmentOptions.Left);
        Place(ringRadiusGO, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(280f, 44f));

        // Next shrink — top left line 2
        GameObject nextShrinkGO = CreateText(hudRoot, "NextShrink", "SHRINKS IN  30s", 26, FontStyles.Normal, TextAlignmentOptions.Left);
        Place(nextShrinkGO, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -80f), new Vector2(280f, 44f));

        // Size text — bottom left
        GameObject sizeTextGO = CreateText(hudRoot, "SizeText", "SIZE  100", 30, FontStyles.Bold, TextAlignmentOptions.Left);
        Place(sizeTextGO, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 65f), new Vector2(260f, 44f));

        // Size bar background
        GameObject sizeBarBG = CreatePanel(hudRoot, "SizeBarBackground", new Color(0f, 0f, 0f, 0.55f));
        Place(sizeBarBG, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 30f), new Vector2(320f, 28f));

        // Size bar fill
        GameObject sizeFillGO = CreatePanel(sizeBarBG, "SizeFill", new Color(0.18f, 0.82f, 0.35f, 1f));
        RectTransform fillRT = sizeFillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(2f, 2f);
        fillRT.offsetMax = new Vector2(-2f, -2f);
        Image fillImage = sizeFillGO.GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 0.1f;

        // Add GameHUD and wire references
        GameHUD hud = hudRoot.AddComponent<GameHUD>();
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("matchTimerText").objectReferenceValue = matchTimerGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("playersRemainingText").objectReferenceValue = playersGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("ringRadiusText").objectReferenceValue = ringRadiusGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("nextShrinkText").objectReferenceValue = nextShrinkGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("sizeText").objectReferenceValue = sizeTextGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("sizeFill").objectReferenceValue = sizeFillGO.GetComponent<Image>();
        so.FindProperty("shrinkingRing").objectReferenceValue = Object.FindFirstObjectByType<ShrinkingRing>();
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(hudRoot.scene);
        Debug.Log("[HUD] GameHUD created.");
    }

    private static GameObject CreateText(GameObject parent, string name, string defaultText, int fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.alignment = alignment;
        return go;
    }

    private static GameObject CreatePanel(GameObject parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = color;
        return go;
    }

    private static void Place(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
    }
}
