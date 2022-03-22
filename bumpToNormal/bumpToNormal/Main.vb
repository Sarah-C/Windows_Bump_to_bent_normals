Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO


Public Class Main

    Public heightName As String = Nothing
    Public normalName As String = Nothing
    Public bentName As String = Nothing
    Public ambientName As String = Nothing

    Public rayCount As Integer = 0
    Public rayLength As Integer = 0
    Public rayStrength As Integer = 0

    Public heightMap As Bitmap = Nothing
    Public normalMap As Bitmap = Nothing

    Public MN As MakeNormals = Nothing
    Public BN As BentNormals = Nothing

    Public Shared Sub Main(ByVal args() As String)
        Dim p As New Main(args)
        Console.WriteLine("Complete.")
        If Debugger.IsAttached Then Console.ReadKey()
    End Sub

    Public Sub New(ByVal args() As String)
        'heightMap = format32BPPARGBBitmap(Bitmap.FromFile("height.jpg"))
        'normalMap = format32BPPARGBBitmap(Bitmap.FromFile("normal.bmp"))
        'BN = New BentNormals(heightMap, normalMap, 10, 50, 20)
        'BN.bentNormalMap.Save("bentNormalMap.png", ImageFormat.Png)
        'BN.ambientOcclusionMap.Save("ambientOcclusionMap.png", ImageFormat.Png)

        'heightMap = format32BPPARGBBitmap(Bitmap.FromFile("height.jpg"))
        'MN = New MakeNormals(heightMap, 0.01)
        'MN.save("normal.png", ImageFormat.Png)

        Select Case args.Length
            Case 7
                heightName = args(0)
                normalName = args(1)
                bentName = args(2)
                ambientName = args(3)
                rayCount = Convert.ToInt16(args(4))
                rayLength = Convert.ToInt16(args(5))
                rayStrength = Convert.ToInt16(args(6))
            Case Else
                help()
                Console.ReadKey()
                Exit Sub
        End Select
        heightMap = format32BPPARGBBitmap(Bitmap.FromFile(heightName))
        If Not File.Exists(normalName) Then
            MN = New MakeNormals(heightMap, 0.01)
            MN.save(normalName, ImageFormat.Png)
        End If
        normalMap = format32BPPARGBBitmap(Bitmap.FromFile(normalName))
        BN = New BentNormals(heightMap, normalMap, 120, 30, 7)
        BN.bentNormalMap.Save(bentName, ImageFormat.Png)
        BN.ambientOcclusionMap.Save(ambientName, ImageFormat.Png)
    End Sub

    Public Sub help()
        Dim filename As String = Process.GetCurrentProcess().ProcessName
        '####################################################################### < Screen edge.
        Console.WriteLine(<preFormatted>
MakeBentNormals HeightMap NormalMap BentName AOName RayCount RayLength RayStrength

  HeightMap
        The filename of the input height map.

  NormalMap
        The filename of the input normal map, if it doesn't exist
        the normal map is created and saved with the provided filename.

  BentName
        The filename of the output bent normal map.

  AmbientName
        The filename of the output ambient occlusion map.

  RayCount
        Rays around a circle to be used to calcualte the nearest horizon.
  
  RayLength
        How many pixels to step when searching for the nearest horizon.
  
  RayStrength 
        How much influence a single ray has on the ambient occlusion.

Example:
BentNormals height.jpg normal.jpg bent.png ao.png 60 40 30 
-Press any key-
</preFormatted>.Value.Replace("MakeBentNormals", filename))
    End Sub

    Public Function format32BPPARGBBitmap(ByVal bm As Bitmap) As Bitmap
        Dim newFormat As New Bitmap(bm.Width, bm.Height, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(newFormat)
            g.DrawImage(bm, 0, 0, bm.Width, bm.Height)
        End Using
        Return newFormat
    End Function

End Class
