Imports System.IO
Imports CDCData

Module Processing
    Friend Class ItemProcessing
        Public Sub ItemsExport(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)

            Dim _lastKeyField As String = String.Empty
            Dim db As New CDCDataDataContext
            Dim x As String
            Dim ConfigCls As New GetConfigInfo
            Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "ITEM")
            Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "ITEM")
            Dim _MaxExport As Integer = ConfigCls.getMaxRecordsPerXML(nConnection, "ITEM")
            Dim _FileExtension = ConfigCls.getGPFileExtention(nConnection, "ITEM")
            Dim _GoLiveDate As Date = ConfigCls.getGoLiveDate(nConnection)
            Dim _SplitItem As String = ConfigCls.getSplitItemVal(nConnection)
            Dim _x As Integer = 0
            Dim _GPPubPrice As String
            Dim _GPStatus As Integer
            Dim _DnldStatus() As String = {"10", "20", "95"}

            Try
                db.Connection.ConnectionString = nConnection.ConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.Database)

                Dim myXML = <Items></Items>
                Dim query = From rec In db.vwNewItems
                            Select rec

                For Each rec In query
                    x = rec.ItemCode
                    If _MaxExport <> 0 AndAlso _x >= _MaxExport Then
                        myXML.Save(GetFileName(_FileExportLocation, "ITEMS", _FileExtension))
                        myXML = <Items></Items>
                        _x = 0
                    End If

                    If Len(rec.WhsleText) > 1 Then
                        _GPPubPrice = rec.WhsleText.ToString
                    Else
                        _GPPubPrice = rec.MSRP.ToString
                    End If

                    Select Case rec.Status
                        Case 95
                            _GPStatus = 3
                        Case 10
                            _GPStatus = 0
                        Case Else
                            _GPStatus = 1
                    End Select

                    _lastKeyField = GetGPItemNo(rec.ItemCode, rec.Company, rec.CreateDate, _GoLiveDate).Trim

                    If rec.Company.ToUpper = "TTB" And _SplitItem = "Y" Then
                        myXML.Add(<Item>
                                      <ItemNumber><%= GetGPItemNo(rec.ItemCode, rec.Company, rec.CreateDate, _GoLiveDate).Trim %></ItemNumber>
                                      <Description><%= rec.Description.Trim %></Description>
                                      <ShortDesc><%= rec.ISBN.Trim %></ShortDesc>
                                      <GenericDesc><%= rec.UPC.Trim %></GenericDesc>
                                      <Company><%= "HPB" %></Company>
                                      <Cost><%= rec.Cost %></Cost>
                                      <ListPrice><%= rec.Price %></ListPrice>
                                      <UserCat1><%= rec.DistCat.Trim %></UserCat1>
                                      <UserCat2><%= _GPPubPrice.Trim %></UserCat2>
                                      <UserCat3><%= rec.HPBProdType.Trim %></UserCat3>
                                      <UserCat4><%= rec.Markdown.Trim %></UserCat4>
                                      <UserCat5><%= rec.TTBProdType.Trim %></UserCat5>
                                      <UserCat6><%= rec.SchemeID.Trim %></UserCat6>
                                      <WrntyDays><%= rec.UnitsPer %></WrntyDays>
                                      <ClassID><%= rec.Section.Trim %></ClassID>
                                      <UOM><%= "EACH" %></UOM>
                                      <ItemType>Sales Inventory</ItemType>
                                      <SalesTax>Base On Customers</SalesTax>
                                      <PurchaseTax>NonTaxible</PurchaseTax>
                                      <ABCCode><%= _GPStatus %></ABCCode>
                                      <VendorID><%= rec.VendorID.Trim %></VendorID>
                                      <VendorItm><%= GetGPItemNo(rec.ItemCode, rec.Company, rec.CreateDate, _GoLiveDate).Trim %></VendorItm>
                                      <VendorDesc><%= rec.Description.Trim %></VendorDesc>
                                      <MrkDwnPrcLv><%= rec.MarkDownPrcLv %></MrkDwnPrcLv>
                                      <UseItem>0</UseItem>
                                  </Item>)
                        _x += 1
                    End If

                    myXML.Add(<Item>
                                  <ItemNumber><%= GetGPItemNo(rec.ItemCode, rec.Company, rec.CreateDate, _GoLiveDate).Trim %></ItemNumber>
                                  <Description><%= rec.Description.Trim %></Description>
                                  <ShortDesc><%= rec.ISBN.Trim %></ShortDesc>
                                  <GenericDesc><%= rec.UPC.Trim %></GenericDesc>
                                  <Company><%= rec.Company.Trim %></Company>
                                  <Cost><%= rec.Cost %></Cost>
                                  <ListPrice><%= rec.Price %></ListPrice>
                                  <UserCat1><%= rec.DistCat.Trim %></UserCat1>
                                  <UserCat2><%= _GPPubPrice.Trim %></UserCat2>
                                  <UserCat3><%= rec.HPBProdType.Trim %></UserCat3>
                                  <UserCat4><%= rec.Markdown.Trim %></UserCat4>
                                  <UserCat5><%= rec.TTBProdType.Trim %></UserCat5>
                                  <UserCat6><%= rec.SchemeID.Trim %></UserCat6>
                                  <WrntyDays><%= rec.UnitsPer %></WrntyDays>
                                  <ClassID><%= rec.Section.Trim %></ClassID>
                                  <UOM><%= "EACH" %></UOM>
                                  <ItemType>Sales Inventory</ItemType>
                                  <SalesTax>Base On Customers</SalesTax>
                                  <PurchaseTax>NonTaxible</PurchaseTax>
                                  <ABCCode><%= _GPStatus %></ABCCode>
                                  <VendorID><%= rec.VendorID.Trim %></VendorID>
                                  <VendorItm><%= GetGPItemNo(rec.ItemCode, rec.Company, rec.CreateDate, _GoLiveDate).Trim %></VendorItm>
                                  <VendorDesc><%= rec.Description.Trim %></VendorDesc>
                                  <MrkDwnPrcLv><%= rec.MarkDownPrcLv %></MrkDwnPrcLv>
                                  <UseItem>1</UseItem>
                              </Item>)
                    _x += 1

                    Console.WriteLine("{0}, {1}, {2}", "DIPS Item Export", rec.ItemCode.Trim, rec.Description.Trim)
                    Try
                        db.DS_ProdMstUpdateStatus(rec.ItemCode, rec.Status)

                    Catch ex As Exception
                        'Throw ex
                        With nErrCls
                            .ErrNumber = 50
                            .ErrExceptionObj = ex
                            .ErrMessage = "Error exporting DIPS items. Last known item#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                            .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                        End With
                    End Try
                Next

                If myXML.HasElements = True Then
                    myXML.Save(GetFileName(_FileExportLocation, "ITEMS", _FileExtension))
                End If
                db.Connection.Close()
                db.Connection.Dispose()
            Catch ex As Exception
                'Throw ex
                With nErrCls
                    .ErrNumber = 50
                    .ErrExceptionObj = ex
                    .ErrMessage = "Error exporting DIPS items. Last known item#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                End With
                If nErrCls.ErrorsExist = True Then
                    nErrCls.ErrXml.Save(_ErrorFileLocation)
                End If
            End Try

        End Sub

        Public Sub ItemsExportILS(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)

            Dim _lastKeyField As String = String.Empty
            Dim db As New CDCDataDataContext
            Dim x As String
            Dim ConfigCls As New GetConfigInfo
            Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "ITEM")
            Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "ITEM")
            Dim _MaxExport As Integer = ConfigCls.getMaxRecordsPerXML(nConnection, "ITEM")
            Dim _FileExtension = ConfigCls.getILSFileExtention(nConnection, "ITEM")
            Dim _GoLiveDate As Date = ConfigCls.getGoLiveDate(nConnection)
            Dim _x As Integer = 0
            Dim _GPPubPrice As String
            Dim _GPStatus As Integer
            Dim _storageTemplate As String = String.Empty
            Dim _height As Integer = 0
            Dim _width As Integer = 0
            Dim _length As Integer = 0

            Dim _DnldStatus() As String = {"10", "20", "95"}

            Try
                db.Connection.ConnectionString = nConnection.ConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.Database)

                Dim myXML = <Items></Items>

                Dim query = From rec In db.vwNewItems
                            Select rec

                For Each rec In query
                    x = rec.ItemCode
                    If _MaxExport <> 0 AndAlso _x >= _MaxExport Then
                        myXML.Save(GetFileName(_FileExportLocation, "ITEMS", _FileExtension))
                        myXML = <Items></Items>
                        _x = 0
                    End If

                    If Len(rec.WhsleText) > 1 Then
                        _GPPubPrice = rec.WhsleText.ToString
                    Else
                        _GPPubPrice = rec.MSRP.ToString
                    End If

                    Select Case rec.Status
                        Case 95
                            _GPStatus = 4
                        Case 10
                            _GPStatus = 1
                        Case Else
                            _GPStatus = 2
                    End Select

                    If rec.Company = "TTB" Then
                        _height = 0
                        _width = 0
                        _length = 0
                        _storageTemplate = "EA-CS-PL"
                    Else
                        _height = 1
                        _width = 1
                        _length = 1
                        _storageTemplate = "EA"
                    End If

                    If rec.DistCat = "Assortment" Then
                        _storageTemplate = "EA-Assortment"
                    End If
                    _lastKeyField = GetGPItemNo(rec.ItemCode, rec.Company, rec.CreateDate, _GoLiveDate).Trim

                    myXML.Add(<Item>
                                  <Action>SAVE</Action>
                                  <UserDef1><%= rec.UnitsPer %></UserDef1>
                                  <UserDef2><%= _GPStatus %></UserDef2>
                                  <UserDef3><%= rec.Markdown.Trim %></UserDef3>
                                  <UserDef4><%= rec.Cost %></UserDef4>
                                  <UserDef5><%= _GPPubPrice.Trim %></UserDef5>
                                  <UserDef6><%= GetGPItemNo(rec.ItemCode, rec.Company, rec.CreateDate, _GoLiveDate).Trim %></UserDef6>
                                  <Company><%= rec.Company.Trim %></Company>
                                  <Desc><%= rec.Description.Trim %></Desc>
                                  <Item><%= GetGPItemNo(rec.ItemCode, rec.Company, rec.CreateDate, _GoLiveDate).Trim %></Item>
                                  <ItemCategories>
                                      <Category1><%= rec.TTBProdType.Trim %></Category1>
                                      <Category2><%= rec.HPBProdType.Trim %></Category2>
                                      <Category3><%= rec.SchemeID.Trim %></Category3>
                                      <Category4><%= rec.DistCat.Trim %></Category4>
                                      <Category5><%= rec.Section.Trim %></Category5>
                                  </ItemCategories>
                                  <InventoryTracking>Y</InventoryTracking>
                                  <ListPrice><%= rec.MSRP %></ListPrice>
                                  <NetPrice><%= rec.Price %></NetPrice>
                                  <Nmfc>
                                      <Identifier>60</Identifier>
                                  </Nmfc>
                                  <StorageTemplate>
                                      <Template><%= _storageTemplate.Trim %></Template>
                                  </StorageTemplate>
                                  <Style><%= rec.ISBN %></Style>
                                  <UOMS>
                                      <UOM>
                                          <Action>Save</Action>
                                          <ConvQty>1</ConvQty>
                                          <Height><%= _height.ToString %></Height>
                                          <Length><%= _length.ToString %></Length>
                                          <QtyUm>EA</QtyUm>
                                          <Sequence>1</Sequence>
                                          <Width><%= _width.ToString %></Width>
                                      </UOM>
                                  </UOMS>
                                  <XRefs>
                                      <%= IIf(Not String.IsNullOrEmpty(rec.ISBN.Trim) AndAlso rec.ISBN.Trim.Length > 3,
                                          <XRef>
                                              <Action>Save</Action>
                                              <XRefItem><%= rec.ISBN.Trim %></XRefItem>
                                              <XRefUM>EA</XRefUM>
                                          </XRef>, "") %>

                                      <%= IIf(Not String.IsNullOrEmpty(rec.UPC.Trim) AndAlso rec.UPC.Trim.Length > 3,
                                          <XRef>
                                              <Action>Save</Action>
                                              <XRefItem><%= rec.UPC.Trim %></XRefItem>
                                              <XRefUM>EA</XRefUM>
                                          </XRef>, "") %>
                                  </XRefs>
                              </Item>)
                    _x += 1

                    Console.WriteLine("{0}, {1}, {2}", "DIPS Item Export", rec.ItemCode.Trim, rec.Description.Trim)

                    Try
                        db.DS_ProdMstUpdateStatus(rec.ItemCode, rec.Status)

                    Catch ex As Exception
                        'Throw ex
                        With nErrCls
                            .ErrNumber = 60
                            .ErrExceptionObj = ex
                            .ErrMessage = "Error exporting ILS items. Last known item#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                            .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                        End With
                    End Try
                Next

                If myXML.HasElements = True Then
                    myXML.Save(GetFileName(_FileExportLocation, "ITEMS", _FileExtension))
                End If
            Catch ex As Exception
                'Throw ex
                With nErrCls
                    .ErrNumber = 60
                    .ErrExceptionObj = ex
                    .ErrMessage = "Error exporting ILS items. Last known item#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                End With
                If nErrCls.ErrorsExist = True Then
                    nErrCls.ErrXml.Save(_ErrorFileLocation)
                End If
            End Try

        End Sub

        Public Sub ILSItemExport(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)

        End Sub

        Public Sub ILSItemBalanceExport(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)

            'export item balances from ILS to GP....
            'declarations....
            Dim _lastKeyField As String = String.Empty
            Dim db As New ILSDataDataContext
            Dim x As String
            Dim ConfigCls As New GetConfigInfo
            Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "ITEMBALANCE")
            Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "ITEMBALANCE")
            Dim _MaxExport As Integer = ConfigCls.getMaxRecordsPerXML(nConnection, "ITEMBALANCE")
            Dim _FileExtension = ConfigCls.getILSFileExtention(nConnection, "ITEMBALANCE")
            Dim _x As Integer = 0

            Try
                'foreach loop for each item in balance upload table.....
                db.Connection.ConnectionString = nConnection.ILSConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.ILSDatabase)

                Dim myXML = <Items></Items>

                Dim query = From rec In db.vwItemBalance2Exports
                            Select rec

                For Each rec In query

                    'x = rec.ITEM
                    If _MaxExport <> 0 AndAlso _x >= _MaxExport Then
                        myXML.Save(GetFileName(_FileExportLocation, "ITEMBALANCE", _FileExtension))
                        myXML = <Items></Items>
                        _x = 0
                    End If
                    _lastKeyField = rec.ITEM.ToString()

                    myXML.Add(<Item>
                                  <Company><%= rec.COMPANY.Trim %></Company>
                                  <ItemCode><%= rec.ITEM.Trim %></ItemCode>
                                  <Desc><%= rec.ITEM_DESC.Trim %></Desc>
                                  <ISBN><%= rec.ISBN.Trim %></ISBN>
                                  <OnHandQTY><%= rec.ON_HAND_QTY.Value %></OnHandQTY>
                                  <AllocQTY><%= rec.ALLOCATED_QTY.Value %></AllocQTY>
                                  <AvailQTY><%= rec.AVAILQTY.Value %></AvailQTY>
                                  <UOM><%= rec.QUANTITY_UM.Trim %></UOM>
                                  <Warehouse><%= rec.WAREHOUSE.Trim %></Warehouse>
                                  <User><%= rec.USER_STAMP.Trim %></User>
                                  <DateTime><%= rec.DATE_TIME_STAMP %></DateTime>
                              </Item>)
                    _x += 1

                    Console.WriteLine("{0}, {1}, {2}", "ILS Item Balance Export ", rec.ITEM.Trim, rec.ITEM_DESC.Trim)

                    Try
                        'delete from ILS item balance upload table.....
                        ' db.CDC_DeleteItemFromUploadTbl(rec.ITEM)
                    Catch ex As Exception
                        With nErrCls
                            .ErrNumber = 71
                            .ErrExceptionObj = ex
                            .ErrMessage = "Error updating ILS item balance upload table. Last known item#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                            .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                        End With
                    End Try
                Next
            Catch ex As Exception
                With nErrCls
                    .ErrNumber = 71
                    .ErrExceptionObj = ex
                    .ErrMessage = "Error exporting ILS item balance. Last known item#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                End With
                If nErrCls.ErrorsExist = True Then
                    nErrCls.ErrXml.Save(_ErrorFileLocation)
                End If
            End Try

        End Sub

        Friend Function GetCurrentHour() As Integer
            Dim _hour As Integer
            _hour = DatePart(DateInterval.Hour, DateTime.Now)
            If _hour = 0 Then
                _hour = 24
            End If
            Return _hour
        End Function
        Friend Function GetCurrentDay() As Integer
            Dim _day As Integer
            _day = DatePart(DateInterval.Day, DateTime.Now)
            Return _day
        End Function
        Friend Function GetCurrentWeek() As Integer
            Dim _week As Integer
            _week = DatePart(DateInterval.WeekOfYear, DateTime.Now)
            Return _week
        End Function

        Public Function ILS2WEBItemExport(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass, ByVal nRunType As Integer) As Boolean

            Dim _success As Boolean = False
            Dim _lastKeyField As String = String.Empty
            Dim db As New ILSDataDataContext
            Dim ConfigCls As New GetConfigInfo
            Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "ITEM2WEB")
            Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "ITEM2WEB")
            Dim _MaxExport As Integer = ConfigCls.getMaxRecordsPerXML(nConnection, "ITEM2WEB")
            Dim _FileExtension = ConfigCls.getILSFileExtention(nConnection, "ITEM2WEB")
            Dim _fileProcLocType = ConfigCls.getWEBItemFileSaveVal(nConnection)
            Dim _x As Integer = 0
            '_run flag determines whether to run hourly vs weekly based on saved config values....
            Dim _runFlag As Integer = nRunType
            Dim _FileName As String = "WEBITEM"

            Try
                db.Connection.ConnectionString = nConnection.ILSConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.ILSDatabase)

                Dim myXML = <Items></Items>

                If _runFlag = 0 Then 'hourly file....
                    _FileName = "WEBITEMSUM"
                    Dim query = From rec In db.vwWEBItemSummaries
                                Select rec  '' change to new ILS item view 'vwWEBItemSummary'

                    For Each rec In query
                        If _MaxExport <> 0 AndAlso _x >= _MaxExport Then
                            'myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                            If _fileProcLocType = 0 And _runFlag = 0 Then
                                myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                                myXML.Save(GetFileName("\\olive\TXBK\Import\", _FileName, _FileExtension))
                            ElseIf _fileProcLocType = 1 And _runFlag = 0 Then
                                myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                            ElseIf (_fileProcLocType = 2 Or _fileProcLocType = 0) And _runFlag = 1 Then
                                myXML.Save(GetFileName("\\olive\TXBK\Import\", _FileName, _FileExtension))
                            Else
                                myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                            End If
                            myXML = <Items></Items>
                            _x = 0
                        End If

                        _lastKeyField = rec.ITEM.Trim

                        myXML.Add(<Item>
                                      <ItemCode><%= rec.ITEM.Trim %></ItemCode>
                                      <AvailQty><%= rec.AvailableQty %></AvailQty>
                                  </Item>)
                        _x += 1

                        Console.WriteLine("{0}, {1}", "ILS to WEB Item Export", rec.ITEM.Trim)
                    Next
                Else    'weekly file....
                    _FileName = "WEBITEMDTL"
                    _FileExtension = ".iwdxml.xml"
                    Dim query = From rec In db.vwWEBItemsDetails
                                Select rec  '' change to new ILS item view 'vwWEBItemsDetail'

                    For Each rec In query
                        If _MaxExport <> 0 AndAlso _x >= _MaxExport Then
                            'myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                            If _fileProcLocType = 0 And _runFlag = 0 Then
                                myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                                myXML.Save(GetFileName("\\olive\TXBK\Import\", _FileName, _FileExtension))
                            ElseIf _fileProcLocType = 1 And _runFlag = 0 Then
                                myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                            ElseIf (_fileProcLocType = 2 Or _fileProcLocType = 0) And _runFlag = 0 Then
                                myXML.Save(GetFileName("\\olive\TXBK\Import\", _FileName, _FileExtension))
                            Else
                                myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                            End If
                            myXML = <Items></Items>
                            _x = 0
                        End If

                        _lastKeyField = rec.ITEM.Trim

                        myXML.Add(<Item>
                                      <ItemCode><%= rec.ITEM.Trim %></ItemCode>
                                      <ItemDesc><%= rec.ITEM_DESC.Trim %></ItemDesc>
                                      <ISBNUPC><%= rec.ISBN_UPC.Trim %></ISBNUPC>
                                      <Cost><%= rec.Cost %></Cost>
                                      <Price><%= rec.Price %></Price>
                                      <MSRP><%= rec.MSRP %></MSRP>
                                      <AvailQty><%= rec.AvailableQty %></AvailQty>
                                      <BookType><%= rec.BookType.Trim %></BookType>
                                      <Length><%= rec.Lenght %></Length>
                                      <Width><%= rec.Width %></Width>
                                      <Height><%= rec.Height %></Height>
                                      <Weight><%= rec.Weight %></Weight>
                                      <SectionCode><%= rec.SectionCode %></SectionCode>
                                      <PublisherName><%= rec.PublisherName.Trim %></PublisherName>
                                      <PubDate><%= rec.PubDate %></PubDate>
                                      <Author><%= rec.Author.Trim %></Author>
                                      <Subtitle><%= rec.Subtitle.Trim %></Subtitle>
                                      <NrOfPages><%= rec.NrOfPages %></NrOfPages>
                                      <BISACSubDesc><%= rec.BISACSubjectDesc.Trim %></BISACSubDesc>
                                      <AcademicSubDesc><%= rec.AcademicSubjectDesc.Trim %></AcademicSubDesc>
                                  </Item>)
                        _x += 1

                        Console.WriteLine("{0}, {1}, {2}", "ILS to WEB Item Export", rec.ITEM.Trim, rec.ITEM_DESC.Trim)
                    Next
                End If

                'save file....
                If myXML.HasElements = True Then
                    If _fileProcLocType = 0 And _runFlag = 0 Then
                        myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                        myXML.Save(GetFileName("\\olive\TXBK\Import\", _FileName, _FileExtension))
                    ElseIf _fileProcLocType = 1 And _runFlag = 0 Then
                        myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                    ElseIf (_fileProcLocType = 2 Or _fileProcLocType = 0) And _runFlag = 1 Then
                        myXML.Save(GetFileName("\\olive\TXBK\Import\", _FileName, _FileExtension))
                    Else
                        myXML.Save(GetFileName(_FileExportLocation, _FileName, _FileExtension))
                    End If
                    _success = True
                End If
                db.Connection.Close()
                db.Connection.Dispose()
                Return _success
            Catch ex As Exception
                'Throw ex
                _success = False
                With nErrCls
                    .ErrNumber = 250
                    .ErrExceptionObj = ex
                    .ErrMessage = "Error exporting ILS items for WEB. Last known item#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                End With
                If nErrCls.ErrorsExist = True Then
                    nErrCls.ErrXml.Save(_ErrorFileLocation)
                End If
                Return _success
            End Try
        End Function
    End Class
    Friend Class EDIProcessing
        Public Sub EDIInvExport(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)

            Dim _lastKeyField As String = String.Empty
            Dim db As New HPBDataContext
            Dim ConfigCls As New GetConfigInfo
            Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "EDIINV")
            Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "EDIINV")
            Dim _FileExtenson = ConfigCls.getGPFileExtention(nConnection, "EDIINV")
            Dim _DocName As String

            Try
                db.Connection.ConnectionString = nConnection.HPBConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.HPBDatabase)

                Dim query = From rec In db.vw_BT_Inv_Hdrs
                            Select rec

                For Each rec In query

                    _lastKeyField = rec.InvoiceNo

                    Dim myXML = <EDIInvoices></EDIInvoices>
                    myXML.Add(<EDIInvoice>
                                  <InvoiceID><%= rec.InvoiceID %></InvoiceID>
                                  <InvoiceNo><%= rec.InvoiceNo.Trim %></InvoiceNo>
                                  <IssueDate><%= rec.IssueDate.Trim %></IssueDate>
                                  <VendorID><%= rec.VendorID.Trim %></VendorID>
                                  <PONumber><%= rec.PONumber.Trim %></PONumber>
                                  <ReferenceNo><%= rec.ReferenceNo.Trim %></ReferenceNo>
                                  <ShipToLoc><%= rec.ShipToLoc.Trim %></ShipToLoc>
                                  <ShipToSAN><%= rec.ShipToSAN.Trim %></ShipToSAN>
                                  <BillToLoc><%= rec.BillToLoc.Trim %></BillToLoc>
                                  <BillToSAN><%= rec.BillToSAN.ToString().Trim %></BillToSAN>
                                  <ShipFromLoc><%= rec.ShipFromLoc %></ShipFromLoc>
                                  <ShipFromSAN><%= rec.ShipFromSAN.Trim %></ShipFromSAN>
                                  <TotalLines><%= rec.TotalLines %></TotalLines>
                                  <ToatalQty><%= rec.TotalQty %></ToatalQty>
                                  <TotalPayable><%= rec.TotalPayable %></TotalPayable>
                                  <CurrencyCode><%= rec.CurrencyCode.Trim %></CurrencyCode>
                                  <Details>
                                      <%= From Lines In db.vw_BT_Inv_Dtls
                                          Where Lines.InvoiceID = rec.InvoiceID
                                          Select <InvoiceDetail>
                                                     <ItemInvoiceID><%= Lines.ItemInvoiceID %></ItemInvoiceID>
                                                     <InvoiceID><%= Lines.InvoiceID %></InvoiceID>
                                                     <LineNo><%= Lines.LineNo.Trim %></LineNo>
                                                     <ItemIDCode><%= Lines.ItemIDCode.Trim %></ItemIDCode>
                                                     <ItemIdentifier><%= Lines.ItemIdentifier.Trim %></ItemIdentifier>
                                                     <ItemDesc><%= Lines.ItemDesc.Trim %></ItemDesc>
                                                     <InvoiceQty><%= Lines.InvoiceQty.Trim %></InvoiceQty>
                                                     <UnitPrice><%= Lines.UnitPrice.Trim %></UnitPrice>
                                                     <DiscountPrice><%= Lines.DiscountPrice.Trim %></DiscountPrice>
                                                     <DiscountCode><%= Lines.DiscountCode %></DiscountCode>
                                                     <DiscountPct><%= Lines.DiscountPct.Trim %></DiscountPct>
                                                 </InvoiceDetail> %>
                                  </Details>
                              </EDIInvoice>)
                    '_DocName = "EDIINV" + rec.InvoiceNo.ToString.Trim
                    _DocName = "EDIINV" + rec.IssueDate.ToString.Trim + rec.PONumber.ToString.Trim
                    myXML.Save(GetFileName(_FileExportLocation, _DocName, _FileExtenson))

                    Console.WriteLine("{0}, {1}, {2}", "Invoice Export", rec.InvoiceNo.Trim, rec.VendorID.Trim)
                    Try
                        db.EDI_InvoiceHdrUpdStatus(rec.InvoiceID, rec.VendorID)
                    Catch ex As Exception
                        With nErrCls
                            .ErrNumber = 810
                            .ErrExceptionObj = ex
                            .ErrMessage = "Error exporting Invoice File. Last known Invoice#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                            .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                        End With
                    End Try
                Next

            Catch ex As Exception
                With nErrCls
                    .ErrNumber = 810
                    .ErrExceptionObj = ex
                    .ErrMessage = "Error exporting Invoice File. Last known Invoice#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                End With
                If nErrCls.ErrorsExist = True Then
                    nErrCls.ErrXml.Save(_ErrorFileLocation)
                End If
            End Try
        End Sub
        Public Sub WEBInvExport(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)

            Dim _lastKeyField As String = String.Empty
            Dim db As New HPBDataContext
            Dim ConfigCls As New GetConfigInfo
            Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "WEBINV")
            Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "WEBINV")
            Dim _FileExtenson = ConfigCls.getGPFileExtention(nConnection, "WEBINV")
            Dim _DocName As String

            Try
                db.Connection.ConnectionString = nConnection.HPBConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.HPBDatabase)

                Dim query = From rec In db.vw_WEB_Invoices
                            Select rec

                For Each rec In query

                    _lastKeyField = rec.InvoiceNumber

                    Dim myXML = <WEBInvoices></WEBInvoices>
                    myXML.Add(<WEBInvoice>
                                  <InvoiceNo><%= rec.InvoiceNumber.Trim %></InvoiceNo>
                                  <IssueDate><%= rec.IssueDateTime %></IssueDate>
                                  <VendorID><%= rec.VendorID.Trim %></VendorID>
                                  <PONumber><%= rec.OrderNumber.Trim %></PONumber>
                                  <ReferenceNo><%= rec.ASNNumber.Trim %></ReferenceNo>
                                  <ShipToLoc><%= rec.ShipTo.Trim %></ShipToLoc>
                                  <TotalLines><%= rec.TotalLines %></TotalLines>
                                  <ToatalQty><%= rec.InvoiceQuantity %></ToatalQty>
                                  <TotalPayable><%= rec.TotalPayable %></TotalPayable>
                                  <PurchaseAmount><%= rec.PurchaseAmount.Value %></PurchaseAmount>
                                  <ShippingAmount><%= rec.ShippingAmount %></ShippingAmount>
                              </WEBInvoice>)
                    _DocName = "WEBINV" + rec.IssueDateTime.ToString.Trim + rec.InvoiceNumber.ToString.Trim
                    myXML.Save(GetFileName(_FileExportLocation, _DocName, _FileExtenson))

                    Console.WriteLine("{0}, {1}, {2}", "Invoice Export", rec.InvoiceNumber.Trim, rec.VendorID.Trim)
                    Try
                        db.WEB_InvoiceHdrUpdStatus(rec.InvoiceNumber, rec.OrderNumber)
                    Catch ex As Exception
                        With nErrCls
                            .ErrNumber = 810
                            .ErrExceptionObj = ex
                            .ErrMessage = "Error exporting Invoice File. Last known Invoice#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                            .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                        End With
                    End Try
                Next

            Catch ex As Exception
                With nErrCls
                    .ErrNumber = 810
                    .ErrExceptionObj = ex
                    .ErrMessage = "Error exporting Invoice File. Last known Invoice#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                End With
                If nErrCls.ErrorsExist = True Then
                    nErrCls.ErrXml.Save(_ErrorFileLocation)
                End If
            End Try
        End Sub


        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class

    Friend Class POProcessing
        Public Sub POExport(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)

            Dim _lastKeyField As String = String.Empty
            Dim db As New CDCDataDataContext
            Dim x As String
            Dim ConfigCls As New GetConfigInfo
            Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "POP")
            Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "POP")
            Dim _FileExtenson = ConfigCls.getGPFileExtention(nConnection, "POP")
            Dim _GoLiveDate As Date = ConfigCls.getGoLiveDate(nConnection)
            Dim _DocName As String

            Try
                db.Connection.ConnectionString = nConnection.ConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.Database)

                Dim query = From rec In db.vwPO2Exports
                            Select rec

                For Each rec In query

                    _lastKeyField = rec.PONumber.Trim
                    rec.PODate = FormatDateTime(rec.PODate, DateFormat.ShortDate) 'change by JoeyB
                    rec.VenTerms = "" 'set to empty for now so it will use GP data....

                    Dim myXML = <PurchaseOrders></PurchaseOrders>
                    myXML.Add(<PurchaseOrder>
                                  <PONumber><%= rec.PONumber.Trim %></PONumber>
                                  <BuyerID><%= rec.BuyerID.Trim %></BuyerID>
                                  <VendorID><%= rec.VendorID.Trim %></VendorID>
                                  <VendorName><%= rec.VenName.Trim %></VendorName>
                                  <PurchAddress1><%= rec.VenAdd1.Trim %></PurchAddress1>
                                  <PurchAddress2><%= rec.VenAdd2.Trim %></PurchAddress2>
                                  <PurchCity><%= rec.VenCity.Trim %></PurchCity>
                                  <PurchState><%= rec.VenState.Trim %></PurchState>
                                  <PurchZip><%= rec.VenZip.Trim %></PurchZip>
                                  <Terms><%= rec.VenTerms.Trim %></Terms>
                                  <DocDate><%= rec.PODate %></DocDate>
                                  <CompanyName><%= rec.ShipToName.Trim %></CompanyName>
                                  <CreateBy><%= rec.BuyerID.Trim %></CreateBy>
                                  <VendorAddressCode>PRIMARY</VendorAddressCode>
                                  <PurchaseAddressCode>PRIMARY</PurchaseAddressCode>
                                  <ShipToAddress1><%= rec.ShipToAddress1.Trim %></ShipToAddress1>
                                  <ShipToAddress2></ShipToAddress2>
                                  <ShipToState><%= rec.StateCode.Trim %></ShipToState>
                                  <ShipToCity></ShipToCity>
                                  <ShipToZip></ShipToZip>
                                  <ShipToCCode><%= rec.CountryCode.Trim %></ShipToCCode>
                                  <POType><%= rec.OrderType %></POType>
                                  <Details>
                                      <%= From Lines In db.vwPODtl2Exports
                                          Where Lines.PONumber = rec.PONumber
                                          Select <PODetail>
                                                     <LineNumber><%= Lines.POLine.Trim %></LineNumber>
                                                     <ItemNumber><%= GetGPItemNo(Lines.GPItemCode, Lines.Company, Lines.CreateDate, _GoLiveDate).Trim %></ItemNumber>
                                                     <ItemDescription><%= Lines.Description.Trim %></ItemDescription>
                                                     <VendorID><%= Lines.VendorID.Trim %></VendorID>
                                                     <OrderQty><%= Lines.OrderQty %></OrderQty>
                                                     <VendorItemNumber><%= GetGPItemNo(Lines.GPItemCode, Lines.Company, Lines.CreateDate, _GoLiveDate).Trim %></VendorItemNumber>
                                                     <VendorDesc><%= Lines.Description.Trim %></VendorDesc>
                                                     <HPBProdType><%= Lines.ProductType.Trim %></HPBProdType>
                                                     <TTBProdType><%= Lines.TTBProdType.Trim %></TTBProdType>
                                                     <SectionCode><%= Lines.SectionCode.Trim %></SectionCode>
                                                     <SchemeID><%= Lines.SchemeID.Trim %></SchemeID>
                                                     <UOM><%= Lines.UOM.Trim %></UOM>
                                                     <UnitCost><%= Lines.UnitCost %></UnitCost>
                                                     <ExtendedCost><%= Lines.ExtendedLineCost %></ExtendedCost>
                                                     <SiteID><%= Lines.Company.Trim %></SiteID>
                                                     <POType><%= rec.OrderType %></POType>
                                                 </PODetail> %>
                                  </Details>
                              </PurchaseOrder>)
                    _DocName = "PO" + rec.PONumber.ToString.Trim
                    myXML.Save(GetFileName(_FileExportLocation, _DocName, _FileExtenson))

                    Console.WriteLine("{0}, {1}, {2}", "DIPS POP Export", rec.PONumber.Trim, rec.VendorID.Trim)
                    Try
                        db.DS_OrderHdrUpdateStatus(rec.PONumber, rec.Status)
                    Catch ex As Exception
                        'Throw ex
                        With nErrCls
                            .ErrNumber = 51
                            .ErrExceptionObj = ex
                            .ErrMessage = "Error exporting DIPS PO. Last known Order#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                            .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                        End With
                    End Try
                Next
                db.Connection.Close()
                db.Connection.Dispose()
            Catch ex As Exception
                'Throw ex
                With nErrCls
                    .ErrNumber = 51
                    .ErrExceptionObj = ex
                    .ErrMessage = "Error exporting DIPS PO. Last known Order#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                End With
                If nErrCls.ErrorsExist = True Then
                    nErrCls.ErrXml.Save(_ErrorFileLocation)
                End If
            End Try
        End Sub

        Public Sub POExportILS(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
            Try
                Dim db As New CDCDataDataContext
                Dim x As String
                Dim ConfigCls As New GetConfigInfo
                Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "POP")
                Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "POP")
                Dim _FileExtenson = ConfigCls.getILSFileExtention(nConnection, "POP")
                Dim _GoLiveDate As Date = ConfigCls.getGoLiveDate(nConnection)

                Dim _DocName As String

                db.Connection.ConnectionString = nConnection.ConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.Database)

                Dim query = From rec In db.vwPO2Exports
                            Select rec

                For Each rec In query
                    Dim myXML = <Receipts></Receipts>
                    myXML.Add(<PurchaseOrder>
                                  <Action>Save</Action>
                                  <UserDef7><%= rec.VenTerms.Trim %></UserDef7>
                                  <PurchaseOrderID><%= rec.PONumber.Trim %></PurchaseOrderID>
                                  <ReceiptType><%= rec.OrderType.ToString %></ReceiptType>
                                  <Vendor>
                                      <ShipFrom><%= rec.VendorID.Trim %></ShipFrom>
                                      <ShipFromAddress>
                                          <Address1><%= rec.VenAdd1.Trim %></Address1>
                                          <City><%= rec.VenCity.Trim %></City>
                                          <Name><%= rec.VenName.Trim %></Name>
                                          <PostalCode><%= rec.VenZip.Trim %></PostalCode>
                                          <State><%= rec.VenState.Trim %></State>
                                      </ShipFromAddress>
                                      <SourceAddress>
                                          <Address1><%= rec.VenAdd1.Trim %></Address1>
                                          <City><%= rec.VenCity.Trim %></City>
                                          <PostalCode><%= rec.VenZip.Trim %></PostalCode>
                                          <State><%= rec.VenState.Trim %></State>
                                      </SourceAddress>
                                  </Vendor>
                                  <Warehouse>Lonestar</Warehouse>
                                  <Details>
                                      <%= From Lines In db.vwPODtl2Exports
                                          Where Lines.PONumber = rec.PONumber
                                          Select <PurchaseOrderDetail>
                                                     <Action>Save</Action>
                                                     <UserDef1>EACH</UserDef1>
                                                     <UserDef6><%= GetGPItemNo(Lines.GPItemCode, Lines.Company, Lines.CreateDate, _GoLiveDate).Trim %></UserDef6>
                                                     <LineNumber><%= Lines.POLine.Trim %></LineNumber>
                                                     <SKU>
                                                         <UserDef3><%= Lines.Markdown.Trim %></UserDef3>
                                                         <UserDef4><%= Lines.Cost.ToString %></UserDef4>
                                                         <UserDef5><%= Lines.MfgSuggestedPrice.ToString %></UserDef5>
                                                         <Company><%= Lines.Company.Trim %></Company>
                                                         <Cost><%= Lines.Cost %></Cost>
                                                         <Desc><%= Lines.Description.Trim %></Desc>
                                                         <Item><%= GetGPItemNo(Lines.GPItemCode, Lines.Company, Lines.CreateDate, _GoLiveDate).Trim %></Item>
                                                         <ItemCategories>
                                                             <Category1><%= Lines.SectionCode.Trim %></Category1>
                                                             <Category2><%= Lines.ProductType.Trim %></Category2>
                                                             <Category3><%= Lines.SchemeID.Trim %></Category3>
                                                             <Category4><%= Lines.DistributionCategory.Trim %></Category4>
                                                         </ItemCategories>
                                                         <ListPrice><%= Lines.MfgSuggestedPrice %></ListPrice>
                                                         <NetPrice><%= Lines.Price %></NetPrice>
                                                         <Quantity><%= Lines.OrderQty %></Quantity>
                                                     </SKU>
                                                 </PurchaseOrderDetail> %>
                                  </Details>
                              </PurchaseOrder>)
                    _DocName = "PO" + rec.PONumber.ToString.Trim
                    myXML.Save(GetFileName(_FileExportLocation, _DocName, _FileExtenson))
                    Try
                        db.DS_OrderHdrUpdateStatus(rec.PONumber, rec.Status)

                    Catch ex As Exception
                        Throw ex
                    End Try
                Next
            Catch ex As Exception
                Throw ex
            End Try
        End Sub

        Public Sub ILSPOExport(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)

            Dim _lastKeyField As String = String.Empty
            Dim db As New ILSDataDataContext
            Dim x As String
            Dim ConfigCls As New GetConfigInfo
            Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "POP")
            Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "POP")
            Dim _FileExtenson = ConfigCls.getILSFileExtention(nConnection, "POP")
            Dim _GoLiveDate As Date = ConfigCls.getGoLiveDate(nConnection)

            Dim _DocName As String
            Try
                db.Connection.ConnectionString = nConnection.ILSConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.ILSDatabase)

                Dim query = From rec In db.vwReceipts2Exports
                            Select rec

                'add join on status...

                For Each rec In query

                    _lastKeyField = rec.PurchaseOrdNum.Trim
                    Dim RCTDate = FormatDateTime(DateAndTime.Now, DateFormat.ShortDate)
                    Dim myXML = <Receipts></Receipts>
                    myXML.Add(<Receipt>
                                  <OrdType><%= rec.OrderType.Trim %></OrdType>
                                  <Company><%= rec.COMPANY.Trim %></Company>
                                  <Carrier><%= rec.CARRIER.Trim %></Carrier>
                                  <POType><%= rec.RECEIPT_TYPE.Trim %></POType>
                                  <ReceiptIDType><%= rec.RECEIPT_ID_TYPE.Trim %></ReceiptIDType>
                                  <ReceiptID><%= rec.RECEIPT_ID.Trim %></ReceiptID>
                                  <PONumber><%= rec.PurchaseOrdNum.Trim %></PONumber>
                                  <ShipFrom><%= rec.SHIP_FROM.Trim %></ShipFrom>
                                  <ShipFromName><%= rec.SHIP_FROM_NAME.Trim %></ShipFromName>
                                  <Warehouse><%= rec.WAREHOUSE.Trim %></Warehouse>
                                  <ReceptDate><%= RCTDate %></ReceptDate>
                                  <ProcType><%= rec.ProcType.Trim %></ProcType>
                                  <Details>
                                      <%= From Lines In db.vwReceiptDtls2Exports
                                          Where Lines.PURCHASE_ORDER_ID.Trim = rec.PurchaseOrdNum.Trim _
                                          And Lines.INTERNAL_RECEIPT_NUM = rec.INTERNAL_RECEIPT_NUM _
                                          And Lines.RECEIPT_ID = rec.RECEIPT_ID
                                          Select <RCTDetail>
                                                     <OrdType><%= Lines.ITEM_CATEGORY4.Trim %></OrdType>
                                                     <InternalReceiptNo><%= Lines.INTERNAL_RECEIPT_NUM %></InternalReceiptNo>
                                                     <LineNumber><%= Lines.PURCHASE_ORDER_LINE_NUMBER %></LineNumber>
                                                     <ItemNumber><%= Lines.ITEM.Trim %></ItemNumber>
                                                     <ItemDescription><%= Lines.ITEM_DESC.Trim %></ItemDescription>
                                                     <ItemStyle><%= Lines.ITEM_STYLE.Trim %></ItemStyle>
                                                     <Company><%= Lines.COMPANY.Trim %></Company>
                                                     <ReciptQty><%= Lines.TOTAL_QTY %></ReciptQty>
                                                     <VendorItemNumber><%= Lines.GPItemNo.Trim %></VendorItemNumber>
                                                     <VendorDesc><%= Lines.ITEM_DESC.Trim %></VendorDesc>
                                                     <UOM><%= "EACH" %></UOM>
                                                     <UnitCost><%= Lines.ITEM_NET_PRICE %></UnitCost>
                                                     <ExtendedCost><%= Lines.ITEM_LIST_PRICE %></ExtendedCost>
                                                     <InvStatus><%= Lines.INVENTORY_STS.Trim %></InvStatus>
                                                     <Warehouse><%= Lines.WAREHOUSE.Trim %></Warehouse>
                                                     <POType><%= rec.RECEIPT_TYPE.Trim %></POType>
                                                     <PONumber><%= Lines.PURCHASE_ORDER_ID.Trim %></PONumber>
                                                     <RctNumber><%= Lines.RECEIPT_ID.Trim %></RctNumber>
                                                 </RCTDetail> %>
                                  </Details>
                              </Receipt>)

                    _DocName = "RCT" + rec.PurchaseOrdNum.ToString.Trim
                    myXML.Save(GetFileName(_FileExportLocation, _DocName, _FileExtenson))

                    Console.WriteLine("{0}, {1}, {2}", "ILS Receipt Export", rec.PurchaseOrdNum.Trim, rec.SHIP_FROM)
                    Try
                        'need to delete orders once finished here......
                        'sp parameter....(rec.INTERNAL_RECEIPT_NUM)
                        'CDC_DeleteReceiptFromUploadTbl
                        db.CDC_DeleteReceiptFromUploadTbl(rec.INTERNAL_RECEIPT_NUM, rec.RECEIPT_ID, rec.ProcType) 'add receiptid and status to params....

                    Catch ex As Exception
                        'Throw ex
                        With nErrCls
                            .ErrNumber = 61
                            .ErrExceptionObj = ex
                            .ErrMessage = "Error exporting ILS receipt. Last known Order#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                            .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                        End With
                    End Try
                Next
                db.Connection.Close()
                db.Connection.Dispose()
            Catch ex As Exception
                'Throw ex
                With nErrCls
                    .ErrNumber = 61
                    .ErrExceptionObj = ex
                    .ErrMessage = "Error exporting ILS receipt. Last known Order#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                End With
                If nErrCls.ErrorsExist = True Then
                    nErrCls.ErrXml.Save(_ErrorFileLocation)
                End If
            End Try
        End Sub
    End Class
    Friend Class SOProcessing
        '' Store Order Processing
        Public Sub SOExport(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)

            Dim _lastKeyField As String = String.Empty
            Dim db As New CDCDataDataContext
            Dim x As String
            Dim ConfigCls As New GetConfigInfo
            Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "SOP")
            Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "SOP")
            Dim _SOPFileExtension = ConfigCls.getGPFileExtention(nConnection, "SOP")
            Dim _POPFileExtension = ConfigCls.getGPFileExtention(nConnection, "POP")
            Dim _GoLiveDate As Date = ConfigCls.getGoLiveDate(nConnection)
            Dim _DocName As String
            Dim _OrdName As String
            Dim _EndFileName As String = ""
            Dim _storeCounter As Integer = 0
            Dim _maxStoreCnt As Integer = ConfigCls.getMaxStoreCnt(nConnection)

            Try
                db.Connection.ConnectionString = nConnection.ConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.Database)

                Dim query = From rec In db.vwStorePOs2Exports
                            Select rec

                For Each rec In query
                    If String.IsNullOrEmpty(rec.OrderType) = False Then
                        Dim _POHdr = ConfigCls.getStorePOHdr(nConnection, rec.OrderType.Trim)
                        If _POHdr Is Nothing Then _POHdr = ""
                        If Not String.IsNullOrEmpty(_POHdr) Then
                            _lastKeyField = _POHdr.Trim & rec.PONumber.Trim
                            rec.PODate = FormatDateTime(rec.PODate, DateFormat.ShortDate) 'change by JoeyB

                            ' Need to Create a PO also
                            Dim myPOXML = <PurchaseOrders></PurchaseOrders>
                            myPOXML.Add(<PurchaseOrder>
                                            <PONumber><%= _POHdr.Trim & rec.PONumber.Trim %></PONumber>
                                            <BuyerID><%= rec.BuyerID.Trim %></BuyerID>
                                            <VendorID><%= rec.VendorID.Trim %></VendorID>
                                            <VendorName><%= rec.VenName.Trim %></VendorName>
                                            <PurchAddress1><%= rec.VenAdd1.Trim %></PurchAddress1>
                                            <PurchAddress2><%= rec.VenAdd2.Trim %></PurchAddress2>
                                            <PurchCity><%= rec.VenCity.Trim %></PurchCity>
                                            <PurchState><%= rec.VenState.Trim %></PurchState>
                                            <PurchZip><%= rec.VenZip.Trim %></PurchZip>
                                            <Terms><%= rec.VenTermsCode.Trim %></Terms>
                                            <DocDate><%= rec.PODate %></DocDate>
                                            <CompanyName><%= rec.ShipToName.Trim %></CompanyName>
                                            <CreateBy><%= rec.BuyerID.Trim %></CreateBy>
                                            <VendorAddressCode>PRIMARY</VendorAddressCode>
                                            <PurchaseAddressCode>PRIMARY</PurchaseAddressCode>
                                            <ShipToAddress1><%= rec.ShipToAddress1.Trim %></ShipToAddress1>
                                            <ShipToAddress2></ShipToAddress2>
                                            <ShipToState><%= rec.StateCode.Trim %></ShipToState>
                                            <ShipToCity></ShipToCity>
                                            <ShipToZip></ShipToZip>
                                            <ShipToCCode><%= rec.CountryCode.Trim %></ShipToCCode>
                                            <POType><%= rec.OrderType.Trim %></POType>
                                            <Details>
                                                <%= From Lines In db.vwPODtl2Exports
                                                    Where Lines.PONumber = rec.PONumber
                                                    Select <PODetail>
                                                               <LineNumber><%= Lines.POLine.Trim %></LineNumber>
                                                               <ItemNumber><%= GetGPItemNo(Lines.GPItemCode, Lines.Company, Lines.CreateDate, _GoLiveDate).Trim %></ItemNumber>
                                                               <ItemDescription><%= Lines.Description.Trim %></ItemDescription>
                                                               <VendorID><%= Lines.VendorID.Trim %></VendorID>
                                                               <OrderQty><%= Lines.OrderQty %></OrderQty>
                                                               <VendorItemNumber><%= GetGPItemNo(Lines.GPItemCode, Lines.Company, Lines.CreateDate, _GoLiveDate).Trim %></VendorItemNumber>
                                                               <VendorDesc><%= Lines.Description.Trim %></VendorDesc>
                                                               <HPBProdType><%= Lines.ProductType.Trim %></HPBProdType>
                                                               <TTBProdType><%= Lines.TTBProdType.Trim %></TTBProdType>
                                                               <SectionCode><%= Lines.SectionCode.Trim %></SectionCode>
                                                               <SchemeID><%= Lines.SchemeID.Trim %></SchemeID>
                                                               <UOM><%= Lines.UOM.Trim %></UOM>
                                                               <UnitCost><%= Lines.UnitCost %></UnitCost>
                                                               <ExtendedCost><%= Lines.ExtendedLineCost.Value %></ExtendedCost>
                                                               <SiteID><%= Lines.Company.Trim %></SiteID>
                                                               <POType><%= rec.OrderType.Trim %></POType>
                                                           </PODetail> %>
                                            </Details>
                                        </PurchaseOrder>)
                            _DocName = _POHdr.ToString.Trim + rec.PONumber.ToString.Trim
                            myPOXML.Save(GetFileName(_FileExportLocation, _DocName, _POPFileExtension))
                        End If

                        Console.WriteLine("{0}, {1}, {2}", "DIPS SOP Export", _POHdr.Trim & rec.PONumber.Trim, rec.VendorID)

                        Dim _SOHdr = ConfigCls.getStoreOrderHdr(nConnection, rec.OrderType.Trim)
                        If _SOHdr Is Nothing Then _SOHdr = ""
                        If Not String.IsNullOrEmpty(_SOHdr) Then
                            '' Create Orders from Requisitions
                            Dim Orders = From ord In db.vwStoreOrds2Exports
                                         Where ord.PONumber = rec.PONumber
                                         Select ord
                            Dim OrdXML = <StoreOrders></StoreOrders>
                            For Each ord In Orders
                                If _maxStoreCnt <> 0 AndAlso _storeCounter >= _maxStoreCnt Then

                                    _OrdName = _SOHdr.Trim & rec.PONumber.ToString.Trim
                                    If _SOHdr.Trim <> "DRO" Then
                                        OrdXML.Save(GetFileName(_FileExportLocation, _OrdName, _SOPFileExtension))
                                    End If

                                    OrdXML = <StoreOrders></StoreOrders>
                                    _storeCounter = 0
                                End If

                                _lastKeyField = _SOHdr.Trim & ord.RequisitionNo.Trim
                                OrdXML.Add(<StoreOrder>
                                               <POLinkNo><%= _SOHdr.Trim & ord.PONumber.Trim %></POLinkNo>
                                               <OrderNo><%= _SOHdr.Trim & ord.RequisitionNo.Trim %></OrderNo>
                                               <StoreNo><%= ord.GPCustomer.Trim %></StoreNo>
                                               <SiteID><%= ord.SiteID.Trim %></SiteID>
                                               <DocDate><%= FormatDateTime(ord.DateApprovedDisapproved, DateFormat.ShortDate) %></DocDate>
                                               <DocID><%= GetOrdType(ord.OrderType.Trim) %></DocID>
                                               <VendorID><%= ord.VendorID.Trim %></VendorID>
                                               <OrderType><%= ord.OrderType.Trim %></OrderType>
                                               <Details>
                                                   <%= From dtl In db.vwStoreOrdDtls2Exports
                                                       Where dtl.RequisitionNo = ord.RequisitionNo
                                                       Select <OrdDetail>
                                                                  <OrdKey><%= _SOHdr.Trim & ord.RequisitionNo.Trim %></OrdKey>
                                                                  <LineNumber><%= dtl.LineID.Value.ToString %></LineNumber>
                                                                  <ItemCode><%= GetGPItemNo(dtl.GPItemCode, dtl.Company, dtl.CreateDate, _GoLiveDate).Trim %></ItemCode>
                                                                  <Qty><%= dtl.OrderQty.Value %></Qty>
                                                                  <ItemDescription><%= dtl.Description.Trim %></ItemDescription>
                                                                  <UOM>EACH</UOM>
                                                                  <UnitPrice><%= Convert.ToDecimal(dtl.Price) %></UnitPrice>
                                                              </OrdDetail> %>
                                               </Details>
                                           </StoreOrder>)
                                'IIf(ord.SiteID.ToUpper.Trim = "TTB", dtl.Cost, IIf(_SOHdr.Trim = "DRO", dtl.Price, Convert.ToDecimal(0)))
                                Console.WriteLine("{0}, {1}, {2}", "DIPS Store Ord Export", _SOHdr.Trim & ord.RequisitionNo.Trim, ord.GPCustomer.Trim)
                                _storeCounter = _storeCounter + 1
                            Next
                            If _SOHdr.Trim <> "DRO" Then
                                _OrdName = _SOHdr.Trim & rec.PONumber.ToString.Trim
                                OrdXML.Save(GetFileName(_FileExportLocation, _OrdName, _SOPFileExtension))
                            Else
                                'add call into insert SOP information for DropShipments back into DIPS for Store Receiving....
                                _EndFileName = GetFileName(_FileExportLocation, _SOHdr.Trim & rec.PONumber.ToString.Trim, _SOPFileExtension)
                                _EndFileName = _EndFileName.Remove(0, _FileExportLocation.Length - _SOPFileExtension.Length)
                                If db.WMS_CDC_DropShipments(OrdXML.ToString(), _EndFileName) <> 0 Then
                                    Console.WriteLine("{0}, {1}", "HPB_db DropShipment Update Failed!!! ", _SOHdr.Trim & rec.PONumber.ToString.Trim)
                                    With nErrCls
                                        .ErrNumber = 53
                                        .ErrExceptionObj = New Exception
                                        .ErrMessage = "HPB_db DropShipment Update Failed for PO#: " & rec.PONumber
                                        .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                                    End With
                                    'update status to error....
                                    db.DS_OrderHdrUpdateStatus(rec.PONumber, 25)
                                End If

                                'newly added section for updating Silverbell/Porcupine for StoreReceiving with Dropshipments....
                                ''update above will be removed once new functions take over process..... 
                                Dim xDB As New HPBDataContext()
                                xDB.Connection.ConnectionString = nConnection.HPBConnectionString
                                xDB.Connection.Open()
                                xDB.Connection.ChangeDatabase(nConnection.HPBDatabase)

                                If xDB.HPB_CDC_DropShipments(OrdXML.ToString(), _EndFileName) <> 0 Then
                                    Console.WriteLine("{0}, {1}", "HPB_db DropShipment Update Failed!!! ", _SOHdr.Trim & rec.PONumber.ToString.Trim)
                                    With nErrCls
                                        .ErrNumber = 53
                                        .ErrExceptionObj = New Exception
                                        .ErrMessage = "HPB_db DropShipment Update Failed for PO#: " & rec.PONumber
                                        .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                                    End With
                                    'update status to error....
                                    db.DS_OrderHdrUpdateStatus(rec.PONumber, 25)
                                End If
                            End If
                        End If

                        Try
                            db.DS_OrderHdrUpdateStatus(rec.PONumber, rec.Status)
                        Catch ex As Exception
                            'Throw ex
                            With nErrCls
                                .ErrNumber = 52
                                .ErrExceptionObj = ex
                                .ErrMessage = "Error exporting DIPS orders. Last known Order#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                                .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                            End With
                        End Try
                    End If
                Next
                db.Connection.Close()
                db.Connection.Dispose()
            Catch ex As Exception
                'Throw ex
                With nErrCls
                    .ErrNumber = 52
                    .ErrExceptionObj = ex
                    .ErrMessage = "Error exporting DIPS orders. Last known Order#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                End With
                If nErrCls.ErrorsExist = True Then
                    nErrCls.ErrXml.Save(_ErrorFileLocation)
                End If
            End Try
        End Sub

        Public Sub SOExportILS(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)
            Try
                Dim db As New CDCDataDataContext
                Dim x As String
                Dim ConfigCls As New GetConfigInfo
                Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "SOP")
                Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "SOP")
                Dim _SOPFileExtension = ConfigCls.getILSFileExtention(nConnection, "SOP")
                Dim _POPFileExtension = ConfigCls.getILSFileExtention(nConnection, "POP")
                Dim _GoLiveDate As Date = ConfigCls.getGoLiveDate(nConnection)

                Dim _DocName As String
                Dim _OrdName As String

                db.Connection.ConnectionString = nConnection.ConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.Database)

                Dim query = From rec In db.vwStorePOs2Exports
                            Select rec

                For Each rec In query
                    If String.IsNullOrEmpty(rec.OrderType) = False Then
                        Dim _POHdr = ConfigCls.getStorePOHdr(nConnection, rec.OrderType.Trim)
                        ''If Not String.IsNullOrEmpty(_POHdr) AndAlso rec.OrderType.ToString = "HPBReorder" Then
                        If Not String.IsNullOrEmpty(_POHdr) Then
                            ' Need to Create a PO also
                            Dim myPOXML = <Receipts></Receipts>
                            myPOXML.Add(<PurchaseOrder>
                                            <Action>Save</Action>
                                            <UserDef7><%= rec.TermsDays %></UserDef7>
                                            <PurchaseOrderID><%= _POHdr & rec.PONumber.Trim %></PurchaseOrderID>
                                            <ReceiptType><%= rec.OrderType.Trim %></ReceiptType>
                                            <Vendor>
                                                <ShipFrom><%= rec.VendorID.Trim %></ShipFrom>
                                                <ShipFromAddress>
                                                    <Address1><%= rec.VenAdd1.Trim %></Address1>
                                                    <City><%= rec.VenCity.Trim %></City>
                                                    <Name><%= rec.VenName.Trim %></Name>
                                                    <PostalCode><%= rec.VenZip.Trim %></PostalCode>
                                                    <State><%= rec.VenState.Trim %></State>
                                                </ShipFromAddress>
                                                <SourceAddress>
                                                    <Address1><%= rec.VenAdd1.Trim %></Address1>
                                                    <City><%= rec.VenCity.Trim %></City>
                                                    <Name><%= rec.VenName.Trim %></Name>
                                                    <PostalCode><%= rec.VenZip.Trim %></PostalCode>
                                                    <State><%= rec.VenState.Trim %></State>
                                                </SourceAddress>
                                            </Vendor>
                                            <Warehouse>Lonestar</Warehouse>
                                            <Details>
                                                <%= From Lines In db.vwPODtl2Exports
                                                    Where Lines.PONumber = rec.PONumber
                                                    Select <PurchaseOrderDetail>
                                                               <Action>Save</Action>
                                                               <UserDef1>EACH</UserDef1>
                                                               <UserDef6><%= GetGPItemNo(Lines.GPItemCode, Lines.Company, Lines.CreateDate, _GoLiveDate).Trim %></UserDef6>
                                                               <LineNumber><%= Lines.POLine.Trim %></LineNumber>
                                                               <SKU>
                                                                   <UserDef3><%= Lines.Markdown.Trim %></UserDef3>
                                                                   <UserDef4><%= Lines.Cost.ToString %></UserDef4>
                                                                   <UserDef5><%= Lines.MfgSuggestedPrice.Value %></UserDef5>
                                                                   <Company><%= Lines.Company.Trim %></Company>
                                                                   <Cost><%= Lines.Cost.ToString %></Cost>
                                                                   <Desc><%= Lines.Description.Trim %></Desc>
                                                                   <Item><%= GetGPItemNo(Lines.GPItemCode, Lines.Company, Lines.CreateDate, _GoLiveDate).Trim %></Item>
                                                                   <ItemCategories>
                                                                       <Category1><%= Lines.SectionCode.Trim %></Category1>
                                                                       <Category2><%= Lines.ProductType.Trim %></Category2>
                                                                       <Category3><%= Lines.SchemeID.Trim %></Category3>
                                                                       <Category4><%= Lines.DistributionCategory.Trim %></Category4>
                                                                   </ItemCategories>
                                                                   <ListPrice><%= Lines.MfgSuggestedPrice.Value %></ListPrice>
                                                                   <NetPrice><%= Lines.Price %></NetPrice>
                                                                   <Quantity><%= Lines.OrderQty.Value %></Quantity>
                                                               </SKU>
                                                           </PurchaseOrderDetail> %>
                                            </Details>
                                        </PurchaseOrder>)
                            _DocName = _POHdr.Trim & rec.PONumber.ToString.Trim
                            myPOXML.Save(GetFileName(_FileExportLocation, _DocName, _POPFileExtension))
                        End If

                        Dim _SOHdr = ConfigCls.getStoreOrderHdr(nConnection, rec.OrderType.Trim)
                        If Not String.IsNullOrEmpty(_SOHdr) Then
                            Dim Orders = From ord In db.vwStoreOrds2Exports
                                         Where ord.PONumber = rec.PONumber
                                         Select ord
                            Dim OrdXML = <Shipments></Shipments>
                            For Each ord In Orders
                                OrdXML.Add(<Shipment>
                                               <Action>Save</Action>
                                               <AllocateComplete>Y</AllocateComplete>
                                               <ConsolidationAllowed>Y</ConsolidationAllowed>
                                               <Customer>
                                                   <Customer><%= ord.LocationNo.Trim %></Customer>
                                                   <ShipTo><%= ord.LocationNo.Trim %></ShipTo>
                                               </Customer>
                                               <CustomerPO><%= ord.RequisitionNo.Trim %></CustomerPO>
                                               <OrderType><%= ord.OrderType.Trim %></OrderType>
                                               <ShipmentID><%= ord.RequisitionNo.Trim %></ShipmentID>
                                               <StoreDistribution><%= ord.RequisitionNo %></StoreDistribution>
                                               <Warehouse>Lonestar</Warehouse>
                                               <Details>
                                                   <%= From dtl In db.vwStoreOrdDtls2Exports
                                                       Where dtl.RequisitionNo = ord.RequisitionNo
                                                       Select <ShipmentDetail>
                                                                  <Action>Save</Action>
                                                                  <ErpOrder><%= ord.RequisitionNo.Trim %></ErpOrder>
                                                                  <ErpOrderLineNum><%= dtl.RequisitionNo.Trim %></ErpOrderLineNum>
                                                                  <MarkFor><%= ord.LocationNo.Trim %></MarkFor>
                                                                  <MarkForAddress>
                                                                      <Address1><%= ord.MailToAddress2.Trim %></Address1>
                                                                      <City><%= ord.MailToCity.Trim %></City>
                                                                      <Country>US</Country>
                                                                      <Name><%= ord.Name.Trim %></Name>
                                                                      <PostalCode><%= ord.MailToZip.Trim %></PostalCode>
                                                                      <State><%= ord.MailToState.Trim %></State>
                                                                  </MarkForAddress>
                                                                  <SKU>
                                                                      <Action>Save</Action>
                                                                      <UserDef4><%= dtl.MfgSuggestedPrice %></UserDef4>
                                                                      <UserDef6><%= GetGPItemNo(dtl.GPItemCode, dtl.Company, dtl.CreateDate, _GoLiveDate).Trim %></UserDef6>
                                                                      <Company><%= dtl.Company.Trim %></Company>
                                                                      <Item><%= GetGPItemNo(dtl.GPItemCode, dtl.Company, dtl.CreateDate, _GoLiveDate).Trim %></Item>
                                                                      <ItemCategories>
                                                                          <Category3><%= dtl.SchemeID.Trim %></Category3>
                                                                          <Category4><%= dtl.DistributionCategory.Trim %></Category4>
                                                                      </ItemCategories>
                                                                      <NetPrice><%= dtl.Price %></NetPrice>
                                                                      <Quantity><%= dtl.OrderQty.Value %></Quantity>
                                                                  </SKU>
                                                                  <StoreDistribution><%= dtl.RequisitionNo.Trim %></StoreDistribution>
                                                              </ShipmentDetail> %>
                                               </Details>
                                           </Shipment>)
                            Next
                            _OrdName = _SOHdr & rec.PONumber.ToString.Trim
                            OrdXML.Save(GetFileName(_FileExportLocation, _OrdName, _SOPFileExtension))
                        End If

                    End If

                    Try
                        db.DS_OrderHdrUpdateStatus(rec.PONumber, rec.Status)

                    Catch ex As Exception
                        Throw ex
                    End Try
                Next
            Catch ex As Exception
                Throw ex
            End Try
        End Sub

        Public Sub ILSSOExport(ByVal nConnection As CDC.CDCConnection, ByVal nErrCls As CDCErrorClass)

            Dim _lastKeyField As String = String.Empty
            Dim db As New ILSDataDataContext
            Dim hpb_db As New CDCDataDataContext
            Dim x As String
            Dim ConfigCls As New GetConfigInfo
            Dim _FileExportLocation = ConfigCls.getFileExportLocation(nConnection, "SOP")
            Dim _ErrorFileLocation = ConfigCls.getErrorFileLocation(nConnection, "SOP")
            Dim _SOPFileExtension = ConfigCls.getILSFileExtention(nConnection, "SOP")
            Dim _POPFileExtension = ConfigCls.getILSFileExtention(nConnection, "POP")
            Dim _GoLiveDate As Date = ConfigCls.getGoLiveDate(nConnection)
            Dim _DocName As String
            Dim _InterfaceRecID As Integer
            Dim _EndFileName As String
            Dim _updHPBShipRecs As Boolean = False
            Dim _ProcShipXML As String = ConfigCls.getProcShipXMLVal(nConnection)
            Dim _sqlProcDate As DateTime
            _sqlProcDate = DateTime.Now()

            Try
                'HPB db
                hpb_db.Connection.ConnectionString = nConnection.ConnectionString
                hpb_db.Connection.Open()
                hpb_db.Connection.ChangeDatabase(nConnection.Database)
                'ILS db
                db.Connection.ConnectionString = nConnection.ILSConnectionString
                db.Connection.Open()
                db.Connection.ChangeDatabase(nConnection.ILSDatabase)

                Dim query = From rec In db.vwHPBOrders2Exports
                            Select rec

                For Each rec In query
                    If String.IsNullOrEmpty(rec.ORDER_TYPE.Trim) = False Then
                        Dim _POHdr = "SHP"
                        Dim _docDate As DateTime
                        _lastKeyField = rec.ERP_ORDER.Trim
                        _docDate = FormatDateTime(rec.ACTUAL_SHIP_DATE_TIME, DateFormat.ShortDate) 'change by JoeyB
                        _InterfaceRecID = rec.INTERFACE_RECORD_ID
                        _EndFileName = ""

                        Dim _HPBShipTo As String = ""
                        Dim cnt As Integer = 0
                        If rec.SHIP_TO.Trim = "MAIN" And rec.SHIP_TO_NAME.Substring(0, 4) = "HALF" Then
                            _HPBShipTo = rec.SHIP_TO_NAME.Trim
                            cnt = _HPBShipTo.Length - 3
                            _HPBShipTo = _HPBShipTo.Substring(cnt, 3).Trim
                        Else
                            _HPBShipTo = rec.SHIP_TO.Trim
                        End If

                        ' Need to Create a PO also
                        Dim myPOXML = <Shipments></Shipments>
                        myPOXML.Add(<Shipment>
                                        <ERPOrdNum><%= rec.ERP_ORDER.Trim %></ERPOrdNum>
                                        <ShipmentID><%= rec.SHIPMENT_ID.Trim %></ShipmentID>
                                        <ShipmentLoadNum><%= rec.SHIPPING_LOAD_NUM.Value %></ShipmentLoadNum>
                                        <BOLNum><%= rec.BOLNUM.Trim %></BOLNum>
                                        <PRONum><%= rec.PRONUM.Trim %></PRONum>
                                        <Company><%= rec.COMPANY.Trim %></Company>
                                        <ContainerCount><%= rec.TOTAL_CONTAINERS.Value %></ContainerCount>
                                        <BoxCount><%= rec.BoxCount.Value %></BoxCount>
                                        <TotalWeight><%= rec.TOTAL_WEIGHT %></TotalWeight>
                                        <Carrier><%= rec.CARRIER.Trim %></Carrier>
                                        <Customer><%= rec.CUSTOMER.Trim %></Customer>
                                        <CustomerName><%= rec.CUSTOMER_NAME.Trim %></CustomerName>
                                        <CustomerPO><%= IIf(rec.CUSTOMER_PO.Trim = "NA", rec.ERP_ORDER.Trim, rec.CUSTOMER_PO.Trim) %></CustomerPO>
                                        <DocDate><%= _docDate %></DocDate>
                                        <ShipTime><%= rec.ACTUAL_SHIP_DATE_TIME.Value %></ShipTime>
                                        <ShipTo><%= _HPBShipTo %></ShipTo>
                                        <ShipToName><%= rec.SHIP_TO_NAME.Trim %></ShipToName>
                                        <ShipToAttention><%= rec.SHIP_TO_ATTENTION_TO.Trim %></ShipToAttention>
                                        <ShipToAddress1><%= rec.SHIP_TO_ADDRESS1.Trim %></ShipToAddress1>
                                        <ShipToAddress2><%= rec.SHIP_TO_ADDRESS2 %></ShipToAddress2>
                                        <ShipToState><%= rec.SHIP_TO_STATE.Trim %></ShipToState>
                                        <ShipToCity><%= rec.SHIP_TO_CITY.Trim %></ShipToCity>
                                        <ShipToZip><%= rec.SHIP_TO_POSTAL_CODE.Trim %></ShipToZip>
                                        <ShipToCCode><%= rec.SHIP_TO_COUNTRY.Trim %></ShipToCCode>
                                        <BaseFreightCharge><%= rec.BASE_FREIGHT_CHARGE %></BaseFreightCharge>
                                        <TotalFreightCharge><%= rec.TOTAL_FREIGHT_CHARGE %></TotalFreightCharge>
                                        <POType><%= rec.ORDER_TYPE.ToUpper.Trim %></POType>
                                        <OrderType><%= rec.OrderType.ToUpper.Trim %></OrderType>
                                        <GLAcct><%= rec.GLAcct.Trim %></GLAcct>
                                        <InvAcct><%= rec.InvAcct.Trim %></InvAcct>
                                        <ShipmentDetails>
                                            <%= From Lines In db.vwHPBOrderDtls2Exports
                                                Where Lines.SHIPMENT_ID = rec.SHIPMENT_ID _
                                                And Lines.ERP_ORDER = rec.ERP_ORDER _
                                                And Lines.ORDER_TYPE.Substring(0, 7) = rec.ORDER_TYPE.Substring(0, 7)
                                                Select <ShipmentDetail>
                                                           <ERPOrdNum><%= Lines.ERP_ORDER.Trim %></ERPOrdNum>
                                                           <LineNumber><%= Lines.ERP_ORDER_LINE_NUM.Value %></LineNumber>
                                                           <ItemNumber><%= Lines.ITEM.Trim %></ItemNumber>
                                                           <ItemDescription><%= Lines.ITEM_DESC.Trim %></ItemDescription>
                                                           <ShippedQty><%= Lines.SHIPPED_QTY.Value %></ShippedQty>
                                                           <VendorItemNumber><%= Lines.GPItemNo.Trim %></VendorItemNumber>
                                                           <VendorDesc><%= Lines.ITEM_DESC.Trim %></VendorDesc>
                                                           <UOM><%= Lines.QUANTITY_UM.Trim %></UOM>
                                                           <UnitCost><%= Lines.ITEM_NET_PRICE.Value %></UnitCost>
                                                           <ExtendedCost><%= Lines.ITEM_LIST_PRICE.Value %></ExtendedCost>
                                                           <SiteID><%= Lines.WAREHOUSE.Trim %></SiteID>
                                                           <TrackingNum><%= Lines.TrackingNum.Trim %></TrackingNum>
                                                           <POType><%= Lines.ORDER_TYPE.ToUpper.Trim %></POType>
                                                           <POLink><%= Lines.POLink.Trim %></POLink>
                                                           <GLAcct><%= Lines.GLAcct.Trim %></GLAcct>
                                                           <InvAcct><%= Lines.InvAcct.Trim %></InvAcct>
                                                       </ShipmentDetail> %>
                                        </ShipmentDetails>
                                    </Shipment>)
                        _DocName = _POHdr.ToString.Trim + rec.SHIPMENT_ID.ToString.Trim
                        _EndFileName = GetFileName(_FileExportLocation, _DocName, _SOPFileExtension)
                        myPOXML.Save(_EndFileName)
                        'delete from ILS upload tables....
                        db.CDC_DeleteOrderFromUploadTbl(rec.ERP_ORDER.Trim, _InterfaceRecID, rec.ORDER_TYPE.Trim, rec.SHIPMENT_ID)

                        'to be removed
                        If _ProcShipXML.Trim <> "N" Then
                            'check if HPB store and update HPB_db.....
                            If rec.CUSTOMER.ToUpper.Trim = "HPB" And rec.COMPANY.ToUpper.Trim <> "SUP" Then
                                'call HPB_db and pass in the xml for processing....
                                _updHPBShipRecs = True
                                _EndFileName = _EndFileName.Remove(0, _FileExportLocation.Length - _SOPFileExtension.Length)
                                Dim _XMLStr As String = myPOXML.ToString()
                                '_XMLStr = _XMLStr.Replace("utf-8", "utf-16")
                                If hpb_db.WMS_ILS_ShipmentRecords(_XMLStr.ToString(), _EndFileName) <> 0 Then
                                    Console.WriteLine("{0}, {1}, {2}", "HPB_db Shipment Update Failed!!! ", rec.SHIPMENT_ID.Trim, rec.CUSTOMER)
                                    With nErrCls
                                        .ErrNumber = 63
                                        .ErrExceptionObj = New Exception
                                        .ErrMessage = "HPB_db Shipment Update Failed for Shipment#: " & rec.SHIPMENT_ID.Trim
                                        .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                                    End With
                                    If nErrCls.ErrorsExist = True Then
                                        nErrCls.ErrXml.Save(_ErrorFileLocation)
                                    End If
                                End If
                            End If
                        End If
                    End If

                    Console.WriteLine("{0}, {1}, {2}", "ILS Shipment Export ", rec.SHIPMENT_ID.ToString.Trim, rec.CUSTOMER)

                    'Try
                    '    'need to delete rows from upload tables once done...... 
                    '    'sp parameter (rec.ERP_ORDER.Trim ,_InterfaceRecID)
                    '    'CDC_DeleteOrderFromUploadTbl
                    '    db.CDC_DeleteOrderFromUploadTbl(rec.ERP_ORDER.Trim, _InterfaceRecID, rec.ORDER_TYPE.Trim)

                    'Catch ex As Exception
                    '    With nErrCls
                    '        .ErrNumber = 62
                    '        .ErrExceptionObj = ex
                    '        .ErrMessage = "Error exporting ILS orders. Last known Order#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    '        .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                    '    End With
                    'End Try
                Next

                'to be removed.....
                'call stored proc to update DIPS wmsshipment tables..... pass in _sqlProcDate
                If _ProcShipXML.Trim <> "N" Then
                    If _updHPBShipRecs Then
                        If hpb_db.WMS_ILS_UpdShipRecs(_sqlProcDate) <> 0 Then
                            Console.WriteLine("HPB_db Shipment Record Update Failed!!! ")
                            With nErrCls
                                .ErrNumber = 63
                                .ErrExceptionObj = New Exception
                                .ErrMessage = "HPB_db Shipment Record Update Failed for dates >: " + _sqlProcDate.ToString
                                .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                            End With
                            If nErrCls.ErrorsExist = True Then
                                nErrCls.ErrXml.Save(_ErrorFileLocation)
                            End If
                        End If
                    End If
                End If

                hpb_db.Connection.Close()
                hpb_db.Connection.Dispose()
                db.Connection.Close()
                db.Connection.Dispose()
            Catch ex As Exception
                'Throw ex
                With nErrCls
                    .ErrNumber = 62
                    .ErrExceptionObj = ex
                    .ErrMessage = "Error exporting ILS orders. Last known Order#: " & _lastKeyField.Trim & "  " & ex.Message.ToString()
                    .HandleError(.ErrNumber, .ErrMessage, .ErrExceptionObj)
                End With
                If nErrCls.ErrorsExist = True Then
                    nErrCls.ErrXml.Save(_ErrorFileLocation)
                End If
            End Try
        End Sub
    End Class

