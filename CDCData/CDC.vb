Enum POType
    HPBReorder
    TTBReorder
    Supplies
    DropShip
End Enum
Public Class CDC
    Public Class CDCConnection
        Private _ConnectionString As String
        Private _User As String
        Private _Database As String
        Private _Datasource As String
        Private _Password As String
        Private _ConfigLocation As String
        Private _ILSConnectionString As String
        Private _ILSuser As String
        Private _ILSDatabase As String
        Private _ILSDataSource As String
        Private _ILSPassword As String
        Private _ConfigXML As XDocument
        Private _HPBConnectionString As String
        Private _HPBUser As String
        Private _HPBDatabase As String
        Private _HPBDatasource As String
        Private _HPBPassword As String

        Public Property ConfigXml() As XDocument
            Get
                Return _ConfigXML
            End Get
            Set(ByVal value As XDocument)
                _ConfigXML = value
            End Set
        End Property

        Public Property ConfigFileLocation() As String
            Get
                Return _ConfigLocation
            End Get
            Set(ByVal value As String)
                _ConfigLocation = value
            End Set
        End Property

        Public Property Password() As String
            Get
                Return _Password
            End Get
            Set(ByVal value As String)
                _Password = value
            End Set
        End Property
        Public Property Database() As String
            Get
                Return _Database
            End Get
            Set(ByVal value As String)
                _Database = value
            End Set
        End Property

        Public Property User() As String
            Get
                Return _User
            End Get
            Set(ByVal value As String)
                _User = value
            End Set
        End Property

        Public Property ConnectionString() As String
            Get
                Return _ConnectionString
            End Get
            Set(ByVal value As String)
                _ConnectionString = value
            End Set
        End Property

        Public Property Datasource() As String
            Get
                Return _Datasource
            End Get
            Set(ByVal value As String)
                _Datasource = value
            End Set
        End Property

        Public Property ILSConnectionString() As String
            Get
                Return _ILSConnectionString
            End Get
            Set(ByVal value As String)
                _ILSConnectionString = value
            End Set
        End Property

        Public Property ILSDatasource() As String
            Get
                Return _ILSDataSource
            End Get
            Set(ByVal value As String)
                _ILSDataSource = value
            End Set
        End Property

        Public Property ILSPassword() As String
            Get
                Return _ILSPassword
            End Get
            Set(ByVal value As String)
                _ILSPassword = value
            End Set
        End Property
        Public Property ILSDatabase() As String
            Get
                Return _ILSDatabase
            End Get
            Set(ByVal value As String)
                _ILSDatabase = value
            End Set
        End Property

        Public Property ILSUser() As String
            Get
                Return _ILSuser
            End Get
            Set(ByVal value As String)
                _ILSuser = value
            End Set
        End Property

        Public Property HPBConnectionString() As String
            Get
                Return _HPBConnectionString
            End Get
            Set(ByVal value As String)
                _HPBConnectionString = value
            End Set
        End Property

        Public Property HPBDatasource() As String
            Get
                Return _HPBDatasource
            End Get
            Set(ByVal value As String)
                _HPBDatasource = value
            End Set
        End Property

        Public Property HPBPassword() As String
            Get
                Return _HPBPassword
            End Get
            Set(ByVal value As String)
                _HPBPassword = value
            End Set
        End Property
        Public Property HPBDatabase() As String
            Get
                Return _HPBDatabase
            End Get
            Set(ByVal value As String)
                _HPBDatabase = value
            End Set
        End Property

        Public Property HPBUser() As String
            Get
                Return _HPBUser
            End Get
            Set(ByVal value As String)
                _HPBUser = value
            End Set
        End Property

        Sub New()

        End Sub

        Sub New(ByVal nDatabase As String, ByVal nConnString As String)
            Try
                Dim db As New CDCDataDataContext
                Me.ConnectionString = nConnString
                Me.Database = nDatabase
            Catch ex As Exception
                Throw ex
            End Try
        End Sub
    End Class

