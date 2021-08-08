using System;
using GeneticAlgorithms.Entities;
using UnityEngine;

namespace UI
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager current;
        
        public static event Action<GameData> OnSendGameStats;

        public static event Action<int> OnCurrentChromosomeInPlay;
        public static event Action OnGameStart;
        
        public static event Action<bool> OnGameLocked;
        public static event Action OnGameEnd;
        
        public static event Action OnGAStart;
        public static event Action OnGAEnd;
        
        private void Awake()
        {
            current = this;
        }

        public void CurrentChromosomeInPlay(int id)
        {
            OnCurrentChromosomeInPlay?.Invoke(id);
        }
        
        public void GameLocked(bool isLocked)
        {
            OnGameLocked?.Invoke(isLocked);
        }

        public void SendGameStats(GameData gamedata)
        {
            OnSendGameStats?.Invoke(gamedata);
        }

        public void GameStart()
        {
            OnGameStart?.Invoke();
        }
        
        public void GameEnd()
        {
            OnGameEnd?.Invoke();
        }

        public void GAStart()
        {
            OnGAStart?.Invoke();
        }
        
        public void GAEnd()
        {
            OnGAEnd?.Invoke();
        }
    }
}
