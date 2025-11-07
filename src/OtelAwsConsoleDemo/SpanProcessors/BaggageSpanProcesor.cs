using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;

public sealed class BaggageSpanProcessor : BaseProcessor<Activity>
{
    public override void OnStart(Activity activity)
    {
        var tenant = Baggage.GetBaggage("tenant.id");
        if (tenant != null)
            activity.SetTag("tenant.id", tenant);
    }
}