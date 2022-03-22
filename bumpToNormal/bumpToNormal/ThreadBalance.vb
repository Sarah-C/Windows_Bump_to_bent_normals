Imports System.Threading
Imports System.ComponentModel

Public Class ThreadBalance

'#Region " Single thread create and callback."

'    Public Delegate Sub theNewThreadDelegate(ByVal communicationObject As Object)
'    Public Delegate Sub theNewThreadCallbackDelegate(ByVal data As callback)

'    Public Delegate Sub theNewThreadDelegateNoParams()
'    Public Delegate Sub theNewThreadCallbackDelegateNoParams(ByVal data As callbackNoParams)

'    Public Class callback
'        Public theCallback As theNewThreadCallbackDelegate = Nothing
'        Public asyncResult As IAsyncResult = Nothing
'        Public communicationObject As Object = Nothing
'        Public Sub doCallbackOnEnd(ByVal AR As IAsyncResult)
'            Me.asyncResult = AR
'            'Run on the UI thread.
'            If theCallback IsNot Nothing Then
'                Dim caller As Control = TryCast(theCallback.Target, Control)
'                If caller IsNot Nothing Then caller.Invoke(theCallback, Me)
'            End If
'        End Sub
'    End Class

'    Public Class callbackNoParams
'        Public theCallback As theNewThreadCallbackDelegateNoParams = Nothing
'        Public asyncResult As IAsyncResult = Nothing
'        Public Sub doCallbackOnEnd(ByVal AR As IAsyncResult)
'            Me.asyncResult = AR
'            'Run on the UI thread.
'            If theCallback IsNot Nothing Then
'                Dim caller As Control = TryCast(theCallback.Target, Control)
'                If caller IsNot Nothing Then caller.Invoke(theCallback, Me)
'            End If
'        End Sub
'    End Class

'    'http://tech.xster.net/tips/multi-threading-in-vb-net/
'    Public Sub runThread(ByVal threadToRun As theNewThreadDelegate, ByVal callbackOnEnd As theNewThreadCallbackDelegate, ByVal communicationObject As Object)
'        Dim cbc As New callback()
'        cbc.theCallback = callbackOnEnd
'        cbc.communicationObject = communicationObject
'        threadToRun.BeginInvoke(communicationObject, New AsyncCallback(AddressOf cbc.doCallbackOnEnd), Nothing)
'    End Sub

'    Public Sub runThread(ByVal threadToRun As theNewThreadDelegateNoParams, ByVal callbackOnEnd As theNewThreadCallbackDelegateNoParams)
'        Dim cbc As New callbackNoParams()
'        cbc.theCallback = callbackOnEnd
'        threadToRun.BeginInvoke(New AsyncCallback(AddressOf cbc.doCallbackOnEnd), Nothing)
'    End Sub

'#End Region

#Region " Batch leveller."

    Public Delegate Sub theActionDelegate(ByVal core As Integer, ByVal startIndex As Integer, ByVal endIndex As Integer)
    Public theAction As theActionDelegate

    Public batchSize As Integer = 0
    Public remainder As Integer = 0

    Public startIndex As Integer = 0
    Public endIndex As Integer = 0

    Public Sub runThreads(ByVal theAction As theActionDelegate, ByVal objectCount As Integer, Optional ByVal cores As Integer = 0)
        Me.theAction = theAction
        If cores = 0 Then cores = Environment.ProcessorCount
        If objectCount < cores Then cores = objectCount
        batchSize = objectCount \ cores
        remainder = objectCount - batchSize * cores
        System.Threading.Tasks.Parallel.[For](0, cores, New Action(Of Integer)(AddressOf runUpdateBlockThread))
    End Sub

    Public Sub runUpdateBlockThread(ByVal core As Integer)
        If core < remainder Then
            startIndex = (batchSize + 1) * core
            endIndex = startIndex + batchSize
        Else
            startIndex = ((batchSize + 1) * remainder) + (((core - remainder)) * batchSize)
            endIndex = startIndex + batchSize - 1
        End If
        theAction(core, startIndex, endIndex)
    End Sub

#End Region

End Class