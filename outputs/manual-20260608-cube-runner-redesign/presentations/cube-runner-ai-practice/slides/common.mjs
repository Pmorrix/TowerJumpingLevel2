const C = {
  red: "#e61b2a",
  redSoft: "#fff0f2",
  ink: "#222733",
  muted: "#596272",
  greyText: "#7d8796",
  card: "#f3f5f7",
  card2: "#f7f8fa",
  line: "#dde2e8",
  navy: "#111a28",
  blue: "#2f65ff",
  yellow: "#ffca18",
  green: "#22a366",
  unityDark: "#252932",
  unityMid: "#323843",
  unityPanel: "#3e4654",
};

const ROOT = "C:/TowerJumpingLevel2/outputs/manual-20260608-cube-runner-redesign/presentations/cube-runner-ai-practice";
const HEADER_LOGO = `${ROOT}/assets/drive/logo-title-07-cropped.png`;
const COVER_ICON = `${ROOT}/assets/drive/logo-title-01.png`;

const FOOTER = "Máster en Creación de Videojuegos - Universidad de Málaga";

function line(ctx, color = "transparent", width = 0) {
  return ctx.line(color, width);
}

async function frame(slide, ctx) {
  ctx.addShape(slide, { left: 0, top: 0, width: ctx.W, height: ctx.H, fill: "#ffffff", line: line(ctx, "#ffffff", 0) });
  ctx.addShape(slide, { left: 35, top: 27, width: 1210, height: 4, fill: C.red, line: line(ctx, C.red, 0) });
  await ctx.addImage(slide, { path: HEADER_LOGO, left: 996, top: 48, width: 200, height: 42, fit: "contain", alt: "Máster en Creación de Videojuegos" });
  ctx.addText(slide, { text: FOOTER, left: 75, top: 652, width: 520, height: 18, fontSize: 11, color: "#8b93a1" });
  ctx.addText(slide, { text: String(ctx.slideNumber).padStart(2, "0"), left: 1150, top: 652, width: 34, height: 18, fontSize: 11, color: "#8b93a1", align: "right" });
}

function title(slide, ctx, text, sub = "") {
  ctx.addText(slide, {
    text,
    left: 75,
    top: 100,
    width: 920,
    height: sub ? 52 : 64,
    fontSize: 45,
    bold: true,
    color: C.ink,
    typeface: "Aptos Display",
  });
  if (sub) {
    ctx.addText(slide, { text: sub, left: 75, top: 165, width: 820, height: 44, fontSize: 22, color: C.muted });
  }
}

function bulletList(slide, ctx, items, x = 87, y = 229, opts = {}) {
  const gap = opts.gap ?? 85;
  const fs = opts.fontSize ?? 32;
  items.forEach((text, i) => {
    const top = y + i * gap;
    ctx.addShape(slide, { left: x, top: top + 15, width: 9, height: 9, geometry: "ellipse", fill: C.red, line: line(ctx, C.red, 0) });
    ctx.addText(slide, { text, left: x + 42, top, width: opts.width ?? 900, height: gap - 8, fontSize: fs, color: opts.color ?? C.ink });
  });
}

function card(slide, ctx, x, y, w, h, head, body = "", mode = "plain") {
  const fill = mode === "red" ? C.redSoft : C.card;
  const stroke = mode === "red" ? C.red : C.line;
  const box = ctx.addShape(slide, { left: x, top: y, width: w, height: h, fill, line: line(ctx, stroke, 1) });
  box.borderRadius = 8;
  ctx.addText(slide, { text: head, left: x + 16, top: y + 14, width: w - 32, height: 24, fontSize: 13.2, bold: true, color: C.ink });
  if (body) ctx.addText(slide, { text: body, left: x + 16, top: y + 42, width: w - 32, height: h - 50, fontSize: 10.8, color: C.muted });
  return box;
}

function redClaim(slide, ctx, text, x = 82, y = 225, w = 960, fs = 24) {
  ctx.addText(slide, { text, left: x, top: y, width: w, height: 58, fontSize: fs, bold: true, color: C.red, typeface: "Aptos Display" });
}

function codeBox(slide, ctx, text, x = 82, y = 205, w = 1000, h = 300, fs = 13.4) {
  const box = ctx.addShape(slide, { left: x, top: y, width: w, height: h, fill: C.navy, line: line(ctx, C.navy, 0) });
  box.borderRadius = 7;
  ctx.addText(slide, {
    text,
    left: x + 22,
    top: y + 18,
    width: w - 44,
    height: h - 30,
    fontSize: fs,
    color: "#eef4ff",
    typeface: "Consolas",
  });
}

