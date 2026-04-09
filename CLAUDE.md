# CLAUDE.md — Shrink (Unity 6)

## Proyecto

Juego móvil puzzle maze 2D top-down para iOS y Android.
Motor: Unity 6 | Resolución: 1080x1920 portrait | Estilo: minimalista (líneas y círculos, sin assets externos)
Input System: **New Input System** — NO usar `UnityEngine.Input` legacy.

## Ruta raíz de código

```
Assets/_Project/
```

## Estructura de scripts

```
Scripts/
  Core/          ShapeFactory, GameManager, SaveManager, GameData, LocalizationManager
  Events/        GameEvents.cs — eventos globales estáticos
  Maze/          MazeGenerator, MazeData, CellType, MazeRenderer
                 Editor/MazeLevelEditor.cs, Editor/MazeDebugVisualizerEditor.cs
  Player/        SphereController, ShrinkMechanic, Crumb, Star
  Enemies/       EnemyController, PatrolEnemy, TrailEnemy, ChaserEnemy
  Movement/      PlayerMovement
  Camera/        CameraFollow
  UI/            HUDController, PauseMapController, GameResultController, LocalizedText,
                 LevelSelectController, DPadController, DPadButton
  Level/         LevelManager, LevelData (ScriptableObject), LevelLoader, LevelTimer,
                 CellOverride, EnemySpawn
  Monetization/  AdManager, IAPManager
  Audio/         AudioManager
  Multiplayer/   MultiplayerManager, MultiplayerGameManager, NetworkMazeState,
                 NetworkPlayer, NetworkBotPlayer, NetworkPatrolEnemy

ScriptableObjects/Levels/  LevelData assets (Level_01…Level_N)
Prefabs/Player.prefab      (SphereController + ShrinkMechanic + PlayerMovement)
```

## Sistemas — estado actual

| #   | Sistema                        | Estado         | Notas                                                           |
| --- | ------------------------------ | -------------- | --------------------------------------------------------------- |
| 1   | Generación procedural de maze  | ✅ Completo    | BSP + Labyrinth + Hybrid                                        |
| 2   | Esfera y mecánica de desgaste  | ✅ Completo    | Migajas, puertas, estrellas, calibración auto                   |
| 3   | Movimiento — joystick flotante | ✅ Completo    | Invisible, re-anclaje dinámico, velocidad por tamaño            |
| 3.5 | Enemigos / Mobs                | ✅ Completo    | PatrolEnemy, TrailEnemy, ChaserEnemy                            |
| 4   | Pausa + HUD                    | ✅ Completo    | HUDController + PauseMapController + D-pad                      |
| 5   | Sistema de niveles y semillas  | ✅ Completo    | LevelData, LevelManager, LevelLoader, LevelTimer, Trampas       |
| 6   | Monetización                  | ✅ Completo    | IAPManager (Unity IAP v5) + AdManager (AdMob)                   |
| 7   | Juice y sonido                 | ✅ Completo    | AudioManager, playlists aleatorias, SFX por eventos             |
| S   | SaveManager                    | ✅ Completo    | GameData JSON, LevelRecord, AudioSettings, GameStats            |
| L   | Localización                  | ✅ Completo    | EN, ES, PT, FR, DE — auto-detect + guardado en SaveManager      |
| 8   | UI completa                    | ✅ Completo    | Boot + Menu + LevelSelect + GameResult + Localización           |
| D   | D-pad táctil                  | ✅ Completo    | DPadController + DPadButton — posición/tamaño/alpha persistidos |
| E   | Editor visual de niveles       | ✅ Completo    | Window → Shrink → Level Editor                                  |
| 9   | Modo Infinito                  | ✅ Completo    | InfiniteGameManager + InfiniteHUDController + InfiniteScene     |
| UGS | Auth + CloudSave + Leaderboard | ✅ Completo    | UGS anónimo, sync GameData, tabla `infinite_leaderboard`        |
| M   | Modo Multijugador              | 🔧 En progreso | Photon Fusion 2 Shared Mode — ver sección Multijugador          |
| F   | Mecánicas futuras              | ⬜ Backlog     | Enemigos adicionales, trampas avanzadas, celdas especiales      |

## Generación de maze — MazeStyle

```csharp
public enum MazeStyle { Dungeon, Labyrinth, Hybrid }
```

