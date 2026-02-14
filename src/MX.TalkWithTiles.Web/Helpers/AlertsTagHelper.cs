using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MX.TalkWithTiles.Web.Models;

namespace MX.TalkWithTiles.Web.Helpers;

public class AlertsTagHelper : TagHelper
{
    private const string AlertKey = "Alerts";

    [ViewContext] public required ViewContext ViewContext { get; set; }

    protected ITempDataDictionary TempData => ViewContext.TempData;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";

        if (TempData[AlertKey] == null)
            TempData[AlertKey] = JsonSerializer.Serialize(new HashSet<Alert>());

        var alerts = JsonSerializer.Deserialize<ICollection<Alert>>(TempData[AlertKey]!.ToString()!);

        var sb = new StringBuilder();

        foreach (var alert in alerts!)
            sb.Append($"<div class='alert {alert.Type}' id='inner-alert' role='alert' style='padding-top:10px'>")
              .Append("<button type='button' class='close' data-dismiss='alert' aria-label='Close'>")
              .Append("<span aria-hidden='true'>&times;</span>")
              .Append("</button>")
              .Append(alert.Message)
              .Append("</div>");

        output.Content.SetHtmlContent(sb.ToString());
    }
}