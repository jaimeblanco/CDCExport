Public Class XMLDataHandler
    Private _myxml As XDocument
    Private _fileName As String

    Property FileName() As String
        Get
            Return _fileName
        End Get
        Set(ByVal value As String)
            _fileName = value
        End Set
    End Property
    Public Property myXML() As XDocument
        Get
            Return _myxml
        End Get
        Set(ByVal value As XDocument)
            _myxml = value
        End Set
    End Property

    Public Sub xmlWriteData()
        Try
            myXML.Save(Me.FileName)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
End Class
