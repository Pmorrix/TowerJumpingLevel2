from pathlib import Path

from PIL import Image
from reportlab.lib import colors
from reportlab.lib.pagesizes import landscape
from reportlab.lib.units import inch
from reportlab.pdfbase.pdfmetrics import stringWidth
from reportlab.pdfgen import canvas


OUT = Path(r"C:\Users\Phillips\Documents\LLM_ChatGPT_Go_Unity_Clase1_Master_UMA_v2.pdf")
ASSET_DIR = Path(r"C:\TowerJumpingLevel2\_deck_assets")
WORK_DIR = Path(r"C:\TowerJumpingLevel2\_pdf_assets")
WORK_DIR.mkdir(exist_ok=True)

W, H = landscape((13.333 * inch, 7.5 * inch))

RED = colors.HexColor("#E3192A")
DARK = colors.HexColor("#202124")
MID = colors.HexColor("#555A60")
LIGHT = colors.HexColor("#F3F4F6")
SOFT_RED = colors.HexColor("#FDE8EC")
LINE = colors.HexColor("#D7DCE2")
WHITE = colors.white


def crop_logo() -> Path:
    src = ASSET_DIR / "Archivos (logo y título) - 07.png"
    out = WORK_DIR / "uma_master_logo_clean.png"
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
        max(min(xs) - 20, 0),
        max(min(ys) - 20, 0),
        min(max(xs) + 20, im.width),
        min(max(ys) + 20, im.height),
    )
    im.crop(box).save(out)
    return out


LOGO = crop_logo()
LOGO_WHITE = WORK_DIR / "uma_master_logo_white.png"


def make_white_logo() -> None:
    src = ASSET_DIR / "Archivos (logo y título) - 06.png"
    im = Image.open(src).convert("RGBA")
    px = im.load()
    xs, ys = [], []
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            is_white = r > 235 and g > 235 and b > 235
            px[x, y] = (255, 255, 255, 255) if is_white else (255, 255, 255, 0)
            if is_white:
                xs.append(x)
                ys.append(y)
    box = (
        max(min(xs) - 20, 0),
        max(min(ys) - 20, 0),
        min(max(xs) + 20, im.width),
        min(max(ys) + 20, im.height),
    )
    im.crop(box).save(LOGO_WHITE)


make_white_logo()


def lines_for(text, font, size, width):
    result = []
    for paragraph in text.split("\n"):
        if not paragraph:
            result.append("")
            continue
        words = paragraph.split()
        line = ""
        for word in words:
            test = word if not line else f"{line} {word}"
            if stringWidth(test, font, size) <= width:
                line = test
            else:
                if line:
                    result.append(line)
                line = word
        if line:
            result.append(line)
    return result


def draw_wrapped(c, text, x, y, width, font="Helvetica", size=18, color=DARK, leading=None):
    leading = leading or size * 1.28
    c.setFont(font, size)
    c.setFillColor(color)
    yy = y
    for line in lines_for(text, font, size, width):
        c.drawString(x, yy, line)
        yy -= leading
    return yy


def page(c, title, subtitle=None, red=False):
    if red:
        c.setFillColor(RED)
        c.rect(0, 0, W, H, fill=1, stroke=0)
        c.drawImage(str(LOGO_WHITE), W - 3.05 * inch, H - 0.86 * inch, width=2.2 * inch, height=0.54 * inch, mask="auto")
        c.setFillColor(WHITE)
        c.setFont("Helvetica-Bold", 34)
        c.drawString(0.85 * inch, H - 2.65 * inch, title)
        if subtitle:
            c.setStrokeColor(WHITE)
            c.setLineWidth(1.2)
            c.line(0.85 * inch, H - 2.98 * inch, 5.2 * inch, H - 2.98 * inch)
            draw_wrapped(c, subtitle, 0.85 * inch, H - 3.42 * inch, 7.2 * inch, size=18, color=WHITE)
        return

    c.setFillColor(WHITE)
    c.rect(0, 0, W, H, fill=1, stroke=0)
    c.setStrokeColor(RED)
    c.setLineWidth(4)
    c.line(0.36 * inch, H - 0.28 * inch, W - 0.36 * inch, H - 0.28 * inch)
    c.drawImage(str(LOGO), W - 3.0 * inch, H - 0.92 * inch, width=2.15 * inch, height=0.52 * inch, mask="auto")
    c.setFillColor(DARK)
    c.setFont("Helvetica-Bold", 29)
    c.drawString(0.75 * inch, H - 1.42 * inch, title)
    if subtitle:
        draw_wrapped(c, subtitle, 0.75 * inch, H - 1.92 * inch, 9.0 * inch, size=17, color=MID)


