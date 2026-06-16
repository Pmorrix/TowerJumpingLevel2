# -*- coding: utf-8 -*-
from pathlib import Path
from PIL import Image, ImageChops
from reportlab.lib import colors
from reportlab.lib.pagesizes import landscape
from reportlab.lib.utils import ImageReader
from reportlab.pdfgen import canvas


ROOT = Path(r"C:\TowerJumpingLevel2")
OUT_DIR = ROOT / "outputs" / "clase_practica_pdf"
ASSET_DIR = OUT_DIR / "assets"
PDF_OUT = ROOT / "UMAClase2_PROYECTABLE_UMA_FINAL_CUBE_RUNNER_ARENA_IA.pdf"

W, H = 960, 540
RED = colors.HexColor("#E3192A")
INK = colors.HexColor("#20242A")
MUTED = colors.HexColor("#596273")
LIGHT_GRAY = colors.HexColor("#EEEEEE")
BOX_GRAY = colors.HexColor("#F3F5F7")
BORDER = colors.HexColor("#D8DEE6")
CODE_BG = colors.HexColor("#F4F6F9")


def crop_non_white(src: Path, dst: Path, pad=6):
    im = Image.open(src).convert("RGB")
    bg = Image.new("RGB", im.size, (255, 255, 255))
    diff = ImageChops.difference(im, bg)
    bbox = diff.getbbox()
    if not bbox:
        im.save(dst)
        return
    left, top, right, bottom = bbox
    left = max(0, left - pad)
    top = max(0, top - pad)
    right = min(im.width, right + pad)
    bottom = min(im.height, bottom + pad)
    im.crop((left, top, right, bottom)).save(dst)


LOGO = ASSET_DIR / "logo_header_cropped.png"
ICON = ASSET_DIR / "logo_icon_cropped.png"
if not LOGO.exists():
    crop_non_white(ASSET_DIR / "logo_titulo_07.png", LOGO, pad=8)
if not ICON.exists():
    crop_non_white(ASSET_DIR / "logo_titulo_01.png", ICON, pad=8)


def font(c, name="Helvetica", size=20, color=INK):
    c.setFont(name, size)
    c.setFillColor(color)


def wrap_text(c, text, max_width, font_name, size):
    words = text.split()
    lines = []
    cur = ""
    c.setFont(font_name, size)
    for word in words:
        test = word if not cur else cur + " " + word
        if c.stringWidth(test, font_name, size) <= max_width:
            cur = test
        else:
            if cur:
                lines.append(cur)
            cur = word
    if cur:
        lines.append(cur)
    return lines


def draw_logo(c, x=755, y=469, width=150):
    im = Image.open(LOGO)
    h = width * im.height / im.width
    c.drawImage(ImageReader(str(LOGO)), x, y, width=width, height=h, mask="auto")


def draw_header(c):
    c.setFillColor(RED)
    c.rect(26, 506, 908, 4, stroke=0, fill=1)
    draw_logo(c)


def cover(c, title, subtitle, band, footer):
    c.setFillColor(colors.white)
    c.rect(0, 0, W, H, stroke=0, fill=1)
    c.setFillColor(RED)
    c.rect(0, 0, 20, H, stroke=0, fill=1)
    draw_logo(c, x=690, y=462, width=210)
    font(c, "Helvetica-Bold", 41, INK)
    c.drawString(66, 345, title)
    font(c, "Helvetica", 25, INK)
    lines = subtitle.split("\n")
    yy = 300
    for line in lines:
        c.drawString(66, yy, line)
        yy -= 32
    c.setFillColor(LIGHT_GRAY)
    c.rect(60, 205, 650, 45, stroke=0, fill=1)
    font(c, "Helvetica", 16, MUTED)
    c.drawString(66, 222, band)
    font(c, "Helvetica", 10, INK)
    c.drawString(66, 52, footer)
    c.showPage()


def title_slide(c, title, subtitle=None):
    c.setFillColor(colors.white)
    c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_header(c)
    font(c, "Helvetica-Bold", 34, INK)
    for i, line in enumerate(title.split("\n")):
        c.drawString(58, 355 - i * 44, line)
    if subtitle:
        c.setFillColor(LIGHT_GRAY)
        c.rect(58, 238, 650, 42, stroke=0, fill=1)
        font(c, "Helvetica", 17, MUTED)
        c.drawString(66, 253, subtitle)
    c.showPage()


