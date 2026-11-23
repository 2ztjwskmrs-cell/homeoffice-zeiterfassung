"""
Erzeugt zusätzliche Excel-Testdateien für homeoffice.exe auf Basis der Musterdatei
../Homeoffice-Stundenzettel-Muster.xlsx und berechnet die erwarteten Ergebnisse
(/imp, /count, /max, /time) in erwartung.md.

Aufruf (aus diesem Ordner):  python erzeuge_testdaten.py
Benötigt: openpyxl. Zufallswerte mit festem Seed -> bei jedem Lauf identische Dateien.
"""
import datetime as dt
import glob
import os
import random
import re

import openpyxl

HIER = os.path.dirname(os.path.abspath(__file__))
MUSTER = os.path.join(HIER, "..", "Homeoffice-Stundenzettel-Muster.xlsx")

TAETIGKEITEN = ["Scripterstellung", "Online-Meeting", "Hotlineunterstützung",
                "Switchkonfiguration", "Netzwerkdokumentation", "Serverwartung",
                "Ticketbearbeitung", "Backup-Kontrolle", "Firewall-Regeln pflegen",
                "Clientinstallation"]
VORGESETZTE = ["Hansen", "Müller", "Petersen", "Franzen", "Jensen"]

rnd = random.Random(2026)


# ----------------------------------------------------------------------------
# Hilfsfunktionen
# ----------------------------------------------------------------------------
def hm(minuten):
    return f"{minuten // 60}:{minuten % 60:02d}"


def zeit_text(beginn, ende, stil):
    """Arbeitszeit in verschiedenen Schreibweisen, wie sie in echten Dateien vorkommen."""
    def u(m, s):
        h, mi = divmod(m, 60)
        if s == "kurz" and mi == 0:
            return str(h)                 # "8"
        if s == "punkt":
            return f"{h}.{mi:02d}"        # "8.30"
        if s == "null":
            return f"{h:02d}:{mi:02d}"    # "08:30"
        return f"{h}:{mi:02d}"            # "8:30"
    sep = {"kurz": "-", "punkt": "-", "null": " - ", "normal": " - "}[stil]
    return u(beginn, stil) + sep + u(ende, stil)


def zufallstage(start, anzahl_tage, wahrscheinlichkeit):
    tage = []
    d = start
    while len(tage) < anzahl_tage:
        if d.weekday() < 5 and rnd.random() < wahrscheinlichkeit:
            tage.append(d)
        d += dt.timedelta(days=1)
    return tage


def zufallszeile(datum, min_dauer, max_dauer, stile=("normal", "null", "kurz", "punkt")):
    dauer = rnd.randrange(min_dauer, max_dauer + 1, 15)
    beginn = rnd.randrange(6 * 60, 20 * 60 - dauer + 1, 15)
    ende = beginn + dauer
    stil = rnd.choice(stile)
    vertauscht = rnd.random() < 0.15
    text = zeit_text(ende, beginn, stil) if vertauscht else zeit_text(beginn, ende, stil)
    return [datum, rnd.choice(TAETIGKEITEN), text, hm(dauer), rnd.choice(VORGESETZTE)]


# ----------------------------------------------------------------------------
# Testdateien: (Dateiname, Nachname, Vorname, Zeilen, Beschreibung)
# Zeile = [Datum, Tätigkeit, Arbeitszeit, Zeit (h), Vorgesetzter]
# Datum als datetime = echtes Excel-Datum, als str = Text "TT.MM.JJJJ"
# ----------------------------------------------------------------------------
D = dt.datetime
dateien = []

# 1) Minimalfall: genau eine Zeile
dateien.append(("homeoffice_Becker_Lena_1.xlsx", "Becker", "Lena", [
    [D(2024, 10, 1), "Ticketbearbeitung", "9:00 - 9:15", "0:15", "Jensen"],
], "1 Zeile, kürzester Eintrag (0:15)"))

# 2) Viele Einträge über zwei Monate, verteilt auf zwei Dateien, mit Überschneidung
nowak = [zufallszeile(d, 120, 480) for d in zufallstage(D(2024, 10, 1), 30, 0.9)]
dateien.append(("homeoffice_Nowak_Piotr_1.xlsx", "Nowak", "Piotr", nowak[:20],
                "20 Zeilen Okt./Nov., gemischte Schreibweisen"))
