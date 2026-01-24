#if CMPSETUP_COMPLETE
using UnityEngine;
using Fusion;
using System;
using System.Collections;

namespace DiceBaby
{
    public enum GamePhase
    {
        WaitingForPlayers,
        Countdown,
        Player1Turn,
        Player2Turn,
        RoundResult,
        GameOver
    }

    public class DiceGameManager : NetworkBehaviour
    {
        public static DiceGameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private int maxRollsPerRound = 2;
        [SerializeField] private float countdownTime = 3f;
        [SerializeField] private float resultDisplayTime = 2f;

        // Networked game state
        [Networked] public GamePhase CurrentPhase { get; set; }
        [Networked] public int Player1Lives { get; set; }
        [Networked] public int Player2Lives { get; set; }
        [Networked] public int Player1DiceResult { get; set; }
        [Networked] public int Player2DiceResult { get; set; }
        [Networked] public int Player1RollsThisRound { get; set; }
        [Networked] public int Player2RollsThisRound { get; set; }
        [Networked] public NetworkBool Player1FinishedRolling { get; set; }
        [Networked] public NetworkBool Player2FinishedRolling { get; set; }
        [Networked] public float CountdownTimer { get; set; }
        [Networked] public PlayerRef Player1Ref { get; set; }
        [Networked] public PlayerRef Player2Ref { get; set; }

        // Events for UI
        public event Action<GamePhase> OnPhaseChanged;
        public event Action<int, int> OnLivesChanged; // player1Lives, player2Lives
        public event Action<int> OnMyDiceResultChanged; // only your own result
        public event Action<float> OnCountdownTick;
        public event Action<int, int, int> OnRoundComplete; // winner, myResult, opponentResult

        private GamePhase _lastPhase;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                // Initialize game state
                Player1Lives = maxLives;
                Player2Lives = maxLives;
                CurrentPhase = GamePhase.WaitingForPlayers;
                ResetRound();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;

            switch (CurrentPhase)
            {
                case GamePhase.WaitingForPlayers:
                    CheckForPlayers();
                    break;
                case GamePhase.Countdown:
                    UpdateCountdown();
                    break;
                case GamePhase.Player1Turn:
                case GamePhase.Player2Turn:
                    CheckTurnComplete();
                    break;
                case GamePhase.RoundResult:
                    // Handled by coroutine
                    break;
            }

            // Notify phase changes
            if (_lastPhase != CurrentPhase)
            {
                _lastPhase = CurrentPhase;
                RPC_NotifyPhaseChanged(CurrentPhase);
            }
        }

        private void CheckForPlayers()
        {
            // Check if we have 2 players
            int playerCount = 0;
            foreach (var player in Runner.ActivePlayers)
            {
                playerCount++;
                if (playerCount == 1 && Player1Ref == default)
                    Player1Ref = player;
                else if (playerCount == 2 && Player2Ref == default)
                    Player2Ref = player;
            }

            if (playerCount >= 2)
            {
                StartCountdown();
            }
        }

        private void StartCountdown()
        {
            CountdownTimer = countdownTime;
            CurrentPhase = GamePhase.Countdown;
        }

        private void UpdateCountdown()
        {
            CountdownTimer -= Runner.DeltaTime;
            RPC_NotifyCountdown(CountdownTimer);

            if (CountdownTimer <= 0)
            {
                CurrentPhase = GamePhase.Player1Turn;
            }
        }

