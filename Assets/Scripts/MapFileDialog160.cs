using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Selector de fitxers del SISTEMA per exportar/importar mapes (.canicamap), sense paquets externs:
//  - Windows: diàleg de Windows Forms a través de PowerShell (ve amb Windows).
//  - Linux: «zenity» (ve amb Linux Mint i Ubuntu) i, si no n'hi ha, «kdialog».
// No bloqueja el joc mentre el diàleg és obert. Si no n'hi ha cap, el Creador continua demanant la ruta escrita.
public static class MapFileDialog160
{
    public static bool Available
    {
        get
        {
            if (Application.isMobilePlatform) return false;
            var p = Application.platform;
            return p == RuntimePlatform.WindowsPlayer || p == RuntimePlatform.WindowsEditor ||
                   p == RuntimePlatform.LinuxPlayer || p == RuntimePlatform.LinuxEditor;
        }
    }

    // result: la ruta triada; "" si l'usuari cancel·la; null si no s'ha pogut obrir cap selector.
    public static IEnumerator Run(bool save, string mapName, Action<string> result)
    {
        string defaultName = SafeName(mapName) + ".canicamap";
        foreach (var start in Candidates(save, defaultName))
        {
            System.Diagnostics.Process process = null;
            try { process = System.Diagnostics.Process.Start(start); }
            catch (Exception) { process = null; } // l'eina no existeix en este sistema: es prova la següent
            if (process == null) continue;
            while (!process.HasExited) yield return null;
            string output = "";
            try { output = process.StandardOutput.ReadToEnd(); } catch (Exception) { }
            int code = 0;
            try { code = process.ExitCode; } catch (Exception) { code = -1; }
            process.Dispose();
            output = output.Trim('﻿', ' ', '\r', '\n', '\t');
            int lineEnd = output.IndexOfAny(new[] { '\r', '\n' });
            if (lineEnd >= 0) output = output.Substring(0, lineEnd);
            if (code == 0 && output.Length > 0) { result(output); yield break; }   // ha triat un fitxer
            if (code == 0 || code == 1) { result(""); yield break; }                // ha cancel·lat (Windows: eixida buida; zenity/kdialog: codi 1)
            // qualsevol altre codi vol dir que l'eina ha fallat (p. ex. sense pantalla): es prova la següent
        }
        result(null);
    }

    static IEnumerable<System.Diagnostics.ProcessStartInfo> Candidates(bool save, string defaultName)
    {
        string title = save ? "Exportar mapa" : "Importar mapa";
        string home = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrEmpty(home)) home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        home = home.Replace("\"", "").Replace("'", "''");
        bool windows = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;
        if (windows)
        {
            // Un propietari invisible i «TopMost» fa que el diàleg isca davant del joc i no darrere.
            string script =
                "[Console]::OutputEncoding=[Text.Encoding]::UTF8; Add-Type -AssemblyName System.Windows.Forms; " +
                "$f=New-Object System.Windows.Forms.Form; $f.TopMost=$true; " +
                (save ? "$d=New-Object System.Windows.Forms.SaveFileDialog; $d.OverwritePrompt=$true; $d.FileName='" + defaultName + "'; $d.DefaultExt='canicamap'; "
                      : "$d=New-Object System.Windows.Forms.OpenFileDialog; $d.CheckFileExists=$true; ") +
                "$d.Title='" + title + "'; $d.InitialDirectory='" + home + "'; " +
                "$d.Filter='Mapa de Canica (*.canicamap)|*.canicamap|Tots els fitxers (*.*)|*.*'; " +
                "if($d.ShowDialog($f) -eq 'OK'){[Console]::Out.Write($d.FileName)}";
            string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
            yield return Make("powershell.exe", "-NoProfile -NonInteractive -STA -ExecutionPolicy Bypass -EncodedCommand " + encoded);
            yield break;
        }
        string dir = home.Replace("''", "'");
        string suggested = save ? dir + "/" + defaultName : dir + "/";
        yield return Make("zenity", "--file-selection " + (save ? "--save --confirm-overwrite " : "") +
            "--title=\"" + title + "\" --filename=\"" + suggested + "\" --file-filter=\"Mapes de Canica | *.canicamap\" --file-filter=\"Tots els fitxers | *\"");
        yield return Make("kdialog", (save ? "--getsavefilename" : "--getopenfilename") + " \"" + suggested + "\" \"*.canicamap|Mapes de Canica\" --title \"" + title + "\"");
    }

    static System.Diagnostics.ProcessStartInfo Make(string file, string arguments) => new System.Diagnostics.ProcessStartInfo(file, arguments)
    {
        UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, StandardOutputEncoding = Encoding.UTF8
    };

    // Només lletres, números, espai, guió, guió baix i punt: així el nom es pot posar dins d'una ordre sense riscos.
    internal static string SafeName(string name)
    {
        var builder = new StringBuilder();
        foreach (char c in name ?? "")
            if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_' || c == '.') builder.Append(c);
        string safe = builder.ToString().Trim();
        return safe.Length == 0 ? "Mapa" : safe;
    }
}