End Class
Public Class ExportItemData
    Sub exportItems(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.ItemProcessing
        myProcess.ItemsExport(nCDCConnection, nErrCls)
    End Sub
    Sub exportItemsILS(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.ItemProcessing
        myProcess.ItemsExportILS(nCDCConnection, nErrCls)
    End Sub
    Sub ILSItemExport(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.ItemProcessing
        myProcess.ILSItemExport(nCDCConnection, nErrCls)
    End Sub
    Sub ILSItemBalanceExport(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.ItemProcessing
        myProcess.ILSItemBalanceExport(nCDCConnection, nErrCls)
    End Sub
    Sub ILS2WEBItemExport(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.ItemProcessing
        Dim ConfigCls As New GetConfigInfo
        Dim nRunType As Integer
        Dim nCurHour As Integer
        Dim nStrHour As Integer
        Dim nCurDay As Integer
        Dim nStrDay As Integer
        Dim nCurWeek As Integer
        Dim nStrWeek As Integer
        nCurHour = myProcess.GetCurrentHour()
        nCurDay = myProcess.GetCurrentDay()
        nCurWeek = myProcess.GetCurrentWeek()
        nStrHour = ConfigCls.getHourlyWEBItemsVal(nCDCConnection)
        nStrDay = ConfigCls.getDailyWEBItemsVal(nCDCConnection)
        nStrWeek = ConfigCls.getWeeklyWEBItemsVal(nCDCConnection)

        If nCurWeek <> nStrWeek And nStrWeek <> 0 Then
            nRunType = 1
            If myProcess.ILS2WEBItemExport(nCDCConnection, nErrCls, nRunType) = True Then
                ConfigCls.updWeeklyWEBItemsVal(nCDCConnection, nCurWeek)
                ConfigCls.updDailyWEBItemsVal(nCDCConnection, nCurDay)
                ConfigCls.updHourlyWEBItemsVal(nCDCConnection, nCurHour)
            End If
        ElseIf nCurDay <> nStrDay And nStrDay <> 0 Then
            nRunType = 1
            If myProcess.ILS2WEBItemExport(nCDCConnection, nErrCls, nRunType) = True Then
                ConfigCls.updDailyWEBItemsVal(nCDCConnection, nCurDay)
                ConfigCls.updHourlyWEBItemsVal(nCDCConnection, nCurHour)
            End If
        ElseIf nCurHour <> nStrHour And nStrHour <> 0 Then
            nRunType = 0
            If myProcess.ILS2WEBItemExport(nCDCConnection, nErrCls, nRunType) = True Then
                ConfigCls.updHourlyWEBItemsVal(nCDCConnection, nCurHour)
            End If
        End If
    End Sub
End Class
Public Class ExportPOData
    Sub ExportPOs(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.POProcessing
        myProcess.POExport(nCDCConnection, nErrCls)
    End Sub
    Sub ExportPOsILS(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.POProcessing
        myProcess.POExportILS(nCDCConnection, nErrCls)
    End Sub
    Sub ILSPOExport(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.POProcessing
        myProcess.ILSPOExport(nCDCConnection, nErrCls)
    End Sub
End Class
Public Class ExportStoreOrders
    Sub ExportStoreOrd(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.SOProcessing
        myProcess.SOExport(nCDCConnection, nErrCls)
    End Sub
    Sub ExportStoreOrdILS(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.SOProcessing
        myProcess.SOExportILS(nCDCConnection, nErrCls)
    End Sub
    Sub ILSSOExport(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.SOProcessing
        myProcess.ILSSOExport(nCDCConnection, nErrCls)
    End Sub
End Class
Public Class ExportEDIInvoices
    Sub ExportEDIInvs(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.EDIProcessing
        myProcess.EDIInvExport(nCDCConnection, nErrCls)
    End Sub
    Sub ExportWEBInvs(ByVal nCDCConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
        Dim myProcess As New Processing.EDIProcessing
        myProcess.WEBInvExport(nCDCConnection, nErrCls)
    End Sub

End Class

