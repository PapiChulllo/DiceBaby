#if CMPSETUP_COMPLETE
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace DiceBaby
{
    public class DiceGameUI : MonoBehaviour
    {
        [Header("Countdown")]
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("Dice Display")]
        [SerializeField] private GameObject dicePanel;
        [SerializeField] private TextMeshProUGUI myDiceResultText;
        [SerializeField] private TextMeshProUGUI opponentDiceResultText;
        [SerializeField] private TextMeshProUGUI rollsRemainingText;
        
        [Header("Dice Roller Animation")]
        [SerializeField] private DiceRollerAnimation diceRollerAnimation;

        [Header("Buttons")]
        [SerializeField] private Button rollButton;
        [SerializeField] private Button keepButton; // "Done rolling" button
        [SerializeField] private TextMeshProUGUI rollButtonText;

        [Header("Lives Display")]
        [SerializeField] private TextMeshProUGUI myLivesText;
        [SerializeField] private TextMeshProUGUI opponentLivesText;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private TextMeshProUGUI waitingText;

        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;

        [Header("Result")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;

        private DiceGameManager _gameManager;
        private bool _isPlayer1;

        private void Start()
        {
            // Hide all panels initially
            if (countdownPanel) countdownPanel.SetActive(false);
            if (dicePanel) dicePanel.SetActive(false);
            if (waitingPanel) waitingPanel.SetActive(false);
            if (gameOverPanel) gameOverPanel.SetActive(false);
            if (resultPanel) resultPanel.SetActive(false);

            // Setup button listeners
            if (rollButton) rollButton.onClick.AddListener(OnRollButtonClicked);
            if (keepButton) keepButton.onClick.AddListener(OnKeepButtonClicked);

            StartCoroutine(WaitForGameManager());
        }

        private IEnumerator WaitForGameManager()
        {
            while (DiceGameManager.Instance == null)
            {
                yield return null;
            }

            _gameManager = DiceGameManager.Instance;
            
            // Subscribe to events
            _gameManager.OnPhaseChanged += HandlePhaseChanged;
            _gameManager.OnCountdownTick += HandleCountdownTick;
            _gameManager.OnMyDiceResultChanged += HandleMyDiceResultChanged;
            _gameManager.OnLivesChanged += HandleLivesChanged;
            _gameManager.OnRoundComplete += HandleRoundComplete;

            // Determine if we're player 1 or 2
            yield return new WaitForSeconds(0.5f);
            _isPlayer1 = _gameManager.Runner.LocalPlayer == _gameManager.Player1Ref;

            UpdateLivesDisplay();
        }

        private void OnDestroy()
        {
            if (_gameManager != null)
            {
                _gameManager.OnPhaseChanged -= HandlePhaseChanged;
                _gameManager.OnCountdownTick -= HandleCountdownTick;
                _gameManager.OnMyDiceResultChanged -= HandleMyDiceResultChanged;
                _gameManager.OnLivesChanged -= HandleLivesChanged;
                _gameManager.OnRoundComplete -= HandleRoundComplete;
            }
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            // Hide all panels first
            if (countdownPanel) countdownPanel.SetActive(false);
            if (dicePanel) dicePanel.SetActive(false);
            if (waitingPanel) waitingPanel.SetActive(false);
            if (resultPanel) resultPanel.SetActive(false);

            switch (phase)
            {
                case GamePhase.WaitingForPlayers:
                    if (waitingPanel) waitingPanel.SetActive(true);
                    if (waitingText) waitingText.text = "Waiting for opponent...";
                    break;

                case GamePhase.Countdown:
                    if (countdownPanel) countdownPanel.SetActive(true);
                    break;

                case GamePhase.Player1Turn:
                case GamePhase.Player2Turn:
                    if (dicePanel) dicePanel.SetActive(true);
                    // Reset dice display for new round
                    if (myDiceResultText) myDiceResultText.text = "-";
                    if (opponentDiceResultText) opponentDiceResultText.text = "?";
                    UpdateDiceUI();
                    UpdateStatusText(phase);
                    break;

                case GamePhase.RoundResult:
                    if (resultPanel) resultPanel.SetActive(true);
                    break;

                case GamePhase.GameOver:
                    if (gameOverPanel) gameOverPanel.SetActive(true);
                    bool iWon = (_isPlayer1 && _gameManager.Player1Lives > 0) ||
                               (!_isPlayer1 && _gameManager.Player2Lives > 0);
                    if (gameOverText) gameOverText.text = iWon ? "YOU WIN!" : "YOU LOSE!";
                    break;
            }
        }

        private void HandleCountdownTick(float time)
        {
            if (countdownText)
            {
                if (time > 0)
                    countdownText.text = Mathf.CeilToInt(time).ToString();
                else
                    countdownText.text = "ROLL!";
            }
        }

        private void HandleMyDiceResultChanged(int myResult)
        {
            if (myDiceResultText)
                myDiceResultText.text = myResult > 0 ? myResult.ToString() : "-";
            
            // Opponent result stays hidden until round ends
            if (opponentDiceResultText)
                opponentDiceResultText.text = "?";

            UpdateDiceUI();
        }

        private void HandleLivesChanged(int myLives, int opponentLives)
        {
            if (myLivesText)
                myLivesText.text = $"You: {new string('♥', Mathf.Max(0, myLives))}";
            
            if (opponentLivesText)
                opponentLivesText.text = $"Opp: {new string('♥', Mathf.Max(0, opponentLives))}";
        }

        private void HandleRoundComplete(int winner, int myResult, int opponentResult)
        {
            // Now reveal opponent's result
            if (myDiceResultText)
                myDiceResultText.text = myResult.ToString();
            
            if (opponentDiceResultText)
                opponentDiceResultText.text = opponentResult.ToString();

            if (resultText == null) return;

            if (winner == 0)
            {
                resultText.text = $"TIE! ({myResult} vs {opponentResult}) No lives lost.";
            }
            else if (winner == 1)
            {
                resultText.text = $"You WIN! ({myResult} vs {opponentResult})";
            }
            else
            {
                resultText.text = $"You LOSE! ({myResult} vs {opponentResult}) -1 Life";
            }
        }

        private void UpdateDiceUI()
        {
            if (_gameManager == null) return;

            int rollsRemaining = _gameManager.GetLocalPlayerRollsRemaining();
            bool canRoll = _gameManager.CanLocalPlayerRoll();

            if (rollsRemainingText)
                rollsRemainingText.text = $"Rolls Left: {rollsRemaining}";

            if (rollButton)
            {
                rollButton.interactable = canRoll && rollsRemaining > 0;
                if (rollButtonText)
                    rollButtonText.text = rollsRemaining > 0 ? "ROLL" : "No Rolls Left";
            }

            if (keepButton)
            {
                // Can only keep if you've rolled at least once
                int myResult = _gameManager.GetLocalPlayerDiceResult();
                keepButton.interactable = myResult > 0 && canRoll;
            }
        }

        private void UpdateStatusText(GamePhase phase)
        {
            if (statusText == null) return;

            bool isMyTurn = _gameManager.IsLocalPlayerTurn();
            
            if (isMyTurn)
            {
                statusText.text = "YOUR TURN - Roll or Keep!";
            }
            else
            {
                statusText.text = "Opponent's turn...";
            }
        }

        private void UpdateLivesDisplay()
        {
            if (_gameManager == null) return;

            int myLives = _gameManager.GetLocalPlayerLives();
            int oppLives = _gameManager.GetOpponentLives();

            if (myLivesText)
                myLivesText.text = $"You: {new string('♥', Mathf.Max(0, myLives))}";
            
            if (opponentLivesText)
                opponentLivesText.text = $"Opp: {new string('♥', Mathf.Max(0, oppLives))}";
        }

        private void OnRollButtonClicked()
        {
            if (_gameManager == null || !_gameManager.CanLocalPlayerRoll()) return;
            
            // If we have an animation, play it first
            if (diceRollerAnimation != null && !diceRollerAnimation.IsRolling)
            {
                // Disable button during animation
                if (rollButton) rollButton.interactable = false;
                if (keepButton) keepButton.interactable = false;
                
                // Subscribe to animation complete
                diceRollerAnimation.OnRollComplete += OnAnimationComplete;
                diceRollerAnimation.Show();
                diceRollerAnimation.StartRoll();
            }
            else
            {
                // No animation, just roll directly
                _gameManager.RPC_RequestRoll(_gameManager.Runner.LocalPlayer);
            }
        }
        
        private void OnAnimationComplete(int totalResult)
        {
            if (diceRollerAnimation != null)
            {
                diceRollerAnimation.OnRollComplete -= OnAnimationComplete;
            }
            
            // Send the roll result to the network
            if (_gameManager != null)
            {
                _gameManager.RPC_RequestRollWithResult(_gameManager.Runner.LocalPlayer, totalResult);
                
                // Auto-keep after a short delay to let the network update
                StartCoroutine(CheckAutoKeep());
            }
        }
        
        private IEnumerator CheckAutoKeep()
        {
            // Wait a frame for the network to update roll count
            yield return null;
            yield return null;
            
            if (_gameManager == null) yield break;
            
            // If no rolls remaining, automatically finish rolling
            int rollsRemaining = _gameManager.GetLocalPlayerRollsRemaining();
            if (rollsRemaining <= 0 && !_gameManager.HasLocalPlayerFinished())
            {
                _gameManager.RPC_FinishRolling(_gameManager.Runner.LocalPlayer);
            }
            else
            {
                // Re-enable buttons if still have rolls
                UpdateDiceUI();
            }
        }

        private void OnKeepButtonClicked()
        {
            if (_gameManager == null) return;

            _gameManager.RPC_FinishRolling(_gameManager.Runner.LocalPlayer);
        }
    }
}
#endif