dateien.append(("homeoffice_Nowak_Piotr_2.xlsx", "Nowak", "Piotr", nowak[15:],
                "15 Zeilen, die ersten 5 identisch mit Datei 1 -> 5 Doubletten"))

# 3) Lange Arbeitstage, Bindestrich im Vornamen
hoffmann = [zufallszeile(d, 600, 720, ("normal", "null")) for d in zufallstage(D(2024, 9, 16), 8, 1.0)]
dateien.append(("homeoffice_Hoffmann_Klaus-Dieter_1.xlsx", "Hoffmann", "Klaus-Dieter", hoffmann,
                "8 lange Tage (10:00-12:00 h), Bindestrich im Vornamen"))

# 4) Viele kurze Einträge, mehrere pro Tag
weber = []
for d in zufallstage(D(2024, 9, 2), 6, 0.8):
    for _ in range(rnd.randint(3, 5)):
        weber.append(zufallszeile(d, 15, 60))
dateien.append(("homeoffice_Weber_Sophie_1.xlsx", "Weber", "Sophie", weber,
                "Viele kurze Einträge (0:15-1:00), mehrere pro Tag"))

# 5) Neuer Maximalwert für /max, vertauschte Zeiten, Uhrzeit ohne Minuten
dateien.append(("homeoffice_Krüger_Tim_1.xlsx", "Krüger", "Tim", [
    [D(2024, 9, 23), "Serverwartung", "6:00 - 20:30", "14:30", "Franzen"],
    [D(2024, 9, 24), "Serverwartung", "18-8", "10:00", "Franzen"],
    [D(2024, 9, 25), "Backup-Kontrolle", "7.45-9", "1:15", "Jensen"],
    [D(2024, 9, 26), "Online-Meeting", "14:00-14:30", "0:30", "Hansen"],
], "Längster Eintrag 14:30 (/max), '18-8' vertauscht ohne Minuten, '7.45-9'"))

# 6) Umlaute/Sonderzeichen im Namen, Datum als Text, falsche Spalte D
dateien.append(("homeoffice_Özdemir_Ayşe_1.xlsx", "Özdemir", "Ayşe", [
    ["07.10.2024", "Clientinstallation", "8:00 - 11:30", "3:30", "Müller"],
    ["08.10.2024", "Clientinstallation", "12:00 - 16:45", "1:00", "Müller"],
    [D(2024, 10, 9), "Firewall-Regeln pflegen", "13:15 - 8:00", "5:15", "Petersen"],
    [D(2024, 10, 9), "Firewall-Regeln pflegen", "8:00 - 13:15", "9:99", "Petersen"],
], "ş im Vornamen, Datum als Text, Spalte D falsch, vertauscht + normal = 1 Doublette"))

# 7) Leerzeichen im Nachnamen, Leerzeichen um die Werte
dateien.append(("homeoffice-van der Berg_Jan_1.xlsx", "van der Berg", "Jan", [
    [D(2024, 11, 4), "  Netzwerkdokumentation ", "  9:00 -   12:00 ", "3:00", " Hansen  "],
    [D(2024, 11, 4), "Netzwerkdokumentation", "9:00 - 12:00", "3:00", "Hansen"],
    [D(2024, 11, 5), "Hotlineunterstützung", "13:00 - 17:00", "4:00", "Jensen"],
], "Leerzeichen im Namen und um Werte, Zeile 1/2 nach Trim identisch -> 1 Doublette"))

# 8) Mitarbeiter über drei Monatsdateien, gleicher Tag mit unterschiedlichem Vorgesetzten
fischer_sep = [zufallszeile(d, 180, 480) for d in zufallstage(D(2024, 9, 2), 12, 0.7)]
fischer_okt = [zufallszeile(d, 180, 480) for d in zufallstage(D(2024, 10, 1), 15, 0.8)]
fischer_nov = [zufallszeile(d, 180, 480) for d in zufallstage(D(2024, 11, 1), 10, 0.6)]
fischer_nov.append([D(2024, 11, 29), "Online-Meeting", "10:00 - 11:00", "1:00", "Hansen"])
fischer_nov.append([D(2024, 11, 29), "Online-Meeting", "10:00 - 11:00", "1:00", "Müller"])
dateien.append(("homeoffice_Fischer_Paul_2024-09.xlsx", "Fischer", "Paul", fischer_sep, "September"))
dateien.append(("homeoffice_Fischer_Paul_2024-10.xlsx", "Fischer", "Paul", fischer_okt, "Oktober"))
dateien.append(("homeoffice_Fischer_Paul_2024-11.xlsx", "Fischer", "Paul", fischer_nov,
                "November, letzte 2 Zeilen nur Vorgesetzter verschieden -> keine Doublette"))