def bullet_slide(c, title, bullets, sub=None, size=24):
    c.setFillColor(colors.white)
    c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_header(c)
    font(c, "Helvetica-Bold", 32, INK)
    c.drawString(56, 420, title)
    if sub:
        font(c, "Helvetica", 15, MUTED)
        c.drawString(58, 386, sub)
    y = 330 if sub else 342
    for bullet in bullets:
        c.setFillColor(RED)
        c.circle(69, y + 7, 4.2, stroke=0, fill=1)
        font(c, "Helvetica", size, INK)
        lines = wrap_text(c, bullet, 760, "Helvetica", size)
        for j, line in enumerate(lines):
            c.drawString(94, y - j * (size + 8), line)
        y -= (size + 30) * max(1, len(lines))
    c.showPage()


def two_col_slide(c, title, left_title, left_items, right_title, right_items):
    c.setFillColor(colors.white)
    c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_header(c)
    font(c, "Helvetica-Bold", 31, INK)
    c.drawString(56, 420, title)
    for x, head, items in [(56, left_title, left_items), (508, right_title, right_items)]:
        c.setFillColor(BOX_GRAY)
        c.roundRect(x, 100, 385, 250, 4, stroke=0, fill=1)
        c.setStrokeColor(BORDER)
        c.roundRect(x, 100, 385, 250, 4, stroke=1, fill=0)
        font(c, "Helvetica-Bold", 18, RED)
        c.drawString(x + 22, 315, head)
        y = 275
        for item in items:
            c.setFillColor(RED)
            c.circle(x + 25, y + 5, 3.5, stroke=0, fill=1)
            font(c, "Helvetica", 16, INK)
            for j, line in enumerate(wrap_text(c, item, 310, "Helvetica", 16)):
                c.drawString(x + 43, y - j * 20, line)
            y -= 45
    c.showPage()


def box_slide(c, title, box_title, lines, footer=None, mono=False, size=15):
    c.setFillColor(colors.white)
    c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_header(c)
    font(c, "Helvetica-Bold", 30, INK)
    c.drawString(56, 420, title)
    c.setFillColor(CODE_BG if mono else BOX_GRAY)
    c.roundRect(72, 78, 810, 288, 4, stroke=0, fill=1)
    c.setStrokeColor(BORDER)
    c.roundRect(72, 78, 810, 288, 4, stroke=1, fill=0)
    font(c, "Helvetica-Bold", 16, RED if not mono else DARK_BLUE if False else RED)
    c.drawString(96, 335, box_title)
    f = "Courier" if mono else "Helvetica"
    y = 306
    for raw in lines:
        if raw == "":
            y -= size * 0.7
            continue
        prefix = ""
        line = raw
        if raw.startswith("- "):
            prefix = "- "
            line = raw[2:]
        font(c, f, size, INK)
        wrapped = wrap_text(c, line, 725, f, size)
        for j, wline in enumerate(wrapped):
            c.drawString(98, y, (prefix if j == 0 else "  ") + wline)
            y -= size + 4
        if y < 98:
            break
    if footer:
        font(c, "Helvetica", 13, MUTED)
        c.drawString(74, 47, footer)
    c.showPage()


def arena_slide(c):
    c.setFillColor(colors.white)
    c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_header(c)
    font(c, "Helvetica-Bold", 31, INK)
    c.drawString(56, 420, "El terreno de juego")
    font(c, "Helvetica", 15, MUTED)
    c.drawString(58, 386, "Todo se construye con primitivas basicas de Unity.")

    # Arena diagram
    x, y, w, h = 445, 112, 390, 250
    c.setFillColor(colors.HexColor("#F1F2F0"))
    c.rect(x, y, w, h, stroke=0, fill=1)
    c.setStrokeColor(colors.HexColor("#B8BEC8"))
    c.setLineWidth(2)
    c.rect(x, y, w, h, stroke=1, fill=0)
    c.setFillColor(colors.HexColor("#666A73"))
    c.rect(x, y + h - 14, w, 14, stroke=0, fill=1)
    c.rect(x, y, w, 14, stroke=0, fill=1)
    c.rect(x, y, 14, h, stroke=0, fill=1)
    c.rect(x + w - 14, y, 14, h, stroke=0, fill=1)

    # Player capsule
    c.setFillColor(colors.HexColor("#2E74B5"))
    c.roundRect(x + 170, y + 95, 46, 80, 20, stroke=0, fill=1)
    # collectibles
    c.setFillColor(colors.HexColor("#F2C94C"))
    for dx, dy in [(58, 55), (105, 185), (270, 188), (320, 62), (260, 106)]:
        c.circle(x + dx, y + dy, 10, stroke=0, fill=1)
    # obstacles
    c.setFillColor(RED)
    for dx, dy in [(112, 105), (292, 140)]:
        c.rect(x + dx, y + dy, 34, 34, stroke=0, fill=1)

    labels = [
        ("Capsula = Player", 72, 315),
        ("Esferas = coleccionables", 72, 275),
        ("Cubos rojos = obstaculos", 72, 235),
        ("Plano + cubos = arena", 72, 195),
        ("Camara fija", 72, 155),
    ]
    for text, lx, ly in labels:
        c.setFillColor(RED)
        c.circle(lx, ly + 5, 4, stroke=0, fill=1)
        font(c, "Helvetica", 21, INK)
        c.drawString(lx + 24, ly, text)
    c.showPage()


