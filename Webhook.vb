Imports System.Net.Http
Imports System.Text.Json.Nodes

Public Class Webhook
    Public Shared Function FromJson(webhookJson As JsonObject) As Webhook
        If webhookJson Is Nothing Then Throw New ArgumentNullException(NameOf(webhookJson), "Webhook JSON cannot be null.")
        Dim url = webhookJson("url").ToString()
        Dim method As HttpMethod
        If Not webhookJson.ContainsKey("method") Then
            method = HttpMethod.Post ' Default method if not specified
        Else
            method = HttpMethod.Parse(webhookJson("method").ToString())
        End If
        Dim name = webhookJson("name").ToString()
        Dim description = webhookJson("description").ToString()
        Dim parameters As WebhookParameterCollection = Nothing
        If webhookJson.ContainsKey("parameter") AndAlso Not webhookJson("parameter") Is Nothing Then
            parameters = WebhookParameterCollection.FromJson(webhookJson("parameter").AsObject())
        End If
        Dim secretName As String = Nothing
        If webhookJson.ContainsKey("secret") Then
            secretName = webhookJson("secret").ToString()
        End If
        Return New Webhook(url, method, name, description, parameters, secretName)
    End Function

    Public Property Url As String
    Public Property Method As HttpMethod
    Public Property Name As String
    Public Property Description As String
    Public Property Parameters As WebhookParameterCollection
    Public Property SecretName As String

    Public Sub New(url As String, method As HttpMethod, name As String, description As String, parameters As WebhookParameterCollection, secret_name As String)
        If String.IsNullOrWhiteSpace(url) Then Throw New ArgumentException("Webhook URL cannot be null or empty.", NameOf(url))
        If method Is Nothing Then method = HttpMethod.Post
        If String.IsNullOrWhiteSpace(name) Then Throw New ArgumentException("Webhook name cannot be null or empty.", NameOf(name))
        If String.IsNullOrWhiteSpace(description) Then Throw New ArgumentException("Webhook description cannot be null or empty.", NameOf(description))
        Me.Url = url
        Me.Method = method
        Me.Name = name
        Me.Description = description
        Me.Parameters = parameters
        Me.SecretName = secret_name
    End Sub

    Public Sub New(url As String, method As HttpMethod, name As String, description As String, parameters As IEnumerable(Of WebhookParameter), secret_name As String)
        Me.New(url, method, name, description, New WebhookParameterCollection(parameters), secret_name)
    End Sub

    Public Sub New(url As String, method As HttpMethod, name As String, description As String, parameters As IEnumerable(Of WebhookParameter))
        Me.New(url, method, name, description, New WebhookParameterCollection(parameters), Nothing)
    End Sub

    Public Sub New(url As String, method As HttpMethod, name As String, description As String)
        Me.New(url, method, name, description, Nothing, Nothing)
    End Sub

    Public Sub New(url As String, name As String, description As String)
        Me.New(url, HttpMethod.Post, name, description, Nothing, Nothing)
    End Sub

    Public Sub New()
        Me.New(String.Empty, HttpMethod.Put, String.Empty, String.Empty, Nothing, Nothing)
    End Sub

    Public Function AsJson() As JsonObject
        Dim json As New JsonObject From {
            {"url", Url},
            {"method", Method.Method},
            {"name", Name},
            {"description", Description}
        }
        If Parameters IsNot Nothing Then
            json("parameters") = Parameters.ToJsonObject()
        End If
        If Not String.IsNullOrWhiteSpace(SecretName) Then
            json("secret_name") = SecretName
        End If
        Return json
    End Function

    Public Overrides Function ToString() As String
        Return $"{Name} ({Method}) - {Url}"
    End Function
End Class
