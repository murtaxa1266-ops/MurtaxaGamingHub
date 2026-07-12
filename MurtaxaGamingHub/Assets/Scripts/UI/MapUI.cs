// ============================================================
//  MapUI.cs
//  Place in: Assets/Scripts/UI/
//  Simple overhead minimap + full map panel with icons for
//  player, enemies, NPCs, and points of interest.
// ============================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.UI
{
    [System.Serializable]
    public class MapIcon
    {
        public string   id;
        public RectTransform iconTransform;
    }

    public class MapUI : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Map Bounds (World)")]
        [SerializeField] private Vector2 worldMin  = new Vector2(-500f, -500f);
        [SerializeField] private Vector2 worldMax  = new Vector2( 500f,  500f);

        [Header("Map Panel")]
        [SerializeField] private RectTransform mapPanel;    // Full map rect
        [SerializeField] private Button        closeButton;
        [SerializeField] private KeyCode       toggleKey = KeyCode.M;

        [Header("Player Marker")]
        [SerializeField] private RectTransform playerMarker;

        [Header("Icon Prefabs")]
        [SerializeField] private GameObject questIconPrefab;      // Yellow !
        [SerializeField] private GameObject checkpointIconPrefab; // Star/fire icon
        [SerializeField] private GameObject npcIconPrefab;        // NPC marker

        [Header("Zone Name")]
        [SerializeField] private TextMeshProUGUI zoneLabel;

        // ── Runtime State ────────────────────────────────────────
        private bool                _isOpen = false;
        private Transform           _player;
        private List<MapIcon>       _icons = new List<MapIcon>();

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            closeButton?.onClick.AddListener(() => Toggle(false));
        }

        private void Start()
        {
            GameObject pg = GameObject.FindGameObjectWithTag("Player");
            if (pg != null) _player = pg.transform;

            // Auto-register world points of interest
            RegisterWorldIcons();
        }

        private void OnEnable()  => GameEvents.OnZoneChanged += OnZoneChanged;
        private void OnDisable() => GameEvents.OnZoneChanged -= OnZoneChanged;

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) Toggle(!_isOpen);
            if (_isOpen && _player != null) UpdatePlayerMarker();
        }

        // ── Public API ────────────────────────────────────────────

        public void Toggle(bool open)
        {
            _isOpen = open;
            gameObject.SetActive(open);
            GameEvents.TriggerMapToggled(open);
            if (open) { UpdatePlayerMarker(); GameEvents.TriggerPlaySFX("UIOpen"); }
            else GameEvents.TriggerPlaySFX("UIClose");
        }

        // ── Private ───────────────────────────────────────────────

        private void UpdatePlayerMarker()
        {
            if (playerMarker == null || _player == null || mapPanel == null) return;
            playerMarker.anchoredPosition = WorldToMap(_player.position);

            // Rotate marker to match player yaw
            float yaw = _player.eulerAngles.y;
            playerMarker.localRotation = Quaternion.Euler(0f, 0f, -yaw);
        }

        private Vector2 WorldToMap(Vector3 worldPos)
        {
            float nx = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
            float ny = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.z);

            Rect r = mapPanel.rect;
            return new Vector2(
                Mathf.Lerp(-r.width  * 0.5f, r.width  * 0.5f, nx),
                Mathf.Lerp(-r.height * 0.5f, r.height * 0.5f, ny)
            );
        }

        private void RegisterWorldIcons()
        {
            if (mapPanel == null) return;

            // Checkpoints
            foreach (var cp in FindObjectsOfType<Systems.Checkpoint>())
            {
                if (checkpointIconPrefab == null) break;
                GameObject icon = Instantiate(checkpointIconPrefab, mapPanel);
                RectTransform rt = icon.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = WorldToMap(cp.transform.position);
                _icons.Add(new MapIcon { id = cp.CheckpointId, iconTransform = rt });
            }

            // NPCs
            foreach (var npc in FindObjectsOfType<NPC.NPCInteraction>())
            {
                if (npcIconPrefab == null) break;
                GameObject icon = Instantiate(npcIconPrefab, mapPanel);
                RectTransform rt = icon.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = WorldToMap(npc.transform.position);
            }
        }

        private void OnZoneChanged(string zoneName)
        {
            if (zoneLabel != null) zoneLabel.text = zoneName;
        }
    }
}
