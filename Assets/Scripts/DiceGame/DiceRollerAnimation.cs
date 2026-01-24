#if CMPSETUP_COMPLETE
using UnityEngine;
using TMPro;
using System;
using System.Collections;

namespace DiceBaby
{
    public class DiceRollerAnimation : MonoBehaviour
    {
        [Header("Dice Text Elements")]
        [SerializeField] private TextMeshProUGUI leftDiceText;
        [SerializeField] private TextMeshProUGUI rightDiceText;
        
        [Header("Result Display")]
        [SerializeField] private TextMeshProUGUI totalResultText;
        [SerializeField] private GameObject rollerPanel;
        
        [Header("Animation Settings")]
        [SerializeField] private float rollDuration = 3f;
        [SerializeField] private float initialSpeed = 0.05f;
        [SerializeField] private float finalSpeed = 0.3f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color rollingColor = Color.yellow;
        [SerializeField] private Color finalColor = Color.green;
        
        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip tickSound;
        [SerializeField] private AudioClip finalSound;
        
        private bool _isRolling = false;
        private int _leftResult = 0;
        private int _rightResult = 0;
        
        public event Action<int> OnRollComplete; // Returns the total (left + right)
        
        public bool IsRolling => _isRolling;
        public int LastResult => _leftResult + _rightResult;
        
        private void Awake()
        {
            ResetDisplay();
        }
        
        public void Show()
        {
            if (rollerPanel) rollerPanel.SetActive(true);
            ResetDisplay();
        }
        
        public void Hide()
        {
            if (rollerPanel) rollerPanel.SetActive(false);
        }
        
        private void ResetDisplay()
        {
            if (leftDiceText) 
            {
                leftDiceText.text = "?";
                leftDiceText.color = normalColor;
            }
            if (rightDiceText) 
            {
                rightDiceText.text = "?";
                rightDiceText.color = normalColor;
            }
            if (totalResultText) totalResultText.text = "";
        }
        
        public void StartRoll(int? forcedLeftResult = null, int? forcedRightResult = null)
        {
            if (_isRolling) return;
            
            // Determine final results (random or forced)
            _leftResult = forcedLeftResult ?? UnityEngine.Random.Range(1, 7);
            _rightResult = forcedRightResult ?? UnityEngine.Random.Range(1, 7);
            
            StartCoroutine(RollAnimation());
        }
        
        private IEnumerator RollAnimation()
        {
            _isRolling = true;
            
            if (leftDiceText) leftDiceText.color = rollingColor;
            if (rightDiceText) rightDiceText.color = rollingColor;
            if (totalResultText) totalResultText.text = "";
            
            float elapsed = 0f;
            float lastTickTime = 0f;
            
            while (elapsed < rollDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / rollDuration;
                
                // Speed decreases as we approach the end (easing)
                float currentSpeed = Mathf.Lerp(initialSpeed, finalSpeed, Mathf.Pow(progress, 2));
                
                if (elapsed - lastTickTime >= currentSpeed)
                {
                    // Show random numbers during animation
                    int randomLeft = UnityEngine.Random.Range(1, 7);
                    int randomRight = UnityEngine.Random.Range(1, 7);
                    
                    if (leftDiceText) leftDiceText.text = randomLeft.ToString();
                    if (rightDiceText) rightDiceText.text = randomRight.ToString();
                    
                    PlayTickSound();
                    lastTickTime = elapsed;
                }
                
                yield return null;
            }
            
            // Show final results
            if (leftDiceText) 
            {
                leftDiceText.text = _leftResult.ToString();
                leftDiceText.color = finalColor;
            }
            if (rightDiceText) 
            {
                rightDiceText.text = _rightResult.ToString();
                rightDiceText.color = finalColor;
            }
            
            // Pulse effect on final numbers
            StartCoroutine(PulseText(leftDiceText));
            StartCoroutine(PulseText(rightDiceText));
            
            // Show total
            int total = _leftResult + _rightResult;
            if (totalResultText)
            {
                totalResultText.text = $"= {total}";
                totalResultText.color = finalColor;
            }
            
            PlayFinalSound();
            
            _isRolling = false;
            OnRollComplete?.Invoke(total);
        }
        
        private IEnumerator PulseText(TextMeshProUGUI text)
        {
            if (text == null) yield break;
            
            Vector3 originalScale = text.transform.localScale;
            Vector3 targetScale = originalScale * 1.3f;
            
            // Scale up
            float duration = 0.1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                text.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                yield return null;
            }
            
            // Scale down
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                text.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
                yield return null;
            }
            
            text.transform.localScale = originalScale;
        }
        
        private void PlayTickSound()
        {
            if (audioSource != null && tickSound != null)
            {
                audioSource.PlayOneShot(tickSound, 0.3f);
            }
        }
        
        private void PlayFinalSound()
        {
            if (audioSource != null && finalSound != null)
            {
                audioSource.PlayOneShot(finalSound, 1f);
            }
        }
        
        // For testing in editor
        [ContextMenu("Test Roll")]
        private void TestRoll()
        {
            Show();
            StartRoll();
        }
    }
}
#endif
