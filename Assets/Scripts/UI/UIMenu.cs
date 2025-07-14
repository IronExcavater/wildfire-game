using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class UIMenu : MonoBehaviour
    {
        public UIMenu parentMenu;
        public List<UIMenu> childMenus;

        public CanvasGroup canvasGroup;
        public Button backButton;

        public event Action OnOpen;
        public event Action OnClose;

        private void Awake()
        {
            backButton?.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            OnOpen += OpenTransition;
            OnClose += CloseTransition;
        }

        private void OnDisable()
        {
            OnOpen -= OpenTransition;
            OnClose -= CloseTransition;
        }

        public void Open() => OnOpen?.Invoke();
        public void Close() => OnClose?.Invoke();

        protected virtual void OpenTransition()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.DOFade(1, 0.4f).SetEase(Ease.OutCubic);
        }

        protected virtual void CloseTransition()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.DOFade(0, 0.4f).SetEase(Ease.OutCubic);
        }
    }
}
