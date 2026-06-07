from pathlib import Path

from PIL import Image
from reportlab.lib import colors
from reportlab.lib.pagesizes import landscape
from reportlab.lib.units import inch
from reportlab.pdfbase.pdfmetrics import stringWidth
from reportlab.pdfgen import canvas


OUT = Path(r"C:\Users\Phillips\Documents\LLM_ChatGPT_Go_Unity_Clase1_PROYECTABLE_UMA.pdf")
ASSET_DIR = Path(r"C:\TowerJumpingLevel2\_deck_assets")
WORK_DIR = Path(r"C:\TowerJumpingLevel2\_pdf_assets")
WORK_DIR.mkdir(exist_ok=True)

W, H = landscape((13.333 * inch, 7.5 * inch))

RED = colors.HexColor("#E3192A")
DARK = colors.HexColor("#1F2328")
MID = colors.HexColor("#4E5561")
LIGHT = colors.HexColor("#F4F6F8")
SOFT_RED = colors.HexColor("#FDE8EC")
LINE = colors.HexColor("#D5DAE1")
BLACK_PANEL = colors.HexColor("#111827")
WHITE = colors.white


def make_logo() -> Path:
    src = ASSET_DIR / "Archivos (logo y título) - 07.png"
    out = WORK_DIR / "uma_master_logo_projectable.png"
    im = Image.open(src).convert("RGBA")
    px = im.load()
    xs, ys = [], []
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a and not (r > 246 and g > 246 and b > 246):
                xs.append(x)
                ys.append(y)
    box = (
        max(min(xs) - 24, 0),
        max(min(ys) - 24, 0),
        min(max(xs) + 24, im.width),
        min(max(ys) + 24, im.height),
    )
    im.crop(box).save(out)
    return out


LOGO = make_logo()


def wrap_lines(text, font, size, width):
    lines = []
    for paragraph in text.split("\n"):
        if not paragraph:
            lines.append("")
            continue
        current = ""
        for word in paragraph.split():
            trial = word if not current else f"{current} {word}"
            if stringWidth(trial, font, size) <= width:
                current = trial
            else:
                if current:
                    lines.append(current)
                current = word
        if current:
            lines.append(current)
    return lines


def draw_text(c, text, x, y, width, font="Helvetica", size=22, color=DARK, leading=None):
    leading = leading or size * 1.28
    c.setFont(font, size)
    c.setFillColor(color)
    yy = y
    for line in wrap_lines(text, font, size, width):
        c.drawString(x, yy, line)
        yy -= leading
    return yy


def frame(c, title):
    c.setFillColor(WHITE)
    c.rect(0, 0, W, H, fill=1, stroke=0)
    c.setStrokeColor(RED)
    c.setLineWidth(4)
    c.line(0.36 * inch, H - 0.30 * inch, W - 0.36 * inch, H - 0.30 * inch)
    c.drawImage(str(LOGO), W - 3.05 * inch, H - 0.96 * inch, width=2.25 * inch, height=0.55 * inch, mask="auto")
    c.setFillColor(DARK)
    c.setFont("Helvetica-Bold", 34)
    c.drawString(0.78 * inch, H - 1.55 * inch, title)


def bullets(c, items, x=0.95, y=4.75, width=10.9, size=24, gap=0.46):
    yy = y * inch
    for item in items:
        c.setFillColor(RED)
        c.circle(x * inch, yy + 0.10 * inch, 0.052 * inch, fill=1, stroke=0)
        yy = draw_text(c, item, (x + 0.32) * inch, yy, width * inch, size=size)
        yy -= gap * inch
    return yy


def card(c, x, y, w, h, title, body="", fill=LIGHT, stroke=LINE, title_size=18, body_size=16):
    x, y, w, h = x * inch, y * inch, w * inch, h * inch
    c.setFillColor(fill)
    c.setStrokeColor(stroke)
    c.setLineWidth(1.2)
    c.roundRect(x, y, w, h, 10, fill=1, stroke=1)
    c.setFillColor(DARK)
    c.setFont("Helvetica-Bold", title_size)
    c.drawString(x + 0.27 * inch, y + h - 0.44 * inch, title)
    if body:
        draw_text(c, body, x + 0.27 * inch, y + h - 0.86 * inch, w - 0.54 * inch, size=body_size)


