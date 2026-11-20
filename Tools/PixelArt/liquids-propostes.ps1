# Propostes de tiles de lava i acid amb bombolles (FIX-025). Nomes genera una imatge de previsualitzacio;
# no toca el joc. Cada tile es de 16x16 i el dibuix es periodic (dona la volta), aixi les peces quedes connectades.
# Us: powershell -File Tools\PixelArt\liquids-propostes.ps1 [-Out ruta.png]
param([string]$Out = (Join-Path $PSScriptRoot '..\..\..\Art per editar\Propostes\Liquids - propostes.png'))

Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System; using System.Drawing; using System.Collections.Generic;
public static class LiquidPreview
{
    static Color H(string s) { return ColorTranslator.FromHtml("#" + s); }
    // Bombolles: 'o' gran 5x5, 's' xicoteta 3x3, 'd' punt
    static readonly string[] Big = { ".XXX.", "X...X", "X...X", "X...X", ".XXX." };
    static readonly string[] Small = { ".X.", "X.X", ".X." };

    // cada disseny = llista de bombolles (tipus, x, y); el dibuix dona la volta al voltant de la peça de 16x16
    static readonly string[][] Designs = new string[][] {
        // 1 Bombolles grans: poques i clares
        new[] { "o 2 2", "o 10 9", "s 11 2", "s 3 11", "d 7 6", "d 14 14", "d 6 14" },
        // 2 Bombolles molt, xicotetes: bullint
        new[] { "s 1 1", "s 9 0", "s 5 6", "s 13 6", "s 2 11", "s 10 12", "d 6 2", "d 14 3", "d 9 8", "d 1 7", "d 6 10", "d 14 11", "d 4 15" },
        // 3 Bombolles + taques fosques (escuma o crosta)
        new[] { "o 9 1", "s 2 3", "s 3 11", "d 12 9", "d 7 7", "d 14 14", "t 0 7", "t 8 11", "t 11 4" },
        // 4 Bombolles en cadena que pugen en diagonal
        new[] { "s 1 12", "s 5 8", "o 9 3", "d 3 5", "d 12 11", "d 14 7", "d 7 14", "d 0 1" },
    };

    static bool Bubble(int design, int x, int y, out int kind)
    {
        kind = 0;
        foreach (var e in Designs[design])
        {
            var p = e.Split(' '); char t = p[0][0]; int bx = int.Parse(p[1]), by = int.Parse(p[2]);
            if (t == 'd') { if (x == bx && y == by) { kind = 3; return true; } continue; }
            if (t == 't')
            {   // taca fosca de 4x2
                for (int dy = 0; dy < 2; dy++) for (int dx = 0; dx < 4; dx++)
                    if (((bx + dx) & 15) == x && ((by + dy) & 15) == y) { kind = 4; return true; }
                continue;
            }
            string[] shape = t == 'o' ? Big : Small;
            for (int dy = 0; dy < shape.Length; dy++) for (int dx = 0; dx < shape[dy].Length; dx++)
                if (shape[dy][dx] == 'X' && ((bx + dx) & 15) == x && ((by + dy) & 15) == y)
                { kind = (dx == 1 && dy == 1 || (t == 's' && dx == 1 && dy == 0)) ? 2 : 1; return true; }
        }
        return false;
    }

    // mask: 1=N 2=E 4=S 8=O 16=NE 32=SE 64=SO 128=NO. y=0 dalt.
    static Color Pixel(int x, int y, int mask, bool lava, int design)
    {
        int ey = 15 - y; // com a Unity: y cap amunt
        int edge = 99;
        if ((mask & 1) == 0) edge = Math.Min(edge, y);
        if ((mask & 2) == 0) edge = Math.Min(edge, 15 - x);
        if ((mask & 4) == 0) edge = Math.Min(edge, ey);
        if ((mask & 8) == 0) edge = Math.Min(edge, x);
        if ((mask & 3) == 3 && (mask & 16) == 0) edge = Math.Min(edge, y + (15 - x));
        if ((mask & 6) == 6 && (mask & 32) == 0) edge = Math.Min(edge, ey + (15 - x));
        if ((mask & 12) == 12 && (mask & 64) == 0) edge = Math.Min(edge, ey + x);
        if ((mask & 9) == 9 && (mask & 128) == 0) edge = Math.Min(edge, y + x);
        Color dark = H(lava ? "37272e" : "20372f"), rim = H(lava ? "ab442e" : "4e7339"), glow = H(lava ? "ffc34f" : "c4df66");
        Color baseC = H(lava ? "df6135" : "70a34b"), deep = H(lava ? "b8452b" : "5a8a3e");
        Color ring = H(lava ? "ffb347" : "cfe86f"), spark = H(lava ? "ffe28a" : "e6f5a0");
        if (edge == 0) return dark;
        if (edge == 1) return rim;
        if (edge == 2) return glow;
        // l'interior es periodic: no mira mai la vora, aixi s'uneix amb la peça del costat
        int kind;
        if (Bubble(design, x, y, out kind))
            return kind == 1 ? ring : kind == 2 ? spark : kind == 3 ? ring : deep;
        return baseC;
    }

    public static void Render(string path)
    {
        string[] pool = {
            "..XXXXXX......",
            ".XXXXXXXXX..X.",
            "XXXXXXXXXXXXXX",
            "XXXXXXXXXXXXX.",
            ".XXXXX..XXXX..",
            "..XXXX..XXX...",
        };
        int rows = pool.Length, cols = pool[0].Length, T = 16, S = 3;
        int cw = cols * T * S + 30, ch = rows * T * S + 56;
        string[] names = { "1 Bombolles grans", "2 Bullint (moltes xicotetes)", "3 Bombolles i crosta", "4 Bombolles que pugen" };
        var bmp = new Bitmap(cw * 2 + 30, ch * 4 + 50);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(H("0d1618"));
            var f = new Font("Arial", 15, FontStyle.Bold);
            for (int liq = 0; liq < 2; liq++)
                for (int d = 0; d < 4; d++)
                {
                    int ox = 15 + liq * (cw + 0), oy = 40 + d * ch;
                    g.DrawString((liq == 0 ? "LAVA - " : "ACID - ") + names[d], f, Brushes.White, ox, oy - 28);
                    for (int r = 0; r < rows; r++) for (int c = 0; c < cols; c++)
                    {
                        if (pool[r][c] != 'X') { using (var b = new SolidBrush(H("1b2a2e"))) g.FillRectangle(b, ox + c * T * S, oy + r * T * S, T * S, T * S); continue; }
                        Func<int, int, bool> on = (rr, cc) => rr >= 0 && cc >= 0 && rr < rows && cc < cols && pool[rr][cc] == 'X';
                        int m = (on(r - 1, c) ? 1 : 0) | (on(r, c + 1) ? 2 : 0) | (on(r + 1, c) ? 4 : 0) | (on(r, c - 1) ? 8 : 0)
                              | (on(r - 1, c + 1) ? 16 : 0) | (on(r + 1, c + 1) ? 32 : 0) | (on(r + 1, c - 1) ? 64 : 0) | (on(r - 1, c - 1) ? 128 : 0);
                        for (int y = 0; y < T; y++) for (int x = 0; x < T; x++)
                        {
                            Color col = Pixel(x, y, m, liq == 0, d);
                            using (var b = new SolidBrush(col)) g.FillRectangle(b, ox + (c * T + x) * S, oy + (r * T + y) * S, S, S);
                        }
                    }
                }
        }
        bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }
}
'@

$dir = Split-Path $Out -Parent
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force $dir | Out-Null }
[LiquidPreview]::Render($Out)
Write-Host "Escrit $Out"
