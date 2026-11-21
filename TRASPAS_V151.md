# Traspàs · candidata Alpha v1.5.0

**21 de novembre de 2026.** Branca local `alpha/arsenal` de `StabilityWorkspace`, Unity 6000.0.53f1, paquet `1.5.0-alpha`, protocol **24** i codi de versió Android **16**. L'última versió publicada és la v1.4.2.

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

## Publicació pendent del permís de Ferran

Aplica els pegats oficials d'Unity a Windows i Android, comprova els manifests, empaqueta i valida les sumes. Crea l'etiqueta i la release quan Ferran ho autoritze. [Passos de publicació](TRASPAS_CODEX_V150.md#6-com-publicar-només-amb-permís-explícit-de-ferran).

La v1.6 queda reservada per a l'editor de mapes i les seues plantilles. En la v1.5, la targeta «Creador» mostra «Pròximament» i es queda al selector.