def timeline_slide(c):
    c.setFillColor(colors.white)
    c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_header(c)
    font(c, "Helvetica-Bold", 31, INK)
    c.drawString(56, 420, "Ritmo de la clase")
    steps = [
        ("0:00", "Reto"),
        ("0:15", "Escena"),
        ("1:30", "Scripts"),
        ("2:30", "ChatGPT"),
        ("3:20", "Debug"),
        ("4:10", "Comparar"),
        ("5:00", "Entrega"),
    ]
    x0, x1, y = 90, 850, 255
    c.setStrokeColor(RED)
    c.setLineWidth(4)
    c.line(x0, y, x1, y)
    for idx, (time, label) in enumerate(steps):
        x = x0 + (x1 - x0) * idx / (len(steps) - 1)
        c.setFillColor(colors.white)
        c.setStrokeColor(RED)
        c.circle(x, y, 13, stroke=1, fill=1)
        c.setFillColor(RED)
        c.circle(x, y, 6, stroke=0, fill=1)
        font(c, "Helvetica-Bold", 16, INK)
        c.drawCentredString(x, y - 48, time)
        font(c, "Helvetica", 15, MUTED)
        c.drawCentredString(x, y - 72, label)
    c.showPage()


def table_slide(c, title):
    c.setFillColor(colors.white)
    c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_header(c)
    font(c, "Helvetica-Bold", 31, INK)
    c.drawString(56, 420, title)
    rows = [
        ("Alumno", "IA", "Final"),
        ("Que scripts propone", "Que scripts propone", "Que scripts quedan"),
        ("Responsabilidades", "Suposiciones", "Codigo que compila"),
        ("Inspector previsto", "Restricciones respetadas", "Prueba en Play Mode"),
        ("Errores encontrados", "Errores de la IA", "Correcciones aplicadas"),
    ]
    x, y = 70, 115
    col_w = [260, 260, 260]
    row_h = 52
    for r, row in enumerate(rows):
        yy = y + row_h * (len(rows) - 1 - r)
        for i, text in enumerate(row):
            xx = x + sum(col_w[:i])
            c.setFillColor(LIGHT_GRAY if r == 0 else colors.white)
            c.rect(xx, yy, col_w[i], row_h, stroke=0, fill=1)
            c.setStrokeColor(BORDER)
            c.rect(xx, yy, col_w[i], row_h, stroke=1, fill=0)
            font(c, "Helvetica-Bold" if r == 0 else "Helvetica", 15 if r == 0 else 14, RED if r == 0 else INK)
            for j, line in enumerate(wrap_text(c, text, col_w[i] - 28, "Helvetica-Bold" if r == 0 else "Helvetica", 15 if r == 0 else 14)):
                c.drawString(xx + 14, yy + 31 - j * 17, line)
    c.showPage()


