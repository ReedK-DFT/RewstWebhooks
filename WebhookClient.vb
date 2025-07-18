
Imports System.Net.Http
Imports System.Net.Http.Json
Imports System.Text.Json.Nodes
Imports System.Text.Json.Serialization

''' <summary>
''' Represents a client for managing and calling Rewst webhooks.
''' Allows you to register secrets, call webhooks by name, and handle parameters.
''' </summary>
Public Class WebhookClient
    Private _webhookSecrets As New WebhookSecretCollection
    Private _client As HttpClient
    Private _lastSecretId As Guid = Guid.Empty

    ''' <summary>
    ''' Gets or sets the collection of webhooks available to this client.
    ''' </summary>
    ''' <remarks>
    ''' This collection can be populated from a JSON file or defined programmatically.
    ''' </remarks>
    Public Property Webhooks As New WebhookCollection

    ''' <summary>
    ''' Initializes a new instance of the <see cref="WebhookClient"/> class.
    ''' </summary>
    ''' <remarks>
    ''' Sets up the HTTP client with default headers for user agent and accept type.
    ''' </remarks>
    Public Sub New()
        _client = New HttpClient
        _client.DefaultRequestHeaders.Add("User-Agent", "RewstWebhookClient/1.0")
        _client.DefaultRequestHeaders.Add("Accept", "application/json")
    End Sub

    ''' <summary>
    ''' Calls a webhook by its name with optional parameters.
    ''' This method looks up the webhook in the Webhooks collection and calls it.
    ''' If the webhook is not found, an exception is thrown.
    ''' </summary>
    ''' <param name="webhookName">The name of the webhook in the <see cref="WebhookCollection"/>.</param>
    ''' <param name="parameters">Optional JsonObject containing the parameter values.</param>
    ''' <returns>A JsonObject containing the result from the webhook call.</returns>
    ''' <exception cref="ArgumentException">Thrown if the webhook name is not found in the collection.</exception>
    ''' <exception cref="HttpRequestException">Thrown if the webhook call fails.</exception>
    ''' <exception cref="ArgumentNullException">Thrown if the webhook is null or has an invalid URL.</exception>
    ''' <exception cref="ArgumentException">Thrown if the webhook URL is null or empty.</exception>"
    Public Async Function CallWebhookAsync(webhookName As String, Optional parameters As JsonObject = Nothing) As Task(Of JsonObject)
        If Webhooks Is Nothing OrElse Not Webhooks.Contains(webhookName) Then
            Throw New ArgumentException($"Webhook '{webhookName}' not found.", NameOf(webhookName))
        End If
        Return Await CallWebhookAsync(Webhooks(webhookName), parameters)
    End Function

    ''' <summary>
    ''' Calls a webhook with the specified parameters.
    ''' </summary>
    ''' <param name="webhook">The webhook to call.</param>
    ''' <param name="parameters">Optional JsonObject containing the parameter values.</param>
    ''' <returns>A JsonObject containing the result from the webhook call.</returns>
    ''' <exception cref="ArgumentNullException">Thrown if the webhook is null.</exception>
    ''' <exception cref="ArgumentException">Thrown if the webhook URL is null or empty.</exception>
    ''' <exception cref="HttpRequestException">Thrown if the webhook call fails.</exception>
    Public Async Function CallWebhookAsync(webhook As Webhook, Optional parameters As JsonObject = Nothing) As Task(Of JsonObject)
        If webhook Is Nothing Then Throw New ArgumentNullException(NameOf(webhook), "Webhook cannot be null.")
        If String.IsNullOrWhiteSpace(webhook.Url) Then Throw New ArgumentException("Webhook URL cannot be null or empty.", NameOf(webhook.Url))
        Dim response As HttpResponseMessage = Await SendWebhookAsync(webhook, parameters)
        If Not response.IsSuccessStatusCode Then
            If response.StatusCode = 401 Then
                Throw New HttpRequestException($"Webhook '{webhook.Name}' requires a secret that was not found.")
            End If
            Throw New HttpRequestException($"Failed to call webhook '{webhook.Name}': {response.ReasonPhrase}")
        End If
        Return Await ProcessContent(response.Content)
    End Function

    ''' <summary>
    ''' Calls a webhook with the specified URL and parameters.
    ''' This is a one-off call without a predefined webhook definition.
    ''' </summary>
    ''' <param name="webhookUrl">The URL of the webhook to call.</param>
    ''' <param name="parameters">Optional JsonObject containing the parameter values.</param>
    ''' <returns>A JsonObject containing the result from the webhook call.</returns>
    ''' <exception cref="ArgumentNullException">Thrown if the webhook URL is null or empty.</exception>
    ''' <exception cref="HttpRequestException">Thrown if the webhook call fails.</exception>
    Public Async Function CallRawWebhookAsync(webhookUrl As String, Optional parameters As JsonObject = Nothing) As Task(Of JsonObject)
        Dim webhook As New Webhook(webhookUrl, HttpMethod.Post, "GenericWebhook", "A one-off webhook with no definition.")
        Return Await CallWebhookAsync(webhook, parameters)
    End Function

    ''' <summary>
    ''' Calls a webhook with the specified URL and secret name.
    ''' This is a one-off call without a predefined webhook definition.
    ''' </summary>
    ''' <param name="webhookUrl">The URL of the webhook to call.</param>
    ''' <param name="secretName">The name of the secret to use for authentication.</param>
    ''' <param name="parameters">Optional JsonObject containing the parameter values.</param>
    ''' <returns>A JsonObject containing the result from the webhook call.</returns>
    ''' <exception cref="ArgumentNullException">Thrown if the webhook URL is null or empty.</exception>
    ''' <exception cref="HttpRequestException">Thrown if the webhook call fails.</exception>
    Public Async Function CallRawWebhookAsync(webhookUrl As String, secretName As String, Optional parameters As JsonObject = Nothing) As Task(Of JsonObject)
        Dim webhook As New Webhook(webhookUrl, HttpMethod.Post, "GenericWebhook", "A one-off webhook with no definition.", Nothing, secretName)
        Return Await CallWebhookAsync(webhook, parameters)
    End Function

    ''' <summary>
    ''' Calls a webhook with the specified URL and parameters as a collection of key-value pairs.
    ''' This is a one-off call without a predefined webhook definition.
    ''' </summary>
    ''' <param name="webhookUrl">The URL of the webhook to call.</param>
    ''' <param name="parameters">An enumerable collection of key-value pairs representing the parameters.</param>
    ''' <returns>A JsonObject containing the result from the webhook call.</returns>
    ''' <exception cref="ArgumentNullException">Thrown if the webhook URL is null or empty.</exception>
    ''' <exception cref="HttpRequestException">Thrown if the webhook call fails.</exception>
    Public Async Function CallRawWebhookAsync(webhookUrl As String, parameters As IEnumerable(Of Tuple(Of String, String))) As Task(Of JsonObject)
        Dim paramObject As New JsonObject
        If parameters IsNot Nothing Then
            For Each param In parameters
                If param IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(param.Item1) Then
                    paramObject(param.Item1) = param.Item2
                End If
            Next
        End If
        Dim webhook As New Webhook(webhookUrl, HttpMethod.Post, "GenericWebhook", "A one-off webhook with no definition.")
        Return Await CallWebhookAsync(webhook, paramObject)
    End Function

    Private Async Function ProcessContent(content As HttpContent) As Task(Of JsonObject)
        Dim contentType As IEnumerable(Of String) = Nothing
        content.Headers.TryGetValues("Content-Type", contentType)
        If contentType Is Nothing OrElse Not contentType.Any() Then
            Throw New HttpRequestException($"Invalid response header. No Content-Type specified.")
        End If
        Dim contentTypeValue As String = contentType.FirstOrDefault().ToLower
        If contentTypeValue.StartsWith("application/json") Then
            Try
                Return Await content.ReadFromJsonAsync(Of JsonObject)()
            Catch ex As Exception
                Return New JsonObject From {
                    {"error", "Failed to parse JSON response."},
                    {"details", ex.Message}
                }
            End Try
        ElseIf contentTypeValue.StartsWith("application/octet-stream") Then
            Try
                Dim result As Byte() = Await content.ReadAsByteArrayAsync()
                Return New JsonObject From {
                    {"base64", Convert.ToBase64String(result)}
                }
            Catch ex As Exception
                Return New JsonObject From {
                    {"error", "Failed to read binary response."},
                    {"details", ex.Message}
                }
            End Try
        Else
            Try
                Dim result As String = Await content.ReadAsStringAsync()
                Return New JsonObject From {
                    {contentTypeValue, result}
                }
            Catch ex As Exception
                Return New JsonObject From {
                    {"error", "Failed to read text response."},
                    {"details", ex.Message}
                }
            End Try
        End If
    End Function

    Private Async Function SendWebhookAsync(webhook As Webhook, Optional parameters As JsonObject = Nothing) As Task(Of HttpResponseMessage)
        If webhook Is Nothing Then Throw New ArgumentNullException(NameOf(webhook), "Webhook cannot be null.")
        If String.IsNullOrWhiteSpace(webhook.Url) Then Throw New ArgumentException("Webhook URL cannot be null Or empty.", NameOf(webhook.Url))
        Dim request As New HttpRequestMessage(webhook.Method, webhook.Url)
        request.Headers.Add("x-rewst-webhook-name", webhook.Name)
        If Not String.IsNullOrWhiteSpace(webhook.SecretName) AndAlso Not webhook.SecretName.ToLower = "none" Then
            Dim secret As WebhookClientSecret
            If webhook.SecretName.ToLower = "default" Then
                secret = _webhookSecrets.DefaultSecret
            Else
                secret = _webhookSecrets(webhook.SecretName)
            End If

            If secret IsNot Nothing Then
                secret.WriteToHeader(request.Headers, _lastSecretId)
            Else
                Throw New InvalidOperationException($"Webhook secret '{webhook.SecretName}' not found.")
            End If
        ElseIf _webhookSecrets.DefaultSecret IsNot Nothing Then
            _webhookSecrets.DefaultSecret.WriteToHeader(request.Headers, _lastSecretId)
        End If
        If parameters IsNot Nothing AndAlso parameters.Count > 0 Then
            request.Content = JsonContent.Create(Of JsonObject)(parameters)
        End If
        Return Await _client.SendAsync(request)
    End Function

    ''' <summary>
    ''' Registers a secret with the client.
    ''' Webhooks can use this secret by name to authenticate requests.
    ''' If a secret with the same name already exists, an exception is thrown.
    ''' If is_default is true, this secret will be set as the default secret for the client.
    ''' </summary>
    ''' <param name="name">The name of the secret.</param>
    ''' <param name="value">The value of the secret.</param>
    ''' <param name="is_default">Optional. If true, this secret will be set as the default secret for the client.</param>
    ''' <exception cref="ArgumentNullException">Thrown if the secret is null or has an invalid name.</exception>
    ''' <exception cref="InvalidOperationException">Thrown if a secret with the same name already exists.</exception>
    Public Sub RegisterSecret(name As String, value As String, Optional is_default As Boolean = False)
        Dim secret As WebhookClientSecret = New WebhookClientSecret(name, value, is_default)
        If secret Is Nothing Then Throw New ArgumentNullException(NameOf(secret), "WebhookClientSecret cannot be null.")
        If _webhookSecrets.Contains(secret.Name) Then
            Throw New InvalidOperationException($"A secret with the name '{secret.Name}' already exists.")
        End If
        _webhookSecrets.Add(secret)
    End Sub

End Class
