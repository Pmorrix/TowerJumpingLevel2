# Guion de clase practica: Cube Runner Arena con IA

**Duracion:** 5 horas  
**Contexto:** Master en Creacion de Videojuegos con Unity 3D y C#  
**Bloque:** Inteligencia Artificial aplicada al desarrollo de videojuegos  
**Herramienta de apoyo:** ChatGPT gratuito, sin suscripcion

## Objetivo general

Crear un minijuego completo en Unity usando primitivas basicas y apoyandose en ChatGPT como asistente tecnico.

El alumnado primero razonara la estructura del minijuego y los scripts necesarios. Despues pedira a la IA que proponga la arquitectura y genere los scripts. Finalmente se compararan ambas soluciones.

La idea principal de la clase no es solo que el juego funcione, sino aprender a evaluar criticamente lo que propone una IA.

## Minijuego propuesto

**Nombre:** Cube Runner Arena

El jugador controla una capsula dentro de una arena sencilla. Debe recoger todas las esferas antes de que termine el tiempo y evitar los cubos rojos en movimiento.

### Elementos de la escena

- Player: una capsula.
- Coleccionables: esferas.
- Obstaculos: cubos rojos.
- Suelo: un plano.
- Paredes: cubos.
- Camara: fija, mostrando toda la arena.
- UI: puntuacion, tiempo y mensaje final.

### Condiciones de juego

- El jugador se mueve por el plano XZ.
- Cada esfera recogida suma puntos.
- Al recoger todas las esferas, el jugador gana.
- Si el jugador toca un obstaculo, pierde.
- Si el tiempo llega a 0, pierde.
- Al aparecer Game Over, el Player se desactiva.
- La escena se puede reiniciar pulsando R.

## Resultado esperado al final de la practica

```text
Escena jugable.
Scripts funcionando.
Game Over.
Victoria.
Reinicio.
Comparacion entre razonamiento humano e IA.
```

# 0:00 - 0:15 | Apertura de la practica

## Explicacion inicial

El profesor presenta el reto:

> Vamos a crear un minijuego muy pequeno, pero completo. Primero pensaremos como desarrolladores: que objetos necesita la escena, que comportamientos hacen falta y que scripts deberian existir. Despues pediremos a ChatGPT que proponga los scripts y compararemos su solucion con la nuestra.

## Ideas clave

- La IA no sustituye la comprension de Unity.
- ChatGPT puede escribir codigo, pero no ve automaticamente la escena.
- En Unity, el comportamiento final depende de codigo, GameObjects, componentes, Inspector, fisicas, tags y UI.
- Una buena solucion no es la mas grande, sino la mas clara y mantenible.

## Objetivo de esta primera parte

Que el alumnado entienda que antes de escribir codigo hay que saber:

```text
Que objetos hay.
Que comportamientos hacen falta.
Que eventos ocurren.
Que datos se comparten.
Que referencias van por Inspector.
Que condiciones hacen ganar o perder.
```

# 0:15 - 1:15 | Preparar el terreno de juego

Durante la primera hora, el alumnado prepara manualmente la escena de Unity.

## Tarea del alumnado

Crear una escena con:

- Un `Plane` como suelo.
- Una `Capsule` como Player.
- Un `Rigidbody` en el Player.
- Tag `Player` asignado al Player.
- Cubos como paredes.
- Esferas como coleccionables.
- Cubos rojos como obstaculos.
- Camara fija.
- Luz principal.
- Canvas con textos de UI.
- Un GameObject vacio llamado `GameManager`.

## UI necesaria

En el Canvas deben existir:

```text
ScoreText
TimeText
MessageText
```

`MessageText` puede empezar vacio o desactivado.

## Checklist de escena

```text
Player tiene Rigidbody.
Player tiene Collider.
Player tiene tag Player.
Coleccionables tienen Collider con Is Trigger.
Obstaculos tienen Collider normal.
Camara ve toda la arena.
UI existe.
GameManager esta en escena.
```

## Si no terminan la escena

Si pasado este tiempo algun alumno o grupo no ha conseguido preparar el terreno, se entregara un `.unitypackage` con la escena ya montada, pero sin scripts.

Esto permite que todo el grupo pueda continuar con la parte importante de la clase:

```text
Pensar los scripts.
Pedir ayuda a la IA.
Integrar codigo.
Depurar.
Comparar soluciones.
```