| Estilo    | Algoritmo                      | Usar para                         |
| --------- | ------------------------------ | --------------------------------- |
| Dungeon   | BSP (cuartos + corredores)     | Niveles 1–8, Infinito mazes 1–6  |
| Labyrinth | Recursive Backtracker DFS      | Niveles 9–30+, Infinito mazes 17+ |
| Hybrid    | Backtracker + cuartos tallados | Infinito mazes 7–16               |

- **Labyrinth recomendado** para gameplay real — corredores de 1 celda de ancho.
- El maze siempre se valida con BFS. `ShortestPathLength` queda en `MazeData`.

## Mecánica central — reglas inamovibles

- Tamaño inicial: `1.0` | Mínimo (muerte): `0.15` | Rango usable: `0.85`
- `NARROW_06`: requiere tamaño < 0.6 | `NARROW_04`: requiere tamaño < 0.4
- Migajas: una por celda, recuperan exactamente el tamaño perdido al reabsorberse
- Puertas: consumen tamaño permanentemente (sin migaja), costo default `0.10`
- Maze siempre tiene solución — validado con BFS
- Tocar un enemigo = muerte instantánea

## Tipos de celda (MazeData)

```csharp
public enum CellType
{
    WALL, PATH, ROOM, CORRIDOR, DOOR,
    START, EXIT, NARROW_06, NARROW_04,
    TRAP_ONESHOT, TRAP_DRAIN, SPIKE
}
```

## Calibración de dificultad — ShrinkMechanic

```
sizePerStep = 0.85 × difficultyFactor ÷ shortestPathLength
```

| difficultyFactor | Niveles / Contexto                        |
| ---------------- | ----------------------------------------- |
| 0.50             | Tutorial (1–3)                            |
| 0.65             | Aprendizaje (4–7)                         |
| 0.75             | Normal (8–11)                             |
| 0.80             | Exigente (12–15) — fin Mundo 1           |
| 0.85             | Difícil (16–20) — inicio Mundo 2        |
| 0.90             | Muy difícil (21–25)                      |
| 0.95             | Casi perfecto (26–30) — fin Mundo 2     |
| 0.85→0.95       | Mundo 3 (31–45), escalado progresivo      |
| 0.95→1.0        | Infinito creciente                        |

`autoCalibrate = true` (default). Los callejones sin salida son gratis si el jugador retrocede completo.

## Enemigos — Sistema 3.5

Todos se mueven celda a celda. Devoran migajas al pasar.

| Tipo            | Comportamiento                                                        | Color                        |
| --------------- | --------------------------------------------------------------------- | ---------------------------- |
| **PatrolEnemy** | Segmento fijo ida y vuelta, rebota en paredes                         | Naranja `(1, 0.30, 0.10)`   |
| **TrailEnemy**  | BFS hacia la migaja más reciente                                      | Naranja (mismo base)         |
| **ChaserEnemy** | BFS directo al jugador. Mejor en Dungeon (rooms). Desde nivel 21.    | Azul `(0.10, 0.45, 0.90)`   |

```csharp
public enum EnemyType { Patrol, Trail, Chaser }
```

**Spawning:** manual via `LevelData.manualEnemySpawns` (`EnemySpawn { cell, type, patrolDir }`), o automático por contadores con seed reproducible, distancia Manhattan ≥ 5 del START.

**Revive tras muerte:** el enemigo killer tiene `_wasKiller = true` → se destruye al revivir. Los demás escuchan `OnPlayerRevived` y reanudan `MoveLoop()`.

**Introducción:** PatrolEnemy nivel 12 | TrailEnemy nivel 19 (solo), 20 (combinado) | ChaserEnemy nivel 21.

## Trampas

Siempre visibles — reto de planning, no de sorpresa.

| Tipo           | Comportamiento                                  | Color                               |
| -------------- | ----------------------------------------------- | ----------------------------------- |
| `TRAP_DRAIN`   | Drena masa al pisarla. Cobra cada vez.          | Rojo oscuro `(0.70, 0.10, 0.30)`   |
| `TRAP_ONESHOT` | Se pisa una vez → WALL permanente.              | Naranja `(0.95, 0.50, 0.10)`       |
| `SPIKE`        | Muerte instantánea al pisar.                    | Rojo `(0.90, 0.05, 0.05)`          |

