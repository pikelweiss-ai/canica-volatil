# Art del menú i fons · revisió de les captures de Ferran

Peces guardades dins del projecte:

- `Assets/Artwork/Alpha/Logo.png`: logo transparent, lettering propi en graella, accent greu i emblema de canica.
- `Assets/Artwork/Alpha/MenuExpedition.png`: il·lustració del portal, plataforma, tresor i canica.
- `Assets/Artwork/Alpha/CavernBackdrop.png`: imatge contínua de caverna; cobreix la vista de la càmera i no es repeteix com un tile.

Font editable i reproduïble: `Assets/Editor/AlphaArtworkBuilder.cs`. Regeneració: `AlphaArtworkBuilder.Build` o la preparació visual de l'Alpha. Píxels, paletes, lletres i composició estan definits en codi. La font de la UI continua sent Liberation Sans; el lettering del logo és una imatge independent.

## Eina i intent de generació

Es va intentar la via integrada `image_gen` per al logo. Va retornar HTTP 429 `usage_limit_reached`, sense produir cap imatge. No s'ha usat el CLI ni una API amb crèdits. Els tres PNG finals són dibuixos nous del generador natiu de píxel art del projecte.

Prompt de l'intent fallit: «Use case: logo-brand. Production game logo asset on genuinely transparent background. Exact text, two stacked lines: CANICA / VOLÀTIL (uppercase, accent grave on À). A confident handcrafted retro pixel-art title for a 2D marble dungeon game. Large chunky cream-to-gold pixel lettering with dark navy extruded shadow, carefully legible, tiny geometric chipped highlights, compact integrated yellow round smiling marble with black rectangular eyes and smiling mouth as small emblem beside lettering. Balanced wide 3:2 composition, close-fitting logo with modest transparent margins. Restrained dark navy, ivory, warm gold palette. Authentic deliberate pixel clusters, crisp stepped outlines, no smooth 3D render, no glow, no gradients except stepped pixel bands. No slogan, no extra words, no version, no surrounding UI or background. Both words must be spelled exactly CANICA VOLÀTIL.»

La composició final per codi conserva la lletra daurada i la canica, i desenvolupa una escena pròpia amb portal i passarel·la. Els assets anteriors es conserven.
