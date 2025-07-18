''' <summary>
''' Represents a collection of client secrets for webhook authentication.
''' This class inherits from KeyedCollection to allow easy access by secret name.
''' It manages a default secret and ensures that only one default secret can exist at a time.
''' </summary>
Public Class WebhookSecretCollection
    Inherits ObjectModel.KeyedCollection(Of String, WebhookClientSecret)

    Private _DefaultSecret As WebhookClientSecret = Nothing
    ''' <summary>
    ''' The default secret for the webhook client.
    ''' This property holds the secret that should be used when the Default secret is specified.
    ''' It can be set to a WebhookClientSecret instance that has IsDefault set to True.
    ''' If no default secret is set, this property will return Nothing.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property DefaultSecret As WebhookClientSecret
        Get
            Return _DefaultSecret
        End Get
    End Property

    Protected Overrides Function GetKeyForItem(item As WebhookClientSecret) As String
        Return item.Name
    End Function

    Protected Overrides Sub InsertItem(index As Integer, item As WebhookClientSecret)
        If item Is Nothing Then Throw New ArgumentNullException(NameOf(item), "WebhookClientSecret cannot be null.")
        If item.IsDefault Then
            If _DefaultSecret IsNot Nothing Then
                Throw New InvalidOperationException("Cannot set multiple default secrets.")
            End If
            _DefaultSecret = item
        End If
        MyBase.InsertItem(index, item)
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As WebhookClientSecret)
        If item Is Nothing Then Throw New ArgumentNullException(NameOf(item), "WebhookClientSecret cannot be null.")
        If item.IsDefault Then
            If _DefaultSecret IsNot Nothing AndAlso _DefaultSecret.Name <> item.Name Then
                Throw New InvalidOperationException("Cannot set multiple default secrets.")
            End If
            _DefaultSecret = item
        End If
        MyBase.SetItem(index, item)
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        If Item(index) Is _DefaultSecret Then
            _DefaultSecret = Nothing
        End If
        MyBase.RemoveItem(index)
    End Sub
End Class