def prompt(c, text, y=1.0, h=3.0, size=15):
    x = 0.85 * inch
    w = 11.75 * inch
    y = y * inch
    h = h * inch
    c.setFillColor(BLACK_PANEL)
    c.roundRect(x, y, w, h, 10, fill=1, stroke=0)
    draw_text(c, text, x + 0.34 * inch, y + h - 0.42 * inch, w - 0.68 * inch, font="Courier", size=size, color=WHITE, leading=size * 1.28)


def cover(c):
    c.setFillColor(WHITE)
    c.rect(0, 0, W, H, fill=1, stroke=0)
    c.setFillColor(RED)
    c.rect(0, 0, 0.28 * inch, H, fill=1, stroke=0)
    c.drawImage(str(LOGO), W - 3.85 * inch, H - 1.20 * inch, width=2.9 * inch, height=0.70 * inch, mask="auto")
    c.setFillColor(DARK)
    c.setFont("Helvetica-Bold", 48)
    c.drawString(0.9 * inch, H - 2.7 * inch, "LLM y ChatGPT Go")
    draw_text(c, "Uso profesional en desarrollo de videojuegos con Unity 3D y C#", 0.9 * inch, H - 3.35 * inch, 8.4 * inch, size=26)
    draw_text(c, "Clase 1 - fundamentos, prompting, riesgos y revisión técnica", 0.9 * inch, H - 4.35 * inch, 8.4 * inch, size=20, color=MID)
    card(c, 10.55, 3.05, 1.85, 1.25, "5 horas", "Teórico-práctica", fill=RED, stroke=RED, title_size=20, body_size=13)
    c.setFillColor(WHITE)
    c.setFont("Helvetica-Bold", 20)
    c.drawString(10.82 * inch, 3.78 * inch, "5 horas")
    c.setFont("Helvetica", 11)
    c.setFillColor(DARK)
    c.drawString(0.9 * inch, 0.55 * inch, "Máster en Creación de Videojuegos - Universidad de Málaga")


