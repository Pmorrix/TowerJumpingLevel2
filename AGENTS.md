# Proyecto: Tower Jumping / Videojuego de edificios (Unity)

## Contexto general
Este proyecto es un juego arcade 3D en Unity donde el jugador salta entre edificios en varios carriles. El objetivo es mantener el flujo de saltos, sumar puntuacion, gestionar boosters, volver al goal para completar el nivel y pasar entre escenas con persistencia basica de score/vidas/nivel.

## Prioridades de trabajo
1. Mantener la solucion lo mas simple posible.
2. Hacer cambios minimos y localizados.
3. No introducir nuevas capas, sistemas o scripts si no son estrictamente necesarios.
4. Reutilizar scripts existentes antes de crear otros nuevos.
5. Mantener coherencia con la arquitectura actual.
6. No romper referencias de Inspector.
7. Si una mejora requiere refactor grande, primero proponerla y no aplicarla directamente.

## Estilo de implementacion
- Lenguaje: C#
- Entorno: Unity
- Enfoque: gameplay arcade, UI clara, scripts desacoplados
- Preferencia: metodos pequenos, nombres claros, cambios contenidos
- Evitar sobreingenieria
- Evitar DOTween salvo peticion explicita
- No cambiar nombres publicos/serializados sin motivo
- No eliminar campos `[SerializeField]` usados por Inspector sin comprobar impacto

## Que debe hacer Codex al empezar una tarea
1. Leer primero los scripts relacionados directamente con la tarea.
2. Detectar dependencias entre scripts antes de editar.
3. Identificar si el comportamiento depende de:
   - escenas
   - referencias por Inspector
   - eventos
   - flags estaticos
   - `Time.timeScale`
   - flujo UI
4. Proponer la solucion mas pequena viable.
5. Aplicar solo lo pedido.
6. Explicar brevemente:
   - que cambia
   - por que
   - que hay que revisar en Inspector
   - riesgos colaterales

## Que no debe hacer Codex
- No inventar features no pedidas.
- No reestructurar sistemas enteros sin autorizacion.
- No tocar scripts no relacionados "por limpieza".
- No introducir patrones complejos si un cambio simple resuelve el problema.
- No asumir que una referencia existe en Inspector: comprobar nulls y preservar fallback simple.
- No cambiar flujo de escenas sin revisar persistencia.

## Arquitectura actual relevante

### Persistencia entre escenas
La persistencia basica entre niveles usa `GameSession` con:
- `CurrentScore`
- `CurrentLives`
- `CurrentLevel`

`LevelLoader` guarda score, vidas y nivel antes de cargar:
- `Scene Level 2`
- `Scene Level 3`
- `Phase 1 Pass`
- menu `Menu`

### Vidas
`LivesTextDisplay`:
- empieza con `startingLives = 3`
- emite `OnLivesDepleted`
- mantiene snapshot de score al inicio de cada vida para calculos por vida
- actualiza el HUD `Lives: X`

### Game Over / TAX / High Score
`GameOverController`:
- detiene el score
- desactiva behaviours del player
- congela con `Time.timeScale = 0`
- ejecuta TAX visual mediante `newPhaseManager.PlayExitTaxVisualThen(...)`
- luego muestra panel y delega a `HighScoreSystem`

`GameOverPanelUI`:
- muestra score en 5 digitos
- puede animar descuento visual del TAX en realtime (`WaitForSecondsRealtime`)

`HighScoreSystem`:
- guarda en `Application.persistentDataPath/highscore.json`
- usa iniciales + score
- si hay nuevo high score, apaga `gameOverPanel` y enciende `highScorePanel`
- puede pintar `hudHighScoreTxt`

### Boosters
`BoostersManager`:
- gestiona edificios con booster
- apaga todos al inicio
- activa un subconjunto aleatorio
- usa `maxActive`, `activeDuration`, `autoStart`

`BuildingBooster`:
- cada edificio puede tener `boosterRoot` visual asociado

### Tiempo de edificios
`BuildingTimeController`:
- cada edificio puede consumir tiempo y desactivarse
- soporta `immuneToTime`
- cambia color de ventanas
- puede hacer `DisableBuildingImmediate`
- en Bonus existe `forcedCountdown` para que el edificio se destruya aunque el player no este encima

`MenuControllerUI`:
- navegacion por opciones con teclado
- SFX de move/select
- visual de opcion activa
- integracion con `MenuPanelsUI`

### Combos
`ComboManager`:
- combo basado en aterrizajes validos
- depende de estado boost
- puede romperse por repetir edificio o por exceder tiempo
- usa raycast hacia abajo

`ComboUIController`:
- muestra `Xn COMBO`
- oculta automaticamente
- permite `ForceHide()`

## Reglas de edicion
- Si editas un script, conserva:
  - nombres de clases
  - serializados del Inspector
  - callbacks publicos usados por botones/UI
- Si un cambio afecta flujo de juego, revisar:
  - respawn
  - booster
  - goal
  - TAX
  - game over
  - carga de escena
- Anadir null-checks simples cuando sea razonable
- Si hay dos opciones, elegir la mas simple y menos invasiva

## Formato de respuesta esperado al terminar una tarea
Entregar siempre:
1. Resumen corto del cambio
2. Script completo si el cambio afecta varios puntos o puede romper copy-paste parcial
3. Lista breve de que revisar en Inspector
4. Posibles efectos colaterales
5. No anadir alternativas implementadas sin permiso
