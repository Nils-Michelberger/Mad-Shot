using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
    {
        #region Public Fields
        
        
        [Tooltip("Pixel offset from the player target")]
        [SerializeField]
        private Vector3 screenOffset = new Vector3(0f,10f,0f);

        public float heightOffset;
        
        #endregion
        
        
        #region Private Fields


        [Tooltip("UI Text to display Player's Name")]
        [SerializeField]
        private Text playerNameText;
        
        [Tooltip("UI Slider to display Player's Health")]
        [SerializeField]
        private Slider playerHealthSlider;

        private PlayerShoot target;
        
        float characterControllerHeight;
        Transform targetTransform;
        Renderer targetRenderer;
        CanvasGroup _canvasGroup;
        Vector3 targetPosition;
        
        #endregion


        #region MonoBehaviour Callbacks

        
        void Awake()
        {
            transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
            
            _canvasGroup = this.GetComponent<CanvasGroup>();
        }
        
        void Update()
        {
            // Reflect the Player Health
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = target.health;
            }
            
            Debug.Log(playerHealthSlider.value);
            Debug.Log(target.health);
            
            // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
            if (target == null)
            {
                Destroy(gameObject);
            }
        }
        
        void LateUpdate()
        {
            // Do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI, but not the player itself.
            if (targetRenderer!=null)
            {
                this._canvasGroup.alpha = targetRenderer.isVisible ? 1f : 0f;
            }


            // #Critical
            // Follow the Target GameObject on screen.
            if (targetTransform != null)
            {
                targetPosition = targetTransform.position;
                targetPosition.y += characterControllerHeight;

                Vector3 worldToScreenPoint = Camera.main.WorldToScreenPoint (targetPosition) + screenOffset;
                if (worldToScreenPoint.z < 0)
                {
                    worldToScreenPoint *= -1;
                }
                
                transform.position = worldToScreenPoint;
            }
        }

        #endregion


        #region Public Methods


        public void SetTarget(PlayerShoot _target)
        {
            if (_target == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayerShoot target for PlayerUI.SetTarget.", this);
                return;
            }
            // Cache references for efficiency
            target = _target;
            if (playerNameText != null)
            {
                playerNameText.text = target.photonView.Owner.NickName;
            }
            
            targetTransform = this.target.GetComponent<Transform>();
            targetRenderer = this.target.GetComponent<Renderer>();
            CharacterController characterController = _target.GetComponent<CharacterController> ();
            // Get data from the Player that won't change during the lifetime of this Component
            if (characterController != null)
            {
                characterControllerHeight = characterController.height + heightOffset;
            }
        }
        
        
        #endregion


    }
