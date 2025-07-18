Imports System.Net.Http
Imports System.Text.Json.Nodes

''' <summary>
''' Represents a webhook definition with properties such as URL, method, name, description, parameters, and secret name.
''' This class can be used to create, parse, and serialize webhook definitions in JSON format.
''' </summary>
Public Class Webhook
    ''' <summary>
    ''' Creates a new instance of the Webhook class from a JSON
    ''' </summary>
    ''' <param name="webhookJson"></param>
    ''' <returns>
    ''' A new instance of the Webhook class populated with the data from the provided JSON object.
    ''' Throws ArgumentNullException if the webhookJson is null.
    ''' Throws ArgumentException if required fields are missing or invalid in the JSON object.
    ''' </returns>
    ''' <exception cref="ArgumentNullException">Thrown when webhookJson is null.</exception>
    ''' <exception cref="ArgumentException">Thrown when required fields are missing or invalid in the JSON object.</exception>
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

    ''' <summary>
    ''' The URL of the webhook endpoint.
    ''' This is the address where the webhook will send requests.
    ''' It must be a valid URL and cannot be null or empty.
    ''' </summary>
    ''' <returns>
    ''' A string representing the URL of the webhook.
    ''' </returns>
    Public Property Url As String

    ''' <summary>
    ''' The HTTP method to use when calling the webhook.
    ''' This can be any valid HTTP method such as GET, POST, PUT, DELETE, etc.
    ''' If not specified, it defaults to POST.
    ''' </summary>
    ''' <returns>
    ''' An instance of HttpMethod representing the HTTP method to use.
    ''' </returns>
    ''' <remarks>
    ''' If the method is not specified in the constructor, it defaults to HttpMethod.Post.
    ''' </remarks>
    ''' <exception cref="ArgumentNullException">Thrown when the method is null.</exception>
    ''' <exception cref="ArgumentException">Thrown when the method is not a valid HTTP method.</exception>
    ''' <seealso cref="HttpMethod"/>
    ''' <seealso cref="HttpClient"/>
    ''' <seealso cref="HttpRequestMessage"/>
    Public Property Method As HttpMethod

    ''' <summary>
    ''' The name of the webhook.
    ''' This is a human-readable identifier for the webhook.
    ''' It cannot be null or empty and should be unique within the context of its usage.
    ''' </summary>
    ''' <returns>
    ''' A string representing the name of the webhook.
    ''' </returns>
    ''' <remarks>
    ''' The name is used to identify the webhook in logs, user interfaces, and other contexts.
    ''' The name should be descriptive enough to convey the purpose of the webhook.
    ''' </remarks>
    ''' <exception cref="ArgumentException">Thrown when the name is null or empty.</exception>
    ''' <seealso cref="Description"/>
    ''' <seealso cref="Parameters"/>
    ''' <seealso cref="SecretName"/>
    ''' <seealso cref="WebhookParameterCollection"/>
    ''' <seealso cref="WebhookClient"/>
    ''' <seealso cref="WebhookCollection"/>
    ''' <seealso cref="FromJson(JsonObject)"/>
    ''' <seealso cref="ToJsonObject()"/>
    ''' <seealso cref="ToString()"/>
    Public Property Name As String

    ''' <summary>
    ''' The description of the webhook.
    ''' This provides additional information about what the webhook does and how it should be used.
    ''' It cannot be null or empty and should be concise yet informative.
    ''' </summary>
    ''' <returns>
    ''' A string representing the description of the webhook.
    ''' </returns>
    ''' <remarks>
    ''' The description is useful for documentation purposes and helps users understand the purpose of the webhook.
    ''' It can include details about the expected input, output, and any special considerations.
    ''' </remarks>
    Public Property Description As String
    ''' <summary>
    ''' The parameters for the webhook.
    ''' This is a collection of parameters that can be passed to the webhook when it is called.
    ''' It can be null if no parameters are defined, or it can contain one or more WebhookParameter objects.
    ''' </summary>
    ''' <returns>
    ''' An instance of WebhookParameterCollection representing the parameters for the webhook.
    ''' </returns>
    ''' <remarks>
    ''' Parameters are used to customize the behavior of the webhook when it is called.
    ''' Each parameter can have a name, type, and description, and can be required or optional.
    ''' </remarks>
    Public Property Parameters As WebhookParameterCollection
    ''' <summary>
    ''' The name of the secret to use for this webhook.
    ''' This is used to authenticate the webhook call and should match a registered secret in the WebhookClient.
    ''' It can be null or empty if no secret is required.
    ''' </summary>
    ''' <returns>
    ''' A string representing the name of the secret for the webhook.
    ''' </returns>
    ''' <remarks>
    ''' The secret name is used to ensure that only authorized calls can trigger the webhook.
    ''' It should match a secret registered with the WebhookClient.
    ''' </remarks>
    Public Property SecretName As String

    ''' <summary>
    ''' Initializes a new instance of the Webhook class with the specified parameters.
    ''' </summary>
    ''' <param name="url">The URL of the webhook endpoint.</param>
    ''' <param name="method">The HTTP method to use when calling the webhook.</param>
    ''' <param name="name">The name of the webhook.</param>
    ''' <param name="description">The description of the webhook.</param>
    ''' <param name="parameters">The parameters for the webhook, if any.</param>
    ''' <param name="secret_name">The name of the secret to use for this webhook, if any.</param>
    ''' <exception cref="ArgumentException">Thrown when required fields are null or empty.</exception>
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

    ''' <summary>
    ''' Initializes a new instance of the Webhook class with the specified parameters.
    ''' </summary>
    ''' <param name="url">The URL of the webhook endpoint.</param>
    ''' <param name="method">The HTTP method to use when calling the webhook.</param>
    ''' <param name="name">The name of the webhook.</param>
    ''' <param name="description">The description of the webhook.</param>
    ''' <param name="parameters">The parameters for the webhook, if any.</param>
    ''' <param name="secret_name">The name of the secret to use for this webhook, if any.</param>
    ''' <exception cref="ArgumentException">Thrown when required fields are null or empty.</exception>
    Public Sub New(url As String, method As HttpMethod, name As String, description As String, parameters As IEnumerable(Of WebhookParameter), secret_name As String)
        Me.New(url, method, name, description, New WebhookParameterCollection(parameters), secret_name)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the Webhook class with the specified parameters.
    ''' </summary>
    ''' <param name="url">The URL of the webhook endpoint.</param>
    ''' <param name="method">The HTTP method to use when calling the webhook.</param>
    ''' <param name="name">The name of the webhook.</param>
    ''' <param name="description">The description of the webhook.</param>
    ''' <exception cref="ArgumentException">Thrown when required fields are null or empty.</exception>
    Public Sub New(url As String, method As HttpMethod, name As String, description As String, parameters As IEnumerable(Of WebhookParameter))
        Me.New(url, method, name, description, New WebhookParameterCollection(parameters), Nothing)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the Webhook class with the specified parameters.
    ''' </summary>
    ''' <param name="url">The URL of the webhook endpoint.</param>
    ''' <param name="method">The HTTP method to use when calling the webhook.</param>
    ''' <param name="name">The name of the webhook.</param>
    ''' <param name="description">The description of the webhook.</param>
    ''' <exception cref="ArgumentException">Thrown when required fields are null or empty.</exception>
    Public Sub New(url As String, method As HttpMethod, name As String, description As String)
        Me.New(url, method, name, description, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the Webhook class with the specified parameters.
    ''' </summary>
    ''' <param name="url">The URL of the webhook endpoint.</param>
    ''' <param name="name">The name of the webhook.</param>
    ''' <param name="description">The description of the webhook.</param>
    ''' <exception cref="ArgumentException">Thrown when required fields are null or empty.</exception>
    Public Sub New(url As String, name As String, description As String)
        Me.New(url, HttpMethod.Post, name, description, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the Webhook class with default values.
    ''' </summary>
    ''' <remarks>
    ''' This constructor initializes the webhook with an empty URL, method set to PUT, and empty name and description.
    ''' The parameters and secret name are set to Nothing.
    ''' </remarks>
    ''' <exception cref="ArgumentException">Thrown when required fields are null or empty.</exception>
    Public Sub New()
        Me.New(String.Empty, HttpMethod.Put, String.Empty, String.Empty, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Converts the webhook definition to a JSON object.
    ''' </summary>
    ''' <returns>
    ''' A JsonObject representing the webhook definition, including its URL, method, name, description, parameters, and secret name.
    ''' </returns>
    ''' <remarks>
    ''' This method is useful for serializing the webhook definition to JSON format for storage or transmission.
    ''' </remarks>
    Public Function ToJsonObject() As JsonObject
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

    ''' <summary>
    ''' Returns a string representation of the webhook.
    ''' </summary>
    ''' <returns>
    ''' A string that includes the name, method, and URL of the webhook.
    ''' </returns>
    ''' <remarks>
    ''' This method is useful for logging or displaying the webhook information in a user-friendly format.
    ''' </remarks>
    Public Overrides Function ToString() As String
        Return $"{Name} ({Method}) - {Url}"
    End Function
End Class
