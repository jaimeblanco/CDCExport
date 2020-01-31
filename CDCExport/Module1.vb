Imports CDCData.CDC
Imports System.IO
Imports CDCData
Module Module1

    Enum ExportTypes
        ITEM
        POP
        SOP
        EDIINV
        WEBINV
        BOGUS
    End Enum
    Enum ExportDest
        GP
        ILS
    End Enum

    Sub Main()
        Try
            Dim myapp As New CDCConnection
            Dim _exportType As Integer = ExportTypes.ITEM
            'Dim _defaultConfigLocation As String = "C:\Projects\CDCInterface\Config.xml"
            Dim _defaultConfigLocation As String = "C:\Blanco\Repos\CDCExport\CDCExport\Config.xml"
            '/CC:\Projects\CDCExport\config.xml

            myapp.ConfigFileLocation = _defaultConfigLocation
            'For Each a In My.Application.CommandLineArgs
            '    Select Case UCase(Mid(a, 1, 2))
            '        Case "/?", "?H"
            '            MsgBox("Set Config File Location as /C[FileLocation and name]" & vbCr _
            '                   & "Example:  /Cc:\apps\CDCExport\config.xml")
            '        Case "/C"
            '            myapp.ConfigFileLocation = Mid(a.ToString, 3, Len(a.ToString) - 2)
            '        Case Else
            '            myapp = Nothing
            '            Throw New ApplicationException(a.ToString & " is not a valid Parameter")
            '    End Select
            'Next

            If Not File.Exists(myapp.ConfigFileLocation) Then
                MsgBox(myapp.ConfigFileLocation & " does not exist!", MsgBoxStyle.Critical)
                Throw New ApplicationException(myapp.ConfigFileLocation & " does not exist!")
            Else
                Dim configXML = XDocument.Load(My.Settings.ConfigLocation)

                myapp.ConfigXml = configXML
                myapp.Database = (From ele In configXML...<Config> _
                                          Select ele.<Catalog>.Value.Trim).SingleOrDefault
                myapp.Datasource = (From ele In configXML...<Config> _
                                          Select ele.<DSN>.Value.Trim).SingleOrDefault
                myapp.ConnectionString = (From ele In configXML...<Config> _
                                         Select ele.<ConnectionString>.Value.Trim).SingleOrDefault
                myapp.ILSDatabase = (From ele In configXML...<Config> _
                          Select ele.<ILSCatalog>.Value.Trim).SingleOrDefault
                myapp.ILSDatasource = (From ele In configXML...<Config> _
                                          Select ele.<ILSDSN>.Value.Trim).SingleOrDefault
                myapp.ILSConnectionString = (From ele In configXML...<Config> _
                                         Select ele.<ILSConnectionString>.Value.Trim).SingleOrDefault
                myapp.HPBDatabase = (From ele In configXML...<Config> _
                          Select ele.<HPBCatalog>.Value.Trim).SingleOrDefault
                myapp.HPBDatasource = (From ele In configXML...<Config> _
                                          Select ele.<HPBDSN>.Value.Trim).SingleOrDefault
                myapp.HPBConnectionString = (From ele In configXML...<Config> _
                                         Select ele.<HPBConnectionString>.Value.Trim).SingleOrDefault

                For Each ele In configXML...<errOutputFile>
                    If Not Directory.Exists(Path.GetDirectoryName(ele.Value.Trim)) Then
                        Directory.CreateDirectory(Path.GetDirectoryName(ele.Value.Trim))
                    End If
                Next

                For Each ele In configXML...<xmlOutputFile>
                    If Not Directory.Exists(Path.GetDirectoryName(ele.Value.Trim)) Then
                        Directory.CreateDirectory(Path.GetDirectoryName(ele.Value.Trim))
                    End If
                Next
            End If

            'run DIPS export
            Dim _exportDest = (From ele In myapp.ConfigXml...<Config> _
                                Select ele.<ExportDest>.Value.Trim).SingleOrDefault

            For Each ele In myapp.ConfigXml...<ExportType>
                Dim _exportTypeName = ele.Attribute("Type").Value.Trim.ToUpper
                Select Case _exportTypeName
                    Case "ITEM"
                        _exportType = ExportTypes.ITEM
                        Dim myExport As New CDCData.ExportItemData
                        Dim nErrCls As New CDCErrorClass
                        If _exportDest = "GP" Then
                            myExport.exportItems(myapp, nErrCls)
                        Else
                            myExport.exportItemsILS(myapp, nErrCls)
                        End If
                        nErrCls = Nothing
                    Case "POP"
                        _exportType = ExportTypes.POP
                        Dim POExport As New CDCData.ExportPOData
                        Dim nErrCls As New CDCErrorClass
                        If _exportDest = "GP" Then
                            POExport.ExportPOs(myapp, nErrCls)
                        Else
                            POExport.ExportPOsILS(myapp, nErrCls)
                        End If
                        nErrCls = Nothing
                    Case "SOP"
                        _exportType = ExportTypes.SOP
                        Dim SOExport As New CDCData.ExportStoreOrders
                        Dim nErrCls As New CDCErrorClass
                        If _exportDest = "GP" Then
                            SOExport.ExportStoreOrd(myapp, nErrCls)
                        Else
                            SOExport.ExportStoreOrdILS(myapp, nErrCls)
                        End If
                        nErrCls = Nothing
                    Case "EDIINV"
                        _exportType = ExportTypes.EDIINV
                        Dim IDEExport As New CDCData.ExportEDIInvoices
                        Dim nErrCls As New CDCErrorClass
                        If _exportDest = "GP" Then
                            IDEExport.ExportEDIInvs(myapp, nErrCls)
                        End If
                        nErrCls = Nothing
                    Case "WEBINV"
                        _exportType = ExportTypes.WEBINV
                        Dim EDIExport As New CDCData.ExportEDIInvoices
                        Dim nErrCls As New CDCErrorClass
                        If _exportDest = "GP" Then
                            EDIExport.ExportWEBInvs(myapp, nErrCls)
                        End If
                        nErrCls = Nothing
                    Case Else
                        _exportType = ExportTypes.BOGUS
                End Select
            Next

            'run ILS export........
            Dim _ILSexportDest = (From ele In myapp.ConfigXml...<Config> _
                    Select ele.<ILSExportDest>.Value.Trim).SingleOrDefault

            For Each ele In myapp.ConfigXml...<ExportType>
                Dim _exportTypeName = ele.Attribute("Type").Value.Trim.ToUpper
                Select Case _exportTypeName
                    Case "ITEM2WEB"
                        _exportType = ExportTypes.ITEM
                        Dim myExport As New CDCData.ExportItemData
                        Dim nErrCls As New CDCErrorClass
                        If _ILSexportDest = "ILS" Then
                            myExport.ILS2WEBItemExport(myapp, nErrCls)
                        End If
                        nErrCls = Nothing
                    Case "ITEMBALANCE"
                        _exportType = ExportTypes.ITEM
                        Dim myExport As New CDCData.ExportItemData
                        Dim nErrCls As New CDCErrorClass
                        If _ILSexportDest = "ILS" Then
                            myExport.ILSItemBalanceExport(myapp, nErrCls)
                        End If
                        nErrCls = Nothing
                    Case "POP"
                        _exportType = ExportTypes.POP
                        Dim POExport As New CDCData.ExportPOData
                        Dim nErrCls As New CDCErrorClass
                        If _ILSexportDest = "ILS" Then
                            POExport.ILSPOExport(myapp, nErrCls)
                        End If
                        nErrCls = Nothing
                    Case "SOP"
                        _exportType = ExportTypes.SOP
                        Dim SOExport As New CDCData.ExportStoreOrders
                        Dim nErrCls As New CDCErrorClass
                        If _ILSexportDest = "ILS" Then
                            SOExport.ILSSOExport(myapp, nErrCls)
                        End If
                        nErrCls = Nothing
                    Case Else
                        _exportType = ExportTypes.BOGUS
                End Select
            Next

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
        ' MsgBox("Processing Complete")
    End Sub

End Module