- `TRAP_DRAIN`: costo `trapDrainCost = 0.08`, desde nivel 8.
- `TRAP_ONESHOT`: destruye base floor + overlay al activarse, desde nivel 12.

## Niveles y mundos

| Mundo       | Niveles | Acceso          | Tamaño         | Timer           |
| ----------- | ------- | --------------- | -------------- | --------------- |
| **Mundo 1** | 1–15   | **GRATIS**      | 20×12→25×15   | No              |
| **Mundo 2** | 16–30  | 💰 `full_game`  | 25×15→35×20   | Desde ~nivel 26 |
| **Mundo 3** | 31–45  | 💰 incluido     | 35×20→40×24   | Sí              |
| **Mundo N** | 46+    | 💰 incluido     | escala         | Sí              |

**MazeStyle por mundo:** Mundo 1 (1–8) Dungeon | Mundo 1 (9–15) Labyrinth | Mundo 2–3 Labyrinth.

## Modo Infinito — Sistema 9

Escena: `InfiniteScene`. Scripts: `InfiniteGameManager` + `InfiniteHUDController`.
Se desbloquea completando Mundo 1 o comprando `infinite_pro` ($2.99).

- Masa **no se reinicia** entre mazes — bonus `+0.04` por maze completado
- Score = `mazes × 100 + estrellas × 10`
- `difficultyFactor = Clamp(0.55 + mazeIndex × 0.015, 0.55, 1.0)`
- Semillas: `RunBaseSeed` aleatorio, `mazeSeed = RunBaseSeed + mazeIndex × 7919`

| Maze   | Tamaño         | Estilo             | Elementos nuevos                              |
| ------ | -------------- | ------------------ | --------------------------------------------- |
| 1–3   | 20×12          | Dungeon            | Solo exploración                             |
| 4–6   | 20×12          | Dungeon            | PatrolEnemy (1)                               |
| 7–8   | 20×12          | Dungeon            | TrailEnemy (1)                                |
| 9–10  | 25×15          | Hybrid             | NARROW_06, TRAP_DRAIN, puertas                |
| 11–16 | 25–30×15–18   | Hybrid             | TRAP_ONESHOT, más enemigos                   |
| 17–21 | 35×20          | Hybrid→Labyrinth   | NARROW_04, SPIKE, TrailEnemy (2)              |
| 22+    | 40–45×24–28   | Labyrinth          | Timer (90s→45s), dificultad → 1.0            |

## Movimiento — PlayerMovement

Joystick flotante invisible. No hay modos seleccionables. D-pad táctil tiene prioridad si activo.

- `moveTimeSlow = 0.22` (tamaño 1.0) | `moveTimeFast = 0.08` (tamaño 0.15)
- `duration = Lerp(moveTimeFast, moveTimeSlow, InverseLerp(MinSize, InitialSize, currentSize))`
- `DPadController` envía `SetDPadDirection(Vector2Int)` a `PlayerMovement` — prioridad sobre joystick si `_dpadDir != zero`
- `DPadSettings.initialized`: `false` en primer arranque → posición del editor como default real

## HUD + Pausa

- `PauseMapController`: `Time.timeScale = 0/1`, botones Continuar / Reintentar / Menú / Añadir Masa / Añadir Tiempo / Controles
- `_addSizeButton` rewarded → `+rewardedSizeBonus` (0.15) | `_addTimeButton` → `+rewardedTimeBonus` (30s), solo si hay timer
- Los botones de recompensa se desactivan si ya se usó el rewarded en este nivel
- `GameResultController`: `_continueBonus = 0.30f`, usa `_applyRewardNextFrame` para callbacks AdMob

## Editor Visual — Sistema E

Window → Shrink → Level Editor. Edita: estrellas, trampas, puertas, NARROW, estructura (WALL↔PATH), enemigos, difficultyFactor, starSizeBonus, overlay BFS.

```csharp
List<CellOverride>  manualOverrides    // tipo de celda por posición
List<Vector2Int>    manualStarCells    // posiciones manuales de estrellas
List<EnemySpawn>    manualEnemySpawns  // { cell, type, patrolDir }
```

## Localización — Sistema L

```csharp
LocalizationManager.Init();
LocalizationManager.SetLanguage("fr");
LocalizationManager.Get("play");
```