#Region "Misc Routines"
    Friend Function GetFileName(ByVal nFileDirectory As String, ByVal nDocumentNo As String, ByVal nFileExt As String) As String
        Dim _filename As String

        _filename = nDocumentNo & Now.ToString("yy") &
                Right("000" + DatePart(DateInterval.DayOfYear, Now).ToString, 3) &
                Now.ToString("hhmmssff") & nFileExt
        Return Path.GetDirectoryName(nFileDirectory) + "\" + _filename
    End Function
    Friend Function GetGPItemNo(ByVal nItemNo As String, ByVal nCompany As String, ByVal nCDate As Date, ByVal nGoLive As Date) As String
        Dim ConfigCls As New GetConfigInfo
        If nCompany <> "TTB" AndAlso nCDate < nGoLive Then
            Return Right(nItemNo, 8)
        Else
            Return nItemNo
        End If
    End Function

    Friend Function GetOrdType(ByVal nOrdType As String)
        Select Case nOrdType.ToUpper
            Case "HPBREORDER"
                Return "HPB REORDERS"
            Case "TTBREORDER"
                Return "TTB REORDERS"
            Case "SUPPLIES"
                Return "SUPPLIES"
            Case "DROPSHIP"
                Return "DROPSHIP"
            Case Else
                Return nOrdType.ToUpper
        End Select
    End Function
#End Region

End Module
