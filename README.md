
# RewstWebhooks

Simple library for handling webhooks in Rewst.

## Example
```vb.net

Imports System.Text.Json.Nodes
Imports RewstWebhooks

'Example VB.NET application to demonstrate the use of RewstWebhooks library.
Public Class Form1
    ' TODO:
    ' Add a ListBox, a RichTextBox, and two Buttons to the form in the designer.
    'Create a new instance of the WebhookClient
    Private client As New WebhookClient

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Add your named secrets to the client. The name is the value used by the 'secret' property
        '   in the webhook definition to match this secret.
        client.RegisterSecret("azure-rewst", My.Settings.RewstSecret, True)
    End Sub

    'This button imports webhooks from a JSON file and populates the ListBox with them.
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'Import webhooks from a JSON file.
        Dim testJsonText As String = IO.File.ReadAllText("test.json")
        Dim testJsonObject As JsonObject = JsonObject.Parse(testJsonText)
        Dim webhooks As WebhookCollection = WebhookCollection.FromJson(testJsonObject)
        'Clear the ListBox and add the webhooks to it.
        ListBox1.Items.Clear()
        For Each webhook In webhooks
            ListBox1.Items.Add(webhook)
        Next
    End Sub

    'This button calls the selected webhook from the ListBox and displays the result in the RichTextBox.
    Private Async Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Button2.Enabled = False
        If ListBox1.SelectedIndex > -1 Then
            'Get the selected webhook from the ListBox.
            Dim paramObject As JsonObject = Nothing
            Dim webhook As Webhook = CType(ListBox1.SelectedItem, Webhook)
            'If the webhook has parameters, prompt the user for values.
            If webhook.Parameters IsNot Nothing Then
                'Create an empty instance of the parameters object from the parameters definition.
                paramObject = webhook.Parameters.CreateInstance()
                'Prompt the user for each parameter value.
                For Each prop In paramObject
                    paramObject(prop.Key) = InputBox("Enter value for " & prop.Key, "Parameter Input", prop.Value.ToString())
                Next
            End If
            'Call the webhook with the parameters.
            Dim result As JsonObject = Await client.CallWebhookAsync(webhook, paramObject)
            If result IsNot Nothing Then
                RichTextBox1.Text = result.ToJsonString()
            Else
                RichTextBox1.Text = "No result returned from webhook."
            End If
        End If
        Button2.Enabled = True
    End Sub

End Class
```
