from pathlib import Path
from textwrap import wrap

from PIL import Image
from reportlab.lib import colors
from reportlab.lib.pagesizes import landscape
from reportlab.lib.units import inch
from reportlab.pdfbase.pdfmetrics import stringWidth
from reportlab.pdfgen import canvas


OUT = Path(r"C:\Users\Phillips\Documents\LLM_ChatGPT_Go_Unity_Clase1_Master_UMA.pdf")
ASSET_DIR = Path(r"C:\TowerJumpingLevel2\_deck_assets")
WORK_DIR = Path(r"C:\TowerJumpingLevel2\_pdf_assets")
WORK_DIR.mkdir(exist_ok=True)

W, H = landscape((13.333 * inch, 7.5 * inch))
RED = colors.HexColor("#E3192A")
DARK = colors.HexColor("#1D1D1F")
MUTED = colors.HexColor("#5F6368")
LIGHT = colors.HexColor("#F4F5F7")
PINK = colors.HexColor("#FCE8EC")
LINE = colors.HexColor("#D9DDE3")


def crop_logo() -> Path:
    src = ASSET_DIR / "Archivos (logo y título) - 07.png"
    out = WORK_DIR / "uma_videojuegos_logo_crop.png"
    im = Image.open(src).convert("RGBA")
    pix = im.load()
    xs, ys = [], []
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = pix[x, y]
            if a and not (r > 245 and g > 245 and b > 245):
                xs.append(x)
                ys.append(y)
    box = (max(min(xs) - 18, 0), max(min(ys) - 18, 0), min(max(xs) + 18, im.width), min(max(ys) + 18, im.height))
    im.crop(box).save(out)
    return out


LOGO = crop_logo()


def text_lines(text, font, size, width):
    lines = []
    for paragraph in text.split("\n"):
        if not paragraph:
            lines.append("")
            continue
        words = paragraph.split()
        current = ""
        for word in words:
            test = word if not current else f"{current} {word}"
            if stringWidth(test, font, size) <= width:
                current = test
            else:
                if current:
                    lines.append(current)
                current = word
        if current:
            lines.append(current)
    return lines


def draw_wrapped(c, text, x, y, width, font="Helvetica", size=18, color=DARK, leading=None, max_lines=None):
    leading = leading or size * 1.25
    c.setFont(font, size)
    c.setFillColor(color)
    lines = text_lines(text, font, size, width)
    if max_lines is not None:
        lines = lines[:max_lines]
    yy = y
    for line in lines:
        c.drawString(x, yy, line)
        yy -= leading
    return yy


def header(c, section, num):
    c.setStrokeColor(RED)
    c.setLineWidth(4)
    c.line(0.18 * inch, H - 0.18 * inch, W - 0.18 * inch, H - 0.18 * inch)
    c.setFont("Helvetica-Bold", 8)
    c.setFillColor(DARK)
    c.drawString(0.42 * inch, H - 0.42 * inch, section.upper())
    c.drawImage(str(LOGO), W - 2.6 * inch, H - 0.62 * inch, width=1.55 * inch, preserveAspectRatio=True, mask="auto")
    c.setStrokeColor(LINE)
    c.setLineWidth(1)
    c.line(0.42 * inch, 0.42 * inch, W - 0.92 * inch, 0.42 * inch)
    c.setFont("Helvetica", 8)
    c.setFillColor(DARK)
    c.drawString(0.42 * inch, 0.22 * inch, section.capitalize())
    c.drawRightString(W - 0.52 * inch, 0.22 * inch, f"{num:02d}")


def title(c, section, num, title_text, subtitle=None):
    header(c, section, num)
    c.setFillColor(DARK)
    c.setFont("Helvetica-Bold", 26)
    c.drawString(0.72 * inch, H - 1.22 * inch, title_text)
    if subtitle:
        draw_wrapped(c, subtitle, 0.72 * inch, H - 1.72 * inch, 8.8 * inch, size=15, color=MUTED)


