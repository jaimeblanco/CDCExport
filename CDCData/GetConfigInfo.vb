Enum StoreOrdPOs
    HPBReorder
    DropShip
End Enum
Enum StoreOrders
    HPBReorder
    TTBReorder
    Supplies
End Enum

Public Class GetConfigInfo
    Function getFileExportLocation(ByVal nCDCConn As CDC.CDCConnection, ByVal nDocType As String) As String
        Return (From ele In nCDCConn.ConfigXml...<ExportType>.Where(Function(r) r.@Type = nDocType).Elements("xmlOutputFile")).SingleOrDefault.Value.Trim
    End Function
    Function getErrorFileLocation(ByVal nCDCConn As CDC.CDCConnection, ByVal nDocType As String) As String
        Return (From ele In nCDCConn.ConfigXml...<ExportType>.Where(Function(r) r.@Type = nDocType).Elements("errOutputFile")).SingleOrDefault.Value.Trim
    End Function
    Function getProcessDocType(ByVal nCDCConn As CDC.CDCConnection) As String
        Return (From ele In nCDCConn.ConfigXml...<Config> Select ele.<ProcessingDocType>.Value.Trim).SingleOrDefault
    End Function
    Function getGoLiveDate(ByVal nCDCConn As CDC.CDCConnection) As Date
        Return (From ele In nCDCConn.ConfigXml...<Config> Select ele.<GoLiveDate>.Value).SingleOrDefault
    End Function
    Function getProcShipXMLVal(ByVal nCDCConn As CDC.CDCConnection) As String
        Return (From ele In nCDCConn.ConfigXml...<Config> Select ele.<ProcessShipXML>.Value).SingleOrDefault
    End Function
    Function getSplitItemVal(ByVal nCDCConn As CDC.CDCConnection) As String
        Return (From ele In nCDCConn.ConfigXml...<Config> Select ele.<SplitTTBItems>.Value).SingleOrDefault
    End Function
    Function getMaxStoreCnt(ByVal nCDCConn As CDC.CDCConnection) As Integer
        Return (From ele In nCDCConn.ConfigXml...<Config> Select ele.<MaxStoreCnt>.Value).SingleOrDefault
    End Function
    Function getMaxRecordsPerXML(ByVal nCDCConn As CDC.CDCConnection, ByVal nDocType As String) As String
        Return (From ele In nCDCConn.ConfigXml...<ExportType>.Where(Function(r) r.@Type = nDocType).Elements("MaxRecordsPerXml")).SingleOrDefault.Value
    End Function
    Function getGPFileExtention(ByVal nCDCConn As CDC.CDCConnection, ByVal nDocType As String) As String
        Return (From ele In nCDCConn.ConfigXml...<ExportType>.Where(Function(r) r.@Type = nDocType).Elements("GPExportFileExt")).SingleOrDefault.Value
    End Function
    Function getILSFileExtention(ByVal nCDCConn As CDC.CDCConnection, ByVal nDocType As String) As String
        Return (From ele In nCDCConn.ConfigXml...<ExportType>.Where(Function(r) r.@Type = nDocType).Elements("ILSExportFileExt")).SingleOrDefault.Value
    End Function
    Function getStorePOHdr(ByVal nCDCConn As CDC.CDCConnection, ByVal nPOType As String) As String
        Return (From ele In nCDCConn.ConfigXml...<StoreOrderPO>.Where(Function(r) r.@Type = nPOType).Elements("POHdr")).SingleOrDefault
    End Function
    Function getStoreOrderHdr(ByVal nCDCConn As CDC.CDCConnection, ByVal nPOType As String) As String
        Return (From ele In nCDCConn.ConfigXml...<StoreOrder>.Where(Function(r) r.@Type = nPOType).Elements("SOHdr")).SingleOrDefault
    End Function
    Function getStorePOSiteID(ByVal nCDCConn As CDC.CDCConnection, ByVal nPOType As String) As String
        Return (From ele In nCDCConn.ConfigXml...<StoreOrderPO>.Where(Function(r) r.@Type = nPOType).Elements("SiteID")).SingleOrDefault
    End Function
    Function getStoreOrderSiteID(ByVal nCDCConn As CDC.CDCConnection, ByVal nPOType As String) As String
        Return (From ele In nCDCConn.ConfigXml...<StoreOrder>.Where(Function(r) r.@Type = nPOType).Elements("SiteID")).SingleOrDefault
    End Function
    Function getWEBItemFileSaveVal(ByVal nCDCConn As CDC.CDCConnection) As Integer
        Return (From ele In nCDCConn.ConfigXml...<Config> Select ele.<WEBItemFileSaveLoc>.Value).SingleOrDefault
    End Function
    Function getHourlyWEBItemsVal(ByVal nCDCConn As CDC.CDCConnection) As Integer
        Return (From ele In nCDCConn.ConfigXml...<Config> Select ele.<HourlyWEBItems>.Value).SingleOrDefault
    End Function
    Function getDailyWEBItemsVal(ByVal nCDCConn As CDC.CDCConnection) As Integer
        Return (From ele In nCDCConn.ConfigXml...<Config> Select ele.<DailyWEBItems>.Value).SingleOrDefault
    End Function
    Function getWeeklyWEBItemsVal(ByVal nCDCConn As CDC.CDCConnection) As Integer
        Return (From ele In nCDCConn.ConfigXml...<Config> Select ele.<WeeklyWEBItems>.Value).SingleOrDefault
    End Function
    Sub updHourlyWEBItemsVal(ByVal nCDCConn As CDC.CDCConnection, ByVal HourVal As Integer)
        nCDCConn.ConfigXml.Element("Config").SetElementValue("HourlyWEBItems", HourVal.ToString())
        nCDCConn.ConfigXml.Save(nCDCConn.ConfigFileLocation.ToString())
    End Sub
    Sub updDailyWEBItemsVal(ByVal nCDCConn As CDC.CDCConnection, ByVal DayVal As Integer)
        nCDCConn.ConfigXml.Element("Config").SetElementValue("DailyWEBItems", DayVal.ToString())
        nCDCConn.ConfigXml.Save(nCDCConn.ConfigFileLocation.ToString())
    End Sub
    Sub updWeeklyWEBItemsVal(ByVal nCDCConn As CDC.CDCConnection, ByVal WeekVal As Integer)
        nCDCConn.ConfigXml.Element("Config").SetElementValue("WeeklyWEBItems", WeekVal.ToString())
        nCDCConn.ConfigXml.Save(nCDCConn.ConfigFileLocation.ToString())
    End Sub
End Class
