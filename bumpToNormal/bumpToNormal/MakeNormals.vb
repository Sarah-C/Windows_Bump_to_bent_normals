Imports System.Drawing
Imports System.Drawing.Imaging

Public Class MakeNormals

    Public width As Integer = 0
    Public height As Integer = 0

    Public strength As Double = 1

    Public pixelsToProcess As Integer = 0
    Public pixelsToProcessOffset As Integer = 0

    Public heightMapPixels() As Integer
    Public normalMapPixels() As Integer

    Public t As New ThreadBalance()

    Public Sub New(ByVal bmp As Bitmap, ByVal strength As double)
        width = bmp.Width
        height = bmp.Height
        Me.strength = strength
        heightMapPixels = serialiseHeightMap(bmp)
        pixelsToProcess = ((width * height) - 2 - width) - (width + 1)
        pixelsToProcessOffset = width + 1
        ReDim normalMapPixels(width * height)
        Console.WriteLine("Computing normal map.")
        t.runThreads(AddressOf processHeightChunk, pixelsToProcess)
    End Sub

    Public Function serialiseHeightMap(ByVal bmp As Bitmap) As Integer()
        Dim heightMapPixels() As Integer
        ReDim heightMapPixels(bmp.Width * bmp.Height)
        Dim index As Integer = 0
        For y = 0 To bmp.Height - 1
            For x = 0 To bmp.Width - 1
                heightMapPixels(index) = bmp.GetPixel(x, y).ToArgb()
                index += 1
            Next
        Next
        Return heightMapPixels
    End Function

    Public Sub processHeightChunk(ByVal core As Integer, ByVal startIndex As Integer, ByVal endIndex As Integer)
        startIndex += pixelsToProcessOffset
        endIndex += pixelsToProcessOffset
        Console.WriteLine("Starting thread: " & core & ", pixel " & startIndex & ", to pixel " & endIndex)
        For index As Integer = startIndex To endIndex

            Dim tl As Double = Color.FromArgb(heightMapPixels(index - 1 - width)).R
            Dim t As Double = Color.FromArgb(heightMapPixels(index - width)).R
            Dim tr As Double = Color.FromArgb(heightMapPixels(index + 1 - width)).R

            Dim l As Double = Color.FromArgb(heightMapPixels(index - 1)).R
            Dim r As Double = Color.FromArgb(heightMapPixels(index + 1)).R

            Dim bl As Double = Color.FromArgb(heightMapPixels(index - 1 + width)).R
            Dim b As Double = Color.FromArgb(heightMapPixels(index + width)).R
            Dim br As Double = Color.FromArgb(heightMapPixels(index + 1 + width)).R

            'Sobel filter
            Dim dX As Double = (tr + 2.0 * r + br) - (tl + 2.0 * l + bl)
            Dim dY As Double = (bl + 2.0 * b + br) - (tl + 2.0 * t + tr)
            Dim dZ As Double = 1.0 / strength

            Dim length As Double = Math.Sqrt((dX * dX) + (dY * dY) + (dZ * dZ))

            Dim nX As Double = dX / length
            Dim nY As Double = dY / length
            Dim nZ As Double = dZ / length

            Dim colX As Integer = 255 - ((nX + 1.0) * 127.0)
            Dim colY As Integer = 255 - ((nY + 1.0) * 127.0)
            Dim colZ As Integer = (nZ + 1.0) * 127.0

            normalMapPixels(index) = Color.FromArgb(255, colX, colY, colZ).ToArgb()
        Next

    End Sub

    Public Function getBitmap() As Bitmap
        Dim normalMap = New Bitmap(width, height, PixelFormat.Format32bppArgb)
        Dim index As Integer = 0
        For y = 0 To height - 1
            For x = 0 To width - 1
                normalMap.SetPixel(x, y, Color.FromArgb(normalMapPixels(index)))
                index += 1
            Next
        Next
        Return normalMap
    End Function

    Public Sub save(ByVal name As String, ByVal PF As ImageFormat)
        getBitmap.Save(name, PF)
    End Sub

End Class