def bullets(c, items, x=0.9, y=4.8, width=10.4, size=18, gap=0.43):
    yy = y * inch
    for item in items:
        c.setFillColor(RED)
        c.circle(x * inch, yy + 0.08 * inch, 0.045 * inch, fill=1, stroke=0)
        yy = draw_wrapped(c, item, (x + 0.28) * inch, yy, width * inch, size=size, color=DARK)
        yy -= gap * inch
    return yy


def box(c, x, y, w, h, title, body="", fill=LIGHT, stroke=LINE, title_size=16, body_size=14):
    x, y, w, h = x * inch, y * inch, w * inch, h * inch
    c.setFillColor(fill)
    c.setStrokeColor(stroke)
    c.roundRect(x, y, w, h, 8, fill=1, stroke=1)
    c.setFillColor(DARK)
    c.setFont("Helvetica-Bold", title_size)
    c.drawString(x + 0.25 * inch, y + h - 0.45 * inch, title)
    if body:
        draw_wrapped(c, body, x + 0.25 * inch, y + h - 0.85 * inch, w - 0.5 * inch, size=body_size, color=DARK)


def prompt_box(c, text, x=0.9, y=1.0, w=11.4, h=2.1, size=13):
    x, y, w, h = x * inch, y * inch, w * inch, h * inch
    c.setFillColor(colors.HexColor("#111827"))
    c.roundRect(x, y, w, h, 8, fill=1, stroke=0)
    draw_wrapped(c, text, x + 0.28 * inch, y + h - 0.42 * inch, w - 0.56 * inch, font="Courier", size=size, color=WHITE, leading=size * 1.25)


def cover(c):
    c.setFillColor(WHITE)
    c.rect(0, 0, W, H, fill=1, stroke=0)
    c.setFillColor(RED)
    c.rect(0, 0, 0.25 * inch, H, fill=1, stroke=0)
    c.drawImage(str(LOGO), W - 3.8 * inch, H - 1.15 * inch, width=2.85 * inch, height=0.69 * inch, mask="auto")
    c.setFillColor(DARK)
    c.setFont("Helvetica-Bold", 42)
    c.drawString(0.85 * inch, H - 2.65 * inch, "LLM y ChatGPT Go")
    draw_wrapped(
        c,
        "Uso profesional en desarrollo de videojuegos con Unity 3D y C#",
        0.85 * inch,
        H - 3.25 * inch,
        8.4 * inch,
        size=22,
        color=DARK,
    )
    draw_wrapped(
        c,
        "Clase 1 - 5 horas - fundamentos, prompting, riesgos y revisión técnica",
        0.85 * inch,
        H - 4.18 * inch,
        8.5 * inch,
        size=17,
        color=MID,
    )
    box(c, 10.6, 3.0, 1.65, 1.25, "5 h", "Teórico-práctica", fill=RED, stroke=RED, title_size=22, body_size=12)
    c.setFillColor(WHITE)
    c.setFont("Helvetica-Bold", 22)
    c.drawString(10.85 * inch, 3.72 * inch, "5 h")
    c.setFont("Helvetica", 10)
    c.setFillColor(DARK)
    c.drawString(0.85 * inch, 0.55 * inch, "Máster en Creación de Videojuegos - Universidad de Málaga")