# 9) Namensvetter: gleicher Nachname wie bestehender Testdatensatz, anderer Vorname
dateien.append(("homeoffice_Meier_Jonas_1.xlsx", "Meier", "Jonas", [
    [D(2024, 9, 1), "Scripterstellung", "8:00 - 12:00", "4:00", "Hansen"],
    [D(2024, 9, 6), "Switchkonfiguration", "7:30 - 13:00", "5:30", "Müller"],
], "Gleiche Zeilen wie Meier, Anna -> anderer Mitarbeiter, keine Doublette"))

# 10) Kopf vorhanden, aber keine Datenzeilen
dateien.append(("homeoffice_Schulz_Tom_1.xlsx", "Schulz", "Tom", [],
                "Leere Datei (nur Kopf) -> 0 Zeilen, kein Fehler"))

# 11) Defekte Datei: Arbeitszeit nicht lesbar -> komplette Datei wird übersprungen
dateien.append(("homeoffice_Wagner_Eva_1.xlsx", "Wagner", "Eva", [
    [D(2024, 10, 14), "Ticketbearbeitung", "8:00 - 12:00", "4:00", "Jensen"],
    [D(2024, 10, 15), "Ticketbearbeitung", "ganztags", "8:00", "Jensen"],
], "Fehlerfall: 'ganztags' nicht lesbar -> Datei übersprungen, nichts importiert"))

# 12) Dateien, die NICHT importiert werden dürfen
dateien.append(("homeofficeLange_Ute_1.xlsx", "Lange", "Ute", [
    [D(2024, 10, 1), "Scripterstellung", "8:00 - 20:00", "12:00", "Hansen"],
], "Kein Trennzeichen nach 'homeoffice' -> wird ignoriert"))
dateien.append(("stunden_homeoffice_Lange_Ute_2.xlsx", "Lange", "Ute", [
    [D(2024, 10, 2), "Scripterstellung", "8:00 - 20:00", "12:00", "Hansen"],
], "Beginnt nicht mit 'homeoffice' -> wird ignoriert"))


# ----------------------------------------------------------------------------
# Dateien schreiben (Musterdatei als Vorlage, damit Kopf und Formate passen)
# ----------------------------------------------------------------------------
def schreiben(pfad, nachname, vorname, zeilen):
    wb = openpyxl.load_workbook(MUSTER)
    ws = wb.worksheets[0]
    ws.delete_rows(7, ws.max_row)
    ws["B3"] = nachname
    ws["B4"] = vorname
    for i, (datum, taetigkeit, zeit, stunden, vorgesetzter) in enumerate(zeilen):
        r = 7 + i
        ws.cell(r, 1, datum).number_format = "DD.MM.YYYY" if isinstance(datum, dt.datetime) else "@"
        ws.cell(r, 2, taetigkeit)
        ws.cell(r, 3, zeit).number_format = "@"
        ws.cell(r, 4, stunden).number_format = "@"
        ws.cell(r, 5, vorgesetzter).number_format = "@"
    wb.save(pfad)


for alt in glob.glob(os.path.join(HIER, "*.xlsx")):
    os.remove(alt)
for name, nachname, vorname, zeilen, _ in dateien:
    schreiben(os.path.join(HIER, name), nachname, vorname, zeilen)


# ----------------------------------------------------------------------------
# Erwartete Ergebnisse nachrechnen (gleiche Regeln wie homeoffice.exe)
# ----------------------------------------------------------------------------
def uhrzeit(t):
    t = t.strip()
    m = re.fullmatch(r"(\d{1,2})(?:[:.](\d{2}))?", t)
    if not m:
        raise ValueError(t)
    return int(m.group(1)) * 60 + int(m.group(2) or 0)


def importiert(name):
    n = name.lower()
    return n.startswith("homeoffice_") or n.startswith("homeoffice-")


