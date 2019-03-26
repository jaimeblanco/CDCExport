

Partial Public Class CDCDataDataContext
    Function GetNewPOCount() As Integer
        Return (From results In Me.POHeaders _
               Where results.Printed = 1).Count

    End Function

    Function GetNewItems() As Integer
        Return (From results In Me.ProductMasters _
                Where results.UserInt1 = 10).Count
    End Function

End Class
