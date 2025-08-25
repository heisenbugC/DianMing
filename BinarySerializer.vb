Imports System.Runtime.Serialization.Formatters.Binary
Imports System.IO

Public Class BinarySerializer

    ' Default file path for storing data
    Public Shared Property DefaultFilePath As String = Path.Combine(Application.StartupPath, "data")

    ' Serializes an object of type T to a specified file
    Public Shared Sub Serialize(Of T)(ByVal obj As T, ByVal filePath As String)
        Try
            Dim path As String = If(filePath, DefaultFilePath)
            Using stream As New FileStream(path, FileMode.Create, FileAccess.Write)
                Dim formatter As New BinaryFormatter
                formatter.Serialize(stream, obj)
            End Using
        Catch ex As Exception
            Throw New Exception("Serialization failed: " & ex.Message)
        End Try
    End Sub

    ' Deserializes an object of type T from a specified file
    Public Shared Function Deserialize(Of T)(Optional ByVal filePath As String = Nothing) As T
        Try
            Dim path As String = If(filePath, DefaultFilePath)
            If Not File.Exists(path) Then
                Throw New Exception("File does not exist: " & path)
            End If
            Using stream As New FileStream(path, FileMode.Open, FileAccess.Read)
                Dim formatter As New BinaryFormatter
                Return CType(formatter.Deserialize(stream), T)
            End Using
        Catch ex As Exception
            Throw New Exception("Deserialization failed: " & ex.Message)
        End Try
    End Function
End Class