def simulieren(ordner):
    db = {}            # Schlüssel -> Dauer
    gelesen = neu = doppelt = defekt = 0
    pfade = sorted(p for p in glob.glob(os.path.join(ordner, "*.xlsx")) if importiert(os.path.basename(p)))
    for pfad in pfade:
        ws = openpyxl.load_workbook(pfad).worksheets[0]
        nach, vor = str(ws["B3"].value).strip(), str(ws["B4"].value).strip()
        try:
            eintraege = []
            r = 7
            while ws.cell(r, 1).value is not None:
                d = ws.cell(r, 1).value
                d = d.date() if isinstance(d, dt.datetime) else dt.datetime.strptime(d.strip(), "%d.%m.%Y").date()
                teile = str(ws.cell(r, 3).value).split("-")
                if len(teile) != 2:
                    raise ValueError("Arbeitszeit")
                b, e = sorted((uhrzeit(teile[0]), uhrzeit(teile[1])))
                key = (nach, vor, d, b, e, str(ws.cell(r, 2).value).strip(), str(ws.cell(r, 5).value).strip())
                eintraege.append(key)
                r += 1
        except ValueError:
            defekt += 1
            continue
        for key in eintraege:
            gelesen += 1
            if key in db:
                doppelt += 1
            else:
                db[key] = key[4] - key[3]
                neu += 1
    return len(pfade), gelesen, neu, doppelt, defekt, db


def bericht(titel, ordner):
    n, gelesen, neu, doppelt, defekt, db = simulieren(ordner)
    maxkey = max(db, key=lambda k: db[k])
    zeitsumme = {}
    anzahl = {}
    for k, dauer in db.items():
        zeitsumme[(k[0], k[1])] = zeitsumme.get((k[0], k[1]), 0) + dauer
        anzahl[(k[0], k[1])] = anzahl.get((k[0], k[1]), 0) + 1
    zeilen = [f"## {titel}", "",
              "| Prüfpunkt | Erwartet |", "|---|---|",
              f"| `/imp` Dateien gefunden | {n} |",
              f"| `/imp` Zeilen gelesen | {gelesen} |",
              f"| `/imp` Neu in Datenbank | {neu} |",
              f"| `/imp` Doubletten | {doppelt} |",
              f"| `/imp` Fehlerhafte Dateien | {defekt} |",
              f"| `/imp` erneut | {gelesen} gelesen, 0 neu, {gelesen} Doubletten |",
              f"| `/count` | {len(db)} |",
              f"| `/max` | {hm(db[maxkey])} h am {maxkey[2]:%d.%m.%Y} ({maxkey[0]}, "
              f"{maxkey[3] // 60:02d}:{maxkey[3] % 60:02d} - {maxkey[4] // 60:02d}:{maxkey[4] % 60:02d}) |",
              "", "`/time`:", "", "| Mitarbeiter | Einträge | Gesamtzeit |", "|---|---:|---:|"]
    for (nach, vor) in sorted(zeitsumme):
        zeilen.append(f"| {nach}, {vor} | {anzahl[(nach, vor)]} | {hm(zeitsumme[(nach, vor)])} |")
    return zeilen


md = ["# Erweiterte Testdaten – erwartete Ergebnisse", "",
      "Automatisch erzeugt von `erzeuge_testdaten.py`. Datenbank vor jedem Szenario löschen.", "",
      "## Dateien", "", "| Datei | Mitarbeiter | Zeilen | Prüft |", "|---|---|---:|---|"]
for name, nachname, vorname, zeilen, info in dateien:
    md.append(f"| `{name}` | {nachname}, {vorname} | {len(zeilen)} | {info} |")
md.append("")
md += bericht("Szenario A: nur dieser Ordner (`/imp testdaten\\erweitert`)", HIER)
md.append("")

# Szenario B: zusammen mit den bisherigen Testdateien in einem Ordner
import shutil
import tempfile
with tempfile.TemporaryDirectory() as tmp:
    for p in glob.glob(os.path.join(HIER, "..", "*.xlsx")) + glob.glob(os.path.join(HIER, "*.xlsx")):
        shutil.copy(p, tmp)
    md += bericht("Szenario B: alle Dateien aus `testdaten` und `testdaten\\erweitert` in einem Ordner", tmp)

with open(os.path.join(HIER, "erwartung.md"), "w", encoding="utf-8") as f:
    f.write("\n".join(md) + "\n")
print("\n".join(md))
