# Mini practica: Amo del Calabozo con ChatGPT en tiempo real

## Objetivo

Crear una escena de Unity donde el jugador escribe texto libre y el Amo del Calabozo responde usando la API de OpenAI en tiempo real.

La escena incluye:

- historial de conversacion
- campo para API key
- campo de input del jugador
- boton Enviar
- boton Reiniciar
- llamada HTTP a `https://api.openai.com/v1/responses`

## Como probar

1. Abre el proyecto en Unity.
2. Abre `Assets/Scenes/SampleScene.unity`.
3. Pulsa Play.
4. Pega una OpenAI API key en el campo superior.
5. Escribe una accion del jugador.
6. Pulsa Enviar.

Ejemplos de acciones:

```text
Examino las runas de la puerta.
Pregunto al Amo del Calabozo quien vive aqui.
Enciendo la antorcha apagada.
Intento escuchar detras de la puerta.
```

## Importante sobre la API key

Este ejemplo es didactico.

En un videojuego real no se debe poner una API key secreta dentro del cliente Unity, porque el jugador podria extraerla. En produccion se usaria un servidor propio como intermediario:

```text
Unity -> servidor propio -> OpenAI API
```

Para clase, usar la key directamente en Unity permite ver el flujo completo sin montar backend.

## Script principal

```text
Assets/Scripts/RealtimeDungeonChat.cs
```

Responsabilidades:

- leer el texto escrito por el jugador
- construir el prompt con historial
- llamar a la API de OpenAI
- mostrar la respuesta del Amo del Calabozo
- bloquear la UI mientras espera respuesta
- mostrar errores de API en pantalla

## Modelo

El script usa por defecto:

```text
gpt-4.1-mini
```

Si la cuenta no tiene acceso a ese modelo, cambiar el campo `Model` del componente `RealtimeDungeonChat` en Inspector por otro modelo disponible en la cuenta.

## Prompt del sistema

El comportamiento del Amo del Calabozo se controla desde el campo `System Prompt` del componente `RealtimeDungeonChat`.

Prompt incluido:

```text
Eres el Amo del Calabozo de una aventura conversacional de fantasia medieval.
Hablas en español claro, con tono misterioso pero apto para todos los publicos.
Responde siempre como narrador y NPC, no como asistente tecnico.
Cada respuesta debe tener 2 partes:
1. una consecuencia narrativa breve de la accion del jugador
2. una nueva pregunta o decision para continuar la aventura
No escribas respuestas largas. Maximo 120 palabras.
No menciones que eres una IA ni que usas un modelo de lenguaje.
```

## Preguntas para clase

```text
El modelo mantiene el personaje?
El jugador entiende que puede escribir cualquier accion?
Las respuestas son jugables o solo literarias?
La IA conserva coherencia con el historial?
Que ocurre si el jugador intenta romper el tono?
Que riesgos tiene poner una API key en cliente?
Como cambiaria esto usando un servidor propio?
```