def build():
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    c = canvas.Canvas(str(PDF_OUT), pagesize=(W, H))
    c.setTitle("Cube Runner Arena con IA - Clase practica")

    cover(
        c,
        "Cube Runner Arena con IA",
        "Clase práctica de desarrollo de videojuegos\ncon Unity 3D y C#",
        "Clase 2: ChatGPT gratuito, sin suscripción; scripts y depuración",
        "Máster en Creación de Videojuegos - Universidad de Málaga",
    )

    bullet_slide(c, "Qué haremos hoy", [
        "Preparar una arena jugable en Unity.",
        "Pensar los scripts antes de usar IA.",
        "Pedir a ChatGPT una arquitectura mínima.",
        "Integrar, probar y comparar soluciones.",
    ])
    bullet_slide(c, "Objetivo de la práctica", [
        "No buscamos un juego grande.",
        "Buscamos un ciclo completo: escena, scripts, prueba y revisión.",
        "La IA acelera, pero el criterio técnico sigue siendo nuestro.",
    ], size=23)
    bullet_slide(c, "Resultado esperado", [
        "Escena jugable.",
        "Cuatro scripts principales funcionando.",
        "Victoria, Game Over y reinicio.",
        "Comparación entre solución humana y solución IA.",
    ])
    title_slide(c, "La regla de la clase", "ChatGPT escribe código, pero no ve tu Inspector.")
    bullet_slide(c, "El minijuego", [
        "Player: una cápsula.",
        "Coleccionables: esferas.",
        "Obstáculos: cubos rojos.",
        "Suelo y paredes: primitivas básicas.",
        "Cámara fija y UI sencilla.",
    ])
    arena_slide(c)
    bullet_slide(c, "Condiciones de juego", [
        "Moverse por el plano XZ.",
        "Recoger todas las esferas para ganar.",
        "Tocar un obstáculo provoca Game Over.",
        "Si el tiempo llega a 0, se pierde.",
        "Con R se reinicia la escena.",
    ], size=22)
    timeline_slide(c)
    title_slide(c, "Bloque 1\nPreparar el terreno", "Primero escena. Después código.")
    bullet_slide(c, "Montaje mínimo", [
        "Plane como suelo.",
        "Capsule como Player con Rigidbody.",
        "Cubos como paredes.",
        "Esferas como coleccionables.",
        "Cubos rojos como obstáculos.",
        "Canvas con ScoreText, TimeText y MessageText.",
    ], size=21)
    box_slide(c, "Checklist de escena", "Antes de seguir", [
        "Player tiene Rigidbody.",
        "Player tiene Collider.",
        "Player tiene tag Player.",
        "Coleccionables tienen Collider con Is Trigger.",
        "Obstáculos tienen Collider normal.",
        "Cámara ve toda la arena.",
        "GameManager está en escena.",
    ])
    bullet_slide(c, "Errores típicos", [
        "Falta el tag Player.",
        "El trigger está en el objeto equivocado.",
        "La UI existe, pero no está asignada.",
        "La cámara sigue al Player cuando debería ser fija.",
        "Hay código correcto, pero escena incorrecta.",
    ], size=22)
    title_slide(c, "En Unity,\nel Inspector también es código", "Las referencias de escena forman parte del comportamiento.")

    title_slide(c, "Bloque 2\nDiseñar scripts", "Antes de pedir ayuda a la IA.")
    box_slide(c, "Preguntas guía", "El alumnado debe responder", [
        "¿Qué script mueve al Player?",
        "¿Qué script detecta una esfera recogida?",
        "¿Quién guarda la puntuación?",
        "¿Quién controla el tiempo?",
        "¿Quién decide victoria o derrota?",
        "¿Qué referencias van por Inspector?",
    ])
    bullet_slide(c, "Arquitectura mínima", [
        "PlayerMovement.cs",
        "Collectible.cs",
        "GameManager.cs",
        "MovingObstacle.cs",
    ], sub="Cuatro scripts bastan para esta práctica.", size=28)
    two_col_slide(c, "Responsabilidades 1/2", "PlayerMovement.cs", [
        "Lee teclado.",
        "Mueve con Rigidbody.",
        "Solo plano XZ.",
        "No decide victoria ni derrota.",
    ], "Collectible.cs", [
        "Usa OnTriggerEnter.",
        "Comprueba tag Player.",
        "Avisa al GameManager.",
        "Desactiva la esfera.",
    ])
    two_col_slide(c, "Responsabilidades 2/2", "GameManager.cs", [
        "Score y tiempo.",
        "Victoria y Game Over.",
        "Actualiza UI.",
        "Desactiva Player al perder.",
    ], "MovingObstacle.cs", [
        "Mueve cubo rojo.",
        "Usa colisión normal.",
        "Detecta al Player.",
        "Llama a LoseGame().",
    ])

    title_slide(c, "Bloque 3\nChatGPT como arquitecto", "Primero pedir estructura. Después pedir código.")
    box_slide(c, "Prompt inicial recomendado", "Contexto", [
        "Actúa como desarrollador Unity C# senior.",
        "",
        "Estoy creando un minijuego 3D con primitivas básicas:",
        "- cápsula como Player",
        "- esferas como coleccionables",
        "- cubos rojos como obstáculos",
        "- plano como suelo",
        "- paredes hechas con cubos",
        "- cámara fija",
        "- UI con ScoreText, TimeText y MessageText",
    ], mono=True, size=13)
    box_slide(c, "Prompt inicial recomendado", "Funcionamiento deseado", [
        "- el jugador se mueve con teclado por el plano XZ",
        "- recoge esferas para sumar puntos",
        "- al recoger todas las esferas gana",
        "- si toca un obstáculo pierde",
        "- si el tiempo llega a 0 pierde",
        "- al hacer Game Over, el Player debe desactivarse",
        "- se puede reiniciar la escena con R",
    ], mono=True, size=14)
    box_slide(c, "Prompt inicial recomendado", "Restricciones y salida", [
        "- Unity 3D",
        "- C#",
        "- no usar nuevo Input System",
        "- no usar Cinemachine",
        "- no usar DOTween",
        "- mantenerlo simple",
        "- usar Rigidbody para el Player",
        "- explicar qué revisar en Inspector",
        "",
        "Primero dime qué scripts son necesarios.",
        "Después dame el código completo.",
    ], mono=True, size=13)
    bullet_slide(c, "Qué evaluar de la IA", [
        "¿Propone scripts de más?",
        "¿Respeta la cámara fija?",
        "¿Usa Unity 3D y no 2D?",
        "¿Explica el Inspector?",
        "¿El código se entiende?",
        "¿Hay algo que no hemos pedido?",
    ], size=22)

    title_slide(c, "Bloque 4\nIntegración y debugging", "Probar en Play Mode antes de seguir.")
    box_slide(c, "Checklist de funcionamiento", "Play Mode", [
        "Player se mueve.",
        "Score sube al recoger esferas.",
        "Las esferas desaparecen.",
        "El tiempo baja.",
        "Al recoger todo aparece victoria.",
        "Al tocar obstáculo aparece Game Over.",
        "En Game Over el Player se desactiva.",
        "R reinicia la escena.",
    ])
    box_slide(c, "Error frecuente", "Input System", [
        "Mensaje típico:",
        "You are trying to read Input using the UnityEngine.Input class...",
        "",
        "Causa probable:",
        "El proyecto está configurado solo para el nuevo Input System.",
        "",
        "Solución para esta práctica:",
        "Project Settings > Player > Active Input Handling > Both",
    ], mono=True, size=12)
    two_col_slide(c, "Triggers y colisiones", "Coleccionables", [
        "Collider con Is Trigger.",
        "Player con Rigidbody.",
        "OnTriggerEnter.",
        "Tag Player correcto.",
    ], "Obstáculos", [
        "Collider normal.",
        "No marcar Is Trigger.",
        "OnCollisionEnter.",
        "Llamada a GameManager.",
    ])
    bullet_slide(c, "NullReferenceException", [
        "No significa necesariamente que el algoritmo esté mal.",
        "Suele faltar una referencia en Inspector.",
        "Revisar textos UI, Player y GameManager.",
        "Añadir null-checks simples cuando sea razonable.",
    ], size=22)
    box_slide(c, "Prompt de depuración", "Usarlo cuando Unity dé error", [
        "Tengo este error en Unity:",
        "[pegar error completo]",
        "",
        "Este es el script:",
        "[pegar script]",
        "",
        "Contexto de la escena:",
        "- GameObject donde está el script",
        "- componentes del Player",
        "- tag del Player",
        "- colliders y Rigidbody",
        "- referencias asignadas en Inspector",
        "",
        "Dime causa probable y cambio mínimo.",
    ], mono=True, size=12)

    title_slide(c, "Bloque 5\nComparar soluciones", "La entrega no es solo el juego: también la revisión.")
    table_slide(c, "Comparación técnica")
    bullet_slide(c, "Preguntas de cierre", [
        "¿Qué hizo mejor el alumno?",
        "¿Qué hizo mejor ChatGPT?",
        "¿Qué asumió mal la IA?",
        "¿Qué dependía del Inspector?",
        "¿Qué prompt mejoró más el resultado?",
    ], size=23)
    box_slide(c, "Entrega mínima", "Cada grupo entrega", [
        "Escena jugable.",
        "Scripts funcionando.",
        "Prompt usado con IA.",
        "Lista de scripts propuesta por el alumno.",
        "Lista de scripts propuesta por la IA.",
        "Comparación breve entre ambas.",
        "Lista de errores encontrados.",
    ])
    title_slide(c, "Cierre conceptual", "La respuesta correcta vive entre código, escena, físicas, UI e Inspector.")
    bullet_slide(c, "Idea final", [
        "La IA no solo escribe código.",
        "También propone una arquitectura.",
        "Nuestro trabajo es comprobar si esa arquitectura es mínima, clara y compatible con la escena.",
    ], size=24)

    c.save()
    print(PDF_OUT)


if __name__ == "__main__":
    build()
