Public Class WebhookParameter
    Public Property Name As String
    Public Property Description As String
    Public Property Type As String
    Public Property Required As Boolean

    Public Sub New()
    End Sub

    Public Sub New(name As String, description As String, type As String, Optional required As Boolean = False)
        If String.IsNullOrWhiteSpace(name) Then Throw New ArgumentException("Parameter name cannot be null or empty.", NameOf(name))
        If String.IsNullOrWhiteSpace(type) Then Throw New ArgumentException("Parameter type cannot be null or empty.", NameOf(type))
        Me.Name = name
        Me.Description = description
        Me.Type = type
        Me.Required = required
    End Sub

    Public Overrides Function ToString() As String
        Return $"{Name} ({Type}) - {Description}"
    End Function
End Class
