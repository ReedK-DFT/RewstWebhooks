
Imports System.Net.Http
Imports System.Net.Http.Json
Imports System.Text.Json.Nodes

Public Class WebhookClient
    Private _webhookSecrets As New WebhookSecretCollection
    Private _client As HttpClient
    Private _lastSecretId As Guid = Guid.Empty

    Public Property Webhooks As New WebhookCollection

    Public Sub New()
        _client = New HttpClient
        _client.DefaultRequestHeaders.Add("User-Agent", "RewstWebhookClient/1.0")
        _client.DefaultRequestHeaders.Add("Accept", "application/json")
    End Sub

    Public Async Function CallWebhookAsync(webhookName As String, Optional parameters As JsonObject = Nothing) As Task(Of JsonObject)
        If Webhooks Is Nothing OrElse Not Webhooks.Contains(webhookName) Then
            Throw New ArgumentException($"Webhook '{webhookName}' not found.", NameOf(webhookName))
        End If
        Return Await CallWebhookAsync(Webhooks(webhookName), parameters)
    End Function

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
        Return Await response.Content.ReadFromJsonAsync(Of JsonObject)()
    End Function

    Public Async Function CallRawWebhookAsync(webhookUrl As String, Optional parameters As JsonObject = Nothing) As Task(Of JsonObject)
        Dim webhook As New Webhook(webhookUrl, HttpMethod.Post, "GenericWebhook", "A one-off webhook with no definition.")
        Return Await CallWebhookAsync(webhook, parameters)
    End Function

    Public Async Function CallRawWebhookAsync(webhookUrl As String, secretName As String, Optional parameters As JsonObject = Nothing) As Task(Of JsonObject)
        Dim webhook As New Webhook(webhookUrl, HttpMethod.Post, "GenericWebhook", "A one-off webhook with no definition.", Nothing, secretName)
        Return Await CallWebhookAsync(webhook, parameters)
    End Function

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

    Private Async Function SendWebhookAsync(webhook As Webhook, Optional parameters As JsonObject = Nothing) As Task(Of HttpResponseMessage)
        If webhook Is Nothing Then Throw New ArgumentNullException(NameOf(webhook), "Webhook cannot be null.")
        If String.IsNullOrWhiteSpace(webhook.Url) Then Throw New ArgumentException("Webhook URL cannot be null or empty.", NameOf(webhook.Url))
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

    Public Sub RegisterSecret(name As String, value As String, Optional is_default As Boolean = False)
        Dim secret As WebhookClientSecret = New WebhookClientSecret(name, value, is_default)
        If secret Is Nothing Then Throw New ArgumentNullException(NameOf(secret), "WebhookClientSecret cannot be null.")
        If _webhookSecrets.Contains(secret.Name) Then
            Throw New InvalidOperationException($"A secret with the name '{secret.Name}' already exists.")
        End If
        _webhookSecrets.Add(secret)
    End Sub

End Class