def bullet_list(c, bullets, x, y, width, size=16, gap=0.45):
    yy = y
    for item in bullets:
        c.setFillColor(RED)
        c.circle(x, yy + 0.06 * inch, 0.04 * inch, fill=1, stroke=0)
        yy = draw_wrapped(c, item, x + 0.22 * inch, yy, width - 0.22 * inch, size=size, color=DARK, leading=size * 1.32)
        yy -= gap * inch
    return yy


def card(c, x, y, w, h, heading, body, fill=LIGHT, stroke=LINE, body_size=13):
    c.setFillColor(fill)
    c.setStrokeColor(stroke)
    c.roundRect(x, y, w, h, 8, fill=1, stroke=1)
    c.setFillColor(DARK)
    c.setFont("Helvetica-Bold", 14)
    c.drawString(x + 0.22 * inch, y + h - 0.38 * inch, heading)
    draw_wrapped(c, body, x + 0.22 * inch, y + h - 0.72 * inch, w - 0.44 * inch, size=body_size, color=DARK, leading=body_size * 1.25)


def cover(c):
    c.setFillColor(colors.white)
    c.rect(0, 0, W, H, fill=1, stroke=0)
    c.setFillColor(RED)
    c.rect(0, 0, 0.22 * inch, H, fill=1, stroke=0)
    c.drawImage(str(LOGO), 0.72 * inch, H - 1.35 * inch, width=2.8 * inch, preserveAspectRatio=True, mask="auto")
    c.setFillColor(DARK)
    c.setFont("Helvetica-Bold", 34)
    c.drawString(0.72 * inch, H - 2.65 * inch, "LLM y")
    c.drawString(0.72 * inch, H - 3.14 * inch, "ChatGPT Go")
    draw_wrapped(c, "Uso profesional en desarrollo de videojuegos con Unity 3D y C#", 0.72 * inch, H - 3.7 * inch, 7.1 * inch, size=18, color=DARK)
    draw_wrapped(c, "Clase 1 - 5 horas - fundamentos, prompts, riesgos y revisión técnica", 0.72 * inch, H - 4.35 * inch, 7.4 * inch, size=14, color=MUTED)
    card(c, W - 3.0 * inch, H - 4.25 * inch, 1.8 * inch, 1.45 * inch, "5h", "clase\nteórica-práctica", fill=RED, stroke=RED, body_size=13)
    c.setFillColor(colors.white)
    c.setFont("Helvetica-Bold", 24)
    c.drawString(W - 2.75 * inch, H - 3.38 * inch, "5h")
    c.setFillColor(DARK)
    c.setFont("Helvetica", 9)
    c.drawString(0.72 * inch, 0.5 * inch, "Máster en Creación de Videojuegos - Universidad de Málaga")


def divider(c, num, label, subtitle):
    c.setFillColor(RED)
    c.rect(0, 0, W, H, fill=1, stroke=0)
    c.drawImage(str(LOGO), W - 2.95 * inch, H - 0.88 * inch, width=2.1 * inch, preserveAspectRatio=True, mask="auto")
    c.setFillColor(colors.white)
    c.setFont("Helvetica-Bold", 13)
    c.drawString(0.82 * inch, H - 2.2 * inch, label.upper())
    c.setFont("Helvetica-Bold", 30)
    c.drawString(0.82 * inch, H - 2.88 * inch, subtitle)
    c.setStrokeColor(colors.white)
    c.setLineWidth(1)
    c.line(0.82 * inch, H - 3.25 * inch, 5.4 * inch, H - 3.25 * inch)
    c.setFont("Helvetica", 10)
    c.drawRightString(W - 0.65 * inch, 0.38 * inch, f"{num:02d}")


