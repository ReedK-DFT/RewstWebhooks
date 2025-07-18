''' <summary>
''' Represents a parameter definition for a webhook.
''' This class is used to define the parameters that are required by a webhook request.
''' It includes properties for the parameter name, description, type, and whether it is required.
''' </summary>
Public Class WebhookParameter
    ''' <summary>
    ''' The name of the parameter.
    ''' This is used to identify the parameter in the webhook request.
    ''' It cannot be null or empty.
    ''' </summary>
    ''' <returns></returns>
    Public Property Name As String
    ''' <summary>
    ''' A description of the parameter.
    ''' This is used to provide additional information about the parameter. Use by AI Agents to understand the parameter's purpose.
    ''' It can be null or empty if no description is needed.
    ''' </summary>
    ''' <returns></returns>
    Public Property Description As String
    ''' <summary>
    ''' The type of the parameter.
    ''' This is used to specify the data type of the parameter, such as "string", "integer", etc.
    ''' It cannot be null or empty.
    ''' </summary>
    ''' <returns></returns>
    Public Property Type As String
    ''' <summary>
    ''' Indicates whether the parameter is required.
    ''' If true, the parameter must be provided in the webhook request.
    ''' If false, the parameter is optional.
    ''' </summary>
    ''' <returns></returns>
    Public Property Required As Boolean

    Public Sub New()
    End Sub

    Public Sub New(name As String, description As String, type As String, Optional required As Boolean = False)
        If String.IsNullOrWhiteSpace(name) Then Throw New ArgumentException("Parameter name cannot be null or empty.", NameOf(name))
        If String.IsNullOrWhiteSpace(type) Then Throw New ArgumentException("Parameter type cannot be null or empty.", NameOf(type))
        If String.IsNullOrWhiteSpace(description) Then description = String.Empty ' Allow empty description
        Me.Name = name
        Me.Description = description
        Me.Type = type
        Me.Required = required
    End Sub

    Public Overrides Function ToString() As String
        Return $"{Name} ({Type}) - {Description}"
    End Function
End Class
