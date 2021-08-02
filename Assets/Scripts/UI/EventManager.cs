using System;
using GeneticAlgorithms.Entities;
using UnityEngine;

namespace UI
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager current;
        
        public static event Action<GameDataEnum,int> OnUpdateGameStats;
        public static event Action<GameData> OnSendGameStats;
        
        public static event Action OnGameStart;
        public static event Action OnGameEnd;
        
        public static event Action OnGAStart;
        public static event Action OnGAEnd;
        
        private void Awake()
        {
            current = this;
        }

        public void UpdateGameStats(GameDataEnum field, int value)
        {
            OnUpdateGameStats?.Invoke(field, value);
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
