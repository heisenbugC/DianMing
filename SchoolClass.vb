Imports System.Runtime.Serialization

<Serializable()>
Public Class SchoolClass
    Implements ISerializable

    Public Property ClassName As String
    Public Property Members As List(Of String)

    Public Sub New()
        Members = New List(Of String)()
    End Sub

    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        ClassName = info.GetString("ClassName")
        Members = CType(info.GetValue("Members", GetType(List(Of String))), List(Of String))
    End Sub

    Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) _
        Implements ISerializable.GetObjectData
        info.AddValue("ClassName", ClassName)
        info.AddValue("Members", Members)
    End Sub
End Class
