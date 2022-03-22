Imports System.Drawing
Imports System.Drawing.Imaging

Public Class BentNormals

    Public heightMap(,) As Integer

    Public normalMap As Bitmap = Nothing
    Public bentNormalMap As Bitmap = Nothing
    Public ambientOcclusionMap As Bitmap = Nothing

    Public width As Integer = 0
    Public height As Integer = 0

    Public t As New ThreadBalance()

    Public sLock As New Object()
    Public sLock2 As New Object()
    Public sLock3 As New Object()

    Public rayCount As Integer = 30
    Public rayLength As Integer = 10
    Public strength As Double = 0.01F * 10

    Public Sub New(ByVal heightMapBmp As Bitmap, ByVal normalMapBmp As Bitmap, ByVal _rayCount As Integer, ByVal _rayLength As Integer, ByVal _strength As Double)
        rayCount = _rayCount
        rayLength = _rayLength
        strength = 0.01F * _strength
        width = heightMapBmp.Width
        height = heightMapBmp.Height
        heightMap = readHeightMap(heightMapBmp)
        normalMap = normalMapBmp
        bentNormalMap = New Bitmap(width, height, PixelFormat.Format32bppArgb)
        ambientOcclusionMap = New Bitmap(width, height, PixelFormat.Format32bppArgb)
        Dim rowsToProcess = heightMapBmp.Height - 1
        Console.WriteLine("Computing bent normals.")
        t.runThreads(AddressOf makeBentNormalRows, rowsToProcess)
        'makeBentNormals()
    End Sub

    Public Sub swap(ByRef a As Integer, ByRef b As Integer)
        Dim c As Integer = b
        b = a
        a = c
    End Sub

    Public Function readHeightMap(ByVal bmp As Bitmap) As Integer(,)
        Dim heightMapPixels(,) As Integer
        ReDim heightMapPixels(bmp.Width, bmp.Height)
        Dim index As Integer = 0
        For y = 0 To bmp.Height - 1
            For x = 0 To bmp.Width - 1
                heightMapPixels(x, y) = bmp.GetPixel(x, y).R
                index += 1
            Next
        Next
        Return heightMapPixels
    End Function

    Public Function rayCast(ByVal x0 As Integer, y0 As Integer, ByVal angle As Double, ByVal length As Integer) As Double
        Dim baseRow As Integer = y0
        Dim baseCol As Integer = x0
        Dim startingHeight = heightMap(baseCol, baseRow)

        Dim x1 As Integer = CDbl(x0) + (Math.Cos(angle) * CDbl(length))
        Dim y1 As Integer = CDbl(y0) + (Math.Sin(angle) * CDbl(length))

        Dim steep As Boolean = Math.Abs(y1 - y0) > Math.Abs(x1 - x0)

        If steep Then
            swap(x0, y0)
            swap(x1, y1)
        End If

        If x0 > x1 Then
            swap(x0, x1)
            swap(y0, y1)
        End If

        Dim deltaX As Integer = x1 - x0
        Dim deltaY As Integer = Math.Abs(y1 - y0)

        Dim [error] As Integer = deltaX \ 2

        Dim y As Integer = y0
        Dim yStep As Integer = If(y0 < y1, 1, -1)
        Dim maxElevation As Double = 0

        For x As Integer = x0 To x1
            Dim row As Integer = 0
            Dim col As Integer = 0
            If steep Then
                row = x
                col = y
            Else
                row = y
                col = x
            End If
            If baseRow <> row Or baseCol <> col Then
                Dim distance As Double = Math.Sqrt((row - baseRow) * (row - baseRow) + (col - baseCol) * (col - baseCol))
                maxElevation = Math.Max(maxElevation, (heightMap((col + width) Mod width, (row + height) Mod height) - startingHeight) / distance)
                [error] = [error] - deltaY
                If [error] < 0 Then
                    y = y + yStep
                    [error] = [error] + deltaX
                End If
            End If
        Next
        Return maxElevation
    End Function

    Public Sub makeBentNormalRows(ByVal core As Integer, ByVal startIndex As Integer, ByVal endIndex As Integer)
        Console.WriteLine("Starting thread: " & core & ",  row " & startIndex & ", to row " & endIndex)

        Dim angleStep As Double = 2.0F * Math.PI / rayCount

        Dim w As Integer = width
        Dim h As Integer = height

        For row As Integer = startIndex To endIndex
            For col As Integer = 0 To w - 1

                Dim xSum As Double = 0.0F
                Dim ySum As Double = 0.0F

                Dim averageX As Double = 0.0F
                Dim averageY As Double = 0.0F
                Dim averageTotal As Double = 0.0F

                For a As Double = 0.0F To 2.0F * Math.PI Step angleStep
                    Dim xDir As Double = Math.Cos(a) ' + Math.PI / 4.0F)
                    Dim yDir As Double = Math.Sin(a) ' + Math.PI / 4.0F)

                    xSum += Math.Abs(xDir)
                    ySum += Math.Abs(yDir)

                    Dim ray As Double = rayCast(col, row, a, rayLength)
                    averageTotal += ray
                    averageX += (xDir * ray)
                    averageY += (yDir * ray)
                Next

                averageX /= xSum
                averageY /= ySum

                ' Scale normal map x and y parts
                Dim normalX As Double
                Dim normalY As Double
                SyncLock sLock
                    normalX = normalMap.GetPixel(col, row).R / 128.0F - 1.0F
                    normalY = normalMap.GetPixel(col, row).G / 128.0F - 1.0F
                End SyncLock

                normalX -= strength * averageX
                normalY -= strength * averageY

                normalX = (normalX * 128.0F + 128.0F)
                If normalX < 0 Then
                    normalX = 0
                Else
                    If normalX > 255 Then normalX = 255
                End If

                normalY = (normalY * 128.0F + 128.0F)
                If normalY < 0 Then
                    normalY = 0
                Else
                    If normalY > 255 Then normalY = 255
                End If
                SyncLock sLock2
                    bentNormalMap.SetPixel(col, row, Color.FromArgb(255, normalX, normalY, 255))
                End SyncLock
                'compute average ao
                averageTotal /= rayCount

                averageTotal = (255.0 - 10.0F * strength * averageTotal)
                If averageTotal < 0 Then
                    averageTotal = 0
                Else
                    If averageTotal > 255 Then averageTotal = 255
                End If
                SyncLock sLock3
                    ambientOcclusionMap.SetPixel(col, row, Color.FromArgb(255, averageTotal, averageTotal, averageTotal))
                End SyncLock
            Next
        Next
    End Sub

    'convert.exe normal.tga height.tga 30 60 10
    Public Sub makeBentNormals()

        Dim rayCount As Integer = 30
        Dim rayLength As Integer = 10
        Dim strength As Double = 0.01F * 10

        Dim angleStep As Double = 2.0F * Math.PI / rayCount

        Dim w As Integer = width
        Dim h As Integer = height

        For row As Integer = 0 To h - 1
            For col As Integer = 0 To w - 1

                Dim xSum As Double = 0.0F
                Dim ySum As Double = 0.0F

                Dim averageX As Double = 0.0F
                Dim averageY As Double = 0.0F
                Dim averageTotal As Double = 0.0F

                For a As Double = 0.0F To 2.0F * Math.PI Step angleStep
                    Dim xDir as Double = Math.Cos(a)' + Math.PI / 4.0F)
                    Dim yDir as Double = Math.Sin(a)' + Math.PI / 4.0F)

                    xSum += Math.Abs(xDir)
                    ySum += Math.Abs(yDir)

                    Dim ray As Double = rayCast(col, row, a, rayLength)
                    averageTotal += ray
                    averageX += (xDir * ray)
                    averageY += (yDir * ray)
                Next

                averageX /= xSum
                averageY /= ySum

                'scale normal map x and y parts

                Dim normalX As Double = normalMap.GetPixel(col, row).R / 128.0F - 1.0F
                Dim normalY As Double = normalMap.GetPixel(col, row).G / 128.0F - 1.0F

                normalX -= strength * averageX
                normalY -= strength * averageY

                normalX = (normalX * 128.0F + 128.0F)
                If normalX < 0 Then
                    normalX = 0
                Else
                    If normalX > 255 Then normalX = 255
                End If

                normalY = (normalY * 128.0F + 128.0F)
                If normalY < 0 Then
                    normalY = 0
                Else
                    If normalY > 255 Then normalY = 255
                End If

                bentNormalMap.SetPixel(col, row, Color.FromArgb(255, normalX, normalY, 255))
                'compute average ao
                averageTotal /= rayCount

                averageTotal = (255.0 - 10.0F * strength * averageTotal)
                If averageTotal < 0 Then
                    averageTotal = 0
                Else
                    If averageTotal > 255 Then averageTotal = 255
                End If

                ambientOcclusionMap.SetPixel(col, row, Color.FromArgb(255, averageTotal, averageTotal, averageTotal))
            Next
        Next
    End Sub

    Public Function getBitmap() As Bitmap
        Return bentNormalMap
        'Dim normalMap = New Bitmap(width, height, PixelFormat.Format32bppArgb)
        'Dim index As Integer = 0
        'For y = 0 To height - 1
        '    For x = 0 To width - 1
        '        normalMap.SetPixel(x, y, Color.FromArgb(255, heightMap(x, y), heightMap(x, y), heightMap(x, y)))
        '        index += 1
        '    Next
        'Next
        'Return normalMap
    End Function

End Class
