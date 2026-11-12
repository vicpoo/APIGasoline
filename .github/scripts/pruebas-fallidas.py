"""
Lee los .trx de una carpeta y publica las pruebas fallidas:
- como anotaciones de GitHub (::error::), que se ven en la página de la corrida y en el PR,
- y como tabla en el resumen del job ($GITHUB_STEP_SUMMARY).
Uso: python3 .github/scripts/pruebas-fallidas.py resultados
"""
import glob
import os
import sys
import xml.etree.ElementTree as ET

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
MAX_ANOTACIONES = 10  # GitHub muestra como máximo 10 errores por paso


def escapar(texto: str) -> str:
    return texto.replace("%", "%25").replace("\r", "").replace("\n", "%0A")


def main() -> None:
    carpeta = sys.argv[1] if len(sys.argv) > 1 else "resultados"
    fallidas = []

    for archivo in glob.glob(os.path.join(carpeta, "**", "*.trx"), recursive=True):
        raiz = ET.parse(archivo).getroot()
        for resultado in raiz.iterfind(".//t:UnitTestResult", NS):
            if resultado.get("outcome") != "Failed":
                continue
            mensaje = resultado.findtext(".//t:ErrorInfo/t:Message", default="", namespaces=NS).strip()
            fallidas.append((resultado.get("testName", "?"), mensaje))

    if not fallidas:
        print("No hay pruebas fallidas en los .trx.")
        return

    for nombre, mensaje in fallidas[:MAX_ANOTACIONES]:
        print(f"::error title={escapar(nombre[:200])}::{escapar(mensaje[:1500])}")

    if len(fallidas) > MAX_ANOTACIONES:
        print(f"::error title=Pruebas fallidas::{len(fallidas)} pruebas fallaron; el resumen del job tiene la lista completa.")

    resumen = os.environ.get("GITHUB_STEP_SUMMARY")
    if resumen:
        with open(resumen, "a", encoding="utf-8") as salida:
            salida.write(f"## {len(fallidas)} prueba(s) fallida(s)\n\n| Prueba | Error |\n|---|---|\n")
            for nombre, mensaje in fallidas:
                linea = mensaje.replace("|", "\\|").replace("\r", "").replace("\n", "<br>")[:1000]
                salida.write(f"| `{nombre}` | {linea} |\n")


if __name__ == "__main__":
    main()
