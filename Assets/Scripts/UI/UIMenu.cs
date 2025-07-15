using System;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public enum ParentBehaviour
    {
        Show,
        Hide
    }

    public class UIMenu : MonoBehaviour
    {
        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                _canvasGroup.blocksRaycasts = value;
                _canvasGroup.interactable = value;
                MenuTransition(value);
            }
        }

        public RectTransform submenuRect;
        private CanvasGroup _canvasGroup;

        public event Action OnOpen;
        public event Action OnClose;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Open()
        {
            Enabled = true;
            OnOpen?.Invoke();
        }
        public void Close()
        {
            Enabled = false;
            OnClose?.Invoke();
        }

        protected virtual void MenuTransition(bool enabled)
        {
            _canvasGroup.DOFade(enabled ? 1 : 0, 0.4f).SetEase(Ease.OutCubic);
        }
    }
}
