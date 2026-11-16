using UnityEditor;
using UnityEngine;

// Tota imatge dins d'una carpeta "Pixel" (p. ex. Assets/Sprites/Pixel/...)
// s'importa automàticament com a píxel art de deveres:
// Sprite, Filter Point (sense borrós), sense compressió i 16 píxels = 1 unitat.
public class PixelArtImporter : AssetPostprocessor
{
    const int PixelsPerUnit = 16; // Mateixa densitat que els blocs de 16x16

    // Si canvies la configuració, puja aquest número: Unity tornarà a importar les textures
    public override uint GetVersion() { return 3; }

    void OnPreprocessTexture()
    {
        if (!assetPath.Replace('\\', '/').Contains("/Pixel/"))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.textureShape = TextureImporterShape.Texture2D; // Mai Cubemap (passava amb .meta incomplets)
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.isReadable = true; // Per a poder fer la vora de raresa dels items per codi
    }
}
