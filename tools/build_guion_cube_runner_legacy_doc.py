# -*- coding: utf-8 -*-
from html import escape
from pathlib import Path


SRC = Path(r"C:\TowerJumpingLevel2\Guion_Clase_Practica_Cube_Runner_Arena_IA.md")
OUT = Path(r"C:\TowerJumpingLevel2\Guion_Clase_Practica_Cube_Runner_Arena_IA_COMPLETO.doc")


STYLE = """
<style>
@page WordSection1 { size: 8.5in 11.0in; margin: 1.0in 1.0in 1.0in 1.0in; }
div.WordSection1 { page: WordSection1; }
body { font-family: Calibri, Arial, sans-serif; color: #1F2933; }
h1 { color: #2E74B5; font-size: 18pt; margin-top: 22pt; margin-bottom: 8pt; }
h2 { color: #2E74B5; font-size: 14pt; margin-top: 16pt; margin-bottom: 7pt; }
h3 { color: #1F4D78; font-size: 12.5pt; margin-top: 12pt; margin-bottom: 6pt; }
p { font-size: 11pt; line-height: 1.25; margin: 0 0 7pt 0; }
ul { margin-top: 0; margin-bottom: 8pt; }
li { font-size: 11pt; line-height: 1.25; margin-bottom: 3pt; }
.cover-title { font-size: 30pt; font-weight: 700; color: #1F2933; margin-top: 80pt; margin-bottom: 8pt; }
.cover-subtitle { font-size: 18pt; color: #1F2933; margin-bottom: 5pt; }
.cover-meta { font-size: 12pt; color: #596579; margin-bottom: 24pt; }
.box { border: 1px solid #C9D3E0; background: #F4F6F9; padding: 10pt 12pt; margin: 8pt 0 12pt 0; }
.box pre { white-space: pre-wrap; font-family: Consolas, monospace; font-size: 9.5pt; margin: 0; line-height: 1.2; }
.quote { border: 1px solid #C9D3E0; background: #F7F9FC; padding: 10pt 12pt; margin: 8pt 0 12pt 0; font-style: italic; }
.rule { height: 6pt; border-left: 18pt solid #D71920; margin-top: 10pt; margin-bottom: 20pt; }
.small { font-size: 9pt; color: #596579; }
</style>
"""


def convert_inline(text):
    text = escape(text)
    text = text.replace("**", "")
    text = text.replace("`", "")
    return text


def markdown_to_html(md):
    html = []
    in_code = False
    code_lines = []
    in_list = False

    def close_list():
        nonlocal in_list
        if in_list:
            html.append("</ul>")
            in_list = False

    for raw in md.splitlines():
        line = raw.rstrip()
        stripped = line.strip()

        if stripped.startswith("```"):
            if in_code:
                html.append('<div class="box"><pre>')
                html.append(escape("\n".join(code_lines)))
                html.append("</pre></div>")
                code_lines = []
                in_code = False
            else:
                close_list()
                in_code = True
                code_lines = []
            continue

        if in_code:
            code_lines.append(line)
            continue

        if stripped == "":
            close_list()
            continue

        if stripped.startswith("# "):
            close_list()
            html.append(f"<h1>{convert_inline(stripped[2:])}</h1>")
        elif stripped.startswith("## "):
            close_list()
            html.append(f"<h2>{convert_inline(stripped[3:])}</h2>")
        elif stripped.startswith("### "):
            close_list()
            html.append(f"<h3>{convert_inline(stripped[4:])}</h3>")
        elif stripped.startswith("- "):
            if not in_list:
                html.append("<ul>")
                in_list = True
            html.append(f"<li>{convert_inline(stripped[2:])}</li>")
        elif stripped.startswith("> "):
            close_list()
            html.append(f'<div class="quote">{convert_inline(stripped[2:])}</div>')
        else:
            close_list()
            html.append(f"<p>{convert_inline(stripped)}</p>")

    close_list()
    if in_code:
        html.append('<div class="box"><pre>')
        html.append(escape("\n".join(code_lines)))
        html.append("</pre></div>")
    return "\n".join(html)


def main():
    md = SRC.read_text(encoding="utf-8")
    body = markdown_to_html(md)
    html = f"""<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<title>Guion clase practica Cube Runner Arena con IA</title>
{STYLE}
</head>
<body>
<div class="WordSection1">
<p class="small" align="right">Master en Creacion de Videojuegos - Universidad de Malaga</p>
<div class="rule"></div>
<p class="cover-title">Cube Runner Arena</p>
<p class="cover-subtitle">Guion completo de clase practica con IA</p>
<p class="cover-meta">Uso de ChatGPT gratuito en Unity 3D y C#</p>
<div class="box">
<p><strong>Documento editable para Word.</strong></p>
<p>Incluye el guion completo, prompts, checklists, textos para el profesor y cuadros destacados.</p>
</div>
<br clear="all" style="page-break-before: always;">
{body}
</div>
</body>
</html>
"""
    OUT.write_text(html, encoding="utf-8")
    print(OUT)


if __name__ == "__main__":
    main()