# 1:15 - 1:30 | Puesta en comun de la escena

## Revision rapida

El profesor revisa problemas habituales:

- Falta el tag `Player`.
- El Player no tiene `Rigidbody`.
- Los coleccionables no tienen `Is Trigger`.
- Los obstaculos tienen `Is Trigger` por error.
- La camara no ve bien la arena.
- La UI no esta enlazada.
- Falta el objeto `GameManager`.

## Mensaje clave

> En Unity, gran parte del comportamiento depende de la escena y del Inspector, no solo del codigo.

## Preguntas al alumnado

```text
Que objeto deberia detectar que se ha recogido una esfera?
Quien deberia guardar la puntuacion?
Quien deberia controlar el tiempo?
Quien decide si se gana o se pierde?
Donde deberia ir el codigo de movimiento?
Que referencias se asignan desde Inspector?
```

# 1:30 - 2:20 | Diseno manual de los scripts

Antes de usar ChatGPT, el alumnado debe proponer que scripts cree que necesita el juego.

## Actividad

Cada grupo debe escribir:

```text
Lista de scripts necesarios.
Responsabilidad de cada script.
GameObject donde iria colocado cada script.
Referencias necesarias en Inspector.
Eventos importantes.
```

## Preguntas guia

```text
Que script debe mover al Player?
Que script debe detectar una esfera recogida?
Que script debe controlar el score?
Que script debe controlar el tiempo?
Que script debe decidir victoria o derrota?
Que script debe mover los obstaculos?
Que script debe detectar choque con obstaculo?
Que referencias deben ir por Inspector?
```

## Solucion minima esperada

La solucion puede quedar controlada por 4 scripts:

```text
PlayerMovement.cs
Collectible.cs
GameManager.cs
MovingObstacle.cs
```

No se da esta lista como respuesta cerrada al principio. Primero se deja que el alumnado razone.

## Responsabilidad de cada script

### PlayerMovement.cs

```text
Mover el Player con teclado.
Usar Rigidbody.
Movimiento en plano XZ.
No decidir victoria ni derrota.
```

### Collectible.cs

```text
Detectar al Player con OnTriggerEnter.
Avisar al GameManager.
Sumar puntos.
Desactivar la esfera recogida.
```

### GameManager.cs

```text
Controlar score.
Controlar tiempo.
Contar coleccionables.
Mostrar victoria.
Mostrar Game Over.
Desactivar el Player al perder.
Permitir reiniciar con R.
Actualizar la UI.
```

### MovingObstacle.cs

```text
Mover un cubo rojo.
Rebotar o alternar direccion.
Detectar colision con el Player.
Avisar al GameManager para perder la partida.
```

## Entrega parcial de esta fase

Cada grupo debe tener una propuesta escrita:

```text
Scripts propuestos por el grupo.
Responsabilidad de cada uno.
Donde se colocan.
Que necesita cada uno por Inspector.
```

# 2:20 - 2:30 | Descanso

Pausa breve antes de pasar al trabajo con IA.

# 2:30 - 3:20 | Desarrollo con ChatGPT

En esta fase se usa ChatGPT gratuito para pedir una solucion tecnica.

La idea no es pedir directamente "hazme el juego", sino formular un prompt claro para que la IA proponga primero la arquitectura.

## Prompt inicial recomendado

```text
Actua como desarrollador Unity C# senior.

Estoy creando un minijuego 3D con primitivas basicas:
- capsula como Player
- esferas como coleccionables
- cubos rojos como obstaculos
- plano como suelo
- paredes hechas con cubos
- camara fija
- UI con ScoreText, TimeText y MessageText

Funcionamiento deseado:
- el jugador se mueve con teclado por el plano XZ
- recoge esferas para sumar puntos
- al recoger todas las esferas gana
- si toca un obstaculo pierde
- si el tiempo llega a 0 pierde
- al hacer Game Over, el Player debe desactivarse
- se puede reiniciar la escena con R

Restricciones:
- Unity 3D
- C#
- no usar nuevo Input System
- no usar Cinemachine
- no usar DOTween
- no usar ScriptableObjects
- mantenerlo simple
- usar Rigidbody para el Player
- usar triggers para coleccionables
- usar colisiones para obstaculos
- incluir null-checks simples
- explicar que revisar en Inspector

Primero:
1. dime que scripts son necesarios
2. explica brevemente la responsabilidad de cada script
3. indica en que GameObject debe ir cada script
4. indica que referencias deben asignarse en Inspector

Despues:
5. dame el codigo completo de cada script necesario
```

