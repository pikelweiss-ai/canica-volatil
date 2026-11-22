# Traspàs històric · Alpha v1.5.0 publicada

**Estat corregit el 22 de novembre de 2026.** La release `v1.5.0-alpha` està publicada a GitHub i marcada **Latest** (release 2026-11-22, commit `6675b44`). El text següent descriu l'estat tècnic de la candidata en el moment del traspàs original: paquet `1.5.0-alpha`, protocol **24** i codi de versió Android **16**. Ja no és una candidata local sense publicar.

## Joc i fonts

- [Deu pendents de la fase 0](Docs/PENDENTS_APUNTATS.md), incloent-hi microbosses des del pis 2.
- Els huit bosses es generen en `AlphaContentBuilder.Bosses.cs`. El Rei de les Clavegueres usa l'sprite original localitzat en `ArtNou_v1.4/Bosses/Sprites`.
- `DungeonBossKey` tria la sala de la clau i conserva l'estat en tornar a un pis. La decisió de recollir i obrir passa per l'amfitrió.
- `RunAmulets` té el catàleg, les ranures, les ofertes per jugador i les regles de combat. La tenda usa `CosmeticStore` i conserva `Cos_Skin`, `Cos_Hat` i `Cos_Trail`.
- `BlockCatalogBuilder` genera 626 fitxes `BlockData` en 15 famílies. Els identificadors combinen família i fitxer per distingir les textures de nom repetit. [Disseny de l'Almanac i de Creador](Docs/Dissenys/v1.5-almanac-blocs-i-mode-creador.md).

## Compilar i provar

L'Editor ha d'estar tancat abans de cada batch. Usa el mateix directori d'eixida per a totes les plataformes.

```powershell
$root = 'Builds\Candidats\Alpha-v1.5.0-2026-11-21'
./Tools/Build/build-alpha.ps1 -Target Windows -BuildRoot $root
./Tools/Build/build-alpha.ps1 -Target Linux -BuildRoot $root
./Tools/Build/build-alpha.ps1 -Target Android -BuildRoot $root
./Tools/Build/check-crossplay.ps1 -BuildRoot $root
./Tools/Net/test-online.ps1 -Exe "$root\Windows\CanicaAlphaReview.exe" -MobileClient
```

Els rols de bot de la candidata són `phase0`, `bosses150`, `keys150`, `amulets150`, `store150`, `almanac150` i `menushot`. Les parelles de bots LAN admeten `-botPhase0`, `-botBosses150`, `-botKeys150` i `-botAmulets150`. Consulta els arxius de registre i les captures a `Builds/Candidats`.

Windows, Linux i Android han compilat sense errors i els manifests coincidixen. El paquet Android porta el codi 16, SDK mínim 26 i binaris ARM64 i ARMv7. Linux i Android queden pendents d'execució en dispositius reals.

## Notes de seguiment

La compilació final ha passat els quatre casos de LAN i relay amb client mòbil simulat. Els dos casos amb directori públic esperen una nova passada quan ntfy.sh torne a admetre els anuncis d'este entorn. La mateixa suite ha donat **6/6** amb un directori local de prova i el túnel real. Els registres són a `Online-repeticio` i `Online-directori-local` dins del directori de la candidata.

## Publicació completada

La release v1.5.0-alpha està publicada; la decisió D1 de la v1.6 queda resolta. No cal cap publicació abans del treball de v1.6.

La v1.6 està dedicada a l'editor de mapes i les seues plantilles. L'estat actual és al [traspàs v1.6](TRASPAS_CODEX_V160.md).
