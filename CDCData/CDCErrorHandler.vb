
Public Class CDCException
    Inherits Exception

    Private _errNumber As Integer
    Public Property ErrNumber() As Integer
        Get
            Return _errNumber
        End Get
        Set(ByVal value As Integer)
            _errNumber = value
        End Set
    End Property


    Public Sub New(ByVal nMessage As String)
        MyBase.New(nMessage)
    End Sub
End Class

Public Class CDCErrorClass

    Private _errorNumber As Integer
    Private _errorMessage As String
    Private _errxml As XDocument
    Private _exception As Exception
    Private _xmlRecordCount As Integer
    Private _errorsexist As Boolean
    Public ReadOnly Property ErrorsExist() As Boolean
        Get
            Return _errorsexist
        End Get
    End Property

    Public Property ErrMessage() As String
        Get
            Return _errorMessage
        End Get
        Set(ByVal value As String)
            _errorMessage = value
        End Set
    End Property
    Public Property ErrNumber() As Integer
        Get
            Return _errorNumber
        End Get
        Set(ByVal value As Integer)
            _errorNumber = value
        End Set
    End Property
    Public Property ErrXml() As XDocument
        Get
            Return _errxml
        End Get
        Set(ByVal value As XDocument)
            _errxml = value
        End Set
    End Property
    Public Property ErrExceptionObj() As Exception
        Get
            Return _exception
        End Get
        Set(ByVal value As Exception)
            _exception = value
        End Set
    End Property


    Public Sub HandleError(ByVal nErrorNumber As Integer, ByVal nErrorMessage As String, ByVal nException As Exception)
        Try
            Dim _exception As Exception = nException

            If String.IsNullOrEmpty(nErrorMessage) Then
                Throw New ArgumentException("Error Message cannot be null or empty")
            ElseIf _exception Is Nothing Then
                Try
                    Throw New CDCException("Econnect Processed with no Error.")
                Catch CDCEx As CDCException
                    _exception = CDCEx
                End Try
            Else

            End If

            Me.ErrMessage = nErrorMessage
            Me.ErrNumber = nErrorNumber

            If nErrorNumber <> 0 Then
                _errorsexist = True
            End If

            Try
                If Me.ErrXml.Root.HasElements = 0 Then
                    ErrXml.Add(<?xml version="1.0"?>
                               <root></root>)
                End If
            Catch nullex As NullReferenceException
                Me.ErrXml = <?xml version="1.0"?>
                            <root></root>
            Catch ex As Exception
                Throw ex
            End Try


            Dim xmlTree = <RecordNumber ID=<%= _xmlRecordCount.ToString %>>
                              <Error>
                                  <ErrorNumber><%= ErrNumber %></ErrorNumber>
                                  <ErrorMessage><%= ErrMessage %></ErrorMessage>
                                  <ErrorExceptionStackTrace><%= _exception.StackTrace.ToString %></ErrorExceptionStackTrace>
                                  <ErrorExceptionSource><%= _exception.Source %></ErrorExceptionSource>
                                  <ErrorExceptionMessage><%= _exception.Message %></ErrorExceptionMessage>
                              </Error>
                          </RecordNumber>

            _xmlRecordCount += 1
            ErrXml.Root.Add(xmlTree)


        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub New()
        _xmlRecordCount = 1
        _errorsexist = False
    End Sub

End Class
