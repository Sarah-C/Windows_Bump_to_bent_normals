# Windows_Bump_to_bent_normals
Make smooth normal maps and bent normal's from a height map.

This project is an extension of this bent normal map creator:
https://blenderartists.org/t/self-shadowing-normal-maps/604178
http://web.archive.org/web/20220318173921/https://blenderartists.org/t/self-shadowing-normal-maps/604178

It's not optimised, but I did dump the process into multiple cores to speed it up without much work.

It's a small command line tool that accepts the usual .Net compatible image types - BMP, PNG, JPG etc..., the outputs are all PNG.


# Command line tool parameters
    bumpToNormal HeightMap NormalMap BentName AOName RayCount RayLength RayStrength

      HeightMap
            The filename of the **input** height map.

      NormalMap
            The filename of the **input** normal map, if it doesn't exist
            the normal map is created and saved with the provided filename.

      BentName
            The filename of the **output** bent normal map.

      AmbientName
            The filename of the **output** ambient occlusion map.

      RayCount
            Rays around a circle to be used to calcualte the nearest horizon.

      RayLength
            How many pixels to step when searching for the nearest horizon.

      RayStrength
            How much influence a single ray has on the ambient occlusion.

    Example:
    BentNormals height.jpg normal.jpg bent.png ao.png 60 40 30
