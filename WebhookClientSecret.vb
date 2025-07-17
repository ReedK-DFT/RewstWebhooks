Public Class WebhookClientSecret
    Const REWST_HEADER As String = "x-rewst-secret"

    Public ReadOnly Property ID As Guid = Guid.NewGuid()
    Public ReadOnly Property Name As String

    Protected _secret As String
    Public WriteOnly Property Secret As String
        Set(value As String)
            _secret = value
        End Set
    End Property

    Public ReadOnly Property IsDefault As Boolean

    Public Sub New(secretName As String, secretValue As String, Optional isDefault As Boolean = False)
        If String.IsNullOrWhiteSpace(secretName) Then Throw New ArgumentException("Secret name cannot be null or empty.", NameOf(secretName))
        If secretName = "None" OrElse secretName = "Default" Then Throw New ArgumentException("Cannot use reserved secret names 'None' or 'Default'.", NameOf(secretName))
        If String.IsNullOrWhiteSpace(secretValue) Then Throw New ArgumentException("Secret value cannot be null or empty.", NameOf(secretValue))
        Name = secretName
        Secret = secretValue
        Me.IsDefault = isDefault
    End Sub

    Public Sub WriteToHeader(header As System.Net.Http.Headers.HttpRequestHeaders, ByRef lastSecretId As Guid)
        If Not String.IsNullOrEmpty(_secret) Then
            header.Remove(REWST_HEADER) ' Remove existing header if it exists
            header.Add(REWST_HEADER, _secret)
            lastSecretId = ID
        End If
    End Sub
End Class
