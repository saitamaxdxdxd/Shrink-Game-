# CLAUDE.md — Shrink (Unity 6)

## Proyecto
Juego móvil puzzle maze 2D top-down para iOS y Android.
Motor: Unity 6 | Resolución: 1920x1080 landscape | Estilo: minimalista (líneas y círculos, sin assets externos)
Input System: **New Input System** (Unity package activo — NO usar `UnityEngine.Input` legacy)

## Ruta raíz de código
```
Assets/_Project/
```
Todo el código del juego vive aquí. No tocar Assets/Scenes, Assets/Settings ni carpetas generadas por Unity fuera de _Project.

## Estructura de carpetas
```
Scripts/
  Core/          ShapeFactory, GameBootstrap (test), GameManager (pendiente Sistema 5)
  Events/        GameEvents.cs — eventos globales estáticos
  Maze/          MazeGenerator, MazeData, CellType, MazeRenderer
                 Editor/MazeDebugVisualizerEditor.cs
  Player/        SphereController, ShrinkMechanic, Crumb
  Movement/      PlayerMovement
  Camera/        CameraFollow
  Level/         LevelManager, LevelData (ScriptableObject), LevelLoader  ← pendiente Sistema 5
  Monetization/  AdManager, IAPManager                                    ← pendiente Sistema 6
  Audio/         AudioManager                                              ← pendiente Sistema 7
  UI/            Un controller por pantalla                               ← pendiente Sistema 8

ScriptableObjects/
  Levels/        30 LevelData assets (Level_01 … Level_30)               ← pendiente Sistema 5
  Colors/        Paletas de color (IAP)

Prefabs/         pendiente
Scenes/
Materials/
Sprites/
Fonts/
Audio/
FX/Particles/
```

## Sistemas — estado actual

| # | Sistema | Estado | Notas |
|---|---------|--------|-------|
| 1 | Generación procedural de maze | ✅ Completo | BSP + Labyrinth + Hybrid |
| 2 | Esfera y mecánica de desgaste | ✅ Completo | Migajas, puertas, calibración auto |
| 3 | Movimiento por swipe + cámara | ✅ Completo | Smart Slide + WASD para testing |
| 4 | Mapa en pausa | ⬜ Pendiente | |
| 5 | Sistema de niveles y semillas | ⬜ Pendiente | |
| 6 | Monetización | ⬜ Pendiente | |
| 7 | Juice y sonido | ⬜ Pendiente | |
| 8 | UI completa | ⬜ Pendiente | |

## Generación de maze — MazeStyle

```csharp
public enum MazeStyle { Dungeon, Labyrinth, Hybrid }
```

| Estilo | Algoritmo | Usar para |
|--------|-----------|-----------|
| Dungeon | BSP (cuartos + corredores) | Niveles 1–9 |
| Labyrinth | Recursive Backtracker DFS | Niveles 10–30 |
| Hybrid | Backtracker + cuartos tallados | Alternativa con zonas abiertas |

- **Labyrinth es el modo recomendado** para gameplay real — el Smart Slide funciona mejor con corredores de 1 celda de ancho.
- BSP genera cuartos grandes donde el Smart Slide se degrada a step-by-step.
- El maze siempre se valida con BFS antes de devolverse. `ShortestPathLength` queda en `MazeData`.

## Movimiento — PlayerMovement

Modo activo: **SlideToWall con Smart Slide**

Reglas de parada al deslizar:
1. Siguiente celda es WALL o fuera de límites
2. Pasaje estrecho bloqueado por tamaño actual
3. Celda actual tiene salidas perpendiculares (intersección)
4. Celda actual es EXIT

Campo `Mode` en Inspector permite cambiar a `StepByStep` para testing con teclado.

## Calibración de dificultad — ShrinkMechanic

```
sizePerStep = 0.85 × difficultyFactor ÷ shortestPathLength
```

