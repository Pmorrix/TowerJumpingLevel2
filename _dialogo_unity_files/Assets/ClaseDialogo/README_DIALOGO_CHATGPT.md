# Mini practica: Amo del Calabozo

## Objetivo

Crear un cuadro de dialogo para una aventura conversacional tipo fantasia medieval.

Unity muestra la interfaz y gestiona las opciones. ChatGPT genera el contenido narrativo:

- escena inicial
- tres opciones
- consecuencia de cada opcion
- nuevo estado narrativo

No se usa API, claves, conexion online ni IA en tiempo real dentro de Unity. El alumno usa ChatGPT gratuito fuera de Unity y copia el resultado al Inspector.

## Como montar la escena

1. Abre el proyecto en Unity.
2. Espera a que compile.
3. Usa el menu superior:

```text
Clase IA Dialogo > Crear escena Amo del Calabozo
```

4. Unity creara una escena nueva:

```text
Assets/Scenes/AmoDelCalabozo_Dialogo.unity
```

5. Pulsa Play.
6. Elige una de las tres respuestas.
7. Comprueba que aparece una consecuencia narrativa.
8. Pulsa Reiniciar dialogo para volver al estado inicial.

## Objetos creados

- Main Camera
- Directional Light
- Suelo
- Puerta de piedra
- Amo del Calabozo
- Antorchas
- Canvas
- EventSystem
- DialogueManager

## Script principal

```text
Assets/Scripts/DialogueManager.cs
```

Responsabilidad:

- mostrar la escena inicial
- mostrar tres opciones
- detectar la opcion elegida
- mostrar la consecuencia
- bloquear las opciones al terminar
- permitir reiniciar el dialogo

## Donde se pega el contenido generado por ChatGPT

Selecciona el GameObject:

```text
DialogueManager
```

En el Inspector cambia los campos:

```text
Initial Scene
Option 1
Option 2
Option 3
Result 1
Result 2
Result 3
Next State
```

## Prompt base para ChatGPT

```text
Actua como disenador narrativo de videojuegos.

Quiero crear una escena breve de aventura conversacional tipo Dungeons & Dragons.

Necesito que generes un dialogo para un NPC llamado Amo del Calabozo.

Formato obligatorio:

ESCENA:
[texto narrativo inicial, maximo 4 lineas]

OPCIONES:
1. [opcion breve]
2. [opcion breve]
3. [opcion breve]

CONSECUENCIAS:
1. [consecuencia narrativa de elegir la opcion 1, maximo 3 lineas]
2. [consecuencia narrativa de elegir la opcion 2, maximo 3 lineas]
3. [consecuencia narrativa de elegir la opcion 3, maximo 3 lineas]

NUEVO ESTADO:
[texto final que deja la aventura preparada para continuar]

Restricciones:
- tono de fantasia medieval
- lenguaje claro
- apto para todos los publicos
- sin violencia explicita
- sin textos largos
- maximo tres opciones
- que cada opcion tenga una consecuencia distinta
```

## Prompt para convertir la salida a campos de Unity

```text
Convierte este dialogo en datos faciles de copiar al Inspector de Unity.

Formato:

Initial Scene:
"..."

Option 1:
"..."

Option 2:
"..."

Option 3:
"..."

Result 1:
"..."

Result 2:
"..."

Result 3:
"..."

Next State:
"..."

No anadas explicaciones. Solo devuelve los campos.
```

## Preguntas para comparar

```text
Las opciones son realmente diferentes?
Cada opcion tiene una consecuencia clara?
El texto cabe en pantalla?
El tono es coherente?
El jugador entiende que puede hacer?
La IA esta escribiendo narrativa o disenando decisiones?
```

## Idea docente

La IA puede generar texto, pero en videojuegos el texto no basta. Hay que controlar:

- decision del jugador
- consecuencia
- claridad
- longitud
- tono
- estructura de datos
- integracion con UI

