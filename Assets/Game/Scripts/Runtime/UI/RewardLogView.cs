using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Core;

namespace VertigoWheel.UI
{
    /// <summary>
    /// Scrollable list of every reward collected during the current run. Rows are cloned from an inactive
    /// template that lives under the scroll content, so the whole widget is self-contained in the scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RewardLogView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scroll;       // reward_log_scroll
        [SerializeField] private RectTransform _content;   // reward_log_content
        [SerializeField] private RewardLogRow _rowTemplate; // reward_log_row_template (kept inactive)

        private readonly List<RewardLogRow> _rows = new List<RewardLogRow>();

        public void AddEntry(Sprite icon, string text)
        {
            if (_rowTemplate == null || _content == null) return;

            var row = Instantiate(_rowTemplate, _content);
            row.gameObject.SetActive(true);
            row.transform.SetAsLastSibling();
            row.SetContent(icon, text);
            _rows.Add(row);

            // Rebuild layout then snap the view to the newest entry at the bottom.
            Canvas.ForceUpdateCanvases();
            if (_scroll != null) _scroll.verticalNormalizedPosition = 0f;
        }

        public void Clear()
        {
            foreach (var row in _rows)
                if (row != null) Destroy(row.gameObject);
            _rows.Clear();
        }

        private void Awake()
        {
            // Make sure the template never shows as a real entry.
            if (_rowTemplate != null) _rowTemplate.gameObject.SetActive(false);
        }

        private void OnValidate()
        {
            if (_scroll == null) _scroll = GetComponentInChildren<ScrollRect>(true);
            if (_content == null)
            {
                var found = UiBinder.FindByName<RectTransform>(transform, "reward_log_content");
                if (found != null) _content = found;
            }
            if (_rowTemplate == null) _rowTemplate = UiBinder.FindByName<RewardLogRow>(transform, "reward_log_row_template");
        }
    }
}
