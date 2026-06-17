using UnityEngine;
using VertigoWheel.Animation;
using VertigoWheel.Core;

namespace VertigoWheel.UI
{
    /// <summary>
    /// Base class for modal popups. Handles show/hide via a CanvasGroup (fade + pop) so subclasses only
    /// describe their own content and buttons. The CanvasGroup also blocks input behind the popup.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupView : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected RectTransform _panel; // the box that pops in (kept off the root transform)

        public bool IsOpen { get; private set; }

        protected virtual void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            ApplyVisible(false);
        }

        public virtual void Show()
        {
            IsOpen = true;
            gameObject.SetActive(true);
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            UiTween.FadeCanvasGroup(_canvasGroup, 1f, 0.2f);
            if (_panel != null) UiTween.PopIn(_panel);
        }

        public virtual void Hide()
        {
            IsOpen = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            UiTween.FadeCanvasGroup(_canvasGroup, 0f, 0.15f, () => gameObject.SetActive(false));
        }

        private void ApplyVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = visible;
            _canvasGroup.interactable = visible;
            gameObject.SetActive(visible);
        }

        protected virtual void OnValidate()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_panel == null) _panel = UiBinder.FindByName<RectTransform>(transform, "popup_panel");
        }
    }
}
