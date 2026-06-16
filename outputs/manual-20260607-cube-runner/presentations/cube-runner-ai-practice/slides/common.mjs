const C = {
  bg: "#ffffff",
  ink: "#151923",
  muted: "#5c6573",
  light: "#eef1f5",
  panel: "#f6f7f9",
  red: "#e3062c",
  redSoft: "#ffecef",
  navy: "#101827",
  blue: "#1f5eff",
  yellow: "#f9c80e",
  green: "#26a269",
  gray: "#c9ced6",
};

const FOOT = "Master en Creacion de Videojuegos - IA aplicada / Unity 3D + C#";
const TITLE = "Cube Runner Arena";
const CONCEPT_IMAGE =
  "C:/TowerJumpingLevel2/outputs/manual-20260607-cube-runner/presentations/cube-runner-ai-practice/assets/cube-runner-arena-concept.png";

function frame(slide, ctx) {
  ctx.addShape(slide, { left: 0, top: 0, width: ctx.W, height: ctx.H, fill: C.bg, line: ctx.line(C.bg, 0) });
  ctx.addShape(slide, { left: 48, top: 40, width: 1120, height: 4, fill: C.red, line: ctx.line(C.red, 0) });
  ctx.addText(slide, {
    text: "IA PRACTICA",
    left: 1010,
    top: 54,
    width: 160,
    height: 18,
    fontSize: 12,
    color: C.red,
    bold: true,
    align: "right",
    typeface: "Aptos",
  });
  ctx.addText(slide, {
    text: FOOT,
    left: 56,
    top: 668,
    width: 760,
    height: 22,
    fontSize: 12,
    color: "#8b93a1",
  });
  ctx.addText(slide, {
    text: String(ctx.slideNumber).padStart(2, "0"),
    left: 1140,
    top: 668,
    width: 40,
    height: 22,
    fontSize: 12,
    color: "#8b93a1",
    align: "right",
  });
}

function title(slide, ctx, text, subtitle) {
  ctx.addText(slide, {
    text,
    left: 70,
    top: 88,
    width: 980,
    height: subtitle ? 58 : 80,
    fontSize: subtitle ? 36 : 46,
    bold: true,
    color: C.ink,
    typeface: "Aptos Display",
  });
  if (subtitle) {
    ctx.addText(slide, {
      text: subtitle,
      left: 72,
      top: 146,
      width: 940,
      height: 58,
      fontSize: 21,
      color: C.muted,
      typeface: "Aptos",
    });
  }
}

function redClaim(slide, ctx, text, top = 230) {
  ctx.addText(slide, {
    text,
    left: 88,
    top,
    width: 980,
    height: 70,
    fontSize: 34,
    bold: true,
    color: C.red,
    typeface: "Aptos Display",
  });
}

function bullets(slide, ctx, items, x = 96, y = 210, opts = {}) {
  const gap = opts.gap ?? 58;
  const fs = opts.fontSize ?? 22;
  items.forEach((item, i) => {
    const yy = y + i * gap;
    ctx.addShape(slide, {
      left: x,
      top: yy + 10,
      width: 7,
      height: 7,
      fill: C.red,
      line: ctx.line(C.red, 0),
      geometry: "ellipse",
    });
    ctx.addText(slide, {
      text: item,
      left: x + 28,
      top: yy,
      width: opts.width ?? 920,
      height: gap - 4,
      fontSize: fs,
      color: opts.color ?? C.ink,
      typeface: "Aptos",
    });
  });
}

function card(slide, ctx, x, y, w, h, head, body, accent = C.red, fill = C.panel) {
  const box = ctx.addShape(slide, {
    left: x,
    top: y,
    width: w,
    height: h,
    fill,
    line: ctx.line(accent, 1),
  });
  box.borderRadius = 10;
  ctx.addText(slide, {
    text: head,
    left: x + 18,
    top: y + 16,
    width: w - 36,
    height: 32,
    fontSize: 18,
    bold: true,
    color: C.ink,
  });
  ctx.addText(slide, {
    text: body,
    left: x + 18,
    top: y + 52,
    width: w - 36,
    height: h - 66,
    fontSize: 15,
    color: C.muted,
  });
}

