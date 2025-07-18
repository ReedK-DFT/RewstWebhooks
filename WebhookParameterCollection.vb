Imports System.Text.Json.Nodes

''' <summary>
''' Represents a collection of parameters for a webhook, allowing easy access by parameter name.
''' It provides methods to create a collection from JSON and to convert the collection back to JSON.
''' </summary>
Public Class WebhookParameterCollection
    Inherits ObjectModel.KeyedCollection(Of String, WebhookParameter)

    ''' <summary>
    ''' Creates a new instance of the WebhookParameterCollection class from a JSON object.
    ''' This method expects the JSON object to contain a property "properties" which is an object with parameter definitions.
    ''' Each parameter definition is converted to a WebhookParameter object and added to the collection.
    ''' If the JSON object does not contain the "properties" property, an empty collection is returned.
    ''' </summary>
    ''' <param name="parametersJson">The JSON object containing parameter definitions.</param>
    ''' <returns>A WebhookParameterCollection populated with parameters from the JSON object.</returns>
    Public Shared Function FromJson(parametersJson As JsonObject) As WebhookParameterCollection
        Dim collection As New WebhookParameterCollection
        If parametersJson Is Nothing OrElse Not parametersJson.ContainsKey("properties") Then
            Return collection
        End If
        Dim properties = parametersJson("properties").AsObject()
        Dim requiredArray = If(parametersJson.ContainsKey("required"), parametersJson("required").AsArray(), New JsonArray())
        For Each prop In properties
            Dim name = prop.Key
            Dim details = prop.Value.AsObject()
            Dim description = If(details.ContainsKey("description"), details("description").ToString(), String.Empty)
            Dim type = If(details.ContainsKey("type"), details("type").ToString(), "string")
            Dim required = IIf(requiredArray.Contains(JsonValue.Create(Of String)(name)), True, False)
            collection.Add(New WebhookParameter(name, description, type, required))
        Next
        Return collection
    End Function

    ''' <summary>
    ''' Creates a new instance of the WebhookParameterCollection class.
    ''' This constructor initializes the collection with a case-insensitive string comparer for parameter names.
    ''' </summary>
    Public Sub New()
        MyBase.New(StringComparer.OrdinalIgnoreCase)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the WebhookParameterCollection class with a collection of parameters.
    ''' This constructor allows you to create a collection from an existing set of WebhookParameter objects.
    ''' </summary>
    ''' <param name="parameters">An IEnumerable of WebhookParameter objects to initialize the collection with.</param>
    Public Sub New(parameters As IEnumerable(Of WebhookParameter))
        Me.New()
        If parameters IsNot Nothing Then
            For Each param In parameters
                If param IsNot Nothing Then
                    Me.Add(param)
                End If
            Next
        End If
    End Sub

    Protected Overrides Function GetKeyForItem(item As WebhookParameter) As String
        Return item.Name
    End Function

    ''' <summary>
    ''' Converts the collection of parameters to a JSON object.
    ''' This method creates a JsonObject with a "type" property set to "object", a "properties" property containing parameter definitions,
    ''' and a "required" property listing required parameters.
    ''' </summary>
    ''' <returns>A JsonObject representing the collection of parameters.</returns>
    Public Function ToJsonObject() As JsonObject
        Dim json As New JsonObject
        json("type") = "object"
        Dim propObject As New JsonObject
        json("properties") = propObject
        Dim requiredList As New JsonArray
        For Each prop In Me
            propObject(prop.Name) = New JsonObject From {
                {"type", prop.Type},
                {"description", prop.Description}
            }
            If prop.Required Then
                requiredList.Add(prop.Name)
            End If
        Next
        json("required") = requiredList
        Return json
    End Function

    ''' <summary>
    ''' Creates a new instance of JsonObject with default values for each parameter in the collection.
    ''' This method initializes each parameter based on its type, providing a default value.
    ''' Configure the property values of this JsonObject and send it as the parameter to the webhook.
    ''' </summary>
    ''' <returns>A JsonObject with default values for each parameter.</returns>
    ''' <remarks>This is useful for creating an empty instance of parameters to be filled later.</remarks>
    Public Function CreateInstance() As JsonObject
        Dim result As New JsonObject
        For Each param In Me
            Select Case param.Type.ToLower()
                Case "string"
                    result(param.Name) = String.Empty
                Case "integer", "number"
                    result(param.Name) = 0
                Case "boolean"
                    result(param.Name) = False
                Case "array"
                    result(param.Name) = New JsonArray()
                Case "object"
                    result(param.Name) = New JsonObject()
                Case Else
                    result(param.Name) = Nothing ' Default case for unknown types
            End Select
        Next
        Return result
    End Function
End Class
