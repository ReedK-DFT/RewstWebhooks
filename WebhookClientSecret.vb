''' <summary>
''' Represents a client secret for webhook authentication.
''' This class is used to manage and send a secret key in HTTP headers for webhook requests.
''' It ensures that the secret is not empty and does not use reserved names.
''' </summary>
Public Class WebhookClientSecret
    Const REWST_HEADER As String = "x-rewst-secret"

    ''' <summary>
    ''' A unique identifier for the secret.
    ''' This ID is generated when the secret is created and can be used to track the secret.
    ''' </summary>
    ''' <returns>A new GUID</returns>
    Public ReadOnly Property ID As Guid = Guid.NewGuid()

    ''' <summary>
    ''' The name of the secret.
    ''' This is used to identify the secret when registering it with a webhook.
    ''' It cannot be null, empty, or one of the reserved names "None" or "Default".
    ''' </summary>
    ''' <returns>A string containing the secret name.</returns>
    Public ReadOnly Property Name As String

    Protected _secret As String
    ''' <summary>
    ''' [WriteOnly] The secret value.
    ''' This is the actual secret key that will be sent in the HTTP headers.
    ''' It cannot be null or empty.
    ''' </summary>
    ''' <exception cref="ArgumentException">Thrown if the secret value is null or empty.</exception>
    Public WriteOnly Property Secret As String
        Set(value As String)
            _secret = value
        End Set
    End Property

    ''' <summary>
    ''' Determines if this secret is the default secret.
    ''' This can be used to identify a primary secret that should be used when the webhook specifies a secret named Default.
    ''' It is set to false by default, but can be set to true when creating the secret.
    ''' </summary>
    ''' <returns>True if this is the default secret, otherwise false.</returns>
    Public ReadOnly Property IsDefault As Boolean

    ''' <summary>
    ''' Initializes a new instance of the WebhookClientSecret class.
    ''' </summary>
    ''' <param name="secretName">The name of the secret. Cannot be null, empty, or one of the reserved names "None" or "Default".</param>
    ''' <param name="secretValue">The value of the secret. Cannot be null or empty.</param>
    ''' <param name="isDefault">Optional parameter to indicate if this is the default secret. Defaults to false.</param>
    ''' <exception cref="ArgumentException">Thrown if the secret name or value is invalid.</exception>
    Public Sub New(secretName As String, secretValue As String, Optional isDefault As Boolean = False)
        If String.IsNullOrWhiteSpace(secretName) Then Throw New ArgumentException("Secret name cannot be null or empty.", NameOf(secretName))
        If secretName = "None" OrElse secretName = "Default" Then Throw New ArgumentException("Cannot use reserved secret names 'None' or 'Default'.", NameOf(secretName))
        If String.IsNullOrWhiteSpace(secretValue) Then Throw New ArgumentException("Secret value cannot be null or empty.", NameOf(secretValue))
        Name = secretName
        Secret = secretValue
        Me.IsDefault = isDefault
    End Sub

    ''' <summary>
    ''' Writes the secret to the provided HTTP request headers.
    ''' This method adds the secret to the headers under the "x-rewst-secret" key.
    ''' If the secret is empty, it does nothing.
    ''' </summary>
    ''' <param name="header">The HTTP request headers to write the secret to.</param>
    ''' <param name="lastSecretId">A reference to a Guid that will be updated with this secret's ID if the secret is written.</param>
    ''' <remarks>This method is used to send the secret in webhook requests for authentication.</remarks>
    Public Sub WriteToHeader(header As System.Net.Http.Headers.HttpRequestHeaders, ByRef lastSecretId As Guid)
        If Not String.IsNullOrEmpty(_secret) Then
            header.Remove(REWST_HEADER) ' Remove existing header if it exists
            header.Add(REWST_HEADER, _secret)
            lastSecretId = ID
        End If
    End Sub
End Class
