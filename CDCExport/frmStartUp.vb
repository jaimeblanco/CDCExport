Imports CDCData.CDC
Imports CDCData
Imports System.IO

Public Class frmStartUp
    Public myApp As New CDCConnection

    Private Sub frmStartUp_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'Module1.Main()

        If Not File.Exists(My.Settings.ConfigLocation) Then
            Throw New ApplicationException(myApp.ConfigFileLocation & " does not exist. ")
        Else
            Dim configXML = XDocument.Load(My.Settings.ConfigLocation)

            myApp.ConfigXml = configXML
            myApp.Database = (From ele In configXML...<Config> _
                                      Select ele.<Catalog>.Value.Trim).SingleOrDefault
            myApp.User = "test"
            myApp.Datasource = (From ele In configXML...<Config> _
                                      Select ele.<DSN>.Value.Trim).SingleOrDefault
            myApp.ConnectionString = (From ele In configXML...<Config> _
                                     Select ele.<ConnectionString>.Value.Trim).SingleOrDefault


        End If
    End Sub

    Private Sub cmdStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdStart.Click
        Dim myCDC As New CDCData.ExportItemData
        Dim nErrCls As New CDCErrorClass
        myCDC.exportItems(myApp, nErrCls)
    End Sub
End Class
