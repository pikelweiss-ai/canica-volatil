# Grup F · Món i art · Alpha v1.2.0 → v1.2.1

Fonts: `DungeonGenerator.cs`, els 8 `Dungeon/Temes/Tema_*.asset` (enemics i boss de cada tema llegits pel GUID), `dossier-canica.md`, fulls `Art per editar/Fulls/` (Armes, Enemics, Masmorra, Alpha) i les 27 PNG de `Sprites/Pixel/Skins` (+ Clàssica = 28), mirades ampliades.

---

## 🧭 Oriol Camps · Dissenyador de nivells
> "Procedural no vol dir variat. Ara mateix teniu un generador que fa el mateix pis amb un altre color."

### Primera impressió
Ferran té raó: no és una sensació, està escrit al codi. Hi ha 4 tipus de sala (`Start`, `Combat`, `Treasure`, `Boss`) i el tema només canvia **tiles, llum, líquid i llista d'enemics**. La geometria la decideix el mateix `switch (rng.Next(7))` en els 8 temes.

### Flux d'un pis tipus (pis 3, Desert)
Inici (cofre) → 2–3 sales de combat → cambra d'elit (la sala de combat més llunyana) → boss Mag Fosc. Tot és tensió: l'única pausa és la sala inicial i un tresor de 9×9 en un cul-de-sac que potser no visites. Com que el mapa és un **arbre sense cercles**, les branques laterals són anar i tornar per un cofre del 45 %, i abans d'entrar no saps què hi ha: **cap decisió de ruta**.

### Per què totes se senten iguals (dades)
| Dada del generador | Efecte |
|---|---|
| Sales de combat: 4 amplades × 3 altures = **12 rectangles**; boss sempre **19×15** amb les mateixes 4 columnes dobles, siga quin siga el boss | La forma no sorprén mai |
| Decoració: 7 opcions, 4 són "alguna cosa als 4 quadrants"; la creu central de 3 d'ample i 2 caselles de vora sempre lliures | Totes les sales es juguen igual: pel mig i en cercle |
| Sala buida: 1/7 directe + quan falla la bassa → **14–21 %** segons el tema | |
| Líquid: **aigua en 6 de 8 temes** (Desert i Neu inclosos); només Claveguera (àcid) i Volcà (lava) canvien | El tema no canvia com es juga |
| Props de tema: només Bosc i Claveguera, i són **els mateixos** (arbust de baies i bolet) | La claveguera és un bosc verd fosc |
| Enemics: 13 tipus repartits en 37 places. Bosc i Claveguera comparteixen 4 de 5; Neu i Cristall, 3 de 4 | Canviar de pis no canvia l'enemic |
| Bosses: 3 per a 8 temes. Golem Gegant als pisos 2, 5 i 7; Mag Fosc al 3, 6 i 8; Rei Llim a l'1 i al 4 | En 5 pisos veus un boss repetit 2 vegades |
| Onades: elecció uniforme de la llista, aparició a cel·les aleatòries. Pis 3 ≈ 2,5 onades × 5,5 = **~14 enemics/sala**; pis 5 ≈ 19 i amb ×2,2 de vida | Més quantitat, no més varietat. Aquí hi ha la sensació d'avorriment |
| Elit = enemic del tema anterior ×1,25 de mida i ×1,5 de vida. Per cert, la cambra només existeix **des del pis 3** (el `context.md` diu "cada pis") | És el mateix enemic, però més gran |

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número o matís |
|---|---|---|---|
| "Les masmorres són iguals" | 👍 | Les dades ho confirmen | Prioritat 1 de la v1.3 |
| "Més modes" | 🤔 | Un mode nou sobre aquest pis heretarà l'avorriment | Primer, varietat dins del pis; després, modes |
| Bosses | 👍 | 3 bosses, mateixa sala | Una sala pròpia per boss (S) abans que bosses nous (L) |

### Les meues propostes (sales i esdeveniments, per esforç)
1. **S · Sala de cada boss.** Rei Llim: basses als costats. Golem: caixes i barrils que es trenquen. Mag Fosc: 8 columnes per teletransportar-se. Només és un `DecorateBoss` per boss.
2. **S · Plantilles d'onada** en lloc de triar a l'atzar: "emboscada" (ixen a les portes), "torreta" (BoletVeri al centre i kamikazes), "eixam" (8 ratpenats de 2 de vida). 3–4 plantilles per tema.
3. **S · Mecànica de tema reaprofitant `ResonancePulses`.** Volcà: erupcions avisades de 1,2 s. Cristall: rajos que rebotin. Desert: arenes movedisses (alenteixen un 40 %) en lloc d'aigua. Neu: gel que rellisca.
4. **S · Sala-repte opcional** en un cul-de-sac, amb porta de calavera: 1 onada de 3 elits i cofre èpic segur. Primera decisió de ruta real.
5. **M · Mercader o santuari.** Les monedes ara no serveixen per a res dins de la partida: santuari amb 1 cor màxim → +1 nivell d'arma, o rerolls d'arma per 15 monedes.
6. **M · Paret secreta** que es trenca amb TNT (els barrils ja existeixen) i amaga una sala amb cofre o cura. Que es veja una esquerda a la paret.
7. **M · 6–10 sales fetes a mà** (estampes de Tilemap) barrejades amb les procedurals, el 30 % de les vegades: forma de L, ponts sobre lava, passadís estret.
8. **L · Bucles al mapa** (1–2 connexions extra) i un boss propi per a cada tema que falta (5).