function routeGrid(slide, ctx, items) {
  items.forEach(([head, min, emph], i) => {
    const col = i % 3;
    const row = Math.floor(i / 3);
    card(slide, ctx, 82 + col * 315, 188 + row * 96, 252, 58, head, min, emph ? "red" : "plain");
  });
}

function primitiveArena(slide, ctx, x, y, scale = 1) {
  const W = 475 * scale, H = 260 * scale;
  ctx.addShape(slide, { left: x, top: y, width: W, height: H, fill: "#eef1f5", line: line(ctx, "#9ba3ae", 8 * scale) });
  ctx.addShape(slide, { left: x + 22 * scale, top: y + 22 * scale, width: W - 44 * scale, height: H - 44 * scale, fill: "#e8ebf0", line: line(ctx, "#ccd2dc", 1) });
  const s = scale;
  [[75, 63], [160, 205], [230, 80], [362, 197], [390, 70], [93, 218], [290, 140], [205, 155]].forEach(([a, b]) => {
    ctx.addShape(slide, { left: x + a * s, top: y + b * s, width: 16 * s, height: 16 * s, geometry: "ellipse", fill: C.yellow, line: line(ctx, "#c89300", 1) });
  });
  [[130, 116], [330, 116], [255, 205]].forEach(([a, b]) => {
    ctx.addShape(slide, { left: x + a * s, top: y + b * s, width: 36 * s, height: 36 * s, fill: C.red, line: line(ctx, "#a80f1f", 0) });
    ctx.addText(slide, { text: "< >", left: x + (a - 5) * s, top: y + (b + 40) * s, width: 48 * s, height: 16 * s, fontSize: 9 * s, color: C.red, align: "center" });
  });
  ctx.addShape(slide, { left: x + 228 * s, top: y + 142 * s, width: 28 * s, height: 44 * s, geometry: "roundRect", fill: C.blue, line: line(ctx, "#234bb5", 0) });
  ctx.addShape(slide, { left: x + 400 * s, top: y + 45 * s, width: 36 * s, height: 36 * s, fill: C.green, line: line(ctx, "#137744", 0) });
}

function unityMock(slide, ctx, x, y, w = 620, h = 320) {
  ctx.addShape(slide, { left: x, top: y, width: w, height: h, fill: C.unityDark, line: line(ctx, "#151820", 1) });
  ctx.addShape(slide, { left: x, top: y, width: w, height: 30, fill: "#1f232b", line: line(ctx, "#1f232b", 0) });
  ctx.addText(slide, { text: "Unity - CubeRunnerArena.unity", left: x + 16, top: y + 8, width: 260, height: 12, fontSize: 8.8, color: "#cbd2df" });
  ctx.addShape(slide, { left: x + 12, top: y + 42, width: 125, height: h - 54, fill: C.unityMid, line: line(ctx, "#444c5a", 1) });
  ctx.addShape(slide, { left: x + w - 150, top: y + 42, width: 138, height: h - 54, fill: C.unityMid, line: line(ctx, "#444c5a", 1) });
  ctx.addShape(slide, { left: x + 150, top: y + 42, width: w - 312, height: h - 54, fill: "#2c313a", line: line(ctx, "#444c5a", 1) });
  ctx.addText(slide, { text: "Hierarchy", left: x + 22, top: y + 52, width: 90, height: 12, fontSize: 8.5, color: "#dbe2ee", bold: true });
  ctx.addText(slide, { text: "Inspector", left: x + w - 138, top: y + 52, width: 90, height: 12, fontSize: 8.5, color: "#dbe2ee", bold: true });
  ["Ground", "Player", "Collectibles", "Obstacles", "Canvas", "GameManager"].forEach((t, i) => {
    ctx.addText(slide, { text: t, left: x + 24, top: y + 76 + i * 22, width: 94, height: 12, fontSize: 8.5, color: i === 1 ? "#ffffff" : "#c6ceda" });
  });
  primitiveArena(slide, ctx, x + 182, y + 80, 0.58);
  ["Transform", "Rigidbody", "Capsule Collider", "PlayerMovement", "Tag: Player"].forEach((t, i) => {
    card(slide, ctx, x + w - 137, y + 78 + i * 42, 110, 26, t, "", i === 4 ? "red" : "plain");
  });
}

