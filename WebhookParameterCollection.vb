Imports System.Text.Json.Nodes

Public Class WebhookParameterCollection
    Inherits ObjectModel.KeyedCollection(Of String, WebhookParameter)

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

    Public Sub New()
        MyBase.New(StringComparer.OrdinalIgnoreCase)
    End Sub

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