Idiomas: EN, ES, PT, FR, DE. Event: `GameEvents.OnLanguageChanged`.

## Convenciones de código

- C# exclusivamente | Comentarios XML en métodos públicos | Cero magic numbers → `[SerializeField]`
- Namespaces: `Shrink.Core`, `Shrink.Maze`, `Shrink.Player`, `Shrink.Enemies`, `Shrink.Level`, `Shrink.UI`
- Un archivo = una clase | Input: `Keyboard.current` y `Touchscreen.current` (New Input System)
- Singletons: `GameManager`, `AdManager`, `AudioManager`, `LevelManager`, `SaveManager`, `IAPManager`

**GameEvents.cs — eventos globales:**
- `OnLevelComplete`, `OnLevelFail`, `OnDoorOpened`
- `OnSizeChanged`, `OnMigajaAbsorbed`, `OnNarrowPassageBlocked`
- `OnStarCollected(int collected, int total)`
- `OnTimerTick(float remaining)`, `OnTrapActivated(Vector2Int cell, CellType type)`
- `OnLanguageChanged`, `OnPlayerRevived`

## Monetización

| Producto          | Precio    | ID             | Desbloquea                                  |
| ----------------- | --------- | -------------- | ------------------------------------------- |
| Juego Completo    | **$2.99** | `full_game`    | Mundo 2, 3 y mundos futuros                 |
| Sin anuncios      | $1.99     | `no_ads`       | Elimina interstitials para siempre          |
| Pack Colores      | $0.99     | `color_pack`   | Paletas de color adicionales                |
| Modo Infinito Pro | $2.99     | `infinite_pro` | Modo Infinito sin completar Mundo 1         |

- Interstitial: cada 3 niveles completados (sin `full_game` ni `no_ads`)
- Rewarded: desde pausa, máximo 1 por nivel
- Precio sube a **$4.99** al lanzar con Mundo 2 + 3 completos

## Rendimiento

- Target: 60 fps estable en Android gama media
- Sin allocations en Update — pools para migajas y celdas
- Mazes >30×18 → `GenerateAsync` (hilo separado)

## UGS Stack (implementado)

- **Auth anónima**: `AuthenticationService.Instance.SignInAnonymouslyAsync()` en `GameBootstrap`
- **Cloud Save**: `GameData` JSON asociado al UUID. Sync en nivel completado y cierre de app. Gana el más reciente por `GameStats.levelsPlayed`
- **Leaderboards**: tabla `infinite_leaderboard` — `mazes × (masa normalizada 0–100)`. Top 100 + posición propia en `RunOverPanel`

---

## Modo Multijugador — Sistema M 🔧

### Stack de red
- **Photon Fusion 2** — Shared Mode
- Escena: `MultiplayerScene`
- Testing: `com.unity.multiplayer.playmode` (Window → Multiplayer Play Mode)

### Archivos clave
```
Scripts/Multiplayer/
  MultiplayerManager.cs        — NetworkRunner, matchmaking, INetworkRunnerCallbacks
  MultiplayerGameManager.cs    — orquesta fases, spawning de bots, cámara
  NetworkMazeState.cs          — estado compartido: seed, crumbs, fase, timer
  NetworkPlayer.cs             — jugador en red (IDPadTarget, BFS-free)
  NetworkBotPlayer.cs          — bot BFS hacia EXIT, spawneado por master si sala no llena
  NetworkPatrolEnemy.cs        — enemigo de patrulla en red, solo en celdas ROOM
Scripts/UI/
  MultiplayerHUDController.cs  — Matchmaking → Waiting → Countdown → HUD → Results
```

### Prefabs (deben estar en Fusion Network Project Config)
- `NetworkMazeState.prefab`, `NetworkPlayer.prefab`, `NetworkBotPlayer.prefab`, `NetworkPatrolEnemy.prefab`

### Flujo de partida
```
Matchmaking automático → Espera máx 20s → master spawna bots hasta 4 total
→ Countdown 5s → JUGANDO: Hybrid 45×35, EXIT al centro, PatrolEnemies en ROOMs
→ GameOver: timer 3min O todos terminaron/murieron
→ Resultados: solo puntúan los que llegaron al EXIT (DNF = 0 pts)
```