## Objetivo pedagogico del prompt

El alumnado debe observar:

```text
Que scripts propone la IA.
Si propone demasiados scripts.
Si olvida alguna responsabilidad.
Si respeta las restricciones.
Si entiende que la camara es fija.
Si usa Unity 3D y no Unity 2D.
Si explica bien el Inspector.
Si el codigo parece integrable.
```

## Preguntas para analizar la respuesta de ChatGPT

```text
Propone una camara dinamica aunque la camara es fija?
Propone un UIManager innecesario?
Propone un GameManager demasiado grande?
Usa Input System nuevo sin pedirlo?
Usa Rigidbody2D por error?
Donde coloca cada responsabilidad?
Que referencias exige por Inspector?
El codigo se entiende?
Hay algo que no hayamos pedido?
```

## Reglas para trabajar con la IA

- No copiar codigo sin leerlo.
- No pegar varios scripts sin comprobar que compilan.
- No aceptar sistemas extra si no hacen falta.
- No dejar que la IA cambie el objetivo del ejercicio.
- Pedir siempre explicacion de Inspector.
- Pedir cambios pequenos cuando haya errores.

# 3:20 - 4:10 | Integracion y debugging en Unity

En esta fase se copian los scripts, se asignan referencias en Inspector y se prueba la escena.

## Checklist de funcionamiento

```text
Player se mueve.
Player no gira raro.
Score sube al recoger esferas.
Las esferas desaparecen.
El tiempo baja.
Al recoger todo aparece victoria.
Al llegar a 0 aparece Game Over.
Al tocar obstaculo aparece Game Over.
En Game Over el Player se desactiva.
R reinicia la escena.
```

## Errores habituales

### Error 1: NullReferenceException

Suele indicar que falta una referencia en Inspector.

Comprobar:

```text
Textos de UI asignados.
Referencia al GameManager asignada.
Referencia al Player asignada.
Objetos en escena activos.
```

### Error 2: OnTriggerEnter no se ejecuta

Comprobar:

```text
La esfera tiene Collider.
La esfera tiene Is Trigger activado.
El Player tiene Collider.
El Player tiene Rigidbody.
El Player tiene tag Player.
```

### Error 3: OnCollisionEnter no se ejecuta

Comprobar:

```text
El obstaculo tiene Collider normal.
El Player tiene Collider.
El Player tiene Rigidbody.
El obstaculo no esta marcado como Trigger.
```

### Error 4: el input no funciona

Si aparece un error relacionado con `UnityEngine.Input`, puede que el proyecto este configurado para usar el nuevo Input System.

Solucion simple para esta practica:

```text
Edit > Project Settings > Player > Active Input Handling
Seleccionar Both o Input Manager (Old)
Reiniciar Unity si lo pide.
```

### Error 5: la UI no actualiza

Comprobar:

```text
Los textos estan asignados al GameManager.
Se esta usando TextMeshPro si el script usa TMP_Text.
El Canvas esta activo.
No hay errores de compilacion.
```

## Prompt de depuracion recomendado

```text
Tengo este error en Unity:
[pegar error completo]

Este es el script:
[pegar script]

Contexto de la escena:
- GameObject donde esta el script:
- componentes del Player:
- tag del Player:
- colliders:
- Rigidbody:
- referencias asignadas en Inspector:

Dime:
1. causa mas probable
2. como comprobarlo en Unity
3. cambio minimo para arreglarlo
4. que no deberia tocar
```

## Idea clave de debugging

> Cuando se depura con IA, el contexto de la escena es tan importante como el codigo.

# 4:10 - 4:40 | Comparacion de scripts

Ahora se comparan tres cosas:

```text
1. La arquitectura pensada por el alumno.
2. La arquitectura propuesta por ChatGPT.
3. El codigo final corregido en clase.
```

## Criterios de comparacion

```text
Compila?
Funciona?
Es simple?
Se entiende?
Respeta Unity 3D?
Usa bien el Inspector?
Tiene null-checks razonables?
Separa responsabilidades?
Evita sistemas innecesarios?
El Game Over desactiva el Player?
El reinicio funciona?
```

## Preguntas para debate