| difficultyFactor | Significado |
|---|---|
| 0.50 | Tutorial (niveles 1–3) |
| 0.65 | Aprendizaje (niveles 4–7) |
| 0.75 | Normal (niveles 8–10) |
| 0.80 | Exigente (niveles 11–15) |
| 0.85 | Difícil (niveles 16–20) |
| 0.90 | Muy difícil (niveles 21–25) |
| 0.95 | Casi perfecto (niveles 26–30) |
| 0.95→1.0 | Infinito creciente |

- `autoCalibrate = true` (default): calcula sizePerStep automáticamente según el maze generado.
- En Sistema 5, `difficultyFactor` vendrá del `LevelData` ScriptableObject de cada nivel.
- Los callejones sin salida son gratis si el jugador retrocede completo (migajas recuperan el tamaño exacto).

## Convenciones de código

- Lenguaje: C# exclusivamente
- Comentarios XML en todos los métodos públicos (`/// <summary>`)
- Cero magic numbers — todo valor configurable es `[SerializeField]`
- Singletons: GameManager, AdManager, AudioManager, LevelManager
- Eventos globales estáticos en `Scripts/Events/GameEvents.cs`:
  - `OnLevelComplete`, `OnLevelFail`, `OnDoorOpened`
  - `OnSizeChanged`, `OnMigajaAbsorbed`, `OnNarrowPassageBlocked`
- Namespaces: `Shrink.Core`, `Shrink.Maze`, `Shrink.Player`, etc.
- Un archivo = una clase (sin excepciones)
- Nombres de archivo en PascalCase, exactamente igual al nombre de la clase
- Input: `Keyboard.current` y `Touchscreen.current` (New Input System)

## Tipos de celda (MazeData)
```csharp
public enum CellType
{
    WALL, PATH, ROOM, CORRIDOR, DOOR,
    START, EXIT, NARROW_06, NARROW_04
}
```

## Mecánica central — reglas inamovibles
- Tamaño inicial esfera: `1.0`
- Tamaño mínimo antes de muerte: `0.15`
- Rango usable: `0.85`
- `NARROW_06`: requiere tamaño < 0.6
- `NARROW_04`: requiere tamaño < 0.4
- Migajas: una por celda, recuperan exactamente el tamaño perdido al ser reabsorbidas
- Puertas: consumen tamaño permanentemente (sin migaja depositada), costo default `0.10`
- El maze siempre tiene solución — validado con BFS, `ShortestPathLength` guardado en `MazeData`

## Niveles
- Mundo 1 (1–10): 20×12, sin tiempo, sin puertas. NARROW_06 desde nivel 5. MazeStyle: Dungeon
- Mundo 2 (11–20): 25×15, sin tiempo, 1–2 puertas. NARROW_04 desde nivel 15. MazeStyle: Labyrinth
- Mundo 3 (21–30): 35×20 → 40×24, con timer, 2–4 puertas (bloqueado tras IAP "Mundo 3"). MazeStyle: Labyrinth
- Modo infinito: desbloqueable al completar nivel 30, semillas aleatorias, dificultad creciente. MazeStyle: Labyrinth

## Monetización
| Producto | Precio | ID sugerido |
|----------|--------|-------------|
| Sin anuncios | $1.99 | `no_ads` |
| Pack Colores | $0.99 | `color_pack` |
| Mundo 3 | $1.99 | `world_3` |
| Modo Infinito Pro | $2.99 | `infinite_pro` |

- Interstitial AdMob: cada 3 niveles completados, solo en pantalla de nivel completado
- Rewarded AdMob: game over → continuar con 50% tamaño / +30 segundos. Máximo 1 por nivel.

## Rendimiento
- Target: 60 fps estable en Android gama media
- Sin allocations en Update — usar pools para migajas y celdas
- El maze se genera en un hilo separado si supera 30×18 (via `GenerateAsync`)

## Lo que NO hacer
- No usar assets externos (sprites, fuentes de terceros, modelos)
- No joystick visible en pantalla durante gameplay
- No mostrar el maze completo durante gameplay (solo en pausa)
- No añadir sistemas fuera del orden listado sin confirmación
- No resumir ni explicar el código entregado — solo código completo y funcional
- No usar `UnityEngine.Input` (legacy) — siempre `UnityEngine.InputSystem`
