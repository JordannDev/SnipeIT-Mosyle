// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using jordanm.cloud.mosyle.Clients.SnipeIt;
using Newtonsoft.Json;
using Xunit;

namespace SnipeMosyle.Tests;

public sealed class SnipeItRequestSerializationTests
{
    [Fact]
    public void UpdateAssetRequest_SerializesPhoneNumberToSnipeCustomFieldName()
    {
        var request = new UpdateAssetRequest
        {
            ModelId = 123,
            Serial = "ABC123",
            Imei = "imei",
            Eid = "eid",
            SimCardPhoneNumber = "555-0100"
        };

        var json = JsonConvert.SerializeObject(request);

        Assert.Contains("\"_snipeit_sim_card_phone_number_15\":\"555-0100\"", json);
        Assert.Contains("\"_snipeit_imei_13\":\"imei\"", json);
        Assert.Contains("\"_snipeit_eid_14\":\"eid\"", json);
        Assert.Contains("\"model_id\":123", json);
        Assert.DoesNotContain("SimCardPhoneNumber", json);
        Assert.DoesNotContain("ModelId", json);
    }
}