def build():
    c = canvas.Canvas(str(OUT), pagesize=(W, H))

    cover(c); c.showPage()

    frame(c, "Qué haremos hoy")
    bullets(c, [
        "Entender qué es un LLM sin misticismo.",
        "Usar ChatGPT Go con criterio técnico.",
        "Aprender a pedir, revisar y verificar.",
        "Preparar la clase práctica del minijuego.",
    ])
    c.showPage()

    frame(c, "La regla de la clase")
    c.setFillColor(RED)
    c.setFont("Helvetica-Bold", 38)
    c.drawString(0.95 * inch, 4.35 * inch, "ChatGPT no decide por nosotros.")
    draw_text(c, "Acelera el trabajo cuando el problema está bien definido y la respuesta se comprueba.", 0.95 * inch, 3.55 * inch, 10.8 * inch, size=28)
    c.showPage()

    frame(c, "Ruta de 5 horas")
    blocks = [
        ("Apertura", "20 min"),
        ("ChatGPT Go", "25 min"),
        ("LLM", "40 min"),
        ("Transformers", "35 min"),
        ("Usos en videojuegos", "35 min"),
        ("Prompts Unity", "45 min"),
        ("Riesgos", "40 min"),
        ("Revisión código", "30 min"),
        ("Cierre", "10 min"),
    ]
    for i, (name, minutes) in enumerate(blocks):
        x = 0.8 + (i % 3) * 4.1
        y = 4.65 - (i // 3) * 1.35
        card(c, x, y, 3.45, 0.95, name, minutes, fill=SOFT_RED if i in (2, 5, 6) else LIGHT, stroke=RED if i in (2, 5, 6) else LINE, title_size=17, body_size=15)
    c.showPage()

    frame(c, "ChatGPT Go en el aula")
    bullets(c, [
        "Más margen de uso que el plan gratuito.",
        "Más acceso a funciones populares como archivos, imágenes y análisis, según disponibilidad.",
        "No es la API: trabajaremos desde la interfaz de ChatGPT.",
        "Más capacidad no significa más verdad.",
    ], size=22)
    c.showPage()

    frame(c, "Prompt inicial de la asignatura")
    prompt(c, """Estoy cursando un máster de creación de videojuegos con Unity 3D y C#.
Durante esta asignatura quiero que actúes como asistente técnico.
Prioriza soluciones simples, código claro, cambios mínimos y explicación de riesgos.
Cuando hablemos de Unity, ten en cuenta Inspector, escenas, prefabs,
eventos, físicas 3D y Time.timeScale.
Si falta información, pregúntame o declara tus supuestos.""", y=1.25, h=3.7, size=15)
    c.showPage()

    frame(c, "Qué es un LLM")
    bullets(c, [
        "Un modelo que genera texto probable a partir de contexto.",
        "No consulta una base de datos perfecta.",
        "No ve tu proyecto Unity si no se lo explicas.",
        "Trabaja con tokens, vectores y predicción.",
    ], size=24)
    c.showPage()

    frame(c, "Tokens: el texto se trocea")
    card(c, 0.9, 3.55, 2.45, 1.2, "Texto", "La ruta natural", fill=SOFT_RED, stroke=RED)
    c.setFont("Helvetica-Bold", 30); c.setFillColor(RED); c.drawString(3.55 * inch, 3.95 * inch, ">")
    card(c, 4.0, 3.55, 2.85, 1.2, "Tokens", '["La", " ruta", " natural"]')
    c.setFont("Helvetica-Bold", 30); c.setFillColor(RED); c.drawString(7.12 * inch, 3.95 * inch, ">")
    card(c, 7.55, 3.55, 3.0, 1.2, "IDs", "[4579, 59781, 6247]")
    draw_text(c, "Los IDs no son significado universal: son índices de vocabulario.", 0.95 * inch, 2.10 * inch, 10.9 * inch, size=25, color=DARK)
    c.showPage()

    frame(c, "Embeddings: texto como vectores")
    bullets(c, [
        "Cada token se transforma en una lista larga de números.",
        "Esas dimensiones no son espacio físico.",
        "Sirven para representar patrones, relaciones y usos.",
        "El modelo opera matemáticamente con lenguaje.",
    ], size=23)
    c.showPage()

    frame(c, "El contexto manda")
    card(c, 0.9, 3.85, 3.45, 0.9, "Proyecto", "género, objetivo, nivel")
    card(c, 4.75, 3.85, 3.45, 0.9, "Escena", "GameObjects, tags, prefabs")
    card(c, 8.6, 3.85, 3.45, 0.9, "Scripts", "responsabilidades")
    card(c, 0.9, 2.35, 3.45, 0.9, "Inspector", "SerializeField, UI, botones")
    card(c, 4.75, 2.35, 3.45, 0.9, "Restricciones", "versión, paquetes, físicas")
    card(c, 8.6, 2.35, 3.45, 0.9, "Tiempo", "Update, FixedUpdate, Time.timeScale")
    draw_text(c, "Cuanto menos contexto das, más huecos tiene que rellenar.", 0.95 * inch, 1.25 * inch, 10.8 * inch, size=25, color=RED)
    c.showPage()

    frame(c, "Transformers y atención")
    bullets(c, [
        "El paper Attention Is All You Need popularizó el Transformer.",
        "La atención relaciona partes distintas del contexto.",
        "El orden sigue importando: cambiar el orden cambia el sentido.",
        "Esto ayuda a conectar instrucciones, código y restricciones.",
    ], size=22)
    c.showPage()

    frame(c, "Atención no es verificación")
    card(c, 0.95, 3.55, 3.4, 1.25, "Frase", "La cámara siguió al jugador porque estaba demasiado cerca.")
    card(c, 4.75, 3.55, 3.4, 1.25, "Pregunta", "¿Qué estaba demasiado cerca?", fill=SOFT_RED, stroke=RED)
    card(c, 8.55, 3.55, 3.4, 1.25, "Unity", "scoreText, Inspector, NullReferenceException")
    draw_text(c, "Relacionar no es comprobar. Una respuesta puede sonar bien y estar mal.", 0.95 * inch, 1.9 * inch, 10.8 * inch, size=27, color=RED)
    c.showPage()

    frame(c, "Mini ejercicio")
    prompt(c, """Explícame qué es un Transformer usando una analogía con Unity 3D.
Máximo 120 palabras.
Evita metáforas mágicas y separa analogía de realidad técnica.""", y=2.25, h=2.25, size=17)
    c.showPage()

    frame(c, "Dónde aporta valor en videojuegos")
    card(c, 0.85, 3.85, 3.45, 0.9, "Diseño", "mecánicas, loops, balance")
    card(c, 4.75, 3.85, 3.45, 0.9, "Programación", "C#, errores, tests")
    card(c, 8.65, 3.85, 3.45, 0.9, "Narrativa", "NPCs, diálogos, lore")
    card(c, 0.85, 2.35, 3.45, 0.9, "Producción", "tareas, mini GDD")
    card(c, 4.75, 2.35, 3.45, 0.9, "QA", "casos de prueba")
    card(c, 8.65, 2.35, 3.45, 0.9, "Aprendizaje", "explicar, comparar")
    c.showPage()

    frame(c, "En Unity no basta con el script")
    bullets(c, [
        "El Inspector puede tener referencias invisibles en el código.",
        "Los prefabs y escenas pueden depender de nombres públicos.",
        "Los colliders, triggers y rigidbodies condicionan el comportamiento.",
        "Time.timeScale puede romper corrutinas y temporizadores.",
    ], size=22)
    c.showPage()

    frame(c, "Ejercicio: ideas con primitivas")
    prompt(c, """Actúa como diseñador de videojuegos.
Propón 5 mecánicas simples para un juego 3D en Unity hecho solo con cubos,
esferas, cápsulas y planos.
Cada mecánica debe poder explicarse en una frase y programarse por un estudiante
principiante-intermedio.""", y=1.55, h=3.05, size=15)
    c.showPage()

    frame(c, "Anatomía de un buen prompt")
    card(c, 0.9, 4.0, 3.3, 0.85, "Rol", "quién debe actuar")
    card(c, 4.65, 4.0, 3.3, 0.85, "Contexto", "proyecto y escena")
    card(c, 8.4, 4.0, 3.3, 0.85, "Objetivo", "qué necesitas")
    card(c, 0.9, 2.55, 3.3, 0.85, "Restricciones", "qué debe respetar")
    card(c, 4.65, 2.55, 3.3, 0.85, "Formato", "cómo lo devuelve")
    card(c, 8.4, 2.55, 3.3, 0.85, "Criterio", "cómo se valida")
    draw_text(c, "Un prompt útil produce una respuesta revisable.", 0.95 * inch, 1.35 * inch, 10.8 * inch, size=27, color=RED)
    c.showPage()

    frame(c, "Plantilla técnica")
    prompt(c, """Actúa como desarrollador Unity C# senior.

Contexto:
[descripción del proyecto y escena]

Objetivo:
[qué quiero conseguir]

Scripts existentes:
[nombres y responsabilidades]

Restricciones:
- Unity 3D
- C#
- solución simple
- cambios mínimos
- preservar SerializeField
- no romper referencias de Inspector""", y=0.95, h=4.85, size=13)
    c.showPage()

    frame(c, "Qué debe devolver")
    prompt(c, """Salida:
1. explicación breve
2. código completo si hace falta
3. qué revisar en Inspector
4. riesgos y pruebas

Antes de responder:
- declara supuestos
- avisa si falta información
- no inventes paquetes ni APIs""", y=1.45, h=3.7, size=15)
    c.showPage()

    frame(c, "Prompt malo vs prompt útil")
    card(c, 0.9, 2.45, 5.35, 2.65, "Prompt débil", '"Hazme un enemigo."\n\nNo hay tipo de juego.\nNo hay escena.\nNo hay física.\nNo hay criterio de prueba.')
    card(c, 6.85, 2.45, 5.35, 2.65, "Prompt útil", '"Actúa como desarrollador Unity C# senior. Necesito un enemigo simple: un cubo con Collider que persiga a una cápsula Player con tag Player. Usa Vector3.MoveTowards, no NavMesh, y dime qué revisar en Inspector."', fill=SOFT_RED, stroke=RED, body_size=15)
    c.showPage()

    frame(c, "La utilidad aparece al iterar")
    bullets(c, [
        "Dame 3 enfoques más simples.",
        "Elimina lo que no sea imprescindible.",
        "Compara coste, riesgo y valor jugable.",
        "Busca bugs, supuestos ocultos y pruebas necesarias.",
    ], size=24)
    c.showPage()

    frame(c, "Alucinaciones en Unity")
    bullets(c, [
        "APIs inventadas o paquetes no instalados.",
        "Mezcla de Rigidbody y Rigidbody2D.",
        "Input System nuevo cuando el proyecto no lo usa.",
        "Campos SerializeField borrados o renombrados.",
        "Errores con tags, colliders, escenas o Time.timeScale.",
    ], size=22)
    c.showPage()

    frame(c, "Checklist antes de aceptar")
    checks = [
        "¿Compila?",
        "¿Es Unity 3D y no 2D?",
        "¿Respeta Inspector, tags y layers?",
        "¿Puede lanzar NullReferenceException?",
        "¿Afecta escenas, eventos o Time.timeScale?",
        "¿Se prueba rápido en Play Mode?",
    ]
    for i, text in enumerate(checks):
        x = 0.85 + (i % 2) * 6.1
        y = 4.55 - (i // 2) * 1.1
        card(c, x, y, 5.25, 0.78, "OK", text, fill=SOFT_RED if i >= 3 else LIGHT, stroke=RED if i >= 3 else LINE, title_size=14, body_size=15)
    c.showPage()

    frame(c, "Prompt anti-alucinaciones")
    prompt(c, """No inventes APIs ni paquetes.
Si no sabes si una función existe en Unity, dilo.
Dame una solución compatible con Unity 3D estándar.
Separa hechos, supuestos y recomendaciones.
Prioriza una solución simple y verificable.""", y=2.0, h=2.8, size=17)
    c.showPage()

    frame(c, "ChatGPT como revisor")
    bullets(c, [
        "Sirve mejor cuando se le pide buscar riesgos concretos.",
        "Hay que dar error completo, script y contexto de escena.",
        "La revisión debe priorizar bugs, no estilo.",
        "La revisión humana sigue siendo obligatoria.",
    ], size=23)
    c.showPage()

    frame(c, "Prompt de revisión")
    prompt(c, """Revisa este script de Unity como code review.
Prioriza:
- errores de compilación
- NullReferenceException
- mal uso de físicas 3D
- referencias del Inspector
- Time.timeScale
- eventos no desuscritos
- cambios que puedan romper escenas o prefabs

No propongas refactors grandes salvo que sean imprescindibles.""", y=1.0, h=4.6, size=14)
    c.showPage()

    frame(c, "Ejercicio de cierre")
    bullets(c, [
        "Elegir un caso: mecánica, enemigo, UI, bug, vidas o QA.",
        "Hacer 3 iteraciones con ChatGPT.",
        "Comparar primera y última respuesta.",
        "Explicar por qué la última es más verificable.",
    ], size=23)
    c.showPage()

    frame(c, "Uso responsable")
    bullets(c, [
        "No subir código propietario o datos sensibles sin permiso.",
        "No copiar código generado sin entenderlo.",
        "Verificar licencias de assets, imágenes o textos.",
        "Declarar uso de IA si el centro lo exige.",
    ], size=23)
    c.showPage()

    frame(c, "Cierre")
    bullets(c, [
        "LLM: tokens, contexto y predicción.",
        "Atención: relaciona, pero no verifica.",
        "ChatGPT Go: más margen, no menos responsabilidad.",
        "Unity: escena e Inspector importan tanto como el código.",
        "Método: pedir, revisar, probar y documentar.",
    ], size=22)
    c.showPage()

    frame(c, "Siguiente clase")
    c.setFillColor(RED)
    c.setFont("Helvetica-Bold", 40)
    c.drawString(0.95 * inch, 4.4 * inch, "Minijuego en Unity")
    draw_text(c, "Aplicaremos este método con cápsulas, cubos, esferas y planos.", 0.95 * inch, 3.55 * inch, 10.5 * inch, size=28)
    draw_text(c, "Movimiento, coleccionables, obstáculos, UI, victoria, derrota y reinicio.", 0.95 * inch, 2.75 * inch, 10.5 * inch, size=24, color=MID)

    c.save()
    print(OUT)


if __name__ == "__main__":
    build()