```text
Que scripts propuso el alumno?
Que scripts propuso la IA?
La IA propuso scripts de mas?
La IA olvido alguna responsabilidad?
Que hizo mejor ChatGPT?
Que hizo mejor el alumno?
Que asumio mal la IA?
Que dependia del Inspector?
Que prompt mejoro mas el resultado?
```

## Conclusiones esperadas

- ChatGPT puede acelerar mucho la escritura de scripts.
- La IA puede proponer arquitectura, no solo codigo.
- La arquitectura propuesta debe revisarse.
- En Unity, muchas decisiones viven fuera del script.
- El Inspector es parte del sistema.
- Un prompt con restricciones claras reduce errores.
- El programador sigue siendo responsable de validar.

# 4:40 - 5:00 | Cierre y entrega final

## Entrega minima del alumnado

Cada grupo debe entregar:

```text
Escena jugable.
Scripts funcionando.
Prompt usado con IA.
Lista de scripts propuesta por el alumno.
Lista de scripts propuesta por la IA.
Comparacion breve entre ambas.
Lista de errores encontrados.
```

## Cierre conceptual

Mensaje final para el alumnado:

> La IA no solo escribe codigo. Tambien propone una arquitectura. Nuestro trabajo como desarrolladores es evaluar si esa arquitectura es minima, clara y compatible con la escena. En Unity, la respuesta correcta vive en la relacion entre codigo, GameObjects, Inspector, fisicas, UI y prueba en Play Mode.

# Anexo: scripts finales esperados

La practica puede resolverse con estos cuatro scripts:

```text
PlayerMovement.cs
Collectible.cs
GameManager.cs
MovingObstacle.cs
```

## PlayerMovement.cs

Responsabilidad:

```text
Mover al Player con teclado usando Rigidbody.
```

Debe ir en:

```text
Capsule / Player
```

Debe revisar en Inspector:

```text
El Player tiene Rigidbody.
El Player tiene Collider.
El Player tiene tag Player.
```

## Collectible.cs

Responsabilidad:

```text
Detectar cuando el Player recoge una esfera.
Avisar al GameManager.
Desactivar la esfera.
```

Debe ir en:

```text
Cada esfera coleccionable.
```

Debe revisar en Inspector:

```text
Collider con Is Trigger activado.
Referencia al GameManager asignada si el script la necesita.
```

## GameManager.cs

Responsabilidad:

```text
Controlar puntuacion.
Controlar tiempo.
Contar coleccionables.
Mostrar mensajes.
Gestionar victoria.
Gestionar derrota.
Desactivar el Player al perder.
Reiniciar la escena con R.
```

Debe ir en:

```text
GameObject vacio llamado GameManager.
```

Debe revisar en Inspector:

```text
ScoreText asignado.
TimeText asignado.
MessageText asignado.
Player asignado.
Tiempo inicial configurado.
Numero de coleccionables correcto.
```

## MovingObstacle.cs

Responsabilidad:

```text
Mover un obstaculo rojo.
Detectar colision con el Player.
Avisar al GameManager para provocar Game Over.
```

Debe ir en:

```text
Cada cubo rojo que actue como obstaculo.
```

Debe revisar en Inspector:

```text
Collider normal.
Is Trigger desactivado.
Referencia al GameManager si el script la necesita.
Velocidad y distancia de movimiento configuradas.
```

# Anexo: prompt corto para pedir mejoras

```text
Este script funciona, pero quiero revisarlo como ejercicio docente.

Analizalo para Unity 3D y dime:
1. si cumple su responsabilidad
2. si tiene dependencias de Inspector
3. si hay riesgo de NullReferenceException
4. si esta mezclando responsabilidades
5. que cambio minimo harias para mejorarlo

No propongas sistemas nuevos.
No uses Cinemachine.
No uses nuevo Input System.
No uses DOTween.
```

# Anexo: prompt corto para comparar solucion humana e IA

```text
Tengo dos propuestas de scripts para un minijuego Unity 3D.

Propuesta del alumno:
[pegar lista]

Propuesta de ChatGPT:
[pegar lista]

El juego necesita:
- mover un Player
- recoger esferas
- sumar score
- controlar tiempo
- detectar victoria
- detectar Game Over
- mover obstaculos
- reiniciar con R
- desactivar el Player al perder

Compara ambas propuestas segun:
1. simplicidad
2. separacion de responsabilidades
3. facilidad de asignar en Inspector
4. riesgos de errores
5. adecuacion para una practica docente

Dame una conclusion breve.
```