function inspectorCards(slide, ctx, x, y, cards) {
  cards.forEach(([h, b, mode], i) => card(slide, ctx, x, y + i * 82, 250, 58, h, b, mode));
}

const slides = [
  async (slide, ctx) => {
    ctx.addShape(slide, { left: 0, top: 0, width: ctx.W, height: ctx.H, fill: "#ffffff", line: line(ctx, "#ffffff", 0) });
    ctx.addShape(slide, { left: 0, top: 0, width: 27, height: 720, fill: C.red, line: line(ctx, C.red, 0) });
    await ctx.addImage(slide, { path: HEADER_LOGO, left: 916, top: 55, width: 266, height: 53, fit: "contain" });
    ctx.addText(slide, { text: "Cube Runner Arena", left: 86, top: 191, width: 760, height: 78, fontSize: 64, bold: true, color: C.ink, typeface: "Aptos Display" });
    ctx.addText(slide, { text: "Clase práctica de IA aplicada a videojuegos con\nUnity 3D y C#", left: 86, top: 284, width: 760, height: 86, fontSize: 35, color: C.ink });
    ctx.addShape(slide, { left: 85, top: 390, width: 635, height: 45, fill: "#eeeeee", line: line(ctx, "#eeeeee", 0) });
    ctx.addText(slide, { text: "Clase 2: prototipado guiado con ChatGPT gratuito", left: 86, top: 400, width: 620, height: 28, fontSize: 24, color: C.muted });
    ctx.addText(slide, { text: FOOTER, left: 86, top: 652, width: 520, height: 18, fontSize: 11, color: "#111827" });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Qué haremos hoy");
    bulletList(slide, ctx, [
      "Construir un minijuego completo con primitivas básicas de Unity.",
      "Usar ChatGPT gratuito como asistente técnico, no como autoridad.",
      "Practicar prompts para código, configuración, revisión y depuración.",
      "Cerrar el ciclo: escena, scripts, UI, prueba y entrega.",
    ]);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "La regla de la clase");
    redClaim(slide, ctx, "ChatGPT no decide por nosotros.");
    ctx.addText(slide, { text: "Acelera el trabajo cuando el problema está bien definido y la respuesta se comprueba dentro de Unity.", left: 84, top: 292, width: 850, height: 44, fontSize: 18.5, color: C.ink });
    card(slide, ctx, 84, 382, 250, 70, "Pedir", "Contexto, objetivo y restricciones.", "plain");
    card(slide, ctx, 366, 382, 250, 70, "Probar", "Play Mode, consola e Inspector.", "red");
    card(slide, ctx, 648, 382, 250, 70, "Corregir", "Volver con datos, no con intuiciones.", "plain");
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Ruta de 5 horas");
    routeGrid(slide, ctx, [
      ["Apertura", "20 min", false], ["Escena base", "30 min", false], ["Player", "40 min", true],
      ["Coleccionables", "40 min", false], ["Estados y UI", "45 min", true], ["Obstáculos", "45 min", false],
      ["Cámara y pulido", "35 min", true], ["Debugging IA", "30 min", false], ["Entrega", "15 min", true],
    ]);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Minijuego objetivo");
    primitiveArena(slide, ctx, 98, 210, 1.04);
    bulletList(slide, ctx, [
      "Cápsula azul: jugador.",
      "Esferas amarillas: coleccionables.",
      "Cubos rojos: obstáculos móviles.",
      "Cubos grises: paredes.",
      "Cubo verde: objetivo visual.",
    ], 690, 218, { width: 390, gap: 42, fontSize: 15.5 });
    redClaim(slide, ctx, "Ganar: recoger todo. Perder: tocar obstáculo o agotar tiempo.", 690, 462, 410, 16);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Prompt inicial de la práctica");
    codeBox(slide, ctx, `Actúa como desarrollador Unity C# senior.
Estoy creando Cube Runner Arena con primitivas básicas:
cápsula, cubos, esferas y plano.

Restricciones:
- Unity 3D y C#
- ChatGPT gratuito, sin suscripción
- no assets externos, DOTween ni Cinemachine
- no nuevo Input System salvo que lo pida
- cambios mínimos, scripts claros
- indica qué revisar en Inspector

Antes de código: lista GameObjects, componentes y riesgos.`, 118, 190, 940, 360, 14.2);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Escena base");
    unityMock(slide, ctx, 82, 190, 630, 330);
    inspectorCards(slide, ctx, 760, 205, [
      ["Player", "Cápsula + Rigidbody + tag Player.", "red"],
      ["Collectibles", "Esferas con Collider en Is Trigger.", "plain"],
      ["Obstacles", "Cubos rojos con Collider normal.", "plain"],
      ["Canvas", "ScoreText, TimeText y MessageText.", "plain"],
    ]);
    ctx.addText(slide, { text: "Antes del primer script, la escena ya debe contar la mecánica.", left: 86, top: 548, width: 820, height: 18, fontSize: 12.5, bold: true, color: C.red });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "El primer prompt no pide código");
    codeBox(slide, ctx, `Estoy preparando una escena básica de Unity 3D para Cube Runner Arena.
Uso solo plano, cápsula, esfera y cubo.

Dame una checklist corta para:
- player con Rigidbody
- suelo con Collider
- paredes
- cámara
- materiales
- jerarquía

No generes código todavía.`, 92, 190, 650, 335, 14);
    inspectorCards(slide, ctx, 800, 220, [
      ["Objetivo", "Reducir incertidumbre antes de programar.", "red"],
      ["Resultado", "Lista de objetos y componentes.", "plain"],
      ["Verificación", "Todo se comprueba en la escena.", "plain"],
    ]);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Movimiento del jugador");
    card(slide, ctx, 86, 198, 270, 88, "Input", "WASD o flechas.\nSe lee en Update.", "plain");
    card(slide, ctx, 408, 198, 270, 88, "Física", "Rigidbody.MovePosition.\nSe aplica en FixedUpdate.", "red");
    card(slide, ctx, 730, 198, 270, 88, "Inspector", "Move Speed editable.\nCongelar rotación X/Z.", "plain");
    codeBox(slide, ctx, `Prompt:
Necesito PlayerMovement para Unity 3D.
Player = cápsula con Rigidbody.
Movimiento en plano XZ.
Usa Rigidbody.MovePosition en FixedUpdate.
No uses el nuevo Input System.
Velocidad con SerializeField.
Código completo + Inspector.`, 110, 354, 920, 180, 14);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
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
}`, 96, 170, 960, 440, 12.4);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Error frecuente: Input System");
    codeBox(slide, ctx, `InvalidOperationException:
You are trying to read Input using the UnityEngine.Input class,
but active Input handling is Input System package.`, 96, 190, 860, 100, 13.5);
    redClaim(slide, ctx, "Para esta práctica: Active Input Handling = Both", 96, 338, 780, 20);
    bulletList(slide, ctx, [
      "Edit > Project Settings > Player > Other Settings.",
      "Cambiar a Both para permitir Input.GetAxisRaw.",
      "Reiniciar Unity si lo pide.",
    ], 118, 420, { width: 760, gap: 38, fontSize: 15.2 });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Coleccionables y score");
    unityMock(slide, ctx, 78, 198, 600, 305);
    inspectorCards(slide, ctx, 728, 198, [
      ["Sphere", "Collider marcado como Is Trigger.", "red"],
      ["Player", "Tag Player y Rigidbody.", "plain"],
      ["GameManager", "ScoreText asignado en Inspector.", "plain"],
    ]);
    ctx.addText(slide, { text: "Si el trigger no salta, casi siempre falta una pieza de Inspector.", left: 86, top: 536, width: 820, height: 18, fontSize: 12.5, bold: true, color: C.red });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
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

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null) gameManager.AddScore(points);

        gameObject.SetActive(false);
    }
}`, 120, 172, 900, 405, 12.8);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "GameManager: primera versión");
    codeBox(slide, ctx, `Responsabilidades:
- guardar score
- contar coleccionables totales al empezar
- actualizar ScoreText
- mostrar victoria al recoger todo

Restricciones:
- no eventos todavía
- no ScriptableObjects
- null-checks simples para UI
- TextMeshProUGUI asignado en Inspector`, 96, 196, 470, 300, 15);
    codeBox(slide, ctx, `public void AddScore(int points)
{
    if (gameEnded) return;

    score += points;
    collectedCount++;

    UpdateScoreUI();

    if (collectedCount >= totalCollectibles)
        WinGame();
}`, 620, 220, 420, 245, 14.2);
    redClaim(slide, ctx, "El manager coordina el prototipo; no convierte todo en sistema.", 96, 535, 820, 16);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Estados, tiempo y reinicio");
    routeGrid(slide, ctx, [
      ["Inicio", "Start() configura escena", false],
      ["Jugando", "Update() descuenta tiempo", true],
      ["Victoria", "todos los coleccionables", false],
      ["Derrota", "tiempo 0 u obstáculo", true],
      ["Bloqueo", "gameEnded evita dobles eventos", false],
      ["Reinicio", "tecla R + LoadScene", true],
    ]);
    ctx.addText(slide, { text: "No usamos Time.timeScale en esta práctica: mantenemos el flujo simple y visible.", left: 90, top: 512, width: 820, height: 20, fontSize: 13, bold: true, color: C.red });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Obstáculos con cubos rojos");
    primitiveArena(slide, ctx, 95, 210, 0.92);
    bulletList(slide, ctx, [
      "Movimiento entre dos puntos.",
      "Vector3.MoveTowards.",
      "Collider normal, no trigger.",
      "OnCollisionEnter llama a LoseGame().",
      "La dificultad se ajusta con distancia y velocidad.",
    ], 660, 215, { width: 390, gap: 42, fontSize: 15.3 });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
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
}`, 74, 166, 1060, 370, 12.2);
    ctx.addText(slide, { text: "En clase se completa con Update() y OnCollisionEnter().", left: 82, top: 558, width: 740, height: 18, fontSize: 12, color: C.muted });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Cámara y pulido");
    bulletList(slide, ctx, [
      "Opción mínima: cámara fija inclinada.",
      "Opción práctica: CameraFollow con offset.",
      "Usar LateUpdate para evitar vibración.",
      "No usar Cinemachine: menos dependencias para una clase inicial.",
    ], 96, 205, { width: 760, gap: 48, fontSize: 16.5 });
    codeBox(slide, ctx, `Prompt:
Necesito CameraFollow simple.
Target = Player.
Mantener offset configurable.
Usar LateUpdate.
No usar Cinemachine.
Código completo + Inspector.`, 710, 364, 390, 160, 12.8);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Unity no se arregla solo con código");
    unityMock(slide, ctx, 78, 190, 620, 320);
    inspectorCards(slide, ctx, 748, 205, [
      ["El error", "NullReferenceException en GameManager.", "plain"],
      ["Lo que mira ChatGPT", "Script, consola y contexto.", "plain"],
      ["Lo que mira Unity", "Inspector, escena, tags y componentes.", "red"],
    ]);
    ctx.addText(slide, { text: "Regla práctica: script + captura + consola + qué hay en Inspector.", left: 82, top: 546, width: 820, height: 18, fontSize: 12.5, bold: true, color: C.red });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Prompt de depuración");
    codeBox(slide, ctx, `Tengo este error en Unity:
[pegar error completo]

Este es el script:
[pegar script]

Contexto de la escena:
- GameObject donde está el script:
- componentes del Player:
- tag del Player:
- colliders:
- Rigidbody:
- referencias asignadas en Inspector:

Dime:
1. causa más probable
2. cómo comprobarlo en Unity
3. cambio mínimo
4. qué no debería tocar`, 112, 170, 930, 430, 12.8);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Bugs típicos de la práctica");
    const bugs = [
      ["Input no funciona", "Active Input Handling no está en Both.", "red"],
      ["Trigger no salta", "Is Trigger, tag Player o Rigidbody.", "plain"],
      ["Colisión no salta", "Algún collider está en trigger.", "plain"],
      ["Texto no cambia", "Referencia UI no asignada.", "red"],
      ["Cámara vibra", "Seguir en Update en vez de LateUpdate.", "plain"],
      ["LoadScene falla", "Escena no guardada o Build Settings.", "plain"],
    ];
    bugs.forEach(([h, b, m], i) => card(slide, ctx, 90 + (i % 2) * 470, 188 + Math.floor(i / 2) * 96, 400, 58, h, b, m));
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Alucinaciones en Unity");
    bulletList(slide, ctx, [
      "APIs inventadas o paquetes no instalados.",
      "Mezcla de Rigidbody y Rigidbody2D.",
      "Input System nuevo cuando el proyecto no lo usa.",
      "Managers innecesarios para una mecánica simple.",
      "Errores con tags, colliders, escenas o Time.timeScale.",
    ], 96, 200, { width: 880, gap: 50, fontSize: 16.8 });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Antes de aceptar código generado");
    const checks = [
      ["Compila sin errores.", "red"], ["Usa Unity 3D, no una solución 2D.", "plain"],
      ["Respeta Inspector, tags y escena.", "plain"], ["No introduce sistemas innecesarios.", "red"],
      ["No cambia nombres públicos sin motivo.", "plain"], ["Se prueba en Play Mode.", "red"],
    ];
    checks.forEach(([t, m], i) => card(slide, ctx, 92 + (i % 2) * 470, 192 + Math.floor(i / 2) * 88, 390, 52, t, "", m));
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Actividad final por grupos");
    bulletList(slide, ctx, [
      "Terminar una escena jugable.",
      "Mostrar un prompt útil y un prompt que falló.",
      "Explicar una corrección de Inspector.",
      "Hacer una prueba completa en Play Mode.",
    ], 96, 204, { width: 780, gap: 54, fontSize: 16.8 });
    card(slide, ctx, 780, 420, 285, 86, "Pregunta clave", "¿Qué parte era código y qué parte era configuración de Unity?", "red");
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Checklist de entrega");
    bulletList(slide, ctx, [
      "Player se mueve y no gira raro.",
      "Score aumenta al recoger esferas.",
      "El tiempo baja y puede provocar derrota.",
      "Obstáculos causan Game Over.",
      "Victoria al recoger todo.",
      "Tecla R reinicia la escena.",
    ], 92, 188, { width: 430, gap: 42, fontSize: 14.5 });
    bulletList(slide, ctx, [
      "Textos UI asignados.",
      "Tag Player correcto.",
      "Colliders y Rigidbody revisados.",
      "Escena guardada.",
      "Build Settings revisado si hace falta.",
      "No hay errores en consola.",
    ], 620, 188, { width: 430, gap: 42, fontSize: 14.5 });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Scripts finales");
    card(slide, ctx, 110, 210, 250, 60, "PlayerMovement.cs", "Input + Rigidbody.MovePosition.", "plain");
    card(slide, ctx, 410, 210, 250, 60, "Collectible.cs", "Trigger + score + desactivar.", "plain");
    card(slide, ctx, 710, 210, 250, 60, "GameManager.cs", "Score, tiempo, estado y reinicio.", "red");
    card(slide, ctx, 260, 330, 250, 60, "MovingObstacle.cs", "Patrulla + derrota por colisión.", "plain");
    card(slide, ctx, 560, 330, 250, 60, "CameraFollow.cs", "Seguimiento simple con offset.", "plain");
    redClaim(slide, ctx, "Cinco scripts. Ningún sistema nuevo innecesario.", 310, 500, 650, 18);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Cierre");
    redClaim(slide, ctx, "La IA acelera el prototipo, no sustituye la prueba.");
    ctx.addText(slide, { text: "El flujo profesional es: describir bien el problema, pedir ayuda concreta, aplicar cambios pequeños, probar en Unity, volver con contexto y verificar antes de confiar.", left: 88, top: 310, width: 900, height: 70, fontSize: 17.5, color: C.ink });
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
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
}`, 74, 150, 1050, 470, 11.6);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
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
}`, 110, 162, 930, 420, 12.2);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
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
    }`, 70, 150, 1060, 470, 11.2);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
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
    }`, 70, 160, 1060, 430, 11.6);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
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
}`, 70, 160, 1060, 430, 11.2);
  },
  async (slide, ctx) => {
    await frame(slide, ctx);
    title(slide, ctx, "Siguiente paso");
    ctx.addText(slide, { text: "Variantes del prototipo", left: 88, top: 225, width: 760, height: 42, fontSize: 25, bold: true, color: C.red, typeface: "Aptos Display" });
    bulletList(slide, ctx, [
      "Añadir niveles con más obstáculos.",
      "Crear boosters temporales.",
      "Mejorar feedback visual y sonoro.",
      "Usar ChatGPT como revisor antes de entregar.",
    ], 98, 302, { width: 760, gap: 46, fontSize: 16 });
  },
];

export async function makeSlide(presentation, ctx, index) {
  const slide = presentation.slides.add();
  await slides[index - 1](slide, ctx);
  return slide;
}

export const slideCount = slides.length;
