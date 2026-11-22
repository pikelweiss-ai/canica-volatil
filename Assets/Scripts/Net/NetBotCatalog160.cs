using System.Collections;
using System.Linq;
using UnityEngine;

public partial class NetBot
{
    IEnumerator Catalog160()
    {
        yield return WaitFor(() => Scene == "MainMenu", 20, "catàleg v1.6");
        if (failed) yield break;
        var blocks = BlockCatalog.Instance?.blocks;
        if (blocks == null || blocks.Length != 626 || blocks.Any(b => b == null || b.sprite == null) ||
            blocks.Select(b => b.family).Distinct().Count() != 15 ||
            blocks.Select(b => b.id).Distinct().Count() != 626)
        { Fail("el catàleg necessita 626 blocs, 15 famílies i ids únics"); yield break; }
        Ok("626 blocs, 15 famílies i ids únics");

        string[] autoTileIds = { "Roca_cova_1", "Roca_cova_marro", "Roca_cova_gran", "Roca_cova_molsa", "Roca_cova_magma", "Roca_cova_blava", "Aigua_profunda", "Aigua_nit", "Aigua_fosca" };
        foreach (string name in autoTileIds)
        {
            var block = blocks.FirstOrDefault(b => b.id == "Natura/" + name);
            if (block == null || block.autoTileSprites == null || block.autoTileSprites.Length != 16 || block.autoTileSprites.Any(sprite => sprite == null))
            { Fail("falten les 16 variants direccionals de " + name); yield break; }
        }
        Ok("roques de cova i aigües amb les 16 màscares d'autotile");

        if (blocks.Any(b => b.family == "Objectes" && !b.passable &&
            (b.hitbox.width <= 0f || b.hitbox.height <= 0f)))
        { Fail("objecte sòlid sense hitbox"); yield break; }
        Ok("tots els objectes sòlids tenen hitbox");

        string[] groups = { "Llit_hospital_", "Llit_blau_", "Llit_roig_", "Llit_verd_", "Sofa_gris_", "Sofa_roig_",
            "Banc_espera_", "Banc_blanc_", "Banc_lab_", "Taula_llarga_marro_", "Taula_llarga_verda_",
            "Taula_reunions_", "Banyera_", "Barca_", "Contenidor_blau_", "Contenidor_roig_", "Contenidor_verd_" };
        foreach (string prefix in groups)
        {
            string[] required = prefix.StartsWith("Sofa_") ? new[] { "esq", "dre" } :
                prefix.StartsWith("Banyera_") ? new[] { "esq", "dre" } :
                prefix.StartsWith("Barca_") ? new[] { "cap", "popa" } :
                prefix.StartsWith("Llit_") ? new[] { "cap", "peus" } :
                new[] { "esq", "mig", "dre" };
            foreach (string part in required)
                if (!blocks.Any(b => b.id.EndsWith("/" + prefix + part) && b.multiPart))
                { Fail("grup multipart incomplet: " + prefix + part); yield break; }
        }
        if (blocks.Any(b => (b.id.EndsWith("_esq") || b.id.EndsWith("_mig") || b.id.EndsWith("_dre")) && !b.multiPart))
        { Fail("peça amb sufix esq/mig/dre fora de multiPart"); yield break; }
        Ok("grups multipart complets i sufixos esq/mig/dre inclosos");

        if (blocks.Any(b => (b.id.Contains("Tronc_caigut") || b.id.Contains("Tronc_betula")) &&
            (b.passable || b.breakable)))
        { Fail("els troncs tombats han de ser sòlids i irrompibles"); yield break; }
        Ok("troncs tombats sòlids i irrompibles");
        Pass();
    }
}
