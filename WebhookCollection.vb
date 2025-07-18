Imports System.Text.Json.Nodes

''' <summary>
''' Represents a collection of webhooks allowing easy access by webhook name.
''' It provides methods to create a collection from JSON and to convert the collection back to JSON.
''' </summary>
Public Class WebhookCollection
    Inherits ObjectModel.KeyedCollection(Of String, Webhook)

    ''' <summary>
    ''' Creates a new instance of the WebhookCollection class from a JSON object.
    ''' This method expects the JSON object to contain a property "webhooks" which is an array of webhook definitions.
    ''' Each webhook definition is converted to a Webhook object and added to the collection.
    ''' If the JSON object does not contain the "webhooks" property, an empty collection is returned.
    ''' </summary>
    ''' <param name="webhooksJson"></param>
    ''' <returns></returns>
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

    ''' <summary>
    ''' Converts the collection of webhooks to a JSON object.
    ''' This method creates a JsonObject with a single property "webhooks" containing an array of webhook JSON objects.
    ''' </summary>
    ''' <returns>A JsonObject representing the collection of webhooks.</returns>
    Public Function ToJsonObject() As JsonObject
        Dim result As New JsonObject
        Dim jsonArray As New JsonArray
        For Each webhook In Me
            jsonArray.Add(webhook.ToJsonObject())
        Next
        result("webhooks") = jsonArray
        Return result
    End Function
End Class