**Esdeveniment de pis (S):** un modificador anunciat amb el bàner que ja existeix, com "Pis daurat" (×2 monedes) o "Pis fosc" (Light2D de radi curt). Molt de gancho per a molt poc codi.

### El que més em preocupa
1. Afegir modes abans d'arreglar el pis.
2. Que 14–19 enemics per sala cansen més que no diverteixen. **[cal provar]** que el pis 3 amb 2 onades i més varietat no siga més divertit.
3. On mor la gent normalment. Si és al pis 3–4, ningú no veu Neu, Cripta, Volcà ni Cristall. **[cal provar]** amb els rècords locals.

### Nota del joc ara mateix: 4,5/10 · i amb els canvis: 7,5/10

---

## 🎨 Júlia Roig · Artista de píxel art
> "L'art individual està bé. El problema és que, en combat, moltes coses es confonen entre elles o amb el terra."

### Primera impressió
Hi ha coherència: contorn fosc d'1 px a tot arreu, 16 PPU i Point. Per tant, les mans diferents (Ferran, Claude, ChatGPT) no xoquen. Però el món és **una sola rajola de maó recolorida 8 vegades** (full Alpha: Terra A/B són el mateix dibuix en tots els temes), i el fons és quasi negre en els 8.

### Per elements
- **Skins (28):** el gat, el llop, el fantasma, la gota i el foc trenquen el cercle i es reconeixen a l'instant. Unes 12 són la mateixa cara de boca gran amb un altre color (Bombolla, Llima, Menta, Perla, Safir, Síndria, Vidre…). **Irisada** té tant de soroll que a 16 px no s'hi veu la cara. Aurora, Cosmos, Ninja i Irisada són fosques i **desapareixen** sobre els terres de Cova, Cripta i Volcà. **Zombi** és verd com Llim i Esporer (Bosc i Claveguera). **Cíclop** és morat com Tirador, Mag Fosc i Ratpenat.
- **Armes:** les tres fulles (EspasaVidre blanca, LlançaTempesta blava i SabreCorall rosa) tenen **la mateixa silueta**: una línia diagonal amb guarda daurada. Només canvia el color. Per això costa distingir la llança del sable. Làser i ProjectorPolsos són dues pistoles blaves allargades; Vareta i VaretaEspores, un pal amb una gemma; AK, Escopeta i Llançagranades, el mateix fusell de fusta. El martell, la minigun, el disc i el shuriken sí que es distingeixen bé.
- **Raresa:** no es veu a l'arma. El Sniper (llegendari) és caqui i sobri; l'EspasaVidre (comuna) brilla més que cap.
- **Enemics:** Aranya, Escarabat, Kamikaze i Golem són gris fosc sobre terres gris fosc o marró fosc (Golem i Escarabat a Volcà). La bala enemiga vermella està bé: que siga **exclusiva**.
- **Temes:** Cova, Cripta i Cristall són tres grisos-lilas; Bosc i Claveguera, dos verds foscos. Desert i Neu són els únics que es reconeixen d'una ullada.

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número o matís |
|---|---|---|---|
| EspasaVidre "no pega en V" | 🤔 | Si el traç no dibuixa l'arc de 120°, no es nota | Efecte d'arc de 0,1 s amb l'angle real **[cal provar]** |
| Vareta d'espores sense seguiment | 👍 | Es llegeix millor: el tir va on apuntes | — |
| Nerf de la Llança | 🤔 | Part del "superior" és visual: no es distingeix del sable | Fulla més llarga amb punta de fletxa i 3 px d'amplada |

### Les meues propostes (què canviaria primer)
1. **S · Anell sota el jugador** (1 px clar o ombra de color) per a totes les skins. Soluciona les skins fosques sense redibuixar-les.
2. **S · Raresa a l'arma a terra i a la hotbar:** vora o brillantor de comú gris, rar blau, èpic morat i llegendari daurat amb una partícula.
3. **M · Silueta pròpia per a cada arma cos a cos:** espasa de fulla ampla i curta, llança llarga amb punta i sable corbat.
4. **M · Un motiu de terra per tema** en lloc del maó comú: esquerdes a la Cova, llosa amb runes a la Cripta, cristalls que sobreïxen al Cristall, reixes i aigua bruta a la Claveguera. Afegiu 2 props propis per tema.
5. **S · Vora clara o ulls brillants** als 4 enemics foscos.

### Regles d'estil per al projecte
1. Primer la silueta, després el color: dues armes de la mateixa família no poden compartir silueta.
2. Diferència de lluminositat ≥ 30 % entre el sprite i el terra del tema on apareix.
3. El vermell de les bales enemigues és només per a l'enemic.
4. Contorn fosc d'1 px i detall mínim de 2 px a 16 px.
5. La raresa sempre amb color i alguna cosa més (forma o brillantor).
6. Abans d'aprovar un sprite, mireu-lo a 1× sobre els 8 terres.

### El que més em preocupa
1. Si l'art no ajuda, les masmorres pareixeran iguals encara que el generador canvie.
2. Les skins fosques i verdes en combat amb molta gent online. **[cal provar]**

### Nota del joc ara mateix: 6,5/10 · i amb els canvis: 8/10