function codeBox(slide, ctx, text, x, y, w, h, fs = 17) {
  const box = ctx.addShape(slide, {
    left: x,
    top: y,
    width: w,
    height: h,
    fill: C.navy,
    line: ctx.line(C.navy, 0),
  });
  box.borderRadius = 8;
  ctx.addText(slide, {
    text,
    left: x + 22,
    top: y + 18,
    width: w - 44,
    height: h - 34,
    fontSize: fs,
    color: "#e7edf7",
    typeface: "Consolas",
    insets: { left: 0, right: 0, top: 0, bottom: 0 },
  });
}

function pill(slide, ctx, text, x, y, w, color = C.red) {
  const p = ctx.addShape(slide, {
    left: x,
    top: y,
    width: w,
    height: 28,
    fill: color === C.red ? C.redSoft : "#edf4ff",
    line: ctx.line(color, 1),
  });
  p.borderRadius = 12;
  ctx.addText(slide, {
    text,
    left: x + 10,
    top: y + 5,
    width: w - 20,
    height: 18,
    fontSize: 12,
    bold: true,
    color,
    align: "center",
  });
}

function sceneDiagram(slide, ctx, x, y) {
  ctx.addShape(slide, { left: x, top: y, width: 510, height: 310, fill: "#d8dbe1", line: ctx.line("#9ca3af", 2) });
  ctx.addShape(slide, { left: x + 18, top: y + 18, width: 474, height: 274, fill: "#eef0f3", line: ctx.line("#b6bbc4", 1) });
  const walls = [
    [x, y, 510, 18],
    [x, y + 292, 510, 18],
    [x, y, 18, 310],
    [x + 492, y, 18, 310],
  ];
  walls.forEach(([a, b, c, d]) => ctx.addShape(slide, { left: a, top: b, width: c, height: d, fill: "#9aa0aa", line: ctx.line("#7b8290", 0) }));
  ctx.addShape(slide, { left: x + 236, top: y + 154, width: 34, height: 54, fill: "#1f5eff", line: ctx.line("#1743b5", 1), geometry: "roundRect" });
  [[90, 78], [160, 210], [250, 82], [370, 230], [420, 94], [90, 240], [310, 170], [195, 125]].forEach(([a, b]) => {
    ctx.addShape(slide, { left: x + a, top: y + b, width: 18, height: 18, fill: C.yellow, line: ctx.line("#c78b00", 1), geometry: "ellipse" });
  });
  [[132, 128], [345, 135], [250, 245]].forEach(([a, b]) => {
    ctx.addShape(slide, { left: x + a, top: y + b, width: 42, height: 42, fill: C.red, line: ctx.line("#a4001e", 1) });
    ctx.addText(slide, { text: "< >", left: x + a - 2, top: y + b + 45, width: 50, height: 16, fontSize: 12, color: C.red, align: "center" });
  });
  ctx.addShape(slide, { left: x + 430, top: y + 38, width: 42, height: 42, fill: C.green, line: ctx.line("#157047", 1) });
}

function scriptBadge(slide, ctx, name, x, y, desc, accent = C.red) {
  ctx.addShape(slide, { left: x, top: y, width: 260, height: 86, fill: C.panel, line: ctx.line("#d9dde5", 1) });
  ctx.addText(slide, { text: name, left: x + 16, top: y + 15, width: 230, height: 24, fontSize: 17, bold: true, color: accent, typeface: "Consolas" });
  ctx.addText(slide, { text: desc, left: x + 16, top: y + 44, width: 230, height: 34, fontSize: 13, color: C.muted });
}