### Mecánicas de red
- **Migajas**: `NetworkArray<NetworkBool>` con fallback visual
- **Colisión p vs p**: el más grande roba `SizePerStep × 5` al más pequeño
- **Score**: `Size × 600 + Stars × 10 + 50 (bonus EXIT)`
- **Bots**: BFS hacia EXIT, respetan NARROW por tamaño, mueren por enemies

### Config Inspector — NetworkMazeState
| Campo | Default | Descripción |
|---|---|---|
| MazeWidth / MazeHeight | 45 / 35 | Máx ~63×31 con Capacity 2000 |
| GameDuration | 180s | Duración de la partida |
| CountdownDuration | 5s | Cuenta regresiva |
| DoorCount / Narrow06Count | 2 / 4 | Obstáculos |
| DrainCount / OneshotCount | 5 / 2 | Trampas |
| PatrolEnemyCount | 5 | Solo en ROOMs |

### Config Inspector — MultiplayerGameManager
| Campo | Default |
|---|---|
| MaxWaitSeconds | 20s |
| MaxPlayers | 4 |
| BotPrefab | NetworkBotPlayer.prefab |

### Asignaciones de escena requeridas
- `MultiplayerGameManager._hud` → `MultiplayerHUDController`
- `MultiplayerGameManager._dpad` → `DPadController`
- `MultiplayerGameManager._botPrefab` → `NetworkBotPlayer.prefab`
- `MultiplayerManager._playerPrefab` → `NetworkPlayer.prefab`
- `MultiplayerManager._mazeStatePrefab` → `NetworkMazeState.prefab`
- `NetworkMazeState._enemyPrefab` → `NetworkPatrolEnemy.prefab`

### Canvas — MultiplayerScene
```
Canvas (MultiplayerHUDController)
  ├── MatchmakingPanel   — spinner + "Conectando..."
  ├── WaitingPanel       — "Buscando jugadores..." con contador
  ├── CountdownPanel     — números 5…1
  ├── HUDPanel           — timer + scores en vivo + barra de masa local
  └── ResultsPanel       — ranking final, DNF en gris, Retry/Menu
```

### Pendiente
- `FindObjectsByType<NetworkPatrolEnemy>` en FixedUpdateNetwork — optimizar con cache si hay lag
- UGS Leaderboard multijugador: tabla `multiplayer_leaderboard` (pendiente)
- Skins de esfera para identificar jugadores (monetización futura)

---

## Backlog de mecánicas (post Mundo 3)

### Nuevos enemigos
| Tipo | Comportamiento | Introducción |
|---|---|---|
| AmbushEnemy | Estático hasta radio N, persigue brevemente | Nivel 22 |
| GhostEnemy | Atraviesa paredes; solo bloqueado por NARROW | Nivel 25 |
| MirrorEnemy | Se mueve en dirección opuesta al jugador | Modo Infinito |
| WanderEnemy | 70% random + 30% BFS; solo en celdas ROOM | Infinito mazes 13–14 |

### Nuevas trampas
`TRAP_SLOW` (×2 moveTime, 4s) | `TRAP_INVERT` (controles invertidos, 3s) | `TRAP_TELEPORT` (celda aleatoria) | `TRAP_CONTINUOUS` (drena 0.02/s parado) | `TRAP_TOGGLE` (WALL temporal, 6s)

### Nuevas celdas
`ICE` (desliza 2–3 celdas) | `PORTAL_A/B` (enlazadas) | `SWITCH` (activa/desactiva WALL) | `CONVEYOR` (empuja una celda extra) | `KEY/LOCK_DOOR` | `CONVEYOR` tipo vagoneta (mueve A→B sin gastar masa, bajada en cualquier punto intermedio)

### Visual jugador — Blob cluster
Código inactivo en `Assets/_Project/Scripts/Player/BlobClusterVisual.cs`. Blob central + satélites con spring physics. Activar en `SphereController.Initialize` → `BlobClusterVisual.Setup(cellSize)`.

---

## Lo que NO hacer

- No usar assets externos (sprites, fuentes de terceros, modelos)
- No joystick visible — siempre invisible/flotante. El D-pad SÍ es visible; no confundir
- No mostrar el maze completo durante gameplay (solo en pausa)
- No añadir sistemas fuera del orden listado sin confirmación
- No resumir ni explicar el código entregado — solo código completo y funcional
- No usar `UnityEngine.Input` (legacy) — siempre `UnityEngine.InputSystem`
