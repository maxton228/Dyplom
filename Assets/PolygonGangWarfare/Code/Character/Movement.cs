// Copyright 2021, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Movement : MovementBehaviour
    {
        #region FIELDS SERIALIZED
        [Header("Jump & Crouch")]
        [SerializeField] private float jumpForce = 5.0f;
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float defaultHeight = 2.0f;

        [Header("Audio Clips")]
        
        [Tooltip("The audio clip that is played while walking.")]
        [SerializeField]
        private AudioClip audioClipWalking;

        [Tooltip("Звук приземлення.")]
        [SerializeField]
        private AudioClip audioClipLand;

        [Tooltip("The audio clip that is played while running.")]
        [SerializeField]
        private AudioClip audioClipRunning;

        [Tooltip("Звук стрибка.")]
        [SerializeField] private AudioClip audioClipJump;
        [Header("Speeds")]

        [Tooltip("How fast the player moves while running."), SerializeField]
        private float speedRunning = 9.0f;
        [SerializeField]
        private float speedWalking = 5.0f;


        [Header("Audio Settings")]
        [SerializeField] private float volumeWalk = 1f;   // Гучність ходьби
        [SerializeField] private float volumeCrouch = 0.6f; // Гучність в присяді
        [SerializeField] private float volumeRun = 2f; // Гучність бігу

        [Header("Audio Settings (Speed/Pitch)")]
        [Tooltip("Швидкість звуку при ходьбі (1 = норма).")]
        [SerializeField] private float pitchWalk = 1.0f;

        [Tooltip("Швидкість звуку при бігу (наприклад, 1.3 = на 30% швидше).")]
        [SerializeField] private float pitchRun = 1.4f;

        [Tooltip("Швидкість звуку в присяді (наприклад, 0.7 = на 30% повільніше).")]
        [SerializeField] private float pitchCrouch = 0.7f;
        [Header("Stealth / Noise Generation")]

        [Tooltip("Радіус шуму, коли крадемось (метри).")]
        [SerializeField] private float noiseRadiusCrouch = 1.0f;

        [Tooltip("Радіус шуму, коли йдемо.")]
        [SerializeField] private float noiseRadiusWalk = 10.0f;

        [Tooltip("Радіус шуму, коли біжимо (дуже гучно!).")]
        [SerializeField] private float noiseRadiusRun = 20.0f;

        [Tooltip("Радіус шуму при стрибку/приземленні.")]
        [SerializeField] private float noiseRadiusImpact = 10.0f;

        [Tooltip("Як часто перевіряти ворогів (секунди). Щоб не грузити процесор.")]
        [SerializeField] private float noiseCheckInterval = 0.2f;
        #endregion

        #region PROPERTIES

        //Velocity.
        private Vector3 Velocity
        {
            //Getter.
            get => rigidBody.linearVelocity;
            //Setter.
            set => rigidBody.linearVelocity = value;
        }

        #endregion

        #region FIELDS
        private float jumpBlockTimer;
        /// <summary>
        /// Attached Rigidbody.
        /// </summary>
        private Rigidbody rigidBody;
        /// <summary>
        /// Attached CapsuleCollider.
        /// </summary>
        private CapsuleCollider capsule;
        /// <summary>
        /// Attached AudioSource.
        /// </summary>
        private AudioSource audioSourceFootsteps; // Для кроків (зациклений)
        private AudioSource audioSourceFX;
        private float noiseTimer;
        /// <summary>
        /// True if the character is currently grounded.
        /// </summary>
        private bool grounded;
        private bool wasGrounded;
        private float airTime;
        private float fallSpeed;
        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        /// <summary>
        /// The player character's equipped weapon.
        /// </summary>
        private WeaponBehaviour equippedWeapon;
        
        /// <summary>
        /// Array of RaycastHits used for ground checking.
        /// </summary>
        private readonly RaycastHit[] groundHits = new RaycastHit[8];
        public bool IsGrounded() => grounded;
        #endregion

        #region UNITY FUNCTIONS

        /// <summary>
        /// Awake.
        /// </summary>
        protected override void Awake()
        {
            //Get Player Character.
            playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        }

        /// Initializes the FpsController on start.
        protected override  void Start()
        {
            //Rigidbody Setup.
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            //Cache the CapsuleCollider.
            capsule = GetComponent<CapsuleCollider>();

            audioSourceFootsteps = GetComponent<AudioSource>();
            audioSourceFootsteps.clip = audioClipWalking;
            audioSourceFootsteps.loop = true;

            // 2. Створюємо другий динамік для ефектів (щоб кроки його не перебивали)
            audioSourceFX = gameObject.AddComponent<AudioSource>();
            audioSourceFX.spatialBlend = 1f; // Робимо звук 3D
            audioSourceFX.playOnAwake = false;
            audioSourceFX.reverbZoneMix = 0f;
        }

        /// Checks if the character is on the ground.
        private void OnCollisionStay()
        {
            //Bounds.
            Bounds bounds = capsule.bounds;
            Vector3 extents = bounds.extents;
            float radius = extents.x - 0.01f;

            Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
                groundHits, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);

            if (!groundHits.Any(hit => hit.collider != null && hit.collider != capsule))
                return;

            for (var i = 0; i < groundHits.Length; i++)
                groundHits[i] = new RaycastHit();

            grounded = true;
        }
			
        protected override void FixedUpdate()
        {
            if (jumpBlockTimer > 0)
            {
                jumpBlockTimer -= Time.fixedDeltaTime;
            }

            // 2. ЛОГІКА ПОВІТРЯ
            if (!grounded)
            {
                airTime += Time.fixedDeltaTime;
                fallSpeed = rigidBody.linearVelocity.y;
            }

            // 2. ПРИЗЕМЛЕННЯ
            if (!wasGrounded && grounded)
            {
                // ПОДВІЙНА ПЕРЕВІРКА:
                // 1. AirTime > 0.2f (щоб не рахувати дрібні стики полігонів)
                // 2. fallSpeed < -4.0f (Тільки якщо ми падали швидко!)
                //    При бігу швидкість падіння десь -0.5...-1.0, тому воно не спрацює.
                //    При стрибку вона буде десь -6...-10.

                if (airTime > 0.2f && fallSpeed < -0.6f)
                {
                    // А) Звук
                    if (audioClipLand != null)
                    {
                        audioSourceFX.volume = 1.0f;
                        audioSourceFX.PlayOneShot(audioClipLand);
                    }

                    // Б) Нахил камери (Drop)
                    if (playerCharacter is Character characterScript)
                    {
                        characterScript.PlayLandingMotion(0.04f);
                        EmitNoise(noiseRadiusImpact, 1.0f);
                    }
                    jumpBlockTimer = 0.25f;
                }

                // Скидаємо лічильники
                airTime = 0f;
                fallSpeed = 0f;
            }

            wasGrounded = grounded;

            MoveCharacter();

            grounded = false;
        }

        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        protected override  void Update()
        {
            //Get the equipped weapon!
            equippedWeapon = playerCharacter.GetInventory().GetEquipped();
            PlayFootstepSounds();
            if (grounded && rigidBody.linearVelocity.sqrMagnitude > 0.1f)
            {
                noiseTimer -= Time.deltaTime;
                if (noiseTimer <= 0)
                {
                    CheckNoiseEmission();
                    noiseTimer = noiseCheckInterval; // Скидаємо таймер
                }
            }
        }

        #endregion

        #region METHODS
        private void CheckNoiseEmission()
        {
            float currentRadius = noiseRadiusWalk;
            float currentVolume = 1.0f; // Умовна "сила" звуку для AI

            if (playerCharacter.IsRunning())
            {
                currentRadius = noiseRadiusRun;
                currentVolume = 2.0f; // Біг дуже помітний
            }
            else if (playerCharacter is Character character && character.IsCrouching())
            {
                currentRadius = noiseRadiusCrouch;
                currentVolume = 0.3f; // Присід майже непомітний
            }

            // Відправляємо сигнал
            EmitNoise(currentRadius, currentVolume);
        }

        // Головний метод, який "кричить" ворогам
        private void EmitNoise(float radius, float volume)
        {
            // Знаходимо всі колайдери навколо в радіусі шуму
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

            foreach (var hitCollider in hitColliders)
            {
                // Шукаємо на об'єкті скрипт ворога
                StealthAgent enemy = hitCollider.GetComponent<StealthAgent>();

                if (enemy != null)
                {
                    // Якщо знайшли - кажемо йому "Я тут!"
                    enemy.RegisterNoise(transform.position, volume);
                }
            }
        }
        private void MoveCharacter()
        {
            #region Calculate Movement Velocity
            Vector2 frameInput = playerCharacter.GetInputMovement();
            var movement = new Vector3(frameInput.x, 0.0f, frameInput.y);

            if (playerCharacter.IsRunning())
                movement *= speedRunning;
            else
                movement *= speedWalking;

            movement = transform.TransformDirection(movement);
            #endregion

            if (playerCharacter is Character characterComponent)
            {
                bool isCrouching = characterComponent.IsCrouching();
                capsule.height = Mathf.Lerp(capsule.height, isCrouching ? crouchHeight : defaultHeight, Time.fixedDeltaTime * 10f);

                if (isCrouching) movement *= 0.5f;
            }

            float currentYVelocity = rigidBody.linearVelocity.y;

            // СТРИБОК
            if (grounded && (playerCharacter as Character).IsJumping() && jumpBlockTimer <= 0)
            {
                currentYVelocity = jumpForce;
                grounded = false;

                // Звук стрибка (через ОКРЕМИЙ канал)
                if (audioClipJump != null)
                {
                    audioSourceFX.PlayOneShot(audioClipJump);
                    EmitNoise(noiseRadiusImpact, 1.0f);

                }
            }

            Velocity = new Vector3(movement.x, currentYVelocity, movement.z);
        }

        /// <summary>
        /// Plays Footstep Sounds. This code is slightly old, so may not be great, but it functions alright-y!
        /// </summary>
        private void PlayFootstepSounds()
        {
            if (grounded && rigidBody.linearVelocity.sqrMagnitude > 0.1f)
            {
                bool isCrouching = false;
                if (playerCharacter is Character character)
                    isCrouching = character.IsCrouching();

                audioSourceFootsteps.clip = playerCharacter.IsRunning() ? audioClipRunning : audioClipWalking;

                if (playerCharacter.IsRunning())
                {
                    audioSourceFootsteps.volume = volumeRun;
                    audioSourceFootsteps.pitch = pitchRun;
                }
                else if (isCrouching)
                {
                    audioSourceFootsteps.volume = volumeCrouch;
                    audioSourceFootsteps.volume = volumeCrouch;
                    audioSourceFootsteps.pitch = pitchCrouch;
                }
                else
                {
                    audioSourceFootsteps.volume = volumeWalk;
                    audioSourceFootsteps.pitch = pitchWalk;   // Нормально
                }

                if (!audioSourceFootsteps.isPlaying)
                    audioSourceFootsteps.Play();
            }
            else if (audioSourceFootsteps.isPlaying)
            {
                // Це тепер зупиняє тільки кроки! Приземлення грає далі.
                audioSourceFootsteps.Pause();
            }
        }

        #endregion
    }
}