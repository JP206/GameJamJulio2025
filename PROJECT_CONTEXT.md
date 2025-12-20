# 🎮 PROYECTO: Survival Isométrico de Pollos - Documentación Completa

## 📋 ÍNDICE
1. [Descripción General](#descripción-general)
2. [Estructura del Proyecto](#estructura-del-proyecto)
3. [Escenas del Juego](#escenas-del-juego)
4. [Sistema del Jugador](#sistema-del-jugador)
5. [Sistema de Enemigos](#sistema-de-enemigos)
6. [Boss Final: Pollo Loco](#boss-final-pollo-loco)
7. [Sistema de Waves](#sistema-de-waves)
8. [Sistema del Portal](#sistema-del-portal)
9. [Persistencia de Datos](#persistencia-de-datos)
10. [Sistema de Cinemáticas](#sistema-de-cinemáticas)
11. [Sistema de UI](#sistema-de-ui)
12. [Sistema de Audio](#sistema-de-audio)
13. [Objetos del Escenario](#objetos-del-escenario)
14. [Animaciones](#animaciones)
15. [Referencia de Scripts](#referencia-de-scripts)
16. [Flujo Completo del Juego](#flujo-completo-del-juego)

---

## Descripción General

**Género:** Survival 2D de vista isométrica  
**Motor:** Unity  
**Objetivo:** Sobrevivir waves de pollos enemigos hasta la wave 5, donde aparece un portal que transporta al jugador a una pelea de boss final contra "Pollo Loco".

### Mecánicas Principales:
- Disparar (municiones limitadas)
- Roll/Dash con invulnerabilidad
- HolyShot (ataque cargado especial)
- Curación con items
- Sistema de waves progresivo
- Boss fight con IA de combate (4 ataques)

---

## Estructura del Proyecto

```
Assets/
├── Scenes/                    # 5 escenas del juego
│   ├── MainMenu.unity         # Menú principal
│   ├── Loading.unity          # Pantalla de carga
│   ├── DIARY CUT.unity        # Cinemática/historia
│   ├── RoyalScene.unity       # Escena principal survival
│   └── Final Boss.unity       # Pelea del boss final
│
├── Scripts/                   # 42 scripts C#
│   ├── Player/                # Scripts del jugador
│   │   ├── PlayerMovement.cs
│   │   ├── PlayerHealth.cs
│   │   ├── _GunController.cs
│   │   ├── PlayerAim.cs
│   │   ├── Bullet.cs
│   │   ├── HolyBullet.cs
│   │   ├── BulletPool.cs
│   │   ├── HolyMeterController.cs
│   │   ├── HolyMeterUI.cs
│   │   ├── HolyShotController.cs
│   │   └── PlayerHealthtUI.cs
│   │
│   ├── Enemy/                 # Scripts de enemigos
│   │   ├── EnemyController.cs
│   │   ├── EnemyPool.cs
│   │   └── Pollo Loco/
│   │       ├── PolloLocoController.cs
│   │       ├── Attacks/
│   │       │   ├── PolloLocoAttackHandler.cs
│   │       │   └── GoldenChairProjectile.cs
│   │       ├── Cinematic/
│   │       │   ├── BossDefeatSequence.cs
│   │       │   ├── CinematicBarsController.cs
│   │       │   ├── CinematicBossIntro.cs
│   │       │   ├── CinematicCameraFocus.cs
│   │       │   ├── CinematicPlayerController.cs
│   │       │   ├── CinematicUIManager.cs
│   │       │   └── FinalBossCinematic.cs
│   │       └── Detector/
│   │           └── PolloLocoBoundary.cs
│   │
│   ├── Waves/                 # Sistema de waves
│   │   └── WaveManager.cs
│   │
│   ├── UI/                    # Interfaces y managers
│   │   ├── GameManager.cs
│   │   ├── UICanvasManager.cs
│   │   ├── BossSceneInitiializer.cs
│   │   ├── FinalDoorCinematic.cs
│   │   ├── UIFinalDoor.cs
│   │   ├── UIAnimation.cs
│   │   ├── UICamera.cs
│   │   ├── ObjectsManager.cs
│   │   └── Fire.cs
│   │
│   ├── Main Menu/
│   │   └── OptionsMenu.cs
│   │
│   ├── MenuManager.cs
│   ├── ChangeScene.cs
│   ├── ChangeScenes.cs
│   ├── LoadingScene.cs
│   ├── IntroCinematicManager.cs
│   ├── Final Door.cs
│   └── IMenuManager.cs
│
├── Prefabs/
│   ├── Player/
│   │   ├── Player.prefab
│   │   ├── Bullet.prefab
│   │   ├── HolyBullet.prefab
│   │   ├── Holy Blast.prefab
│   │   └── ShootingParticleSystem.prefab
│   │
│   ├── Enemies/
│   │   ├── Enemy.prefab
│   │   ├── Enemy boss.prefab
│   │   ├── Gallo boss.prefab
│   │   ├── PolloLoco.prefab
│   │   └── [Particle Systems]
│   │
│   └── Scene elements/
│       ├── Boss Door.prefab
│       ├── GoldenChair.prefab
│       ├── Healing Energizer.prefab
│       ├── Balde pollo.prefab (munición)
│       └── [Otros elementos]
│
├── Animation/
│   ├── Player/                # Animaciones del jugador
│   ├── Enemy/                 # Animaciones de enemigos
│   ├── Pollo Loco/           # Animaciones del boss
│   ├── Gallo/                # Animaciones del gallo
│   └── UI/                   # Animaciones de UI
│
├── Audio/                    # Música y efectos de sonido
├── Images/                   # Sprites
├── Fonts/                    # Fuentes
├── Material/                 # Materiales
└── Textures/                 # Texturas
```

---

## Escenas del Juego

| Escena | Propósito | Scripts Principales |
|--------|-----------|---------------------|
| `MainMenu.unity` | Menú principal con opciones y botón "Jugar" | `MenuManager`, `OptionsMenu` |
| `DIARY CUT.unity` | Cinemática introductoria/narrativa | `ChangeScene` |
| `Loading.unity` | Pantalla de carga con barra de progreso | `LoadingScene` |
| `RoyalScene.unity` | **Escena principal** del survival con waves | `WaveManager`, `IntroCinematicManager`, `EnemyPool` |
| `Final Boss.unity` | Pelea final contra Pollo Loco | `FinalBossCinematic`, `BossSceneInitializer`, `PolloLocoController` |

### Flujo de Escenas:
```
MainMenu → Loading → DIARY CUT → RoyalScene (waves 1-4) → [Portal] → Final Boss
```

---

## Sistema del Jugador

### Componentes del Prefab Player:
- `PlayerMovement` - Movimiento y roll (contiene `IsAnyCinematicPlaying()`)
- `PlayerHealth` - Sistema de vida
- `_GunController` - Disparo y HolyShot
- `PlayerAim` - Apuntado/rotación con mouse (bloquea durante cinemáticas)
- `Rigidbody2D` (Dynamic)
- `BoxCollider2D`
- `SpriteRenderer`
- `Animator`
- `TrailRenderer` (efecto de roll - **desactivado por defecto**, se activa solo durante roll)
- `AudioSource` (sonido de pasos)

### Controles:
| Input | Acción |
|-------|--------|
| WASD | Movimiento |
| Mouse | Apuntar |
| Click Izquierdo | Disparar (consume munición) |
| Click Derecho | HolyShot (cooldown 10s, daño 5x) |
| Espacio | Roll (dash con invulnerabilidad) |
| ESC | Pausa |

### Estadísticas Base:
```csharp
// PlayerMovement.cs
moveSpeed = 5f
rollForce = 10f
rollCooldown = 2f
rollDistance = 3f
rollDuration = 0.2f

// PlayerHealth.cs
maxHealth = 3
invulnerabilityTime = 2f
flashInterval = 0.1f

// _GunController.cs
bulletSpeed = 15f
fireRate = 0.2f
holyBulletInterval = 10f
maxAngle = 45f  // Ángulo máximo de disparo
```

### PlayerMovement.cs - Funcionalidades:
```csharp
// Propiedades públicas
public bool IsRolling => isRolling;
public bool IsCastingHolyShot => isCastingHolyShot;
public bool IsKnockedBack { get; set; }

// Métodos clave
void HandleInput()           // Lee input WASD
void HandleRoll()            // Detecta Espacio y ejecuta roll
IEnumerator RollCoroutine()  // Lerp de posición durante roll + activa TrailRenderer
void HandleAnimation()       // Sincroniza con Animator
void HandleSpriteFlip()      // Flip según posición del mouse
Vector2 GetVelocity()        // Retorna velocidad actual
void SetHolyShotState(bool)  // Bloquea movimiento durante HolyShot
void ResetPlayerState()      // Resetea estados, trail, animaciones

// Método estático para verificar cinemáticas
public static bool IsAnyCinematicPlaying()
```

### Sistema del TrailRenderer (Roll):
El TrailRenderer está **desactivado por defecto** en la escena y en código:
```csharp
// En Start() - Desactivar trail
if (trail != null)
{
    trail.Clear();
    trail.enabled = false;  // Desactivado por defecto
}

// En RollCoroutine() - Activar solo durante roll
if (trail != null)
{
    trail.Clear();
    trail.enabled = true;   // Activar al iniciar roll
}
// ... duración del roll ...
if (trail != null)
{
    trail.enabled = false;  // Desactivar al terminar roll
}
```
**Importante:** El TrailRenderer debe estar con `enabled = false` en el Inspector de Unity.

### PlayerHealth.cs - Funcionalidades:
```csharp
// Propiedades
public int MaxHealth => maxHealth;
public int CurrentHealth => currentHealth;
public UnityEvent<int, int> OnHealthChanged;  // Evento para UI

// Métodos clave
void TakeDamage(int amount)              // Recibe daño
IEnumerator DamageFlashAndInvulnerability()  // Parpadeo rojo
void SetCurrentHealth(int value)         // Para restaurar desde GameManager
void RegisterAttacker(GameObject)        // Registra quién atacó (para celebración)

// Curación automática con items (tag "Life")
// OnTriggerEnter2D: +15% del HP máximo
```

### _GunController.cs - Funcionalidades:
```csharp
// Disparo normal (click izquierdo)
void FireBullet()
- Consume 1 munición
- Obtiene bala del BulletPool
- Calcula dirección con clamp de ángulo
- Añade velocidad del jugador al proyectil
- Ejecuta animación (fire/FireBackwards/StaticFire)
- Activa y reproduce partículas de disparo

// HolyShot (click derecho)
IEnumerator FireHolyShot()
- Requiere cooldown completado
- Activa animación "HolyShot"
- Bloquea movimiento del jugador
- Dispara HolyBullet

// Métodos públicos
float GetAmmo()
void SetAmmo(float amount)
void RefreshAmmoUI()
void ResetHolyShotState()  // Resetea estado completo de HolyShot

// Recolección de munición (tag "Ammo")
// OnTriggerEnter2D: +10 municiones
```

### Sistema de Particle Systems del Jugador:
Todos los particle systems están **desactivados por defecto** y se activan solo cuando se necesitan:

| Particle System | Ubicación | Cuándo se activa |
|-----------------|-----------|------------------|
| `shootParticlesRight` | Player → Weapon | Al disparar (mirando derecha) |
| `shootParticlesLeft` | Player → Weapon | Al disparar (mirando izquierda) |
| `bloomParticleRight` | Player → Weapon → shooting_bloom | Durante HolyShot (derecha) |
| `lightRayParticleRight` | Player → Weapon → light rayss | Durante HolyShot (derecha) |
| `bloomParticleLeft` | Player → Weapon → shooting_bloom Left | Durante HolyShot (izquierda) |
| `lightRayParticleLeft` | Player → Weapon → light rayss Left | Durante HolyShot (izquierda) |
| `holyAuraPS` | Player → Holy Aura PS | Durante HolyShot |

```csharp
// En Start() - Todos desactivados
shootParticlesRight.gameObject.SetActive(false);
bloomParticleRight.gameObject.SetActive(false);
holyAuraPS.gameObject.SetActive(false);
// ... etc

// Al usar - Se activan temporalmente
particle.gameObject.SetActive(true);
particle.Play();

// Al terminar - Se desactivan
particle.Stop();
particle.gameObject.SetActive(false);
```

### ResetHolyShotState() - Reset completo:
```csharp
void ResetHolyShotState()
{
    StopAllCoroutines();              // Detiene corrutinas de HolyShot
    isChargingHolyShot = false;
    canFireHoly = true;
    
    // Resetear triggers y forzar estado Idle
    animator.ResetTrigger("HolyShot");
    animator.Play("Player_Idle", 0, 0f);
    
    // Desactivar TODOS los particle systems
    holyAuraPS.gameObject.SetActive(false);
    bloomParticleRight.gameObject.SetActive(false);
    // ... etc
    
    playerMovement.SetHolyShotState(false);
}
```
**Importante:** Este método se llama en `Start()` para evitar bugs después de Retry.

### Sistema de Proyectiles:

**Bullet.cs:**
```csharp
lifeTime = 2f
// Al impactar enemigo:
- Notifica a HolyMeterController.RegisterEnemyHit()
- Se devuelve al BulletPool
```

**HolyBullet.cs:**
```csharp
speed = 10f
lifeTime = 2f
// Comportamiento:
- Viaja en línea recta
- Al impactar "Building" o timeout: explota
- Animación de explosión antes de destruirse
- Daño: 5 (vs 1 de bala normal)
```

**BulletPool.cs (Singleton):**
```csharp
poolSize = 20
// Métodos:
GameObject GetBullet()      // Obtiene bala del pool
void ReturnBullet(GameObject)  // Devuelve bala al pool
```

### Sistema Holy Meter:

**HolyMeterController.cs:**
```csharp
progressPerHit = 0.03f     // 3% por golpe
holyShotDuration = 3f
bossHitCooldown = 2f       // Cooldown especial para bosses

// Métodos
void RegisterEnemyHit(string tag)  // Aumenta medidor
bool IsCharged()                   // ¿Listo para disparar?
void TryActivateHolyShot()         // Activa efectos visuales
float GetCurrentCharge()           // Para GameManager
void SetCurrentCharge(float)       // Para restaurar
```

**HolyMeterUI.cs:**
```csharp
// UI visual del medidor
fillSpeed = 0.25f
void AddProgress(float)
void ResetMeter()
bool IsFull()
void SetFillAmount(float)
float GetFillAmount()
```

---

## Sistema de Enemigos

### Tipos de Enemigos:

| Prefab | Tag | HP | Daño | Velocidad | Aparece en |
|--------|-----|-----|------|-----------|------------|
| `Enemy.prefab` | Enemy | Bajo | 1 | Normal | Todas las waves |
| `Enemy boss.prefab` | Boss | Alto | Mayor | Lento | Cada 3 waves |
| `Gallo boss.prefab` | Boss | Alto | Mayor | Lento | Con bosses |
| `PolloLoco.prefab` | PolloLoco | 50 | 5 | 4f | Solo Final Boss |

### EnemyController.cs - Comportamiento:
```csharp
// Configuración
[SerializeField] float speed;
[SerializeField] int damage, maxHp, currentHp;
[SerializeField] float invulnerabilityTime = 2f;
[SerializeField] float detectionDistance = 5f;
[SerializeField] float rayAngle = 30f;
[SerializeField] LayerMask obstacleMask;

// Comportamiento de IA
void FixedUpdate()
- Persigue al jugador constantemente
- Detecta obstáculos con 3 Raycasts (frente, izq, der)
- Evade edificios (tag "Building") y fuego (tag "Fire")
- Sistema de evasión con timer de 1.2s
- Deslizamiento en paredes

// Daño
void DoDamage(Collider2D collision)
- Bullet: 1 daño
- HolyBullet: 5 daño

void TakeDamage(int amount)  // Método privado
- Flash rojo
- Invulnerabilidad temporal
- Muerte si HP <= 0

// Ataque al jugador
void OnTriggerEnter2D(Collider2D collision)
- Si toca al jugador y no está en roll
- Activa animación "Attack"
- Llama playerHealth.TakeDamage(damage)

// Notificación de muerte
IEnumerator Death()
- Notifica a WaveManager.NotifyDeath()
- Animación de muerte
- Se desactiva (vuelve al pool)

// Celebración (cuando jugador muere)
void Celebrate()
- Activa animación "Celebration"
```

### EnemyPool.cs - Object Pooling:
```csharp
[SerializeField] GameObject enemyPrefab, bossPrefab, galloPrefab, polloLocoPrefab;
int poolSize = 10;

// Pools separados para cada tipo
List<GameObject> enemyPool = new();
List<GameObject> bossPool = new();
List<GameObject> galloPool = new();
List<GameObject> polloLocoPool = new();

// Métodos
GameObject GetEnemy()      // Obtiene enemigo normal
GameObject GetBoss()       // Obtiene boss
GameObject GetGallo()      // Obtiene gallo
GameObject GetPolloLoco()  // Obtiene Pollo Loco
```

---

## Boss Final: Pollo Loco

### PolloLocoController.cs - Estadísticas:
```csharp
// Stats base
speed = 4f
invulnerabilityTime = 2f
damage = 5
maxHp = 50
attackRange = 4f
attackCooldown = 0.6f

// Rangos de decisión de ataque
detectionRange = 15f
closeAttackRange = 1.5f      // Melee
rangeAttackDistance = 6f      // Dash
areaAttackDistance = 10f      // Area Attack
chairAttackDistance = 15f     // Chair Throw

// Configuración de ataques
dashSpeed = 20f
dashDuration = 0.18f
areaAttackCooldown = 4f
areaDamageRadius = 3.5f
chairAttackCooldown = 10f
chairThrowForce = 15f

// Knockback
normalPushForce = 8f
attackPushForce = 14f
knockbackDuration = 0.25f

// Counter Dash
hitsToTriggerDash = 2
hitResetTime = 1f
counterDashMultiplier = 1.2f
```

### Sistema de 4 Ataques:

| Ataque | Método | Rango | Cooldown | Descripción |
|--------|--------|-------|----------|-------------|
| **Melee** | `DoCloseAttack()` | 0-1.5f | 0.6s | Golpe cuerpo a cuerpo, trigger "Attack" |
| **Dash** | `DoInstantDashAttack()` | 1.5-6f | postDashDelay 0.4s | Embestida hacia el jugador a 20f de velocidad |
| **Area Attack** | `DoAreaAttack()` | 6-10f | 4s | Salto con daño AOE (radio 3.5f), daño 1.8x |
| **Chair Throw** | `DoChairAttack()` | 10-15f | 10s | Lanza silla dorada como proyectil |

### PolloLocoAttackHandler.cs:
```csharp
// Configuración
float attackPause;
float dashSpeed, dashDuration;
float counterDashMultiplier, counterDashRecovery;
float areaJumpSpeed = 25f, areaJumpDuration = 0.6f, areaJumpHeightBoost = 6f;

// Corrutinas de ataque
IEnumerator MeleeAttack(Action onAttackEnd)
- Trigger "Attack"
- Espera attackPause
- Callback

IEnumerator DashAttack(Vector2 direction, Action onAttackEnd)
- Aplica velocidad en dirección
- Espera dashDuration
- Detiene, trigger "Attack"
- Callback

IEnumerator CounterDash(Vector2 direction, Action onAttackEnd)
- Dash con multiplicador (1.2x más rápido)
- Recuperación reducida

IEnumerator AreaAttack(Vector2 direction, Action onAttackEnd)
- Trigger "AreaAttack"
- Impulso hacia arriba + dirección
- Espera areaJumpDuration
- Callback

// 🔹 NUEVO: Detener todos los ataques (para Celebrate)
void StopAllAttacks()
- StopAllCoroutines()
- isDashing = false
- canDealDamage = false
- rb.linearVelocity = Vector2.zero
```

### Celebración del Boss (cuando mata al jugador):
```csharp
// Celebrate() - Detiene TODO y celebra en loop
void Celebrate()
{
    StopAllCoroutines();           // Detiene corrutinas del controller
    attackHandler.StopAllAttacks(); // Detiene corrutinas del handler
    
    celebrating = true;
    rb.bodyType = RigidbodyType2D.Kinematic;
    
    // Resetea TODOS los triggers
    animator.ResetTrigger("Attack/Chair/AreaAttack/GetHit/Death/Celebration");
    
    StartCoroutine(PlayCelebrationAnimation());
}

// PlayCelebrationAnimation() - Loop infinito
IEnumerator PlayCelebrationAnimation()
{
    while (celebrating && !isDead)
    {
        animator.Play("PolloLocoWins", 0, 0f);
        yield return new WaitForSeconds(animationLength);
    }
}
```

### GoldenChairProjectile.cs:
```csharp
damage = 8
lifeTime = 5f
knockbackForce = 14f

// Al impactar jugador:
- TakeDamage(8)
- Knockback con fuerza 14
- Se destruye
```

### Comportamiento Especial - Counter Dash:
```csharp
// Si el jugador golpea al boss 2+ veces en 1 segundo mientras está cerca:
if (hitsWhileClose >= hitsToTriggerDash && !isAttacking && !isDashing)
{
    hitsWhileClose = 0;
    StopAllCoroutines();
    StartCoroutine(DoCounterDashAttack());
}
```

### Secuencia de Muerte del Boss:
```csharp
// Cuando HP <= 0 en TakeDamage():
if (currentHp <= 0)
{
    // 1. INMEDIATAMENTE iniciar cinemática de victoria
    TriggerDefeatSequence();  // Cámara va al boss AL INSTANTE
    // 2. La animación de muerte corre EN PARALELO
    StartCoroutine(Death());
}

// Death() solo maneja la animación visual:
IEnumerator Death()
{
    isDead = true;
    StopPlayerControls();     // Bloquea jugador + PlayerAim + cursor
    animator.SetTrigger("Death");
    // Partículas, espera animación...
    yield return FadeOutSprite(1.2f);
    gameObject.SetActive(false);
}

// StopPlayerControls() - Bloqueo completo:
void StopPlayerControls()
{
    // Desactiva: PlayerMovement, _GunController, PlayerInput, PlayerAim
    // Resetea animaciones a Idle
    // Bloquea cursor (Cursor.lockState = Locked)
}
```

---

## Sistema de Waves

### WaveManager.cs - Configuración:
```csharp
// Referencias
Transform[] spawnPoints;
BoxCollider2D topBound, bottomBound, leftBound, rightBound;

// UI
TextMeshProUGUI roundText, roundTextShade;
TextMeshProUGUI killCountText, killCountTextShade;

// Audio
AudioClip waveMusic, bossMusic;

// Configuración
float waveTimeout = 240f;        // 4 minutos por wave
int bossWaveThreshold = 5;       // Wave donde aparece el portal

// Estado interno
int round = 1;
int enemiesToSpawn = 5;          // Inicial
int bossesToSpawn = 1;           // Inicial
int bossWaves = 3;               // Cada cuántas waves aparecen bosses
int killCount = 0;
```

### Progresión de Waves:
```csharp
void ScaleDifficulty()
{
    enemiesToSpawn += 3;  // +3 enemigos por wave
    round++;
    
    if (round % bossWaves == 0)  // Cada 3 waves
    {
        bossesToSpawn++;
    }
}

// Gallos = bossesToSpawn / 3
```

### Tabla de Progresión:
| Wave | Enemigos | Bosses | Gallos |
|------|----------|--------|--------|
| 1 | 5 | 0 | 0 |
| 2 | 8 | 0 | 0 |
| 3 | 11 | 1 | 0 |
| 4 | 14 | 0 | 0 |
| 5 | **PORTAL** | - | - |

### Lógica de StartWave():
```csharp
void StartWave()
{
    // Si es wave final → Cinemática del portal
    if (round == bossWaveThreshold)
    {
        KillAllEnemiesInstantly();
        FinalDoorCinematic.PlayCinematicToDoor();
        return;
    }
    
    // Spawn enemigos normales
    for (int i = 0; i < enemiesToSpawn; i++)
    {
        GameObject enemy = enemyPool.GetEnemy();
        enemy.GetComponent<EnemyController>().SetWaveManager(this);
        enemy.transform.position = GetValidSpawnPosition(...);
    }
    
    // Spawn bosses cada 3 waves
    if (round % bossWaves == 0)
    {
        // Spawn bossesToSpawn bosses
        // Spawn gallosToSpawn gallos
        // Cambiar a bossMusic
    }
}
```

### Evento Holy Cow:
```csharp
if (killCount % 50 == 0 && killCount > 0)
{
    PlayHolyCowSound(holyCowSource);
}
```

---

## Sistema del Portal

### UIFinalDoor.cs - Secuencia:
```csharp
IEnumerator Sequence()
{
    // 1. Mostrar puerta con fade-in
    EnableDoorRenderer();
    doorAnimator.Play("emerge");
    yield return FadeInSprite(2f);
    
    // 2. Activar colisiones
    doorCollider.enabled = true;
    doorExternalColliders.SetActive(true);
    
    // 3. Mostrar portal
    yield return WaitForSeconds(delayPortal);
    ActivatePortal();
    
    // 4. Mostrar símbolos
    yield return WaitForSeconds(delaySymbols);
    yield return ActivateSymbols();  // Triángulo, luego cuadrado
    
    // 5. Guardar y cargar escena boss
    yield return WaitForSeconds(delayBeforeSceneLoad);
    SaveAndLoadBossScene();
}
```

### FinalDoorCinematic.cs:
```csharp
IEnumerator Co_Play()
{
    IsPlaying = true;
    
    // 1. Fade out de todo el audio
    StartCoroutine(FadeOutAllAudio(1f));
    
    // 2. Ocultar UI
    uiCanvas.SetActive(false);
    
    // 3. Resetear estado del jugador INMEDIATAMENTE
    playerMovement.ResetPlayerState();  // Resetea animaciones, trail, audio
    playerMovement.enabled = false;
    gunController.enabled = false;
    playerInput.enabled = false;
    
    // 4. Forzar animación Idle inmediatamente
    playerAnimator.SetBool("isRunning", false);
    playerAnimator.SetBool("isRunningBackwards", false);
    playerAnimator.Play("Player_Idle", 0, 0f);
    
    // 5. Mover cámara hacia la puerta
    // ...
    
    // 6. Mostrar puerta + shake
    // ...
    
    // 7. Restaurar controles
    playerMovement.enabled = true;
    // etc.
    
    IsPlaying = false;
}
```

---

## Persistencia de Datos

### GameManager.cs (Singleton DontDestroyOnLoad):
```csharp
public static GameManager Instance;

// Datos persistentes
public int playerScore;
public float playerAmmo;
public int playerHealth;
public float holyCharge;  // 0-1 del medidor

void Update()
{
    // Sincronización constante
    var gun = FindAnyObjectByType<_GunController>();
    if (gun != null) playerAmmo = gun.GetAmmo();
    
    var health = FindAnyObjectByType<PlayerHealth>();
    if (health != null) playerHealth = health.CurrentHealth;
    
    var holy = FindAnyObjectByType<HolyMeterController>();
    if (holy != null) holyCharge = holy.GetCurrentCharge();
}

// Métodos
void SavePlayerData(PlayerHealth, _GunController, HolyMeterController)
void LoadPlayerData(PlayerHealth, _GunController, HolyMeterController)
void ResetGameData()
```

### BossSceneInitializer.cs:
```csharp
// 🔹 Variables estáticas para sistema de Retry
public static int originalPlayerHealth;
public static float originalPlayerAmmo;
public static float originalHolyCharge;
public static bool hasOriginalData = false;
public static bool isRetry = false;

// 🔹 Métodos estáticos para Retry
static void RestoreOriginalData()   // Restaura datos al GameManager antes de Retry
static void ResetOriginalData()     // Limpia datos al volver al menú

// 🔹 Campo para crear player en Retry
[SerializeField] GameObject playerPrefab;

IEnumerator Start()
{
    yield return null;
    
    SetupPlayerInstance();     // Evita duplicados, crea desde prefab si no existe
    SetupPlayerPosition();     // Posición en spawn point
    SetupCameraFollow();       // Cinemachine sigue al jugador
    SetupUICamera();           // UI Camera referencia al jugador
    SetupPlayerHealthUI();     // Vincula slider de HP
    SetupTrailRenderer();      // Sorting order del trail
    
    yield return LoadPlayerDataAndUI();  // Restaura desde GameManager
    
    // 🔹 Si es Retry, forzar controles
    if (isRetry)
    {
        yield return new WaitForSeconds(0.5f);
        UICanvasManager.ForceDisableAllCinematics();
        UICanvasManager.ForceEnablePlayerControls();
    }
}

// 🔹 SetupPlayerInstance ahora crea player desde prefab si no existe
void SetupPlayerInstance()
{
    if (mainPlayer == null && playerPrefab != null)
    {
        GameObject newPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        DontDestroyOnLoad(newPlayer);
    }
    ResetPlayerState();  // Resetea todos los componentes del player
}

IEnumerator WaitForHUDAndAssignAmmo()
{
    // Busca y vincula TextMeshPro de munición
    // Reflection para acceder a campos privados
    fieldText.SetValue(gunController, ammoText);
    fieldShade.SetValue(gunController, ammoShade);
    gunController.RefreshAmmoUI();
}
```

---

## Sistema de Cinemáticas

### Sistema de Bloqueo de Controles Durante Cinemáticas

El juego cuenta con un sistema unificado para bloquear todos los controles del jugador durante las cinemáticas.

#### Flags Estáticos de Cinemáticas:

| Flag | Script | Escena | Momento |
|------|--------|--------|---------|
| `IntroCinematicManager.IsCinematicPlaying` | IntroCinematicManager.cs | RoyalScene | Entrada del jugador |
| `FinalDoorCinematic.IsPlaying` | FinalDoorCinematic.cs | RoyalScene | Aparición del portal |
| `FinalBossCinematic.IsPlaying` | FinalBossCinematic.cs | Final Boss | Intro del boss |

#### Método Unificado de Verificación:

```csharp
// En PlayerMovement.cs - Método estático público
public static bool IsAnyCinematicPlaying()
{
    return IntroCinematicManager.IsCinematicPlaying 
        || FinalDoorCinematic.IsPlaying 
        || FinalBossCinematic.IsPlaying;
}
```

#### Scripts que Verifican Cinemáticas:

| Script | Método | Qué Bloquea |
|--------|--------|-------------|
| `PlayerMovement.cs` | `Update()` / `FixedUpdate()` | Movimiento WASD, Roll, Animaciones |
| `PlayerAim.cs` | `Update()` | Rotación del sprite con mouse |
| `_GunController.cs` | `Update()` | Disparo normal y HolyShot |
| `UICanvasManager.cs` | `Update()` | Menú de pausa (ESC) |

#### Comportamiento Durante Cinemáticas:
- ❌ No se puede mover (WASD)
- ❌ No se puede disparar (clicks)
- ❌ No se puede hacer roll (Espacio)
- ❌ No se puede pausar (ESC)
- ❌ No se puede rotar el sprite con el mouse
- ✅ El cursor queda oculto y bloqueado

---

### IntroCinematicManager.cs (RoyalScene):
```csharp
public static bool IsCinematicPlaying { get; private set; } = true;

// Método para resetear el flag (llamado desde UICanvasManager antes de Retry/ReturnToMenu)
public static void ResetCinematicState()
{
    IsCinematicPlaying = true;
}

IEnumerator PlayCinematic()
{
    // 1. Deshabilitar controles
    playerMovement.enabled = false;
    gunController.enabled = false;
    Cursor.visible = false;
    
    // 2. Posicionar en entrada
    player.transform.position = entryPoint.position;
    
    // 3. Esperar antes de mover
    yield return WaitForSeconds(delayBeforeStartMove);
    
    // 4. Activar animación de correr
    animator.SetBool("isRunning", true);
    
    // 5. Reproducir audio de entrada
    audioSourcePedidosYa.PlayOneShot(clipPedidosYa);
    
    // 6. Mover hacia punto objetivo
    yield return MoveToPosition(player.transform, targetPoint.position);
    
    // 7. Detener animación
    animator.SetBool("isRunning", false);
    
    // 8. Pequeña pausa
    yield return WaitForSeconds(delayBeforeEnable);
    
    // 9. Activar gameplay
    waveManager.SetActive(true);
    ulCanvas.SetActive(true);
    playerMovement.enabled = true;
    gunController.enabled = true;
    
    IsCinematicPlaying = false;
    Destroy(gameObject);
}
```

### FinalBossCinematic.cs (Final Boss Scene):
```csharp
public static bool IsPlaying { get; private set; } = false;

// 🔹 Resetea el flag (necesario para Retry)
static void ResetCinematicState() { IsPlaying = false; }

IEnumerator Start()
{
    yield return null;  // Esperar a que BossSceneInitializer configure el player
    
    // 🔹 Si es RETRY, saltar cinemática e ir directo a la pelea
    if (BossSceneInitializer.isRetry)
    {
        yield return StartCoroutine(SetupRetryMode());
        yield break;
    }

    // === CINEMÁTICA NORMAL (primera vez) ===
    IsPlaying = true;

    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
    
    // 1. Deshabilitar PlayerAim
    playerAim.enabled = false;
    
    // 2. INMEDIATAMENTE bloquear al jugador (evita movimiento durante espera)
    playerController.DisablePlayer(player);
    
    // 3. Esperar en el spawn point (jugador ya bloqueado, no se mueve)
    yield return WaitForSeconds(1f);
    
    // 4. Deshabilitar UI Manager
    uiManagerInstance.enabled = false;
    
    // 5. Mostrar barras cinemáticas
    yield return barsController.ShowBars();
    
    // 6. Movimiento automático del jugador hacia PortalExitPoint
    yield return playerController.PlayerWalk(player);
    
    // 7. Secuencia de cierre (símbolos, portal, puerta)
    yield return CloseSequence();
    
    // 8. Intro del jefe
    yield return bossIntro.PlayBossIntro(cameraFocus, uiManager);
    
    // 9. Ocultar barras
    yield return barsController.HideBars();
    
    // 10. Reactivar todo
    playerController.EnablePlayer(player);
    playerAim.enabled = true;
    uiManagerInstance.enabled = true;
    
    // 11. Música del boss con fade-in
    StartCoroutine(FadeInMusic(bossMusic, 2f));

    IsPlaying = false;
}

// 🔹 NUEVO: Configuración para Retry (sin cinemática)
IEnumerator SetupRetryMode()
{
    IsPlaying = false;
    UICanvasManager.ForceDisableAllCinematics();
    
    // Desactivar objetos del portal
    triangleObject.SetActive(false);
    squareObject.SetActive(false);
    portalObject.SetActive(false);
    bossDoor.SetActive(false);
    
    // Activar boss en modo pelea
    bossObject.SetActive(true);
    bossController.isCinematicMode = false;
    bossAnim.Play("PolloLocoMovement");
    
    // Configurar UI (barra HP llena)
    uiCanvas.SetActive(true);
    enemyHpBar.value = enemyHpBar.maxValue;
    
    // Habilitar controles del player
    UICanvasManager.ForceEnablePlayerControls();
    
    // Iniciar música
    bossMusic.Play();
    
    BossSceneInitializer.isRetry = false;
}
```

### CinematicBossIntro.cs:
```csharp
IEnumerator PlayBossIntro(CinematicCameraFocus cameraFocus, CinematicUIManager uiManager)
{
    bossObject.SetActive(true);
    bossController.isCinematicMode = true;
    
    // Empezar en Idle
    anim.Play(idleAnimationName);
    
    // Enfoque de cámara con callback para WarCry
    yield return cameraFocus.FocusOnBoss(() => {
        anim.Play(warCryAnimationName);
        audioSource.PlayOneShot(warCryClip);
    });
    
    // Llenado de barra HP
    yield return uiManager.FillBossHP();
    
    // Volver a Idle
    anim.Play(idleAnimationName);
    yield return WaitForSeconds(delayAfterHPFill);
    
    // Iniciar movimiento
    anim.Play(moveAnimationName);
    bossController.isCinematicMode = false;
}
```

### BossDefeatSequence.cs (Victoria):
```csharp
// Se llama INMEDIATAMENTE cuando el boss HP <= 0
IEnumerator DefeatSequenceCoroutine()
{
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
    
    // Desactivar UI (excepto CanvasWIN)
    // ...
    
    // 1. Cámara va al boss (solo movimiento, SIN shake)
    yield return cinematicCameraFocus.SimpleFocusOnBoss();
    
    // 2. Esperar mientras el boss hace su animación de muerte
    yield return WaitForSeconds(focusOnBossDelay);
    
    // 3. Cámara va al jugador CON efectos (shake, zoom)
    yield return cinematicCameraFocus.FocusOnPlayerWithEffects();
    
    // 4. Mostrar "You Win"
    canvasWin.SetActive(true);
    
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
}
```

### CinematicCameraFocus.cs:
```csharp
// Focus simple en boss (solo movimiento + zoom, sin shake)
// Usado en secuencia de victoria
IEnumerator SimpleFocusOnBoss()
{
    cineCam.Follow = null;
    // Lerp hacia el boss + zoom in
    // NO hace shake, NO vuelve atrás
}

// Focus en jugador CON todos los efectos
// Usado después de que el boss muere
IEnumerator FocusOnPlayerWithEffects()
{
    // Mover cámara hacia el jugador
    // Shake en posición del jugador
    // Esperar
    // cineCam.Follow = player.transform (cámara sigue al jugador)
    // Restaurar zoom original
}

// Focus completo en boss con callback (usado en intro del boss)
IEnumerator FocusOnBoss(Action onBossCentered = null)
{
    // Lerp hacia el boss + zoom in
    // Callback al 60% (para WarCry)
    // Shake
    // Volver a posición original
    // Restaurar cineCam.Follow
}

// Focus simple en jugador (usado en otros contextos)
IEnumerator FocusOnPlayer()
{
    // Lerp hacia el jugador
    // cineCam.Follow = player.transform
}
```

### CinematicBarsController.cs:
```csharp
IEnumerator ShowBars()
{
    cinematicCanvas.SetActive(true);
    yield return FadeBars(0f, 1f, moveIn: true);
}

IEnumerator HideBars()
{
    yield return FadeBars(1f, 0f, moveIn: false, disableAfter: true);
}

// Barras se mueven verticalmente mientras hacen fade
```

### CinematicPlayerController.cs:
```csharp
void DisablePlayer(GameObject player)
{
    isFrozen = true;
    move.enabled = false;
    gun.enabled = false;
    input.enabled = false;
    rb.bodyType = RigidbodyType2D.Kinematic;
    
    // Forzar animación Idle inmediatamente (evita que isRunning quede activo)
    animator.SetBool("isRunning", false);
    animator.SetBool("isRunningBackwards", false);
}

void EnablePlayer(GameObject player)
{
    isFrozen = false;
    move.enabled = true;
    gun.enabled = true;
    input.enabled = true;
    rb.bodyType = RigidbodyType2D.Dynamic;
}

IEnumerator PlayerWalk(GameObject player)
{
    anim.SetBool("isRunning", true);
    stepAudio.Play();
    
    // Lerp de posición hacia playerTargetPoint (PortalExitPoint)
    while (elapsed < walkDuration)
    {
        player.transform.position = Vector3.Lerp(start, end, t);
        yield return null;
    }
    
    anim.SetBool("isRunning", false);
    yield return FadeOutAudio(stepAudio, 0.8f);
}
```

---

## Sistema de UI

### Canvas del Juego:
| Canvas | Contenido |
|--------|-----------|
| `UICanvas` | HP Slider, Ammo Text, Round Text, Kill Count, Holy Meter |
| `PauseCanvas` | Resume, Restart, Main Menu buttons |
| `GameOverCanvas` | Fade panel, Retry/Menu buttons |
| `CanvasWIN` | Pantalla de victoria |
| `CinematicCanvas` | Barras negras top/bottom |

### UICanvasManager.cs:
```csharp
public static bool IsGamePausedOrOver { get; private set; }

void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape) && !gameOverCanvas.activeSelf)
    {
        TogglePause();
    }
}

void TogglePause()
{
    isPaused = !isPaused;
    pauseCanvas.SetActive(isPaused);
    Time.timeScale = isPaused ? 0f : 1f;
    IsGamePausedOrOver = isPaused;
}

// 🔹 NUEVO: Forzar habilitación de controles del player
static void ForceEnablePlayerControls()
{
    // Busca player por tag
    // Habilita: PlayerMovement, _GunController, PlayerInput, PlayerAim
    // Resetea: Rigidbody a Dynamic, SpriteRenderer, Animator a Idle
    // Restaura cursor
}

// 🔹 NUEVO: Forzar todas las cinemáticas a FALSE
static void ForceDisableAllCinematics()
{
    // Usa reflection para forzar:
    // - IntroCinematicManager.IsCinematicPlaying = false
    // - FinalBossCinematic.IsPlaying = false
    // - FinalDoorCinematic.IsPlaying = false
    // - IsGamePausedOrOver = false
}

void TriggerGameOver()
{
    StartCoroutine(DelayedGameOver());
    // 1 segundo de delay
    // Activa gameOverCanvas
    // Fade del panel
}

void RestartGame()
{
    // Resetear flag de cinemática ANTES de cargar escena
    IntroCinematicManager.ResetCinematicState();
    // Destruir players, resetear GameManager
    SceneManager.LoadScene(currentScene);
}

void ReturnToMenu()
{
    IntroCinematicManager.ResetCinematicState();
    // Destruir players, resetear GameManager
    SceneManager.LoadScene("MainMenu");
}
```

### UIAnimation.cs:
```csharp
// Anima textos de UI con efecto de "pop"
void Animation(TextMeshProUGUI text, TextMeshProUGUI textShade)
{
    // Cambia color a rojo/naranja/amarillo
    // Escala de 1x a 1.5x y vuelve
    // Rotación ±15°
    // Duración: 0.2s cada dirección
}
```

### PlayerHealthUI.cs:
```csharp
void Start()
{
    healthSlider.maxValue = playerHealth.MaxHealth;
    healthSlider.value = playerHealth.CurrentHealth;
    playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
}

void UpdateHealthBar(int current, int max)
{
    healthSlider.maxValue = max;
    healthSlider.value = current;
}
```

---

## Sistema de Audio

### Música Dinámica (WaveManager):
```csharp
AudioClip waveMusic;   // Música normal
AudioClip bossMusic;   // Cuando aparecen bosses

// Cambia automáticamente según gallosToSpawn > 0
if (gallosToSpawn > 0)
{
    if (musicSource.clip != bossMusic)
    {
        musicSource.clip = bossMusic;
        musicSource.Play();
    }
}
```

### OptionsMenu.cs:
```csharp
AudioMixer audioMixer;
Slider musicSlider, sfxSlider;

void SetMusicVolume(float value)
{
    // Logarítmico: Mathf.Log10(value) * 20
    audioMixer.SetFloat("MusicVolume", ...);
    PlayerPrefs.SetFloat("MusicVolume", value);
}

void SetSFXVolume(float value)
{
    audioMixer.SetFloat("SFXVolume", ...);
    PlayerPrefs.SetFloat("SFXVolume", value);
    // Preview del sonido
}
```

### Efectos de Sonido por Entidad:

**Player:**
- Pasos (AudioSource del Player)
- Disparo (shotgunAudioSource)
- HolyShot (holyShotSource)
- Hit (3 variantes: hit1, hit2, hit3)
- Curación (lifeUpSound)
- Muerte (deathSound)
- Comer munición (eat1, eat2, eat3)
- Disparo vacío (emptyGunshot)

**Enemy/Boss:**
- Hit (3 variantes)
- Muerte (efecto de partículas)

**Pollo Loco:**
- Hit (3 variantes)
- WarCry
- Lanzar silla (throwChairClip)
- Animación event sounds

---

## Objetos del Escenario

### Items Recolectables:

| Tag | Prefab | Efecto | Respawn |
|-----|--------|--------|---------|
| `Ammo` | Balde pollo | +10 municiones | 3 segundos |
| `Life` | Healing Energizer | +15% HP máximo | 10 segundos |

### ObjectsManager.cs:
```csharp
float bucketRespawnTime = 3f;
float bottleRespawnTime = 10f;

void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Ammo") || other.CompareTag("Life"))
    {
        StartCoroutine(DeactivateAndReactivate(other.gameObject));
    }
}
```

### Obstáculos:

| Tag | Efecto |
|-----|--------|
| `Building` | Bloquea movimiento, enemigos lo evaden con raycast |
| `Fire` | Daño continuo a jugador y enemigos |

### Fire.cs:
```csharp
int damage = 2;
float damageInterval = 1f;

void OnTriggerStay2D(Collider2D collision)
{
    if (Time.time < nextDamageTime) return;
    
    if (collision.CompareTag("Player"))
    {
        playerHealth.TakeDamage(damage);
        nextDamageTime = Time.time + damageInterval;
    }
    else if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
    {
        // Usa reflection para llamar TakeDamage privado
        enemyController.TakeDamage(damage);
    }
}
```

### Límites del Mapa:
- `topBound`, `bottomBound`, `leftBound`, `rightBound` (BoxCollider2D)
- Usados para validar posiciones de spawn

---

## Animaciones

### Player.controller - Estados:
| Estado | Parámetro/Trigger | Condición |
|--------|-------------------|-----------|
| Player_Idle | Default | - |
| run | isRunning (bool) | Movimiento hacia adelante |
| Run_Backwards | isRunningBackwards (bool) | Movimiento hacia atrás |
| Roll | Rolling (trigger) | Presionar Espacio |
| Fire | fire (trigger) | Disparo corriendo adelante |
| Fire_Backwards | FireBackwards (trigger) | Disparo corriendo atrás |
| Static Fire | StaticFire (trigger) | Disparo estático |
| HolyShot | HolyShot (trigger) | Click derecho |
| Get_hit | GetHit (trigger) | Recibir daño |

### Enemy.controller - Estados:
| Estado | Parámetro/Trigger |
|--------|-------------------|
| Movement | isMoving (bool) |
| Attack | Attack (trigger) |
| Death | Death (trigger) |
| Celebration | Celebration (trigger) |

### PolloLoco.controller - Estados:
| Estado | Parámetro/Trigger | Uso |
|--------|-------------------|-----|
| PolloLocoIdle | Default | Espera, cinemáticas |
| PolloLocoMovement | isMoving (bool) | Persiguiendo |
| PolloLocoAttack | Attack (trigger) | Melee y post-dash |
| PolloLocoAreaAttack | AreaAttack (trigger) | Salto AOE |
| PolloLocoChair | Chair (trigger) | Lanzar silla |
| PolloLocoGetHit | GetHit (trigger) | Recibir daño |
| PolloLocoDeath | Death (trigger) | Morir |
| PolloLocoWarCry | - | Intro cinemática |
| PolloLocoWins | Celebration (trigger) | Si mata al jugador |

### Animation Events:
| Animación | Evento | Método |
|-----------|--------|--------|
| HolyShot | Disparar | `FireHolyBullet()` |
| HolyShot | Partículas | `PlayHolyShotParticle()` |
| HolyShot | Aura start | `PlayAura()` |
| HolyShot | Aura stop | `StopAura()` |
| PolloLocoChair | Lanzar | `ThrowChair()` |
| PolloLocoAreaAttack | Impacto | `DoAreaDamage()` |
| PolloLoco* | Sonido | `PlayAnimationEventSound()` |
| PolloLocoAreaAttack | Plumas | `PlayPlumaImpact()` |

---

## Referencia de Scripts

### Todos los Scripts (42 total):

**Player/ (11 scripts)**
| Script | Propósito |
|--------|-----------|
| `PlayerMovement.cs` | Movimiento WASD, roll, estados |
| `PlayerHealth.cs` | HP, daño, invulnerabilidad, muerte |
| `_GunController.cs` | Disparo normal y HolyShot |
| `PlayerAim.cs` | Flip del sprite según mouse (bloquea durante cinemáticas) |
| `Bullet.cs` | Proyectil normal, pooling |
| `HolyBullet.cs` | Proyectil sagrado, explosión |
| `BulletPool.cs` | Singleton para reutilizar balas |
| `HolyMeterController.cs` | Lógica del medidor sagrado |
| `HolyMeterUI.cs` | UI visual del medidor |
| `HolyShotController.cs` | Efectos visuales del HolyShot |
| `PlayerHealthtUI.cs` | Vincula slider de HP |

**Enemy/ (10 scripts)**
| Script | Propósito |
|--------|-----------|
| `EnemyController.cs` | IA de enemigos, movimiento, ataque |
| `EnemyPool.cs` | Pools de todos los tipos de enemigos |
| `PolloLocoController.cs` | IA del boss final, 4 ataques |
| `PolloLocoAttackHandler.cs` | Corrutinas de ataques del boss |
| `GoldenChairProjectile.cs` | Proyectil de silla dorada |
| `PolloLocoBoundary.cs` | Límites del ring del boss |
| `BossDefeatSequence.cs` | Cinemática de victoria |
| `CinematicBarsController.cs` | Barras negras cinemáticas |
| `CinematicBossIntro.cs` | Intro del boss |
| `CinematicCameraFocus.cs` | Control de cámara cinemática |
| `CinematicPlayerController.cs` | Movimiento automático |
| `CinematicUIManager.cs` | Llenado de HP del boss |
| `FinalBossCinematic.cs` | Cinemática completa del boss |

**UI/ (9 scripts)**
| Script | Propósito |
|--------|-----------|
| `GameManager.cs` | Singleton de persistencia |
| `UICanvasManager.cs` | Pausa, game over, menús |
| `BossSceneInitiializer.cs` | Setup de la escena del boss |
| `FinalDoorCinematic.cs` | Cinemática del portal |
| `UIFinalDoor.cs` | Secuencia del portal |
| `UIAnimation.cs` | Animaciones de texto |
| `UICamera.cs` | Seguimiento de cámara UI |
| `ObjectsManager.cs` | Respawn de items |
| `Fire.cs` | Daño del fuego |

**Waves/ (1 script)**
| Script | Propósito |
|--------|-----------|
| `WaveManager.cs` | Sistema completo de waves |

**Root Scripts (5 scripts)**
| Script | Propósito |
|--------|-----------|
| `MenuManager.cs` | Menú principal |
| `OptionsMenu.cs` | Opciones de audio |
| `ChangeScene.cs` | Transición con fade |
| `LoadingScene.cs` | Carga asíncrona |
| `IntroCinematicManager.cs` | Cinemática de entrada |
| `Final Door.cs` | Trigger simple de cambio de escena |

---

## Flujo Completo del Juego

```
┌─────────────────────────────────────────────────────────────────┐
│                         MAIN MENU                                │
│  MenuManager.Jugar() → SceneManager.LoadScene("Loading")        │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                          LOADING                                 │
│  LoadingScene.CargarJuego()                                     │
│  - Carga asíncrona de "DIARY CUT"                               │
│  - Barra de progreso                                            │
│  - Espera 8 segundos                                            │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                        DIARY CUT                                 │
│  Cinemática narrativa                                           │
│  ChangeScene → Carga siguiente escena                           │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                       ROYAL SCENE                                │
│                                                                  │
│  1. IntroCinematicManager                                       │
│     - Jugador entra caminando                                   │
│     - Se activa WaveManager y UI                                │
│                                                                  │
│  2. WaveManager Loop                                            │
│     ┌─────────────────────────────────────────────────────────┐ │
│     │ Wave 1: 5 enemigos                                      │ │
│     │ Wave 2: 8 enemigos                                      │ │
│     │ Wave 3: 11 enemigos + 1 boss + gallos                   │ │
│     │ Wave 4: 14 enemigos                                     │ │
│     │                                                         │ │
│     │ Cada wave:                                              │ │
│     │ - Spawn desde EnemyPool                                 │ │
│     │ - Enemigos persiguen al jugador                         │ │
│     │ - NotifyDeath() cuando mueren                           │ │
│     │ - EndWave() cuando todos muertos o timeout              │ │
│     └─────────────────────────────────────────────────────────┘ │
│                                                                  │
│  3. Wave 5 (bossWaveThreshold)                                  │
│     - KillAllEnemiesInstantly()                                 │
│     - FinalDoorCinematic.PlayCinematicToDoor()                  │
│       * Fade out audio                                          │
│       * Cámara hacia puerta                                     │
│       * UIFinalDoor.ShowFinalDoor()                             │
│         - Animación emerge                                      │
│         - Portal fade in                                        │
│         - Símbolos aparecen                                     │
│       * GameManager.SavePlayerData()                            │
│       * LoadScene("FinalBoss")                                  │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                        FINAL BOSS                                │
│                                                                  │
│  1. BossSceneInitializer                                        │
│     - Posiciona jugador en spawn point                          │
│     - Configura cámara Cinemachine                              │
│     - GameManager.LoadPlayerData() (HP, Ammo, HolyCharge)       │
│     - Vincula UI de HP y Ammo                                   │
│                                                                  │
│  2. FinalBossCinematic                                          │
│     - Barras cinemáticas                                        │
│     - Jugador camina automáticamente                            │
│     - CloseSequence (símbolos, portal, puerta desaparecen)      │
│     - CinematicBossIntro                                        │
│       * Pollo Loco aparece                                      │
│       * Cámara enfoca al boss                                   │
│       * WarCry animation + sound                                │
│       * HP bar se llena                                         │
│     - Barras desaparecen                                        │
│     - Controles restaurados                                     │
│     - Música del boss fade in                                   │
│                                                                  │
│  3. Combate contra PolloLoco                                    │
│     ┌─────────────────────────────────────────────────────────┐ │
│     │ HP: 50                                                  │ │
│     │                                                         │ │
│     │ Ataques según distancia:                                │ │
│     │ - 0-1.5f: Melee                                         │ │
│     │ - 1.5-6f: Dash                                          │ │
│     │ - 6-10f: Area Attack (salto + AOE)                      │ │
│     │ - 10-15f: Chair Throw                                   │ │
│     │                                                         │ │
│     │ Especial:                                               │ │
│     │ - Counter Dash si recibe 2+ hits cerca                  │ │
│     │ - Knockback al jugador en cada impacto                  │ │
│     └─────────────────────────────────────────────────────────┘ │
│                                                                  │
│  4. Victoria (PolloLoco.currentHp <= 0)                         │
│     - TriggerDefeatSequence() INMEDIATAMENTE                    │
│       * Cámara va al boss AL INSTANTE                           │
│     - Death() corre EN PARALELO                                 │
│       * StopPlayerControls() - bloquea jugador + cursor         │
│       * Animación de muerte                                     │
│       * FadeOut del sprite (1.2s)                               │
│     - BossDefeatSequence flujo:                                 │
│       * SimpleFocusOnBoss() - cámara al boss (sin shake)        │
│       * Espera focusOnBossDelay                                 │
│       * FocusOnPlayerWithEffects() - cámara al jugador + shake  │
│       * CanvasWIN.SetActive(true) - "You Win"                   │
│                                                                  │
│  5. Game Over (Player.currentHealth <= 0)                       │
│     - PlayerHealth.TakeDamage() detecta muerte                  │
│     - PolloLoco.Celebrate()                                     │
│     - UICanvasManager.TriggerGameOver()                         │
│     - Opciones: Retry / Main Menu                               │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tags Importantes

| Tag | Uso |
|-----|-----|
| `Player` | Identificar al jugador |
| `Enemy` | Enemigos normales |
| `Boss` | Pollos grandes |
| `PolloLoco` | Boss final |
| `Bullet` | Balas normales |
| `HolyBullet` | Balas sagradas |
| `Building` | Obstáculos sólidos |
| `Fire` | Zonas de fuego |
| `Ammo` | Items de munición |
| `Life` | Items de curación |

---

## Layers Importantes

| Layer | Uso |
|-------|-----|
| Default | Mayoría de objetos |
| Player | Jugador |
| Enemy | Enemigos |
| Bullet | Proyectiles |
| Obstacle | Para obstacleMask en enemigos |

---

## Notas de Desarrollo

1. **Player usa DontDestroyOnLoad** - Persiste entre escenas
2. **GameManager es Singleton** - Persiste datos del jugador
3. **Object Pooling** para balas y enemigos - Mejor rendimiento
4. **Cinemachine** para cámara - Seguimiento suave
5. **Input System nuevo** - `PlayerInput` component
6. **URP** (Universal Render Pipeline) - Configurado en el proyecto
7. **TrailRenderer desactivado por defecto** - Se activa solo durante el roll y se desactiva al terminar. En la escena debe tener `enabled = false`
8. **Sistema de bloqueo de cinemáticas** - Usa `PlayerMovement.IsAnyCinematicPlaying()` para verificar estado de todas las cinemáticas
9. **Particle Systems desactivados por defecto** - Todos los PS del jugador (Holy Aura, shooting_bloom, light rayss) deben estar desactivados en la escena. El código los activa solo cuando se dispara o usa HolyShot
10. **Reset de flags estáticos en Retry** - `UICanvasManager.RestartGame()` llama a `IntroCinematicManager.ResetCinematicState()` antes de recargar la escena para que la cinemática de intro funcione correctamente
11. **ResetHolyShotState() en Start()** - `_GunController` resetea el estado de HolyShot al iniciar para evitar bugs de animación/partículas después de Retry
12. **Reset de animaciones en cinemáticas** - `FinalDoorCinematic`, `FinalBossCinematic` y `BossDefeatSequence` fuerzan `isRunning = false` inmediatamente al iniciar para evitar que la animación de correr quede activa
13. **FinalBossCinematic bloquea jugador inmediatamente** - `DisablePlayer()` se llama ANTES de la espera inicial para evitar que el jugador se mueva durante la cinemática de entrada
14. **Victoria del boss es instantánea** - Cuando el boss muere (HP <= 0), `TriggerDefeatSequence()` se llama INMEDIATAMENTE (cámara va al boss al instante). `Death()` corre en paralelo para la animación visual
15. **StopPlayerControls() bloquea todo** - Al morir el boss, bloquea: PlayerMovement, _GunController, PlayerInput, PlayerAim, y el cursor (Cursor.lockState = Locked)
16. **CinematicCameraFocus tiene 4 métodos** - `SimpleFocusOnBoss()` (sin shake), `FocusOnBoss()` (con callback), `FocusOnPlayer()` (simple), `FocusOnPlayerWithEffects()` (con shake)
17. **Sistema de Retry en Final Boss** - `BossSceneInitializer.isRetry` indica si es un retry. `FinalBossCinematic.SetupRetryMode()` salta la cinemática y va directo a la pelea
18. **ForceDisableAllCinematics()** - Método estático en `UICanvasManager` que fuerza TODOS los flags de cinemáticas a `false` usando reflection. Crítico para Retry en Final Boss
19. **ForceEnablePlayerControls()** - Método estático en `UICanvasManager` que habilita todos los componentes del player y restaura el cursor
20. **Celebrate() del boss con loop infinito** - Cuando el boss mata al player, `Celebrate()` detiene todas las corrutinas (controller + attackHandler) y ejecuta la animación de victoria en loop infinito
21. **BossSceneInitializer guarda datos originales** - Al entrar por primera vez a Final Boss, guarda HP/Ammo/HolyCharge originales para restaurarlos en Retry
22. **PlayerPrefab en BossSceneInitializer** - Si no hay player (después de Retry), lo crea desde el prefab configurado en el inspector
23. **IntroCinematicManager.ResetCinematicState() pone IsCinematicPlaying = TRUE** - No llamarlo en Final Boss porque no hay IntroCinematicManager para ponerlo en false. Usar `ForceDisableAllCinematics()` en su lugar

---

*Documento generado para contexto de desarrollo - Survival Isométrico de Pollos*

