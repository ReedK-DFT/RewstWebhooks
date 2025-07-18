
# RewstWebhooks

*A simple library for managing and calling webhooks in Rewst.*  
[![NuGet](https://img.shields.io/nuget/v/RewstWebhooks.svg)](https://www.nuget.org/packages/RewstWebhooks/)
[![License](https://img.shields.io/github/license/ReedK-DFT/RewstWebhooks.svg)](https://github.com/ReedK-DFT/RewstWebhooks/blob/master/LICENSE.txt)

## Description

While it is easy enough to call a Rewst webhook from a .Net application using HttpClient without any additional libraries, consuming a variety of webhooks and managing their various input parameters and secrets requires some amount of scaffolding. This library facilitates all of that and more.  

Webhook secrets can be registered in the WebhookClient by name and then referenced in a webhook definition.
```vb.net
  client.RegisterSecret("my-workflow-secret", My.Settings.RewstSecret, True)
```

Webhook definitions can be written in Json following a common schema and can be imported as a WebhookCollection.  Example:
```json
{
  "name": "<unique name for webhook>",
  "description": "<descrption of purpose (used by AI agents)>",
  "url": "<the URL to the webhook in Rewst>",
  "method": "POST",
  "parameter": {
    "type": "object",
    "properties": {
      "<parameter_name>": {
        "type": "<parameter_type>",
        "description": "<description of the parameter (used by AI agents)>"
      }
    },
    "required": [
      "<parameter_name> (if required)"
    ]
  },
  "secret": "<secret name>"
}
```
The parameter and secret can be null or omitted if not needed by the webhook. The required property should be an empty array if no parameters are required.  

In addition to defining Webhook object instances, you can also use one of the overloads of the CallRawWebhookAsync method to call a URL string and pass parameters as a JsonObject, list of key-value pairs or string tuples.

## Example Windows Forms Application

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
## Example Webhooks JSON File
```json
{
  "webhooks": [
    {
      "name": "list_devices",
      "description": "List devices for a company",
      "url": "https://engine.rewst.io/webhooks/custom/trigger/000-id-000/1111-id-00011",
      "method": "POST",
      "parameter": {
        "type": "object",
        "properties": {
          "company": {
            "type": "string",
            "description": "The name of the company to filter by."
          }
        },
        "required": [
          "company"
        ]
      },
      "secret": "Default"
    },
    {
      "name": "list_companies",
      "description": "List the companies that are clients of Dragonfly Technologies",
      "url": "https://engine.rewst.io/webhooks/custom/trigger/000-id-000/1111-id-00012",
      "method": "POST",
      "parameter": null,
      "secret": "Default"
    },
    {
      "name": "find_person",
      "description": "Finds name and id information for the person matching the given name",
      "url": "https://engine.rewst.io/webhooks/custom/trigger/000-id-000/1111-id-00013",
      "method": "POST",
      "parameter": {
        "type": "object",
        "properties": {
          "firstname": {
            "type": "string",
            "description": "The first name (given name) of the person to filter by."
          },
          "lastname": {
            "type": "string",
            "description": "The last name (surname) of the person to filter by."
          },
          "email": {
            "type": "string",
            "description": "The email address of the person to filter by."
          }
        },
        "required": [
        ]
      },
      "secret": "Default"
    },
    {
      "name": "get_ticket_schedule_entries",
      "description": "Get the schedule entries for a service ticket, including the assigned engineer and the time of the appointment",
      "url": "https://engine.rewst.io/webhooks/custom/trigger/000-id-000/1111-id-00014",
      "method": "POST",
      "parameter": {
        "type": "object",
        "properties": {
          "ticketid": {
            "type": "integer",
            "description": "The name ticket id/number."
          }
        },
        "required": [
          "ticketid"
        ]
      },
      "secret": "Default"
    }
  ]
}
```