        private void CheckTurnComplete()
        {
            // Check if both players finished rolling
            if (Player1FinishedRolling && Player2FinishedRolling)
            {
                DetermineRoundWinner();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RequestRoll(PlayerRef player)
        {
            // Roll the dice (2-12 for two dice)
            int result = UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7);
            RPC_RequestRollWithResult(player, result);
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RequestRollWithResult(PlayerRef player, int result)
        {
            bool isPlayer1 = player == Player1Ref;
            int currentRolls = isPlayer1 ? Player1RollsThisRound : Player2RollsThisRound;

            if (currentRolls >= maxRollsPerRound) return;

            // Check if it's this player's turn phase or if we allow simultaneous
            bool canRoll = (CurrentPhase == GamePhase.Player1Turn && isPlayer1) ||
                          (CurrentPhase == GamePhase.Player2Turn && !isPlayer1) ||
                          (CurrentPhase == GamePhase.Player1Turn || CurrentPhase == GamePhase.Player2Turn);

            if (!canRoll) return;

            // Clamp result to valid range (2-12 for two dice)
            result = Mathf.Clamp(result, 2, 12);

            if (isPlayer1)
            {
                Player1DiceResult = result;
                Player1RollsThisRound++;
                // Only notify this player of their own result
                RPC_NotifyMyDiceResult(player, result);
                
                // Auto-finish if used all rolls
                if (Player1RollsThisRound >= maxRollsPerRound)
                {
                    Player1FinishedRolling = true;
                    if (!Player2FinishedRolling)
                        CurrentPhase = GamePhase.Player2Turn;
                }
            }
            else
            {
                Player2DiceResult = result;
                Player2RollsThisRound++;
                // Only notify this player of their own result
                RPC_NotifyMyDiceResult(player, result);
                
                // Auto-finish if used all rolls
                if (Player2RollsThisRound >= maxRollsPerRound)
                {
                    Player2FinishedRolling = true;
                    if (!Player1FinishedRolling)
                        CurrentPhase = GamePhase.Player1Turn;
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_FinishRolling(PlayerRef player)
        {
            if (player == Player1Ref)
            {
                Player1FinishedRolling = true;
                if (!Player2FinishedRolling)
                    CurrentPhase = GamePhase.Player2Turn;
            }
            else if (player == Player2Ref)
            {
                Player2FinishedRolling = true;
                if (!Player1FinishedRolling)
                    CurrentPhase = GamePhase.Player1Turn;
            }
        }

        private void DetermineRoundWinner()
        {
            CurrentPhase = GamePhase.RoundResult;
            
            int winner = 0; // 0 = tie
            if (Player1DiceResult > Player2DiceResult)
            {
                winner = 1;
                Player2Lives--;
            }
            else if (Player2DiceResult > Player1DiceResult)
            {
                winner = 2;
                Player1Lives--;
            }
            // If tie, no one loses a life

            // Now reveal both results to both players
            RPC_NotifyRoundComplete(Player1Ref, winner, Player1DiceResult, Player2DiceResult, Player1Lives, Player2Lives);
            RPC_NotifyRoundComplete(Player2Ref, winner, Player2DiceResult, Player1DiceResult, Player2Lives, Player1Lives);

            // Check for game over
            if (Player1Lives <= 0 || Player2Lives <= 0)
            {
                CurrentPhase = GamePhase.GameOver;
                RPC_NotifyGameOver(Player1Lives > 0 ? 1 : 2);
            }
            else
            {
                // Start next round after delay
                StartCoroutine(StartNextRoundAfterDelay());
            }
        }

        private IEnumerator StartNextRoundAfterDelay()
        {
            yield return new WaitForSeconds(resultDisplayTime);
            ResetRound();
            StartCountdown();
        }

        private void ResetRound()
        {
            Player1DiceResult = 0;
            Player2DiceResult = 0;
            Player1RollsThisRound = 0;
            Player2RollsThisRound = 0;
            Player1FinishedRolling = false;
            Player2FinishedRolling = false;
        }

        public bool IsLocalPlayerTurn()
        {
            var localPlayer = Runner.LocalPlayer;
            if (CurrentPhase == GamePhase.Player1Turn && localPlayer == Player1Ref)
                return true;
            if (CurrentPhase == GamePhase.Player2Turn && localPlayer == Player2Ref)
                return true;
            return false;
        }

        public bool CanLocalPlayerRoll()
        {
            var localPlayer = Runner.LocalPlayer;
            bool isPlayer1 = localPlayer == Player1Ref;
            int rolls = isPlayer1 ? Player1RollsThisRound : Player2RollsThisRound;
            bool finished = isPlayer1 ? Player1FinishedRolling : Player2FinishedRolling;
            
            return !finished && rolls < maxRollsPerRound && 
                   (CurrentPhase == GamePhase.Player1Turn || CurrentPhase == GamePhase.Player2Turn);
        }

        public int GetLocalPlayerRollsRemaining()
        {
            var localPlayer = Runner.LocalPlayer;
            bool isPlayer1 = localPlayer == Player1Ref;
            int rolls = isPlayer1 ? Player1RollsThisRound : Player2RollsThisRound;
            return maxRollsPerRound - rolls;
        }

        public int GetLocalPlayerDiceResult()
        {
            var localPlayer = Runner.LocalPlayer;
            return localPlayer == Player1Ref ? Player1DiceResult : Player2DiceResult;
        }

        public int GetLocalPlayerLives()
        {
            var localPlayer = Runner.LocalPlayer;
            return localPlayer == Player1Ref ? Player1Lives : Player2Lives;
        }

        public int GetOpponentLives()
        {
            var localPlayer = Runner.LocalPlayer;
            return localPlayer == Player1Ref ? Player2Lives : Player1Lives;
        }

        public bool HasLocalPlayerFinished()
        {
            var localPlayer = Runner.LocalPlayer;
            return localPlayer == Player1Ref ? Player1FinishedRolling : Player2FinishedRolling;
        }

        public bool HasOpponentFinished()
        {
            var localPlayer = Runner.LocalPlayer;
            return localPlayer == Player1Ref ? Player2FinishedRolling : Player1FinishedRolling;
        }

        // RPCs for notifying all clients
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyPhaseChanged(GamePhase phase)
        {
            OnPhaseChanged?.Invoke(phase);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyCountdown(float time)
        {
            OnCountdownTick?.Invoke(time);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyMyDiceResult([RpcTarget] PlayerRef target, int myResult)
        {
            OnMyDiceResultChanged?.Invoke(myResult);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyRoundComplete([RpcTarget] PlayerRef target, int winner, int myResult, int opponentResult, int myLives, int opponentLives)
        {
            // Convert winner from player1/player2 to local perspective
            int localWinner = winner; // 0 = tie, 1 = I won, 2 = opponent won
            if (winner == 1 && target == Player2Ref) localWinner = 2;
            else if (winner == 2 && target == Player2Ref) localWinner = 1;
            else if (winner == 1 && target == Player1Ref) localWinner = 1;
            else if (winner == 2 && target == Player1Ref) localWinner = 2;
            
            OnRoundComplete?.Invoke(localWinner, myResult, opponentResult);
            OnLivesChanged?.Invoke(myLives, opponentLives);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyGameOver(int winner)
        {
            Debug.Log($"Game Over! Player {winner} wins!");
        }
    }
}
#endif

