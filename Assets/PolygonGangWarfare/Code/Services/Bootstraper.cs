// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Bootstraper.
    /// </summary>
    public static class Bootstraper
    {
        /// <summary>
        /// Initialize.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            ServiceLocator.Initialize();
            ServiceLocator.Current.Register<IGameModeService>(new GameModeService());

            #region Difficulty Manager Service

            var diffPrefab = Resources.Load<DifficultyManager>("DifficultyManager");

            if (diffPrefab != null)
            {
                var diffInstance = Object.Instantiate(diffPrefab);
                diffInstance.name = "Difficulty Manager";

                Object.DontDestroyOnLoad(diffInstance);

                ServiceLocator.Current.Register<IDifficultyService>(diffInstance);
            }

            #endregion

            #region Sound Manager Service

            //Create an object for the sound manager, and add the component!
            var soundManagerObject = new GameObject("Sound Manager");
            var soundManagerService = soundManagerObject.AddComponent<AudioManagerService>();
            
            //Make sure that we never destroy our SoundManager. We need it in other scenes too!
            Object.DontDestroyOnLoad(soundManagerObject);
            
            //Register the sound manager service!
            ServiceLocator.Current.Register<IAudioManagerService>(soundManagerService);

            #endregion
        }
    }
}