const slides = [
  async (slide, ctx) => {
    frame(slide, ctx);
    ctx.addShape(slide, { left: 48, top: 40, width: 8, height: 600, fill: C.red, line: ctx.line(C.red, 0) });
    ctx.addText(slide, { text: TITLE, left: 92, top: 170, width: 850, height: 58, fontSize: 48, bold: true, color: C.ink, typeface: "Aptos Display" });
    ctx.addText(slide, { text: "Clase practica de IA aplicada a Unity 3D y C#", left: 94, top: 238, width: 760, height: 36, fontSize: 22, color: C.muted });
    ctx.addShape(slide, { left: 94, top: 292, width: 585, height: 38, fill: C.panel, line: ctx.line(C.panel, 0) });
    ctx.addText(slide, { text: "5 horas / ChatGPT gratuito / primitivas basicas de Unity", left: 112, top: 301, width: 550, height: 20, fontSize: 15, color: C.muted });
    sceneDiagram(slide, ctx, 730, 178);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Que construiremos hoy");
    bullets(slide, ctx, [
      "Un minijuego 3D completo con capsula, cubos, esferas y plano.",
      "Movimiento, coleccionables, score, tiempo, obstaculos, victoria y derrota.",
      "Un flujo de trabajo con ChatGPT gratuito: pedir, integrar, probar y corregir.",
      "Una base de prototipo reutilizable para mecanicas arcade sencillas.",
    ]);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "La regla de la practica");
    redClaim(slide, ctx, "ChatGPT ayuda, pero Unity verifica.");
    ctx.addText(slide, {
      text: "Cada respuesta generada se prueba en Play Mode. El alumno debe revisar Inspector, componentes, tags, colliders, Rigidbody y version de Input.",
      left: 90,
      top: 322,
      width: 960,
      height: 82,
      fontSize: 24,
      color: C.ink,
    });
    pill(slide, ctx, "no copiar a ciegas", 90, 442, 170);
    pill(slide, ctx, "cambios pequenos", 280, 442, 155);
    pill(slide, ctx, "verificar en escena", 455, 442, 170);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Ruta de 5 horas");
    const blocks = [
      ["Apertura", "20 min"], ["Escena base", "30 min"], ["Player", "40 min"],
      ["Coleccionables", "40 min"], ["Tiempo/estados", "40 min"], ["Obstaculos", "45 min"],
      ["Camara y pulido", "35 min"], ["Debugging IA", "30 min"], ["Entrega", "20 min"],
    ];
    blocks.forEach(([h, b], i) => {
      const col = i % 3;
      const row = Math.floor(i / 3);
      card(slide, ctx, 76 + col * 350, 190 + row * 118, 300, 78, h, b, i % 2 === 0 ? C.red : "#d0d6df", i % 2 === 0 ? C.redSoft : C.panel);
    });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Resultado jugable");
    await ctx.addImage(slide, { path: CONCEPT_IMAGE, left: 86, top: 184, width: 600, height: 338, fit: "contain" });
    bullets(slide, ctx, [
      "Capsule azul: jugador.",
      "Spheres amarillas: coleccionables.",
      "Cubos rojos: obstaculos moviles.",
      "Cubos grises: paredes.",
      "Cubo verde: referencia de goal.",
    ], 735, 205, { width: 390, gap: 52, fontSize: 20 });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Prompt maestro");
    codeBox(slide, ctx, `Actua como desarrollador Unity C# senior.
Estoy creando Cube Runner Arena con primitivas basicas:
capsula, cubos, esferas y plano.

Restricciones:
- Unity 3D, C#
- ChatGPT gratuito, sin suscripcion
- no assets externos, DOTween ni Cinemachine
- no nuevo Input System salvo que lo pida
- cambios pequenos, scripts claros
- dime que revisar en Inspector

Antes de codigo: lista GameObjects, componentes y riesgos.`, 95, 185, 980, 355, 18);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Escena base en Unity");
    scriptBadge(slide, ctx, "Ground", 80, 190, "Plane escalado. Collider por defecto.", C.muted);
    scriptBadge(slide, ctx, "Player", 370, 190, "Capsule + Rigidbody + tag Player.", C.blue);
    scriptBadge(slide, ctx, "Collectibles", 660, 190, "Spheres con Collider en Is Trigger.", C.yellow);
    scriptBadge(slide, ctx, "Obstacles", 950, 190, "Cubos rojos con Collider normal.", C.red);
    scriptBadge(slide, ctx, "Canvas", 225, 330, "ScoreText, TimeText, MessageText.", C.green);
    scriptBadge(slide, ctx, "GameManager", 515, 330, "Estado, puntuacion, tiempo y reinicio.", C.red);
    scriptBadge(slide, ctx, "Main Camera", 805, 330, "Vista inclinada o seguimiento simple.", C.muted);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Terreno de juego");
    sceneDiagram(slide, ctx, 95, 196);
    bullets(slide, ctx, [
      "La escena debe ser legible antes de programar.",
      "El layout crea decisiones de dificultad: riesgo, distancia y tiempo.",
      "Las primitivas bastan para prototipar mecanicas reales.",
    ], 675, 230, { width: 460, gap: 64, fontSize: 21 });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Bloque 1: configurar escena");
    codeBox(slide, ctx, `Prompt:
Estoy preparando una escena basica de Unity 3D para Cube Runner Arena.
Uso solo plano, capsula, esfera y cubo.

Dame una checklist corta para:
- player con Rigidbody
- suelo con Collider
- paredes
- camara
- materiales
- jerarquia

No generes codigo todavia.`, 90, 186, 620, 358, 18);
    bullets(slide, ctx, [
      "Aprendizaje: no todo prompt debe pedir codigo.",
      "Primero ordenamos escena y dependencias.",
      "Despues programamos con menos incertidumbre.",
    ], 770, 220, { width: 370, gap: 66, fontSize: 21 });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Movimiento del player");
    card(slide, ctx, 90, 190, 320, 120, "Input", "WASD o flechas sobre XZ.\nSe lee en Update.", C.red);
    card(slide, ctx, 470, 190, 320, 120, "Fisica", "Rigidbody.MovePosition.\nSe aplica en FixedUpdate.", C.red);
    card(slide, ctx, 850, 190, 320, 120, "Inspector", "Move Speed editable.\nCongelar rotacion X/Z.", C.red);
    codeBox(slide, ctx, `Prompt:
Necesito PlayerMovement para Unity 3D.
Player = Capsule con Rigidbody.
Movimiento en plano XZ.
Usa Rigidbody.MovePosition en FixedUpdate.
No uses el nuevo Input System.
Velocidad con SerializeField.
Codigo completo + Inspector.`, 105, 365, 980, 170, 17);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "PlayerMovement.cs");
    codeBox(slide, ctx, `using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody rb;
    private Vector3 moveDirection;

    private void Awake() => rb = GetComponent<Rigidbody>();

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(h, 0f, v).normalized;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }
}`, 70, 155, 1080, 475, 13);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Primer error real: Input");
    codeBox(slide, ctx, `InvalidOperationException:
You are trying to read Input using the UnityEngine.Input class,
but active Input handling is Input System package.`, 90, 185, 960, 118, 19);
    redClaim(slide, ctx, "Solucion para la practica: Active Input Handling = Both", 345);
    bullets(slide, ctx, [
      "Edit > Project Settings > Player > Other Settings.",
      "Cambiar a Both para permitir Input.GetAxisRaw.",
      "Reiniciar Unity si lo pide.",
    ], 115, 440, { width: 900, gap: 46, fontSize: 20 });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Coleccionables y score");
    bullets(slide, ctx, [
      "Cada Sphere amarilla tiene Collider con Is Trigger.",
      "El player necesita tag Player.",
      "Collectible avisa a GameManager y se desactiva.",
      "GameManager cuenta puntos y detecta victoria.",
    ], 105, 205, { width: 860, gap: 60 });
    card(slide, ctx, 760, 205, 330, 190, "Riesgo Unity", "Si no hay Rigidbody o el tag esta mal, OnTriggerEnter no se ejecuta como esperamos.", C.red, C.redSoft);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Collectible.cs");
    codeBox(slide, ctx, `using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int points = 1;
    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.AddScore(points);

        gameObject.SetActive(false);
    }
}`, 92, 170, 980, 390, 16);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "GameManager v1");
    codeBox(slide, ctx, `Responsabilidades:
- Guardar score.
- Contar coleccionables totales al empezar.
- Actualizar ScoreText.
- Mostrar YOU WIN al recoger todo.

Restricciones:
- no eventos todavia
- no ScriptableObjects
- null-checks simples para UI
- TextMeshProUGUI en Inspector`, 90, 180, 470, 310, 20);
    codeBox(slide, ctx, `public void AddScore(int points)
{
    if (gameEnded) return;

    score += points;
    collectedCount++;

    UpdateScoreUI();

    if (collectedCount >= totalCollectibles)
        WinGame();
}`, 620, 210, 440, 225, 18);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Tiempo, estados y reinicio");
    card(slide, ctx, 90, 190, 300, 135, "Tiempo", "startTime = 60\nResta Time.deltaTime.", C.red);
    card(slide, ctx, 445, 190, 300, 135, "Estados", "gameEnded bloquea score\ny countdown.", C.red);
    card(slide, ctx, 800, 190, 300, 135, "Reinicio", "Tecla R + SceneManager.LoadScene.", C.red);
    codeBox(slide, ctx, `Prompt:
Tengo este GameManager actual: [pegar codigo]
Quiero temporizador de 60s, derrota al llegar a 0,
victoria si recoge todo antes, y reinicio con R.
Modifica solo GameManager. No uses Time.timeScale.`, 108, 385, 950, 145, 18);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Flujo de estado");
    const nodes = [
      ["Inicio", 90, 265, C.panel],
      ["Jugando", 335, 265, C.redSoft],
      ["Victoria", 590, 205, "#edf7f0"],
      ["Derrota", 590, 325, C.redSoft],
      ["R reinicia", 850, 265, C.panel],
    ];
    nodes.forEach(([t, x, y, fill]) => {
      ctx.addShape(slide, { left: x, top: y, width: 170, height: 64, fill, line: ctx.line(C.red, fill === C.redSoft ? 1 : 0) });
      ctx.addText(slide, { text: t, left: x + 10, top: y + 20, width: 150, height: 24, fontSize: 20, bold: true, align: "center", color: C.ink });
    });
    [["->", 270, 282], ["->", 520, 222], ["->", 520, 342], ["->", 765, 282]].forEach(([t, x, y]) => {
      ctx.addText(slide, { text: t, left: x, top: y, width: 50, height: 28, fontSize: 24, color: C.red, bold: true, align: "center" });
    });
    bullets(slide, ctx, [
      "El estado evita sumar puntos despues del final.",
      "El temporizador se detiene logicamente al ganar o perder.",
      "No usamos Time.timeScale para mantenerlo simple.",
    ], 155, 460, { width: 850, gap: 42, fontSize: 19 });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Obstaculos con cubos rojos");
    sceneDiagram(slide, ctx, 80, 225);
    bullets(slide, ctx, [
      "Cubos rojos con Collider normal.",
      "Se mueven entre dos puntos con MoveTowards.",
      "Al colisionar con Player llaman a LoseGame().",
      "La dificultad se ajusta con velocidad, distancia y posicion.",
    ], 665, 210, { width: 465, gap: 58, fontSize: 20 });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "MovingObstacle.cs");
    codeBox(slide, ctx, `public class MovingObstacle : MonoBehaviour
{
    [SerializeField] private float moveDistance = 4f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Vector3 moveDirection = Vector3.right;

    private Vector3 targetA, targetB, currentTarget;
    private GameManager gameManager;

    private void Start()
    {
        Vector3 dir = moveDirection.normalized;
        targetA = transform.position - dir * moveDistance;
        targetB = transform.position + dir * moveDistance;
        currentTarget = targetB;
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, currentTarget) <= 0.05f)
            currentTarget = currentTarget == targetA ? targetB : targetA;
    }
}`, 65, 160, 1110, 430, 13);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Camara y pulido");
    bullets(slide, ctx, [
      "La opcion minima: camara fija inclinada.",
      "La opcion practica: CameraFollow con offset y LateUpdate.",
      "No usamos Cinemachine para reducir dependencias.",
      "Pulido: paredes, colores, posiciones, velocidades y tiempo.",
    ], 100, 200, { width: 880, gap: 58 });
    codeBox(slide, ctx, `Prompt:
Necesito CameraFollow simple.
Target = Capsule Player.
Mantener offset configurable.
Usar LateUpdate.
No usar Cinemachine.
Codigo completo + Inspector.`, 675, 375, 430, 155, 17);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Debugging con ChatGPT");
    codeBox(slide, ctx, `Tengo este error en Unity:
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
3. cambio minimo
4. que no deberia tocar`, 100, 170, 1000, 405, 16);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Bugs tipicos de Unity");
    const bugs = [
      ["NullReferenceException", "Falta asignar texto UI en Inspector."],
      ["OnTriggerEnter no salta", "Is Trigger, tag Player o Rigidbody incorrectos."],
      ["OnCollisionEnter no salta", "Collider en trigger o falta Rigidbody."],
      ["La camara tiembla", "Seguir en Update en vez de LateUpdate."],
      ["LoadScene falla", "Falta using, escena no guardada o Build Settings."],
      ["Input no funciona", "Active Input Handling no esta en Both."],
    ];
    bugs.forEach(([h, b], i) => {
      const x = 90 + (i % 2) * 515;
      const y = 180 + Math.floor(i / 2) * 118;
      card(slide, ctx, x, y, 455, 78, h, b, i % 2 === 0 ? C.red : "#d0d6df");
    });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Alucinaciones frecuentes en Unity");
    bullets(slide, ctx, [
      "Mezclar fisica 2D y 3D: Rigidbody2D, Collider2D, OnTriggerEnter2D.",
      "Inventar metodos o APIs que no existen en la version del alumno.",
      "Usar nuevo Input System aunque la practica pida Input clasico.",
      "Crear managers innecesarios para una mecanica simple.",
      "Olvidar referencias de Inspector o cambiar nombres publicos usados por UI.",
    ], 95, 190, { width: 970, gap: 58, fontSize: 21 });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Actividad final por grupos");
    bullets(slide, ctx, [
      "Cada grupo termina una escena jugable.",
      "Debe explicar un prompt que le ayudo y un prompt que fallo.",
      "Debe mostrar una prueba en Play Mode.",
      "Debe identificar un riesgo de Inspector o configuracion.",
    ], 105, 205, { width: 860, gap: 62 });
    card(slide, ctx, 750, 410, 330, 110, "Pregunta clave", "Que parte fue codigo y que parte era configuracion de Unity?", C.red, C.redSoft);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Checklist de entrega");
    bullets(slide, ctx, [
      "Player se mueve y no gira raro.",
      "Score aumenta al recoger Spheres.",
      "El tiempo baja y puede provocar derrota.",
      "Obstaculos moviles causan Game Over.",
      "Victoria al recoger todo.",
      "Tecla R reinicia la escena.",
    ], 105, 178, { width: 500, gap: 50, fontSize: 20 });
    bullets(slide, ctx, [
      "Textos UI asignados.",
      "Tag Player correcto.",
      "Colliders y Rigidbody revisados.",
      "Escena guardada.",
      "Build Settings revisado si hace falta.",
      "No hay errores en consola.",
    ], 660, 178, { width: 500, gap: 50, fontSize: 20 });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Scripts finales");
    scriptBadge(slide, ctx, "PlayerMovement.cs", 110, 205, "Input + Rigidbody.MovePosition.", C.blue);
    scriptBadge(slide, ctx, "Collectible.cs", 410, 205, "Trigger + score + desactivar.", C.yellow);
    scriptBadge(slide, ctx, "GameManager.cs", 710, 205, "Score, tiempo, estado, reinicio.", C.red);
    scriptBadge(slide, ctx, "MovingObstacle.cs", 260, 335, "Patrulla + derrota por colision.", C.red);
    scriptBadge(slide, ctx, "CameraFollow.cs", 560, 335, "Seguimiento simple con offset.", C.muted);
    redClaim(slide, ctx, "Cinco scripts. Ningun sistema nuevo innecesario.", 485);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Cierre");
    redClaim(slide, ctx, "La IA acelera el prototipo, no sustituye la prueba.");
    ctx.addText(slide, {
      text: "El flujo profesional es: describir bien el problema, pedir ayuda concreta, aplicar cambios pequenos, probar en Unity, volver con contexto y verificar antes de confiar.",
      left: 92,
      top: 332,
      width: 970,
      height: 96,
      fontSize: 25,
      color: C.ink,
    });
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Anexo: PlayerMovement completo");
    codeBox(slide, ctx, `using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    private Rigidbody rb;
    private Vector3 moveDirection;

    private void Awake() => rb = GetComponent<Rigidbody>();

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        Vector3 nextPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }
}`, 70, 150, 1090, 470, 13);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Anexo: Collectible completo");
    codeBox(slide, ctx, `using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int points = 1;
    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
            gameManager.AddScore(points);

        gameObject.SetActive(false);
    }
}`, 110, 165, 980, 390, 15);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Anexo: GameManager 1/3");
    codeBox(slide, ctx, `using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float startTime = 60f;

    private int score;
    private int totalCollectibles;
    private int collectedCount;
    private float timeRemaining;
    private bool gameEnded;

    private void Start()
    {
        totalCollectibles = FindObjectsOfType<Collectible>().Length;
        timeRemaining = startTime;
        UpdateScoreUI();
        UpdateTimeUI();
        if (messageText != null) messageText.text = "";
    }`, 65, 155, 1100, 430, 13);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Anexo: GameManager 2/3");
    codeBox(slide, ctx, `    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { RestartGame(); return; }
        if (gameEnded) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimeUI();
            LoseGame();
            return;
        }
        UpdateTimeUI();
    }

    public void AddScore(int points)
    {
        if (gameEnded) return;
        score += points;
        collectedCount++;
        UpdateScoreUI();
        if (collectedCount >= totalCollectibles) WinGame();
    }`, 65, 155, 1100, 430, 13);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Anexo: GameManager 3/3");
    codeBox(slide, ctx, `    public void LoseGame()
    {
        if (gameEnded) return;
        gameEnded = true;
        if (messageText != null) messageText.text = "GAME OVER - Press R";
    }

    private void WinGame()
    {
        gameEnded = true;
        if (messageText != null) messageText.text = "YOU WIN! - Press R";
    }

    private void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void UpdateScoreUI() { if (scoreText != null) scoreText.text = "Score: " + score; }
    private void UpdateTimeUI() { if (timeText != null) timeText.text = "Time: " + Mathf.CeilToInt(timeRemaining); }
}`, 75, 158, 1080, 420, 13);
  },
  async (slide, ctx) => {
    frame(slide, ctx);
    title(slide, ctx, "Anexo: MovingObstacle + CameraFollow");
    codeBox(slide, ctx, `// MovingObstacle: derrota al tocar Player
private void OnCollisionEnter(Collision collision)
{
    if (!collision.collider.CompareTag("Player")) return;
    if (gameManager != null) gameManager.LoseGame();
}

// CameraFollow: seguimiento simple
private void LateUpdate()
{
    if (target == null) return;

    Vector3 desired = target.position + offset;
    transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    transform.LookAt(target);
}`, 130, 180, 930, 340, 15);
    ctx.addText(slide, { text: "En aula conviene mostrar los scripts completos en Unity y usar estas diapositivas como referencia proyectable.", left: 135, top: 545, width: 900, height: 40, fontSize: 18, color: C.muted, align: "center" });
  },
];

export async function makeSlide(presentation, ctx, index) {
  const slide = presentation.slides.add();
  await slides[index - 1](slide, ctx);
  return slide;
}

export const slideCount = slides.length;