def build():
    c = canvas.Canvas(str(OUT), pagesize=(W, H))

    cover(c); c.showPage()

    page(c, "Objetivo general")
    bullets(c, [
        "Comprender qué es un LLM y cómo se relaciona con ChatGPT.",
        "Usar ChatGPT Go como asistente técnico dentro de un flujo Unity 3D y C#.",
        "Aprender a formular prompts verificables, no solo prompts llamativos.",
        "Detectar errores, alucinaciones y riesgos antes de integrar código o decisiones.",
    ], size=20)
    c.showPage()

    page(c, "Resultado esperado")
    bullets(c, [
        "Explicar tokens, contexto, predicción, embeddings y atención a nivel práctico.",
        "Crear prompts técnicos con contexto, objetivo, restricciones y formato de salida.",
        "Usar ChatGPT Go para diseño, código, documentación, QA y depuración.",
        "Aplicar una checklist de revisión antes de aceptar respuestas generadas.",
        "Prepararse para la clase práctica del minijuego sin mezclar todavía su desarrollo.",
    ], size=18)
    c.showPage()

    page(c, "Ruta de 5 horas")
    timeline = [
        ("0:00-0:20", "Apertura"),
        ("0:20-0:45", "ChatGPT Go en el aula"),
        ("0:45-1:25", "Qué es un LLM"),
        ("1:25-2:00", "Transformers y atención"),
        ("2:00-2:10", "Descanso"),
        ("2:10-2:45", "Usos en videojuegos"),
        ("2:45-3:30", "Prompting técnico para Unity"),
        ("3:30-4:10", "Alucinaciones y verificación"),
        ("4:10-4:20", "Descanso"),
        ("4:20-4:50", "Revisión de código"),
        ("4:50-5:00", "Cierre"),
    ]
    for i, (time, name) in enumerate(timeline):
        x = 0.8 + (i % 4) * 3.05
        y = 4.55 - (i // 4) * 1.2
        box(c, x, y, 2.6, 0.78, time, name, fill=SOFT_RED if i in (2, 6, 7) else LIGHT, stroke=RED if i in (2, 6, 7) else LINE, title_size=13, body_size=12)
    c.showPage()

    page(c, "0:00-0:20 | Apertura")
    bullets(c, [
        "Situar ChatGPT como herramienta profesional, no como magia ni como juguete.",
        "Recordar que en Unity una respuesta puede parecer correcta y romper Inspector, escenas, prefabs o físicas.",
        "Pregunta inicial: quién ha usado ChatGPT para programar, con Unity o para diseño de mecánicas.",
    ], size=18)
    box(c, 0.95, 1.2, 11.1, 1.1, "Idea clave", "ChatGPT no sustituye el criterio técnico. Lo acelera cuando el criterio existe.", fill=SOFT_RED, stroke=RED, title_size=18, body_size=16)
    c.showPage()

    page(c, "0:20-0:45 | ChatGPT Go en el aula")
    bullets(c, [
        "ChatGPT Go ofrece más margen de uso que el plan gratuito, pero no convierte las respuestas en verdaderas.",
        "Puede incluir herramientas como carga de archivos, imágenes o análisis según disponibilidad de la cuenta.",
        "ChatGPT no es lo mismo que la API: en esta clase trabajamos desde la interfaz conversacional.",
        "Las funciones y límites pueden variar. La práctica importante es el método: pedir, revisar y probar.",
    ], size=17)
    c.showPage()

    page(c, "Primer prompt de configuración")
    prompt_box(c, """Estoy cursando un máster de creación de videojuegos con Unity 3D y C#.
Durante esta asignatura quiero que actúes como asistente técnico.
Prioriza soluciones simples, código claro, cambios mínimos y explicación de riesgos.
Cuando hablemos de Unity, ten en cuenta Inspector, escenas, prefabs, eventos,
físicas 3D y Time.timeScale.
Si falta información, pregúntame o declara tus supuestos.""", h=3.3, size=12)
    c.showPage()

    page(c, "0:45-1:25 | Qué es un LLM")
    bullets(c, [
        "Un LLM genera texto probable a partir del contexto que recibe.",
        "No es una base de datos perfecta ni una garantía de verdad.",
        "No ve el proyecto Unity completo si no se le proporciona información suficiente.",
        "Trabaja con tokens, contexto, predicción e inferencia.",
    ], size=18)
    c.showPage()

    page(c, "Tokens: el texto se trocea")
    box(c, 0.9, 3.65, 2.35, 1.2, "Texto", "La ruta natural", fill=SOFT_RED, stroke=RED)
    c.setFont("Helvetica-Bold", 26); c.setFillColor(RED); c.drawString(3.45 * inch, 4.05 * inch, ">")
    box(c, 3.85, 3.65, 2.7, 1.2, "Tokens", '["La", " ruta", " natural"]')
    c.setFont("Helvetica-Bold", 26); c.setFillColor(RED); c.drawString(6.8 * inch, 4.05 * inch, ">")
    box(c, 7.2, 3.65, 2.75, 1.2, "IDs", "[4579, 59781, 6247]")
    box(c, 0.9, 1.45, 11.1, 1.15, "Lectura docente", "Los números son índices de vocabulario. El significado útil aparece al convertirlos en vectores y procesarlos dentro del contexto.", fill=LIGHT, stroke=LINE, title_size=17, body_size=15)
    c.showPage()

    page(c, "Embeddings y vectores")
    bullets(c, [
        "Un token pasa a un ID y luego a un vector numérico interno.",
        "Un vector puede tener muchas dimensiones: muchas columnas de números, no ejes físicos visibles.",
        "Esas dimensiones codifican patrones de uso, relaciones y contexto.",
        "La clave para el alumnado: el modelo opera matemáticamente con lenguaje, no con significado puro.",
    ], size=18)
    c.showPage()

    page(c, "El modelo solo ve lo que le das")
    box(c, 0.85, 3.85, 3.4, 0.9, "Proyecto", "género, objetivo, nivel técnico")
    box(c, 4.85, 3.85, 3.4, 0.9, "Escena", "GameObjects, prefabs, tags, layers")
    box(c, 8.85, 3.85, 3.4, 0.9, "Scripts", "responsabilidades y dependencias")
    box(c, 0.85, 2.45, 3.4, 0.9, "Inspector", "SerializeField, UI, botones, referencias")
    box(c, 4.85, 2.45, 3.4, 0.9, "Restricciones", "versión, paquetes, físicas, Time.timeScale")
    box(c, 8.85, 2.45, 3.4, 0.9, "Salida", "código, checklist, revisión o explicación")
    draw_wrapped(c, "Principio operativo: en Unity, describe escena + GameObjects + componentes + scripts + restricciones.", 0.9 * inch, 1.2 * inch, 10.8 * inch, size=19, color=RED)
    c.showPage()

    page(c, "1:25-2:00 | Transformers y atención")
    bullets(c, [
        "El paper Attention Is All You Need presentó la arquitectura Transformer.",
        "La atención permite relacionar partes distintas del contexto.",
        "Self-attention compara elementos dentro de una misma secuencia.",
        "Multi-head attention permite mirar relaciones desde varios ángulos.",
        "La posición de los tokens sigue importando: el orden cambia el significado.",
    ], size=18)
    c.showPage()

    page(c, "Atención aplicada a Unity")
    box(c, 0.9, 3.95, 3.55, 1.15, "Lenguaje", "La cámara siguió al jugador porque estaba demasiado cerca.")
    box(c, 4.85, 3.95, 3.55, 1.15, "Pregunta", "¿Qué estaba demasiado cerca: la cámara o el jugador?", fill=SOFT_RED, stroke=RED)
    box(c, 8.8, 3.95, 3.55, 1.15, "Código", "scoreText, TextMeshProUGUI, Inspector y NullReferenceException.")
    box(c, 0.9, 1.5, 11.1, 1.05, "Límite", "La atención ayuda a relacionar partes del contexto, pero relacionar no es verificar.", fill=SOFT_RED, stroke=RED, title_size=18, body_size=16)
    c.showPage()

    page(c, "2:10-2:45 | Cómo ayuda en videojuegos")
    box(c, 0.85, 3.85, 3.4, 0.9, "Programación", "C#, pseudocódigo, errores, tests")
    box(c, 4.85, 3.85, 3.4, 0.9, "Diseño", "mecánicas, loops, balance inicial")
    box(c, 8.85, 3.85, 3.4, 0.9, "Producción", "tareas, mini GDD, roadmap")
    box(c, 0.85, 2.45, 3.4, 0.9, "Narrativa", "diálogos, misiones, lore")
    box(c, 4.85, 2.45, 3.4, 0.9, "QA", "casos de prueba, edge cases")
    box(c, 8.85, 2.45, 3.4, 0.9, "Aprendizaje", "explicar, comparar, repasar")
    c.showPage()

    page(c, "Ejercicio breve: mecánicas simples")
    prompt_box(c, """Actúa como diseñador de videojuegos.
Propón 5 mecánicas simples para un juego 3D en Unity hecho solo con cubos,
esferas, cápsulas y planos.
Cada mecánica debe poder explicarse en una frase y programarse por un estudiante
principiante-intermedio.""", h=2.7, size=12)
    bullets(c, ["Comentar viabilidad.", "Detectar alcance excesivo.", "Identificar dependencias de assets.", "Elegir qué ideas se prueban fácilmente."], y=2.85, size=17)
    c.showPage()

    page(c, "2:45-3:30 | Prompting técnico para Unity")
    bullets(c, [
        "Un buen prompt funciona como una especificación técnica breve.",
        "Debe incluir rol, contexto, objetivo, restricciones, datos disponibles y formato de salida.",
        "En Unity conviene añadir Inspector, escenas, prefabs, eventos, físicas y Time.timeScale.",
        "La respuesta debe poder probarse en Play Mode o revisarse con una checklist.",
    ], size=18)
    c.showPage()

    page(c, "Plantilla principal de prompt")
    prompt_box(c, """Actúa como desarrollador Unity C# senior.

Contexto:
[descripción del proyecto]

Objetivo:
[qué quiero conseguir]

Scripts existentes:
[nombres y responsabilidades]

Restricciones:
- Unity 3D
- C#
- solución simple
- cambios mínimos
- no usar assets externos
- preservar SerializeField
- no romper referencias de Inspector

Salida:
1. explicación breve
2. código completo si hace falta
3. qué revisar en Inspector
4. riesgos y pruebas""", h=5.0, size=10.5)
    c.showPage()

    page(c, "Prompt malo vs prompt útil")
    box(c, 0.9, 2.55, 5.3, 2.4, "Prompt débil", '"Hazme un enemigo."\n\nProblemas:\n- no hay tipo de juego\n- no hay escena\n- no hay física\n- no hay restricciones\n- no hay criterio de prueba')
    box(c, 6.9, 2.55, 5.3, 2.4, "Prompt útil", '"Actúa como desarrollador Unity C# senior. Necesito un enemigo simple para Unity 3D. El enemigo será un cubo con Collider. El jugador será una cápsula con tag Player. Usa Vector3.MoveTowards, no NavMesh, y dime qué revisar en Inspector."', fill=SOFT_RED, stroke=RED)
    c.showPage()

    page(c, "Actividad: mejorar prompts vagos")
    bullets(c, [
        "Transformar: Haz un sistema de vidas.",
        "Transformar: Haz un menú.",
        "Transformar: Arregla este error.",
        "Transformar: Haz un boss.",
        "Transformar: Haz una IA.",
    ], size=19)
    box(c, 7.25, 1.65, 4.6, 1.55, "Criterio de mejora", "Cada prompt debe incluir contexto, restricciones, formato y cómo verificar la respuesta.", fill=SOFT_RED, stroke=RED, title_size=18, body_size=15)
    c.showPage()

    page(c, "3:30-4:10 | Alucinaciones y errores")
    bullets(c, [
        "Una alucinación es una respuesta falsa o inventada que suena convincente.",
        "En Unity puede compilar parcialmente y aun así no encajar con la escena.",
        "El modelo puede mezclar versiones, paquetes, 2D/3D o patrones no usados en el proyecto.",
        "La solución es verificar: no discutir con la IA, comprobar.",
    ], size=18)
    c.showPage()

    page(c, "Errores típicos en Unity")
    bullets(c, [
        "Inventar métodos de Unity o paquetes no instalados.",
        "Mezclar Rigidbody y Rigidbody2D.",
        "Proponer Input System nuevo cuando el proyecto usa input clásico.",
        "Borrar campos SerializeField o cambiar nombres usados por el Inspector.",
        "Olvidar tags, colliders, layers o referencias UI.",
        "No tener en cuenta Time.timeScale, escenas o eventos.",
    ], size=18)
    c.showPage()

    page(c, "Checklist de verificación")
    checks = [
        "¿Compila?",
        "¿Es Unity 3D y no 2D?",
        "¿Usa componentes que existen en mi escena?",
        "¿Respeta Inspector, tags y layers?",
        "¿Puede producir NullReferenceException?",
        "¿Afecta Time.timeScale, eventos, escenas o prefabs?",
        "¿Introduce paquetes o sistemas nuevos sin necesidad?",
        "¿Se puede probar en Play Mode rápidamente?",
    ]
    for i, chk in enumerate(checks):
        x = 0.85 + (i % 2) * 6.0
        y = 4.75 - (i // 2) * 0.95
        box(c, x, y, 5.25, 0.62, "OK", chk, fill=SOFT_RED if i >= 4 else LIGHT, stroke=RED if i >= 4 else LINE, title_size=11, body_size=12)
    c.showPage()

    page(c, "Prompt anti-alucinaciones")
    prompt_box(c, """No inventes APIs ni paquetes.
Si no sabes si una función existe en Unity, dilo.
Dame una solución compatible con Unity 3D estándar.
Separa hechos, supuestos y recomendaciones.
Prioriza cambios simples y verificables.""", h=2.35, size=13)
    c.showPage()

    page(c, "4:20-4:50 | Revisión de código")
    bullets(c, [
        "ChatGPT es útil como revisor si se le dan riesgos concretos que buscar.",
        "Primero se pega el error completo o el script completo.",
        "Después se describe GameObject, componentes y referencias del Inspector.",
        "La revisión debe priorizar fallos, no mejoras estéticas.",
    ], size=18)
    c.showPage()

    page(c, "Prompt de revisión")
    prompt_box(c, """Revisa este script de Unity como code review.
Prioriza:
- errores de compilación
- NullReferenceException
- mal uso de físicas 3D
- referencias del Inspector
- Time.timeScale
- eventos no desuscritos
- cambios que puedan romper escenas o prefabs

No propongas refactors grandes salvo que sean imprescindibles.
Devuelve primero los problemas, luego una explicación breve.""", h=3.8, size=12)
    c.showPage()

    page(c, "Actividad de revisión")
    bullets(c, [
        "El docente muestra un script corto con 3 o 4 errores típicos.",
        "El alumnado pide revisión a ChatGPT usando el prompt anterior.",
        "Después se compara la revisión de ChatGPT con una revisión humana.",
        "Objetivo: aprender a usar la IA como segundo par de ojos, no como juez final.",
    ], size=18)
    c.showPage()

    page(c, "4:50-5:00 | Cierre")
    bullets(c, [
        "Un LLM trabaja con tokens, contexto y probabilidad.",
        "La atención permite relacionar partes del texto o código, pero no garantiza verdad.",
        "ChatGPT Go da más margen, pero no elimina límites ni errores.",
        "En Unity, el contexto del Inspector y la escena importa tanto como el código.",
        "La buena práctica es pedir, revisar, probar y documentar.",
    ], size=18)
    c.showPage()

    page(c, "Entregable de la Clase 1")
    bullets(c, [
        "Conversación de ChatGPT configurada para Unity.",
        "Plantilla de prompt técnico.",
        "Checklist anti-alucinaciones.",
        "Prompt de revisión de código.",
        "Criterios claros para saber cuándo confiar y cuándo verificar.",
    ], size=19)
    c.showPage()

    page(c, "Relación con la Clase 2")
    bullets(c, [
        "Clase 1: aprender la herramienta, el criterio y el método.",
        "Clase 2: aplicar ese método a un minijuego en Unity.",
        "La práctica usará primitivas básicas: cápsulas, cubos, esferas y planos.",
        "Se trabajará movimiento, coleccionables, obstáculos, UI, victoria, derrota y reinicio.",
    ], size=19)

    c.save()
    print(OUT)


if __name__ == "__main__":
    build()