def build():
    c = canvas.Canvas(str(OUT), pagesize=(W, H))
    cover(c); c.showPage()

    title(c, "Enfoque", 2, "Objetivo de la clase", "Qué debe dominar el alumnado antes de pasar al taller práctico")
    bullet_list(c, [
        "Comprender qué es un LLM y cómo trabaja con tokens, contexto y predicción.",
        "Usar ChatGPT Go como asistente técnico para Unity, no como autoridad automática.",
        "Formular prompts útiles con contexto, restricciones y formato verificable.",
        "Detectar alucinaciones, errores plausibles y riesgos específicos de Unity.",
    ], 0.92 * inch, 3.95 * inch, 7.3 * inch, size=15)
    card(c, 9.0 * inch, 2.5 * inch, 3.0 * inch, 2.2 * inch, "Idea central", "ChatGPT acelera el aprendizaje y el prototipado cuando el problema está bien formulado y la respuesta se verifica.", fill=PINK, stroke=RED)
    c.showPage()

    title(c, "Plan de clase", 3, "Ruta de 5 horas", "Estructura cerrada de la primera sesión")
    items = [("01", "Apertura", "20 min"), ("02", "ChatGPT Go", "25 min"), ("03", "LLM", "40 min"), ("04", "Transformers", "35 min"), ("05", "Descanso", "10 min"), ("06", "Videojuegos", "35 min"), ("07", "Prompts Unity", "45 min"), ("08", "Riesgos", "40 min"), ("09", "Revisión código", "30 min"), ("10", "Cierre", "10 min")]
    for i, (n, name, mins) in enumerate(items):
        x = 0.72 * inch + (i % 5) * 2.35 * inch
        y = 3.95 * inch - (i // 5) * 1.4 * inch
        card(c, x, y, 1.95 * inch, 0.92 * inch, f"{n}  {name}", mins, fill=PINK if i in [2, 6, 7] else LIGHT, stroke=RED if i in [2, 6, 7] else LINE, body_size=12)
    c.showPage()

    title(c, "Fundamentos", 4, "IA generativa en contexto", "Diferencia práctica para un desarrollador de videojuegos")
    card(c, 0.8 * inch, 2.2 * inch, 5.4 * inch, 2.6 * inch, "IA clásica", "Sistemas orientados a decidir, clasificar, predecir o controlar a partir de reglas, datos o modelos entrenados.\n\nEjemplos: pathfinding, FSM, behavior trees, detección, scoring.", fill=LIGHT, stroke=LINE)
    card(c, 7.0 * inch, 2.2 * inch, 5.4 * inch, 2.6 * inch, "IA generativa", "Sistemas orientados a producir contenido nuevo: texto, código, imágenes, audio, ideas o variantes.\n\nEjemplos: diálogos, misiones, documentación, prototipos de scripts, QA.", fill=PINK, stroke=RED)
    c.showPage()

    title(c, "ChatGPT Go", 5, "La herramienta que usaremos", "Más margen que el plan gratuito, pero no una garantía de verdad")
    bullet_list(c, [
        "Más margen de uso que una cuenta gratuita.",
        "Puede incluir carga de archivos, imágenes, análisis y herramientas según disponibilidad.",
        "No es lo mismo que la API: aquí trabajamos desde la interfaz de ChatGPT.",
        "Sus límites pueden variar por cuenta, región, carga del sistema o cambios del servicio.",
    ], 0.9 * inch, 4.2 * inch, 7.2 * inch)
    card(c, 8.7 * inch, 2.3 * inch, 3.25 * inch, 2.45 * inch, "Para clase", "Lo importante no es la suscripción, sino el método: pedir con precisión, revisar y probar.", fill=LIGHT, stroke=LINE)
    c.showPage()

    title(c, "Fundamentos", 6, "Tokens: el texto se trocea", "El modelo no recibe frases como las vemos nosotros")
    xs = [0.7, 3.5, 6.45, 9.45]
    labels = [("Texto", "La ruta natural"), ("Tokens", '["La", " ruta",\n" natural"]'), ("IDs", "[4579, 59781,\n6247]"), ("Criterio", "Los números son índices de vocabulario. El significado aparece en contexto y vectores internos.")]
    for i, (h, b) in enumerate(labels):
        card(c, xs[i] * inch, 2.75 * inch, 2.35 * inch, 1.25 * inch, h, b, fill=PINK if i == 0 else LIGHT, stroke=RED if i == 0 else LINE, body_size=12)
        if i < 3:
            c.setFont("Helvetica-Bold", 26)
            c.setFillColor(RED)
            c.drawString((xs[i] + 2.48) * inch, 3.18 * inch, ">")
    draw_wrapped(c, "Para Unity: si el contexto no incluye escena, componentes e Inspector, el modelo completa huecos con supuestos.", 0.72 * inch, 1.5 * inch, 10.6 * inch, size=15, color=DARK)
    c.showPage()

    title(c, "Fundamentos", 7, "Embeddings y vectores", "Representaciones numéricas de alta dimensión")
    card(c, 0.8 * inch, 3.45 * inch, 11.2 * inch, 1.4 * inch, "Token -> ID -> vector", "Un fragmento de texto pasa a un número de vocabulario y después a una lista larga de valores.", fill=PINK, stroke=RED)
    card(c, 0.8 * inch, 1.75 * inch, 5.25 * inch, 1.15 * inch, "Idea clave", "No son coordenadas físicas: son coordenadas matemáticas para que el modelo opere con lenguaje.", fill=LIGHT, stroke=LINE)
    card(c, 6.75 * inch, 1.75 * inch, 5.25 * inch, 1.15 * inch, "Para ingenieros", "Es un espacio de cálculo con muchas dimensiones, no un espacio visual de tres ejes.", fill=LIGHT, stroke=LINE)
    c.showPage()

    title(c, "Contexto", 8, "Qué contexto necesita ChatGPT", "En Unity, el código aislado rara vez cuenta toda la historia")
    contexts = [("Proyecto", "género, objetivo, nivel técnico"), ("Escena", "GameObjects, prefabs, tags, layers"), ("Scripts", "responsabilidades y dependencias"), ("Inspector", "SerializeField, UI, botones, referencias"), ("Restricciones", "versión, paquetes, físicas, Time.timeScale")]
    for i, (h, b) in enumerate(contexts):
        x = 0.8 * inch + (i % 3) * 3.9 * inch
        y = 3.95 * inch - (i // 3) * 1.45 * inch
        card(c, x, y, 3.3 * inch, 1.0 * inch, h, b, fill=LIGHT, stroke=LINE, body_size=12)
    draw_wrapped(c, "Principio operativo: cuanto mejor describes el entorno, menos tiene que inventar.", 0.8 * inch, 1.15 * inch, 10.6 * inch, size=16, color=RED)
    c.showPage()

    title(c, "Transformers", 9, "Attention Is All You Need", "Por qué el Transformer cambió la IA generativa moderna")
    bullet_list(c, [
        "Antes: muchos modelos procesaban secuencias de forma más lineal o recurrente.",
        "Transformer: relaciona muchas partes del contexto mediante atención.",
        "Ventaja: entrenamiento más paralelizable y mejor manejo de dependencias largas.",
    ], 0.9 * inch, 4.05 * inch, 7.2 * inch)
    card(c, 8.75 * inch, 2.35 * inch, 3.25 * inch, 2.3 * inch, "Para nosotros", "Ayuda a entender por qué ChatGPT puede relacionar instrucciones, código y restricciones dentro de una conversación.", fill=PINK, stroke=RED)
    c.showPage()

    title(c, "Atención", 10, "Atención no es comprensión perfecta", "Es una forma de ponderar qué partes importan para responder")
    card(c, 0.85 * inch, 3.55 * inch, 3.5 * inch, 1.35 * inch, "Ejemplo", "La cámara siguió al jugador porque estaba demasiado cerca.", fill=LIGHT, stroke=LINE)
    card(c, 4.9 * inch, 3.55 * inch, 3.5 * inch, 1.35 * inch, "Pregunta", "¿Qué estaba demasiado cerca: la cámara o el jugador?", fill=PINK, stroke=RED)
    card(c, 8.95 * inch, 3.55 * inch, 3.5 * inch, 1.35 * inch, "En código", "Relaciona scoreText, TextMeshProUGUI, Inspector y NullReferenceException.", fill=LIGHT, stroke=LINE)
    draw_wrapped(c, "Límite: relacionar partes del contexto no equivale a verificar que la respuesta sea correcta.", 0.85 * inch, 1.7 * inch, 10.8 * inch, size=17, color=RED)
    c.showPage()

    title(c, "Videojuegos", 11, "Dónde aporta valor", "No solo código: también diseño, producción y validación")
    areas = [("Diseño", "mecánicas, loops, economía, niveles"), ("Programación", "pseudocódigo, scripts base, errores"), ("Narrativa", "diálogos, NPCs, misiones, lore"), ("Producción", "tareas, mini GDD, roadmap"), ("QA", "casos de prueba, edge cases, checklist")]
    for i, (h, b) in enumerate(areas):
        x = 0.8 * inch + (i % 3) * 3.9 * inch
        y = 3.95 * inch - (i // 3) * 1.45 * inch
        card(c, x, y, 3.3 * inch, 1.0 * inch, h, b, fill=LIGHT if i != 1 else PINK, stroke=LINE if i != 1 else RED, body_size=12)
    draw_wrapped(c, "Principio: cuanto más concreto sea el problema, más útil será la respuesta.", 0.8 * inch, 1.15 * inch, 10.6 * inch, size=16, color=RED)
    c.showPage()

    title(c, "Unity", 12, "El contexto de Unity importa", "La escena también es parte del problema técnico")
    card(c, 1.0 * inch, 2.65 * inch, 5.0 * inch, 1.75 * inch, "Inspector y escena", "SerializeField, referencias UI, prefabs, tags, layers, botones y GameObjects.", fill=LIGHT, stroke=LINE)
    card(c, 7.0 * inch, 2.65 * inch, 5.0 * inch, 1.75 * inch, "Flujo técnico", "Rigidbody, Collider, Trigger, eventos, escenas, FixedUpdate y Time.timeScale.", fill=PINK, stroke=RED)
    draw_wrapped(c, "Regla: pedir código sin explicar la escena suele producir respuestas frágiles.", 1.0 * inch, 1.4 * inch, 10.4 * inch, size=16, color=RED)
    c.showPage()

    divider(c, 13, "Bloque práctico", "Prompting")
    c.showPage()

    title(c, "Prompts", 14, "Anatomía de un buen prompt", "Una especificación mínima para trabajar con ChatGPT")
    parts = [("Rol", "Quién debe actuar"), ("Contexto", "Proyecto, escena y nivel técnico"), ("Objetivo", "Qué resultado se necesita"), ("Restricciones", "Qué debe evitar o respetar"), ("Formato", "Cómo debe devolver la respuesta"), ("Criterio", "Cómo sabremos si sirve")]
    for i, (h, b) in enumerate(parts):
        x = 0.8 * inch + (i % 3) * 3.9 * inch
        y = 3.75 * inch - (i // 3) * 1.35 * inch
        card(c, x, y, 3.35 * inch, 0.95 * inch, h, b, fill=LIGHT, stroke=LINE, body_size=12)
    draw_wrapped(c, "Prompt útil = instrucciones claras + contexto suficiente + salida evaluable.", 0.8 * inch, 1.0 * inch, 10.6 * inch, size=17, color=RED)
    c.showPage()

    title(c, "Prompts", 15, "Prompt malo vs prompt útil", "Comparar para que la mejora sea evidente")
    card(c, 0.8 * inch, 2.1 * inch, 5.3 * inch, 2.75 * inch, "Prompt débil", '"Hazme un enemigo."\n\nProblemas:\n- no hay tipo de juego\n- no hay escena\n- no hay física\n- no hay restricciones\n- no hay criterio de prueba', fill=LIGHT, stroke=LINE, body_size=12)
    card(c, 6.9 * inch, 2.1 * inch, 5.3 * inch, 2.75 * inch, "Prompt útil", '"Actúa como desarrollador Unity C# senior. Necesito un enemigo simple: un cubo que persiga a una cápsula Player a menos de 8 unidades. Usa Vector3.MoveTowards, no NavMesh, y dime qué revisar en Inspector."', fill=PINK, stroke=RED, body_size=12)
    c.showPage()

    title(c, "Prompts", 16, "La respuesta inicial no es el final", "La utilidad aparece en la iteración")
    steps = [("Pedir alternativas", "Dame 3 enfoques más simples."), ("Pedir reducción", "Elimina lo que no sea imprescindible."), ("Pedir comparación", "Compara coste, riesgo y valor jugable."), ("Pedir revisión", "Busca bugs, supuestos ocultos y pruebas necesarias.")]
    for i, (h, b) in enumerate(steps):
        x = 0.9 * inch + (i % 2) * 5.75 * inch
        y = 3.75 * inch - (i // 2) * 1.55 * inch
        card(c, x, y, 4.9 * inch, 1.05 * inch, h, b, fill=LIGHT if i != 3 else PINK, stroke=LINE if i != 3 else RED)
    c.showPage()

    title(c, "Revisión", 17, "ChatGPT como revisor", "Útil cuando se le pide mirar riesgos concretos")
    review = [("Primero", "Pegar error completo o script completo."), ("Después", "Describir GameObject, componentes y referencias del Inspector."), ("Pedir", "Causa probable, comprobación, cambio mínimo y pruebas."), ("Evitar", 'Preguntas vagas como "no funciona" sin escena ni consola.')]
    for i, (h, b) in enumerate(review):
        x = 0.9 * inch + (i % 2) * 5.75 * inch
        y = 3.75 * inch - (i // 2) * 1.55 * inch
        card(c, x, y, 4.9 * inch, 1.05 * inch, h, b, fill=LIGHT, stroke=LINE)
    draw_wrapped(c, "Regla docente: revisar respuestas con criterio técnico antes de integrarlas.", 0.9 * inch, 1.0 * inch, 10.8 * inch, size=16, color=RED)
    c.showPage()

    title(c, "Checklist", 18, "Antes de aceptar código generado", "Una revisión mínima para Unity 3D")
    checks = ["¿Compila?", "¿Es Unity 3D y no 2D?", "¿Respeta SerializeField e Inspector?", "¿Usa componentes que existen?", "¿Puede producir NullReferenceException?", "¿Afecta Time.timeScale, escenas o prefabs?"]
    for i, chk in enumerate(checks):
        x = 0.9 * inch + (i % 3) * 3.9 * inch
        y = 3.75 * inch - (i // 3) * 1.35 * inch
        card(c, x, y, 3.25 * inch, 0.95 * inch, "OK", chk, fill=PINK if i >= 3 else LIGHT, stroke=RED if i >= 3 else LINE, body_size=12)
    c.showPage()

    divider(c, 19, "Riesgos", "La IA puede fallar de forma convincente")
    c.showPage()

    title(c, "Riesgos", 20, "Alucinaciones en Unity", "Errores típicos que parecen soluciones reales")
    bullet_list(c, [
        "APIs inventadas o paquetes que no existen en la versión del proyecto.",
        "Mezcla 2D/3D: Rigidbody2D en un proyecto 3D o triggers mal planteados.",
        "Complejidad extra: managers, singletons o sistemas no pedidos.",
        "Inspector roto: cambiar nombres públicos o borrar SerializeField usados por escena.",
        "Tiempo y flujo: Time.timeScale, eventos sin desuscribir o escenas sin restaurar estado.",
    ], 0.9 * inch, 4.25 * inch, 7.7 * inch, size=14, gap=0.28)
    card(c, 9.1 * inch, 2.25 * inch, 3.0 * inch, 2.3 * inch, "Pregunta clave", "¿Qué puede romperse si copio esta respuesta sin revisar?", fill=PINK, stroke=RED)
    c.showPage()

    title(c, "Uso responsable", 21, "Privacidad y autoría", "Usar IA en profesional exige límites claros")
    bullet_list(c, [
        "No subir código propietario, datos personales o material confidencial sin permiso.",
        "No copiar código generado sin entenderlo ni probarlo.",
        "Citar o declarar el uso de IA si el centro o el proyecto lo exige.",
        "Verificar licencias de assets, imágenes o textos externos.",
        "Mantener trazabilidad de decisiones técnicas importantes.",
    ], 0.9 * inch, 4.25 * inch, 7.4 * inch, size=14, gap=0.28)
    card(c, 8.9 * inch, 2.35 * inch, 3.25 * inch, 2.2 * inch, "Mensaje clave", "La responsabilidad del resultado sigue siendo del desarrollador.", fill=LIGHT, stroke=LINE)
    c.showPage()

    title(c, "ChatGPT Go", 22, "Archivos, imágenes y análisis", "Más modos de dar contexto al modelo")
    modes = [("Script .cs", "Subirlo para explicación, revisión o depuración."), ("Captura de consola", "Pedir causa probable y comprobaciones."), ("Captura del Inspector", "Detectar referencias vacías o componentes faltantes."), ("Documento de diseño", "Extraer requisitos, riesgos y tareas.")]
    for i, (h, b) in enumerate(modes):
        x = 0.9 * inch + (i % 2) * 5.75 * inch
        y = 3.75 * inch - (i // 2) * 1.55 * inch
        card(c, x, y, 4.9 * inch, 1.05 * inch, h, b, fill=LIGHT, stroke=LINE)
    draw_wrapped(c, "Regla: no pedir código hasta haber identificado el problema y sus restricciones.", 0.9 * inch, 1.0 * inch, 10.7 * inch, size=16, color=RED)
    c.showPage()

    title(c, "Práctica", 23, "Ejercicio de cierre", "Aplicar el método sin crear todavía el minijuego")
    card(c, 0.9 * inch, 2.55 * inch, 3.55 * inch, 1.8 * inch, "Tarea", "Elegir un caso: mecánica, enemigo, UI, bug, sistema de vidas o checklist QA.", fill=LIGHT, stroke=LINE)
    card(c, 4.9 * inch, 2.55 * inch, 3.55 * inch, 1.8 * inch, "Condición", "Realizar 3 iteraciones: prompt inicial, mejora con restricciones y revisión crítica.", fill=PINK, stroke=RED)
    card(c, 8.9 * inch, 2.55 * inch, 3.55 * inch, 1.8 * inch, "Entrega oral", "Explicar qué cambió entre la primera y la última respuesta, y por qué la versión final es más verificable.", fill=LIGHT, stroke=LINE)
    c.showPage()

    title(c, "Cierre", 24, "Mensajes clave", "Qué debe quedar claro al final de la sesión")
    bullet_list(c, [
        "Un LLM trabaja con tokens, contexto y predicción.",
        "La atención relaciona partes del contexto, pero no garantiza verdad.",
        "ChatGPT Go da más margen, no elimina errores.",
        "En Unity, el Inspector y la escena son parte del problema.",
        "La práctica profesional es pedir, revisar, probar y documentar.",
    ], 0.9 * inch, 4.35 * inch, 8.4 * inch, size=15, gap=0.3)
    card(c, 9.3 * inch, 2.2 * inch, 2.8 * inch, 2.5 * inch, "Siguiente sesión", "Minijuego en Unity con cápsulas, cubos, esferas y planos.", fill=RED, stroke=RED)
    c.setFillColor(colors.white)
    c.setFont("Helvetica-Bold", 16)
    c.drawString(9.55 * inch, 3.75 * inch, "Clase 2")
    c.save()


if __name__ == "__main__":
    build()
    print(OUT)
