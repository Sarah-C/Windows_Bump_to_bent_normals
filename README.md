# Windows_Bump_to_bent_normals
Make smooth normal maps and bent normal's from a height map.

This project is an extension of this bent normal map creator:
https://blenderartists.org/t/self-shadowing-normal-maps/604178
http://web.archive.org/web/20220318173921/https://blenderartists.org/t/self-shadowing-normal-maps/604178

It's not optimised, but I did dump the process into multiple cores to speed it up without much work.

It's a small command line tool that accepts the usual .Net compatible image types - BMP, PNG, JPG etc..., the outputs are all PNG.


# Command line tool parameters
If you don't supply all 7 parameters, the program will display the help-page parameter list.
    bumpToNormal.exe HeightMap NormalMap BentName AOName RayCount RayLength RayStrength

      HeightMap
            The filename of the **input** height map.

      NormalMap
            The filename of the **input** normal map, if it doesn't exist
            the normal map is created and saved with the provided filename.

      BentName
            The filename of the **output** bent normal map.

      AmbientName
            The filename of the **output** ambient occlusion map.

      RayCount   (Roughly 10 to 360, 90 is a good value)
            Rays cast equally angled around a circle to be used to calcualte the nearest horizon. If you specify 10, that's 360/10 = 36 degrees between per ray. More rays give a more accurate result but takes a lot longer.

      RayLength   (In pixels, so a 500px wide texture wouldn't need this higher than perhaps 100 to get close-by features, 4K would need around 500 or more)
            The maximum number of pixels to examine away from the current point when searching for the nearest horizon. If you're processing a 4K texture, this needs to be scaled up accordingly as 40 pixel length may not reach important bumps in your heightmap.

      RayStrength (0 to 100)
            How much influence a single ray has on the ambient occlusion.

    Example:
    BentNormals.exe height.jpg normal.jpg bent.png ao.png 60 40 30
