Imports System.Text.Json.Nodes

Public Class WebhookCollection
    Inherits ObjectModel.KeyedCollection(Of String, Webhook)

    Public Shared Function FromJson(webhooksJson As JsonObject) As WebhookCollection
        Dim collection As New WebhookCollection
        If webhooksJson Is Nothing OrElse Not webhooksJson.ContainsKey("webhooks") Then
            Return collection
        End If
        Dim webhooksArray = webhooksJson("webhooks").AsArray()
        For Each webhookJson In webhooksArray
            Dim webhook = RewstWebhooks.Webhook.FromJson(webhookJson.AsObject())
            collection.Add(webhook)
        Next
        Return collection
    End Function

    Protected Overrides Function GetKeyForItem(item As Webhook) As String
        If item Is Nothing Then Throw New ArgumentNullException(NameOf(item), "Webhook cannot be null.")
        Return item.Name
    End Function

    Public Function AsJson() As JsonObject
        Dim result As New JsonObject
        Dim jsonArray As New JsonArray
        For Each webhook In Me
            jsonArray.Add(webhook.AsJson())
        Next
        result("webhooks") = jsonArray
        Return result
    End Function
End Class
